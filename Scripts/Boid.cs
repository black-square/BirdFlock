using UnityEngine;
using System.Collections;
using System;

public class Boid : MonoBehaviour
{
  public Vector3 velocity = Vector3.zero;
  public Vector3 destPos = new Vector3( 0, 0, 3 );

  const float epsilon = 1e-10f;
 
  // Use this for initialization
  void Start()
  {

  }

  void OnGUI()
  {
    //GUI.Label( new Rect(0, 20,2000, 2000),  String.Format("Velocity {0}", velocity) );
  }

  static string ToS( Vector3 vec )
  {
    return String.Format("{0:0.00000}:[{1:0.00000}, {2:0.00000}, {3:0.00000}]", vec.magnitude, vec.x, vec.y, vec.z );
  }

  // Update is called once per frame
  void Update()
  {
    var speedMultipliyer = 3.0f;
    var viewRadius = 0.5f;
    var optDistance = 0.1f;
    var minSpeed = 0.1f * speedMultipliyer;
    var oldVelocityMemory = 0.5f; //Helps to avoid abrupt movements

    //Bird is affected by 3 forses:
    // centroid
    // collisionAvoidance
    // alignmentForce

    //solve( {optFactor / optDistance = optDistance / 2}, {optFactor} );
    var optFactor = optDistance * optDistance / 2;

    // restart;
    // f := x-> factor2*(factor1/x - 1);
    // Mult := 2 * speedMultipliyer; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
    // F := { f(viewRadius) = 0, f(optDistance) = Mult * optFactor/optDistance };
    // Res := solve( F, {factor1, factor2} );
    // RealConsts := {viewRadius = 0.5, optDistance = 0.1, optFactor = 0.005};
    // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(viewRadius, RealConsts) );

#if !NOT_DEFINED
    var optFactor1Walls = viewRadius;
    var optFactor2Walls = 2 * speedMultipliyer * optFactor / (viewRadius - optDistance);
    Func<float, float> walsForceDlg = dist => optFactor2Walls * (optFactor1Walls / dist - 1);
#else
    var optFactor1Walls = viewRadius * viewRadius;
    var optFactor2Walls = 2*speedMultipliyer*optFactor*optDistance/(viewRadius * viewRadius - optDistance * optDistance);
    Func<float, float> walsForceDlg = dist => optFactor2Walls * (optFactor1Walls / (dist * dist) - 1);
#endif

    var neighbour = Physics.OverlapSphere( transform.position, viewRadius );
    var centeroid = Vector3.zero;
    var collisionAvoidance = Vector3.zero;
    var avgSpeed = Vector3.zero;
    var neighbourCount = 0;
   
    foreach( var cur in neighbour )
    {
      if( cur is SphereCollider )
      {
        var revDir = transform.position - cur.transform.position;
        var dist = revDir.magnitude;

        if( dist < epsilon ) // Do not take into account oneself
          continue;

        ++neighbourCount;

        centeroid += cur.transform.position;

        //simplify( revDir / dist * (optFactor / dist) );
        collisionAvoidance += revDir * optFactor / ( dist * dist );

        avgSpeed += cur.GetComponent<Boid>().velocity;
      }
      else
      {
        var bc = cur as BoxCollider;
  
        if( bc )
        {
          var pointOnBounds = cur.ClosestPointOnBounds( transform.position );
          RaycastHit hit;

          if( cur.Raycast(new Ray(transform.position, pointOnBounds - transform.position), out hit, viewRadius) )
          {
            var revDir = transform.position - hit.point;
            var dist = revDir.magnitude;
            var curForce = revDir / dist * walsForceDlg(dist);
            collisionAvoidance += curForce;
            Debug.DrawRay( hit.point, curForce, Color.red );
          }
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

    {
      var newVelocity = speedMultipliyer * (positionForce + alignmentForce) * Time.deltaTime;
      var newVelLen = newVelocity.magnitude;

      Debug.DrawRay( transform.position, velocity, Color.grey );
      Debug.DrawRay( transform.position + velocity, positionForce, Color.cyan );
      Debug.DrawRay( transform.position + velocity, alignmentForce, Color.yellow );

      //print( String.Format("Velocity {0} <- {1}", ToS(velocity), ToS(newVelocity)) );

      //Debug.DrawRay( transform.position + velocity, newVelocity, Color.magenta );

      if( newVelLen > epsilon )
      {
        var oldVelocity = velocity;
        var velLen = velocity.magnitude;

        if( velLen > epsilon )
          velocity /= velLen;
        else
        {
          velocity = transform.rotation * Vector3.forward;
          velLen = 1;
        }

        newVelocity /= newVelLen;

        var rotReqLength = (1 - Vector3.Dot( newVelocity, velocity )) / (2 * velLen); //1 - cos(alpha)
        var rotCoef = newVelLen * rotReqLength;

        var resultLen = 0.0f;

        if( rotCoef > 1 )
          resultLen = (1.0f - rotCoef)/ rotReqLength;

        if( resultLen < minSpeed )
          resultLen = minSpeed;

        velocity = Vector3.Slerp( velocity, newVelocity, rotCoef );
        velocity *= resultLen;

        velocity = (1 - oldVelocityMemory) * velocity + oldVelocityMemory * oldVelocity;
      }
    }

    transform.position += velocity * Time.deltaTime;

    if( velocity.sqrMagnitude > epsilon * epsilon )
      gameObject.transform.rotation = Quaternion.LookRotation( velocity );
  }
}
