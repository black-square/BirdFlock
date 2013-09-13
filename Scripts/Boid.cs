using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour
{
  public Vector3 velocity = Vector3.zero;
  public Vector3 destPos = new Vector3( 0, 0, 3 );

  const float epsilon = 1e-10f;
 
  // Use this for initialization
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    var speedMultipliyer = 1.0f;
    var viewRadius = 0.5f;
    var optDistance = 0.1f;
    var minSpeed = 0.1f * speedMultipliyer;
    var oldVelocityMemory = 0.0f;

    //Bird is affected by 3 forses:
    // centroid
    // collisionAvoidance
    // alignmentForce

    //solve( {optFactor / optDistance = optDistance / 2}, {optFactor} );
    var optFactor = optDistance * optDistance / 2;

    // restart;
    // f := x-> factor2*(factor1/x - 1);
    // Mult := 2; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
    // F := { f(viewRadius) = 0, f(optDistance) = Mult * optFactor/optDistance };
    // Res := solve( F, {factor1, factor2} );
    // RealConsts := {viewRadius = 0.5, optDistance = 0.1, optFactor = 0.005};
    // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(viewRadius, RealConsts) );
    var optFactorWalls = 2 * optFactor / (viewRadius - optDistance);

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
            var curForce = revDir / dist * optFactorWalls * (viewRadius / dist - 1);
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

    Debug.DrawRay( transform.position, positionForce, Color.cyan );
    Debug.DrawRay( transform.position, alignmentForce, Color.yellow );

    {
      var newVelocity = speedMultipliyer * (positionForce + alignmentForce) * Time.deltaTime;
  
      var velMagn = newVelocity.magnitude;

      if( velMagn > epsilon )
      {
        if( (velocity + newVelocity).sqrMagnitude > minSpeed * minSpeed )
        {
          velocity = (1 - oldVelocityMemory) * newVelocity + oldVelocityMemory * velocity;
        }
        else
        {
          var velLen = velocity.magnitude;
          var newVelLen = newVelocity.magnitude;

          if( velLen > epsilon )
            velocity /= velLen;
          else
          {
            velocity = transform.rotation * Vector3.forward;
            velLen = 1;
          }

          newVelocity /= newVelLen;

          velocity = Vector3.Slerp( velocity, newVelocity,  newVelLen / (2 *velLen)  );
          velocity *= minSpeed;
        }
      }
    }

    transform.position += velocity * Time.deltaTime;

    if( velocity.sqrMagnitude > epsilon * epsilon )
      gameObject.transform.rotation = Quaternion.LookRotation( velocity );

    Debug.DrawRay( transform.position, velocity, Color.grey );
  }
}
