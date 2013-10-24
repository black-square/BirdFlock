using System;
using UnityEngine;

public class CameraControl: MonoBehaviour
{
  [SerializeField]
  private Transform target;

  [Serializable]
  public class Settings
  {
    public Vector3 speed = new Vector3( -2.4f, 5.0f, 2.0f );
    public Vector3 rotation;
    public float keyFactor = 1.0f;
  
    public float distance = 2.0f;
    public float minDistance = 0.2f;
  
    public float xMinLimit = -90;
    public float xMaxLimit = 90;
    public bool rotateWithTarget = false;

    public bool isEnabled = true;
    public Vector3 position = Vector3.zero;
    public bool isAttached = true;
  }

  [SerializeField]
  private Settings settings;

  //static is used to keep values after restart
  static private Settings globalSettings;

  public bool Enabled { get{ return settings.isEnabled; } set{ settings.isEnabled = value; } }
  public bool Attached { get{ return settings.isAttached; } set{ settings.isAttached = value; } }
  public Transform Target { get{ return target; } set{ target = value; }   }

  void Start()
  {
    if( globalSettings == null )
    {
      settings.rotation = transform.eulerAngles;
    
      if( target )
      {
        settings.distance = (transform.position - target.transform.position).magnitude;
        settings.position = target.transform.position;
        settings.isAttached = true;
      }

      globalSettings = settings;
    }
    else
      settings = globalSettings;
    
    // Make the rigid body not change rotation
    if (rigidbody)
      rigidbody.freezeRotation = true;
  }

  public void CheckForNewTarget( Vector3 mousePos )
  {
    if( settings.isEnabled )
    {
      var ray = Camera.main.ScreenPointToRay(mousePos);
      RaycastHit[] hits = Physics.RaycastAll( ray, Camera.main.farClipPlane );
  
      foreach( var hit in hits )
      {
        if( hit.collider is BoxCollider || hit.collider is SphereCollider )
        {
          target = hit.collider.gameObject.transform;
          settings.isAttached = true;
          break;
        }
      }
    }
  }

  private bool IsAttached()
  {
    return target && settings.isAttached;
  }

  void LateUpdate()
  {
    if( Input.GetKeyDown(KeyCode.Tab) )
      settings.isAttached = false;

    if( IsAttached() )
      settings.position = target.transform.position;

    if( settings.isEnabled )
    {
      settings.rotation.x += Input.GetAxis("Mouse Y") * settings.speed.x;
      settings.rotation.y += Input.GetAxis("Mouse X") * settings.speed.y;
      settings.rotation.x = MathTools.ClampAngle(settings.rotation.x, settings.xMinLimit, settings.xMaxLimit);
  
      var distRaw = -Input.GetAxis("Mouse ScrollWheel") * settings.speed.z;
  
      if( Input.GetKey(KeyCode.UpArrow) )
        distRaw -= settings.speed.z * settings.keyFactor * Time.deltaTime;
  
      if( Input.GetKey(KeyCode.DownArrow) )
        distRaw += settings.speed.z * settings.keyFactor * Time.deltaTime;

      settings.distance = Mathf.Max( settings.distance + distRaw, settings.minDistance );
    }

    var quatRot = Quaternion.identity;

    if( IsAttached() && settings.rotateWithTarget )
      quatRot *= target.rotation;

    quatRot *= Quaternion.Euler( settings.rotation );

    if( settings.isEnabled )
    {
      var shift = Vector3.zero;

      if( Input.GetKey(KeyCode.W) )
        shift.z += settings.speed.z * settings.keyFactor * Time.deltaTime;

      if( Input.GetKey(KeyCode.S) )
        shift.z -= settings.speed.z * settings.keyFactor * Time.deltaTime;

      if( Input.GetKey(KeyCode.A) )
        shift.x += settings.speed.x * settings.keyFactor * Time.deltaTime;

      if( Input.GetKey(KeyCode.D) )
        shift.x -= settings.speed.x * settings.keyFactor * Time.deltaTime;

      if( shift != Vector3.zero )
      {
        settings.position += quatRot * shift;
        settings.isAttached = false;
      }
    }

    transform.rotation = quatRot;
    transform.position = quatRot * new Vector3(0.0f, 0.0f, -settings.distance) + settings.position;
  }
}