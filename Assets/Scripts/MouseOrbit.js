var target : Transform;

var xSpeed = 250.0;
var ySpeed = 120.0;
var zSpeed = 100.0;

var yMinLimit = -90;
var yMaxLimit = 90;

private var x = 0.0;
private var y = 0.0;
private var distance = 10.0;

@script AddComponentMenu("Camera-Control/Mouse Orbit")

function Start () 
{
  var angles = transform.eulerAngles;
  x = angles.y;
  y = angles.x;

  if( target )
    distance = (transform.position - target.transform.position).magnitude;

  // Make the rigid body not change rotation
  if (rigidbody)
    rigidbody.freezeRotation = true;
}

function LateUpdate ()
{
    if (target)
    {
        x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;

        var distRaw = -Input.GetAxis("Mouse ScrollWheel") * zSpeed;

        if( Input.GetKey("up") )
          --distRaw;
        if( Input.GetKey("down") )
          ++distRaw;

        distance = Mathf.Max( distance + distRaw * 0.02, 0 );

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        var rotation;

        if(true)
          rotation = Quaternion.Euler(y, x, 0);
        else
        {
          var angles = target.eulerAngles;
          rotation = Quaternion.Euler(angles.x + 30, angles.y, 0);
        }
        
        transform.rotation = rotation;
        transform.position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
    }
}

static function ClampAngle (angle : float, min : float, max : float) {
 if (angle < -360)
   angle += 360;
 if (angle > 360)
   angle -= 360;
 return Mathf.Clamp (angle, min, max);
}