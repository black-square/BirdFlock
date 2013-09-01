using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour
{
  public Vector3 velocity = Vector3.zero;
  public Vector3 destPos = new Vector3( 0, 0, 3 );
 
 
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
   
    foreach( var cur in neighbour )
    {
      avgSpeed += cur.GetComponent<Boid>().velocity;

      if( cur == this )
        continue;

      centeroid += cur.transform.position;
     
      var revDir = transform.position - cur.transform.position;
      var dist = revDir.magnitude;
       
      if( dist > 1e-5 )
      {
        //simplify( revDir / dist * (optFactor / dist) );
        collisionAvoidance += revDir * optFactor / ( dist * dist );
      }
    }
   
    centeroid = centeroid / neighbour.Length - transform.position;
    avgSpeed /= neighbour.Length * neighbour.Length;

    //Debug.DrawRay( transform.position, centeroid, Color.magenta );
    //Debug.DrawRay( transform.position, collisionAvoidance, Color.green );
    Debug.DrawRay( transform.position, avgSpeed, Color.yellow );

    var destanationForce = centeroid + collisionAvoidance;
    Debug.DrawRay( transform.position, destanationForce, Color.cyan );
   
    //if( destPos != transform.position )
    //  velocity += ( destPos - transform.position ).normalized / 10;

    var accel = destanationForce + avgSpeed / 0.5f;
    velocity = accel * Time.deltaTime;

    var velMagn = velocity.magnitude;

    if( velMagn < minSpeed )
      velocity = velocity / velMagn * minSpeed;

    transform.position += velocity * Time.deltaTime;
    gameObject.transform.rotation = Quaternion.LookRotation( velocity );

    Debug.DrawRay( transform.position, velocity, Color.white );
  }
}
