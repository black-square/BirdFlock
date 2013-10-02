using UnityEngine;
using System.Collections;
using System;

public class Boid : MonoBehaviour
{
  [Serializable]
  public class Settings
  {
    public float SpeedMultipliyer = 3.0f;
    public float ViewRadius = 0.5f;
    public float OptDistance = 0.1f;
    public float MinSpeed { get{ return 0.1f * SpeedMultipliyer; } }
    public float InclineFactor { get{ return 300.0f / SpeedMultipliyer; } }
    public float AligmentForcePart = 0.003f;
    public float TotalForceMultipliyer = 12;
    public float Inertness = 0.5f;
  }

  const float epsilon = 1e-10f;
  private Settings sts = null;
  public Settings SettingsRef {
    get { return sts; }
    set { sts = value; }
  }

  private Vector3 velocity = Vector3.zero;
  public Vector3 Velocity { get{ return velocity; } }

  void Start()
  {
    if( sts == null )
    {
      sts = Main.GetSettings( gameObject );

      if( sts == null )
      {
        Debug.LogWarning( "Boid initialized with standalone settings copy" );
        sts = new Settings();
      }
    }
  }

  struct SeparationForce
  {
    public SeparationForce( Settings sts )
    {
      //We have to compensate cohesion force which in OptDistance point
      //equals OptDistance / 2
      //solve( {optFactor / OptDistance = OptDistance / 2}, {optFactor} );
      optFactor = sts.OptDistance * sts.OptDistance / 2;
    }

    public bool Calc( Vector3 cur, Vector3 other, out Vector3 force )
    {
      var revDir = cur - other;
      var dist = revDir.magnitude;

      force = Vector3.zero;

      if( dist < epsilon ) // Do not take into account oneself
        return false;

      //simplify( revDir / dist * (optFactor / dist) );
      force = revDir * optFactor / ( dist * dist );
      return true;
    }

    public float Calc( float dist )
    {
      return optFactor/ dist;
    }
    
    readonly float optFactor;
  };

  struct CollisionAvoidanceForce
  {
    public CollisionAvoidanceForce( Settings sts, float sepForceAtOptDistance, bool useSquareFunction )
    {
      // Maple:
      // restart;
      // f := x-> factor2*(factor1/x^2 - 1);
      // Mult := 2 * SpeedMultipliyer; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
      // F := { f(ViewRadius) = 0, f(OptDistance) = Mult * sepForceAtOptDistance }:
      // Res := solve( F, {factor1, factor2} );
      // RealConsts := {ViewRadius = 0.5, OptDistance = 0.1, sepForceAtOptDistance = 0.05, SpeedMultipliyer = 3};
      // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(ViewRadius, RealConsts) );
      forceDlg = null;

      if( useSquareFunction )
      {
        var ViewRadius2 = sts.ViewRadius * sts.ViewRadius;
        var OptDistance2 = sts.OptDistance * sts.OptDistance;
        factor1 = ViewRadius2;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * OptDistance2 / ( OptDistance2 - ViewRadius2 );
        forceDlg = CalcImplSquared;
      }
      else
      {
        factor1 = sts.ViewRadius;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * sts.OptDistance / ( sts.OptDistance - sts.ViewRadius );
        forceDlg = CalcImplLinear;
      }
    }

    public struct Force
    {
      public Vector3 dir;
      public Vector3 pos;
    };

    public bool Calc( Vector3 cur, Collider cld, out Force force )
    {
      var pointOnBounds = CalcPointOnBounds( cld, cur );
      var revDir = cur - pointOnBounds;
      var dist = revDir.magnitude;
      force.dir = revDir / dist * forceDlg(dist);
      force.pos = pointOnBounds;
      return true;
    }

    static Vector3 CalcPointOnBounds( Collider cld, Vector3 cur )
    {
      SphereCollider sphc = cld as SphereCollider;

      if( !sphc )
        return cld.ClosestPointOnBounds( cur );
      else
      {
        //cld.ClosestPointOnBounds returns not precise values for spheres
        //Fortunately they could be calculated easily
        var realPos = sphc.transform.position + sphc.center;
        var dir = cur - realPos;
        var realScale = sphc.transform.lossyScale;
        var realRadius = sphc.radius * Mathf.Max( realScale.x, realScale.y, realScale.z );
        var dirLength = dir.magnitude;

        //BoxCollider.ClosestPointOnBounds returns NaN if points are inside the volume
        if( dirLength < realRadius )
          return new Vector3( float.NaN, float.NaN, float.NaN );

        var dirFraction = realRadius / dirLength;
        return realPos + dirFraction * dir;
      }
    }

    float CalcImplLinear( float dist )
    {
      return factor2 * (factor1 / dist - 1);
    }

    float CalcImplSquared( float dist )
    {
      return factor2 * (factor1 / (dist * dist) - 1);
    }

    delegate float ForceDlg(float dist);
    readonly float factor1;
    readonly float factor2;
    readonly ForceDlg forceDlg;
  };

  void FixedUpdate()
  {
    //Algorithm based on
    //http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
    //http://www.red3d.com/cwr/boids/

    //Bird is affected by 3 forses:
    // cohesion
    // separation + collisionAvoidance
    // alignmentForce

    SeparationForce sepForce = new SeparationForce(sts);
    CollisionAvoidanceForce collAvoid = new CollisionAvoidanceForce( sts, sepForce.Calc(sts.OptDistance), false);

    var centeroid = Vector3.zero;
    var collisionAvoidance = Vector3.zero;
    var avgSpeed = Vector3.zero;
    var neighbourCount = 0;
   
    foreach( var cur in Physics.OverlapSphere(transform.position, sts.ViewRadius) )
    {
      Boid boid = cur.GetComponent<Boid>();

      if( boid ) //Birds processing
      {
        Vector3 separationForce;

        if( !sepForce.Calc(transform.position, cur.transform.position, out separationForce) )
          continue;

        collisionAvoidance += separationForce;
        ++neighbourCount;
        centeroid += cur.transform.position;
        avgSpeed += boid.velocity;
      }
      else //Obstacles processing
      {
        CollisionAvoidanceForce.Force force;
        if( collAvoid.Calc(transform.position, cur, out force) )
        {
          collisionAvoidance += force.dir;
          Debug.DrawRay( force.pos, force.dir, Color.red );
        }
      }
    }

    if( neighbourCount > 0 )
    {
      centeroid = centeroid / neighbourCount - transform.position;
      avgSpeed = avgSpeed / neighbourCount - velocity;
    }

    //Debug.DrawRay( transform.position, centeroid, Color.magenta );
    //Debug.DrawRay( transform.position, collisionAvoidance, Color.green );

    var positionForce = (1.0f - sts.AligmentForcePart) * sts.SpeedMultipliyer * (centeroid + collisionAvoidance);
    var alignmentForce = sts.AligmentForcePart * avgSpeed / Time.deltaTime;
    var totalForce = sts.TotalForceMultipliyer * (positionForce + alignmentForce);

    var newVelocity = (1 - sts.Inertness) * (totalForce * Time.deltaTime) + sts.Inertness * velocity;

    velocity = CalcNewVelocity( sts.MinSpeed, velocity, newVelocity, transform.rotation * Vector3.forward );

    var rotation = CalcRotation( sts.InclineFactor, velocity, totalForce );

    if( IsValid(rotation) )
      gameObject.transform.rotation = rotation;

    Debug.DrawRay( transform.position, velocity, Color.grey );
    Debug.DrawRay( transform.position, positionForce, Color.cyan );
    Debug.DrawRay( transform.position, alignmentForce, Color.yellow );
  }

  void Update()
  {
    transform.position += velocity * Time.deltaTime;
  }

  static Vector3 CalcNewVelocity( float minSpeed, Vector3 curVel, Vector3 dsrVel, Vector3 defaultVelocity )
  {
    var curVelLen = curVel.magnitude;

    if( curVelLen > epsilon )
      curVel /= curVelLen;
    else
    {
      curVel = defaultVelocity;
      curVelLen = 1;
    }

    var dsrVelLen = dsrVel.magnitude;
    var resultLen = minSpeed;

    if( dsrVelLen > epsilon )
    {
      dsrVel /= dsrVelLen;

      var angleFactor = ( 1 - Vector3.Dot(dsrVel, curVel) ) / 2; //smartplot((1-cos(x))/2);
      var rotReqLength =  2 * curVelLen * angleFactor;
      var speedRest = dsrVelLen - rotReqLength;

      if( speedRest > 0 )
      {
        curVel = dsrVel;
        resultLen = speedRest;
      }
      else
      {
        var rotationFactor = (rotReqLength + speedRest) / rotReqLength;
        curVel = Vector3.Slerp( curVel, dsrVel, rotationFactor );
      }

      if( resultLen < minSpeed )
        resultLen = minSpeed;
    }

    return curVel * resultLen;
  }

  static string ToS( Vector3 vec )
  {
    return String.Format("{0:0.00000}:[{1:0.00000}, {2:0.00000}, {3:0.00000}]", vec.magnitude, vec.x, vec.y, vec.z );
  }

  static float AngleXZProjected( Vector3 vec1, Vector3 vec2 )
  {
    vec1.y = 0;
    vec2.y = 0;

    return Vector3.Angle( vec1, vec2);
  }

  static Vector3 RightVectorXZProjected( Vector3 vec )
  {
    vec.y = 0;
    return Quaternion.AngleAxis(90, Vector3.up) * vec;
    //Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * Vector3.right
  }

  static float VecProjectedLength( Vector3 vec, Vector3 vecNormal )
  {
    var proj = Vector3.Project( vec, vecNormal );
    return proj.magnitude  * Mathf.Sign( Vector3.Dot(proj, vecNormal) );
  }

  static Quaternion CalcRotation( float inclineFactor, Vector3 velocity, Vector3 totalForce )
  {
    if( velocity.sqrMagnitude < epsilon )
      return new Quaternion( float.NaN, float.NaN, float.NaN, float.NaN );

    var rightVec = RightVectorXZProjected(velocity);
    var inclineDeg = VecProjectedLength( totalForce, rightVec ) * -inclineFactor;
    return Quaternion.LookRotation( velocity ) * Quaternion.AngleAxis(Mathf.Clamp(inclineDeg, -90, 90), Vector3.forward);
  }

  static bool IsValid ( Quaternion q )
  {
    #pragma warning disable 1718
    return q == q; //Comparisons to NaN always return false, no matter what the value of the float is.
    #pragma warning restore 1718
  }
}
