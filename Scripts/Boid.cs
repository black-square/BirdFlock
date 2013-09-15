using UnityEngine;
using System.Collections;
using System;

public class Boid : MonoBehaviour
{
  const float epsilon = 1e-10f;

  const float speedMultipliyer = 3.0f;
  const float viewRadius = 0.5f;
  const float optDistance = 0.1f;
  const float minSpeed = 0.1f * speedMultipliyer;
  const float oldVelocityMemory = 0.0f; //Helps to avoid abrupt movements
  const float inclineFactor = 300.0f / speedMultipliyer;

  public Vector3 velocity = Vector3.zero;

  struct SeparationForce
  {
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

    //We have to compensate cohesion force which in optDistance point
    //equals optDistance / 2
    //solve( {optFactor / optDistance = optDistance / 2}, {optFactor} );
    const float optFactor = optDistance * optDistance / 2;
  };

  struct CollisionAvoidanceForce
  {
    public CollisionAvoidanceForce( float sepForceAtOptDistance, bool useSquareFunction )
    {
      // Maple:
      // restart;
      // f := x-> factor2*(factor1/x^2 - 1);
      // Mult := 2 * speedMultipliyer; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
      // F := { f(viewRadius) = 0, f(optDistance) = Mult * sepForceAtOptDistance }:
      // Res := solve( F, {factor1, factor2} );
      // RealConsts := {viewRadius = 0.5, optDistance = 0.1, sepForceAtOptDistance = 0.05, speedMultipliyer = 3};
      // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(viewRadius, RealConsts) );
      forceDlg = null;

      if( useSquareFunction )
      {
        var viewRadius2 = viewRadius * viewRadius;
        var optDistance2 = optDistance * optDistance;
        factor1 = viewRadius2;
        factor2 = -2 * speedMultipliyer * sepForceAtOptDistance * optDistance2 / ( optDistance2 - viewRadius2 );
        forceDlg = CalcImplSquared;
      }
      else
      {
        factor1 = viewRadius;
        factor2 = -2 * speedMultipliyer * sepForceAtOptDistance * optDistance / ( optDistance - viewRadius );
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
      var pointOnBounds = cld.ClosestPointOnBounds( cur );
      RaycastHit hit;

      force = new Force();

      if( !cld.Raycast(new Ray(cur, pointOnBounds - cur), out hit, viewRadius) )
        return false;

      var revDir = cur - hit.point;
      var dist = revDir.magnitude;
      force.dir = revDir / dist * forceDlg(dist);
      force.pos = hit.point;
      return true;
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

  void Update()
  {

    //Bird is affected by 3 forses:
    // centroid
    // collisionAvoidance
    // alignmentForce

    SeparationForce sepForce;
    CollisionAvoidanceForce collAvoid = new CollisionAvoidanceForce( sepForce.Calc(optDistance), false);

    var centeroid = Vector3.zero;
    var collisionAvoidance = Vector3.zero;
    var avgSpeed = Vector3.zero;
    var neighbourCount = 0;
   
    foreach( var cur in Physics.OverlapSphere(transform.position, viewRadius) )
    {
      if( cur is SphereCollider )
      {
        Vector3 separationForce;

        if( !sepForce.Calc(transform.position, cur.transform.position, out separationForce) )
          continue;

        collisionAvoidance += separationForce;
        ++neighbourCount;
        centeroid += cur.transform.position;
        avgSpeed += cur.GetComponent<Boid>().velocity;
      }
      else if( cur is BoxCollider )
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

    var positionForce = 1.0f * (centeroid + collisionAvoidance);
    var alignmentForce = 0.5f * avgSpeed;
    var totalForce = (positionForce + alignmentForce);

    var newVelocity = speedMultipliyer * totalForce * Time.deltaTime;

    Debug.DrawRay( transform.position, velocity, Color.grey );
    Debug.DrawRay( transform.position, positionForce, Color.cyan );
    Debug.DrawRay( transform.position, alignmentForce, Color.yellow );

    var oldVelocity = velocity;
    var velLen = velocity.magnitude;

    if( velLen > epsilon )
      velocity /= velLen;
    else
    {
      velocity = transform.rotation * Vector3.forward;
      velLen = 1;
    }

    var newVelLen = newVelocity.magnitude;
    var resultLen = minSpeed;

    if( newVelLen > epsilon )
    {
      newVelocity /= newVelLen;

      var angleFactor = (1 - Vector3.Dot(newVelocity, velocity)) / 2; //smartplot((1-cos(x))/2);
      var rotReqLength = angleFactor / (2 * velLen);
      var rotationFactor = newVelLen * rotReqLength;

      if( rotationFactor > 1 )
        resultLen = (1.0f - rotationFactor) / rotReqLength;

      velocity = Vector3.Slerp( velocity, newVelocity, rotationFactor );

      if( resultLen < minSpeed )
        resultLen = minSpeed;
    }

    velocity = (1 - oldVelocityMemory) * (velocity * resultLen) + oldVelocityMemory * oldVelocity;

    var rightVec = RightVectorXZProjected(velocity);
    var inclineDeg = VecProjectedLength( totalForce, rightVec ) * -inclineFactor;
    transform.position += velocity * Time.deltaTime;
    gameObject.transform.rotation = Quaternion.LookRotation( velocity ) * Quaternion.AngleAxis(Mathf.Clamp(inclineDeg, -90, 90), Vector3.forward);
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
}
