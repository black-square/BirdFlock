using UnityEngine;
using System.Collections;

public class Boid : MonoBehaviour {
	
	public Vector3 velocity = Vector3.zero;
	public Vector3 destPos = new Vector3 ( 0, 0, 3 );
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.transform.Rotate(0, 1, 1);
		
		var viewRadius = 0.3f;
		var optDistance = 0.1f;
		
		//solve( {optFactor / optDistance = optDistance / 2}, {optFactor} );
		var optFactor = optDistance * optDistance / 2; 
		
		var neighbour = Physics.OverlapSphere(transform.position, viewRadius);
		var center = Vector3.zero;
		var collisionAvoidance = Vector3.zero;
		
		foreach( var cur in neighbour )
		{
			if( cur == this )
				continue;
			
			center += cur.transform.position;
			
			var revDir = transform.position - cur.transform.position;
			var dist = revDir.magnitude;
				
			if( dist > 1e-5 )
			{
				//simplify( revDir / dist * (optFactor / dist) );
				collisionAvoidance += revDir * optFactor / ( dist * dist );
			}
		}
		
		center = center / neighbour.Length - transform.position;
		
		velocity = center + collisionAvoidance;
		
		if( destPos != transform.position )
			velocity += (destPos - transform.position).normalized / 10;	
		
		velocity *= 3;
		
		
		Debug.DrawRay(transform.position, center, Color.magenta);
		Debug.DrawRay(transform.position, collisionAvoidance, Color.green);
		
		transform.position += velocity * Time.deltaTime;
	}
}
