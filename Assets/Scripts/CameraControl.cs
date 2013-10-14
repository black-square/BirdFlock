using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
  public Transform target;
  public Vector3 speed = new Vector3( -2.4f, 5.0f, 2.0f );
  public Vector3 rotation;
  public float keyFactor = 0.001f;

  public float distance = 2.0f;
  public float minDistance = 0.2f;

  public float xMinLimit = -90;
  public float xMaxLimit = 90;
  public bool rotateWithTarget = false;

  void Start()
  {
    rotation = transform.eulerAngles;
  
    if( target )
      distance = (transform.position - target.transform.position).magnitude;
  
    // Make the rigid body not change rotation
    if (rigidbody)
      rigidbody.freezeRotation = true;
  }

  void LateUpdate()
  {
    if (!target)
      return;

    rotation.x += Input.GetAxis("Mouse Y") * speed.x;
    rotation.y += Input.GetAxis("Mouse X") * speed.y;
    rotation.x = MathTools.ClampAngle(rotation.x, xMinLimit, xMaxLimit);

    var distRaw = -Input.GetAxis("Mouse ScrollWheel") * speed.z;

    if( Input.GetKey("up") )
      distRaw -= speed.z * keyFactor;

    if( Input.GetKey("down") )
      distRaw += speed.z * keyFactor;

    distance = Mathf.Max( distance + distRaw, minDistance );

    var quatRot = Quaternion.identity;

    if( rotateWithTarget )
      quatRot *= target.rotation;

    quatRot *= Quaternion.Euler( rotation );

    transform.rotation = quatRot;
    transform.position = quatRot * new Vector3(0.0f, 0.0f, -distance) + target.position;

  }
}