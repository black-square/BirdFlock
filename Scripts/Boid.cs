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
    var viewRadius = 0.3f;
    var optDistance = 0.1f;
    var minSpeed = 0.1f;

    //solve( {optFactor / optDistance = optDistance / 2}, {optFactor} );
    var optFactor = optDistance * optDistance / 2;

    var neighbour = Physics.OverlapSphere( transform.position, viewRadius );
    var centeroid = Vector3.zero;
    var collisionAvoidance = Vector3.zero;
    var avgSpeed = Vector3.zero;
    var neighbourCount = 0;
   
    foreach( var cur in neighbour )
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

    if( neighbourCount > 0 )
    {
      centeroid = centeroid / neighbourCount - transform.position;
      avgSpeed /= neighbourCount;
    }

    //Debug.DrawRay( transform.position, centeroid, Color.magenta );
    //Debug.DrawRay( transform.position, collisionAvoidance, Color.green );

    var positionForce = 2.0f * (centeroid + collisionAvoidance);
    var alignmentForce = 0.2f * (avgSpeed - velocity);

    Debug.DrawRay( transform.position, positionForce, Color.cyan );
    Debug.DrawRay( transform.position, alignmentForce, Color.yellow );

    //if( destPos != transform.position )
    //  velocity += ( destPos - transform.position ).normalized / 10;
    var oldVelocityMemory = 0.2f;

    velocity = (1 - oldVelocityMemory) * (positionForce + alignmentForce) * Time.deltaTime + oldVelocityMemory * velocity;

    var velMagn = velocity.magnitude;

    if( velMagn < minSpeed )
      velocity = velocity / velMagn * minSpeed;

    transform.position += velocity * Time.deltaTime;

    if( velMagn > epsilon )
      gameObject.transform.rotation = Quaternion.LookRotation( velocity );

    Debug.DrawRay( transform.position, velocity, Color.white );
  }
}
