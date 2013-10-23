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
    public float AligmentForcePart = 0.002f;
    public float TotalForceMultipliyer = 12;
    public float Inertness = 0.5f;
    public float VerticalPriority = 1.0f;

    [System.Xml.Serialization.XmlIgnore]
    public Trace Trace { get; set; }
    public float AttractrionForce = 0.02f;
  }

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

      if( sts != null )
        Debug.LogWarning( "Boid initialized with default settings" );
      else
      {
        Debug.LogWarning( "Boid initialized with standalone settings copy" );
        sts = new Settings();
      }
    }
  }

  void FixedUpdate()
  {
    //Algorithm based on
    //http://www.cs.toronto.edu/~dt/siggraph97-course/cwr87/
    //http://www.red3d.com/cwr/boids/

    //Bird is affected by 3 forses:
    // cohesion
    // separation + collisionAvoidance
    // alignmentForce

    var sepForce = new BoidTools.SeparationForce(sts);
    var collAvoid = new BoidTools.CollisionAvoidanceForce( sts, sepForce.Calc(sts.OptDistance) );

    var centeroid = Vector3.zero;
    var collisionAvoidance = Vector3.zero;
    var avgSpeed = Vector3.zero;
    var neighbourCount = 0;
    var direction = transform.rotation * Vector3.forward;
    var curPos = transform.position;
   
    foreach( var vis in Physics.OverlapSphere(curPos, sts.ViewRadius) )
    {
      var visPos = vis.transform.position;
      Boid boid;

      if( (boid = vis.GetComponent<Boid>()) != null ) //Birds processing
      {
        Vector3 separationForce;

        if( !sepForce.Calc(curPos, visPos, out separationForce) )
          continue;

        collisionAvoidance += separationForce;
        ++neighbourCount;
        centeroid += visPos;
        avgSpeed += boid.velocity;
      }
      else if( vis.GetComponent<WayPoint>() )
      {
        //Just ignore WayPoints objects to skip collision avoidness
      }
      else //Obstacles processing
      {
        BoidTools.CollisionAvoidanceForce.Force force;
        if( collAvoid.Calc(curPos, direction, vis, out force) )
        {
          collisionAvoidance += force.dir;
          Debug.DrawRay( force.pos, force.dir, Color.red );
        }
      }
    }

    if( neighbourCount > 0 )
    {
      centeroid = centeroid / neighbourCount - curPos;
      centeroid.y *= sts.VerticalPriority; //Spherical shape of flock looks unnatural, so let's scale it along y axis
      avgSpeed = avgSpeed / neighbourCount - velocity;
    }

    //Debug.DrawRay( curPos, centeroid, Color.magenta );
    //Debug.DrawRay( curPos, collisionAvoidance, Color.green );

    var positionForce = (1.0f - sts.AligmentForcePart) * sts.SpeedMultipliyer * (centeroid + collisionAvoidance);
    var alignmentForce = sts.AligmentForcePart * avgSpeed / Time.deltaTime;
    var attractionForce = CalculateAttractionForce( sts, curPos, velocity );
    var totalForce = sts.TotalForceMultipliyer * ( positionForce + alignmentForce + attractionForce );

    var newVelocity = (1 - sts.Inertness) * (totalForce * Time.deltaTime) + sts.Inertness * velocity;

    velocity = CalcNewVelocity( sts.MinSpeed, velocity, newVelocity, direction );

    var rotation = CalcRotation( sts.InclineFactor, velocity, totalForce );

    if( MathTools.IsValid(rotation) )
      gameObject.transform.rotation = rotation;

    Debug.DrawRay( curPos, velocity, Color.grey );
    Debug.DrawRay( curPos, positionForce, Color.cyan );
    Debug.DrawRay( curPos, alignmentForce, Color.yellow );
  }

  void Update()
  {
    transform.position += velocity * Time.deltaTime;
  }

  static Vector3 CalculateAttractionForce( Settings sts, Vector3 curPos, Vector3 curVelocity )
  {
    if( !sts.Trace )
      return Vector3.zero;

    var attrPos = sts.Trace.GetAtractionPoint();
    var direction = (attrPos - curPos).normalized;

    var factor = sts.AttractrionForce * sts.SpeedMultipliyer * MathTools.AngleToFactor( direction, curVelocity );

    return factor * direction;
  }

  static Vector3 CalcNewVelocity( float minSpeed, Vector3 curVel, Vector3 dsrVel, Vector3 defaultVelocity )
  {
    //We have to take into account that bird can't change their direction instantly. That's why
    //dsrVel (desired velocity) influence first of all on flying direction and after that on
    //velocity oneself

    var curVelLen = curVel.magnitude;

    if( curVelLen > MathTools.epsilon )
      curVel /= curVelLen;
    else
    {
      curVel = defaultVelocity;
      curVelLen = 1;
    }

    var dsrVelLen = dsrVel.magnitude;
    var resultLen = minSpeed;

    if( dsrVelLen > MathTools.epsilon )
    {
      dsrVel /= dsrVelLen;

      //Map rotation to factor [0..1]
      var angleFactor = MathTools.AngleToFactor(dsrVel, curVel);

      //If dsrVel is twice bigger than curVelLen bird can rotate on any angle
      var rotReqLength = 2 * curVelLen * angleFactor;

      //Velocity magnitude remained after rotation
      var speedRest = dsrVelLen - rotReqLength;

      if( speedRest > 0 )
      {
        curVel = dsrVel;
        resultLen = speedRest;
      }
      else
      {
        curVel = Vector3.Slerp( curVel, dsrVel, dsrVelLen / rotReqLength );
      }

      if( resultLen < minSpeed )
        resultLen = minSpeed;
    }

    return curVel * resultLen;
  }

  static Quaternion CalcRotation( float inclineFactor, Vector3 velocity, Vector3 totalForce )
  {
    if( velocity.sqrMagnitude < MathTools.sqrEpsilon )
      return new Quaternion( float.NaN, float.NaN, float.NaN, float.NaN );

    var rightVec = MathTools.RightVectorXZProjected(velocity);
    var inclineDeg = MathTools.VecProjectedLength( totalForce, rightVec ) * -inclineFactor;
    return Quaternion.LookRotation( velocity ) * Quaternion.AngleAxis(Mathf.Clamp(inclineDeg, -90, 90), Vector3.forward);
  }
}
