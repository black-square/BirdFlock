using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
  public Object prefab;
  public Transform cameraObject;

  void Start()
  {
    int count = 10;
    float size = 0.1f;
    int lbrd = -count / 2;
    int rbrd = lbrd + count;


    for( int i = lbrd; i < rbrd; ++i )
      for( int j = lbrd; j < rbrd; ++j )
        Instantiate( prefab, cameraObject.position + new Vector3(
          size * i, size * j,
          Random.Range( -size, size ) ),
          Quaternion.Euler( Random.Range(-90, 90), Random.Range(-180, 180), 0) );
  }
 
  // Update is called once per frame
  void Update()
  {
 
  }

  void OnGUI()
  {
    if( GUI.Button(new Rect(0,0,80,20), "Restart") )
      Application.LoadLevel (Application.loadedLevelName);
  }
}
