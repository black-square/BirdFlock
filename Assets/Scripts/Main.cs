using UnityEngine;

public class Main : MonoBehaviour
{
  public Object prefab;
  public Transform cameraObject;
  static Boid.Settings settings = new Boid.Settings();

  void Start_()
  {
    int count = 10;
    float size = 0.1f;
    int lbrd = -count / 2;
    int rbrd = lbrd + count;


    for( int i = lbrd; i < rbrd; ++i )
      for( int j = lbrd; j < rbrd; ++j )
      {
        var obj = Instantiate( prefab,
          cameraObject.position + new Vector3(
          size * i, size * j,
          Random.Range( -size, size ) ),
          Quaternion.Euler( Random.Range(-90, 90), Random.Range(-180, 180), 0)
        ) as GameObject;

        obj.GetComponent<Boid>().SettingsRef = settings;
      }
  }

  public static Boid.Settings GetSettings( GameObject obj )
  {
    return settings;
  }
 
  // Update is called once per frame
  void Update()
  {
 
  }

  private GuiTools guiTools = new GuiTools();

  void SettingsWindow( int windowId )
  {
    GUILayout.BeginHorizontal();
      GUILayout.BeginVertical();
        guiTools.GuiFloatParam( ref settings.SpeedMultipliyer, "Speed", 20 );
        guiTools.GuiFloatParam( ref settings.ViewRadius, "View distance", 20 );
        guiTools.GuiFloatParam( ref settings.OptDistance, "Optimal distance between birds", 2 );
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
        guiTools.GuiFloatParam( ref settings.AligmentForcePart, "Fraction of flock aligment force", 0.3f );
        guiTools.GuiFloatParam( ref settings.TotalForceMultipliyer, "Reaction speed", 50 );
        guiTools.GuiFloatParam( ref settings.Inertness, "Inertness", 1 );
      GUILayout.EndVertical();
    GUILayout.EndHorizontal();
  }

  delegate void SimpleDlg();

  private bool showSettingsWindow = false;

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
