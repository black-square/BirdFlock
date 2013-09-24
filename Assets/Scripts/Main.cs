using UnityEngine;

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

  private GuiTools guiTools = new GuiTools();

  private float val = 12.3f;
  private float val2 = 12.3f;
  void SettingsWindow( int windowId )
  {
    guiTools.GuiFloatParam( ref val, "The long parameter name" );
    guiTools.GuiFloatParam( ref val2, "The other parameter name" );
  }

  delegate void SimpleDlg();

  private bool showSettingsWindow = true;

  void OnGUI()
  {
    var tlbLabels = new string[] { "Restart", "Settings" };
    var tlbActions = new SimpleDlg[] {
      () => Application.LoadLevel (Application.loadedLevelName),
      () => showSettingsWindow = !showSettingsWindow
    };

    var tlbResult = GUILayout.Toolbar( -1, tlbLabels );

    if( tlbResult >= 0 )
      tlbActions[tlbResult]();

    if( showSettingsWindow )
      GUILayout.Window(0, new Rect(10, 30, 2, 2), SettingsWindow, "Test");
  }
}
