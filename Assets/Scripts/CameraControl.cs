using System;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
  public Transform target;

  [Serializable]
  public class Settings
  {
    public Vector3 speed = new Vector3( -2.4f, 5.0f, 2.0f );
    public Vector3 rotation;
    public float keyFactor = 0.005f;
  
    public float distance = 2.0f;
    public float minDistance = 0.2f;
  
    public float xMinLimit = -90;
    public float xMaxLimit = 90;
    public bool rotateWithTarget = false;

    public bool isDisabled = false;
  }

  public Settings settings;
  static private Settings globalSettings;

  public static Settings GlobalSettings { get{ return globalSettings; } }

  void Start()
  {
    if( globalSettings == null )
    {
      settings.rotation = transform.eulerAngles;
    
      if( target )
        settings.distance = (transform.position - target.transform.position).magnitude;

      globalSettings = settings;
    }
    else
      settings = globalSettings;
    
    // Make the rigid body not change rotation
    if (rigidbody)
      rigidbody.freezeRotation = true;
  }

  void CheckForNewTarget()
  {
    if( Input.GetMouseButtonDown(0) )
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit[] hits = Physics.RaycastAll( ray, Camera.main.farClipPlane );
  
      foreach( var hit in hits )
      {
        Boid boid = hit.collider.GetComponent<Boid>();
  
        if( boid )
        {
          target = hit.collider.gameObject.transform;
          break;
        }
      }
    }
  }

  void LateUpdate()
  {
    if( !settings.isDisabled )
      CheckForNewTarget();

    if (!target)
      return;

    if( !settings.isDisabled )
    {
      settings.rotation.x += Input.GetAxis("Mouse Y") * settings.speed.x;
      settings.rotation.y += Input.GetAxis("Mouse X") * settings.speed.y;
      settings.rotation.x = MathTools.ClampAngle(settings.rotation.x, settings.xMinLimit, settings.xMaxLimit);
  
      var distRaw = -Input.GetAxis("Mouse ScrollWheel") * settings.speed.z;
  
      if( Input.GetKey("up") )
        distRaw -= settings.speed.z * settings.keyFactor;
  
      if( Input.GetKey("down") )
        distRaw += settings.speed.z * settings.keyFactor;

      settings.distance = Mathf.Max( settings.distance + distRaw, settings.minDistance );
    }

    var quatRot = Quaternion.identity;

    if( settings.rotateWithTarget )
      quatRot *= target.rotation;

    quatRot *= Quaternion.Euler( settings.rotation );

    transform.rotation = quatRot;
    transform.position = quatRot * new Vector3(0.0f, 0.0f, -settings.distance) + target.position;
  }
}