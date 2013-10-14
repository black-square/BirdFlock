using UnityEngine;

public class Main : MonoBehaviour
{
  public Object birdPrefab;
  private Transform cameraObj;
  public Transform[] instancePoints;
  static Boid.Settings settings = new Boid.Settings();

  public int auxBirdsCount = 10;
  public int instancePointNum = 0;

  void Start()
  {
    float size = 0.1f;
    int lbrd = -auxBirdsCount / 2;
    int rbrd = lbrd + auxBirdsCount;

    cameraObj = InstantiateBird(
      instancePoints[instancePointNum].position,
      instancePoints[instancePointNum].rotation,
      settings
    ).transform;

    GetComponent<CameraControl>().target = cameraObj;

    for( int i = lbrd; i < rbrd; ++i )
      for( int j = lbrd; j < rbrd; ++j )
      {
        InstantiateBird(
          cameraObj.position + new Vector3( size * i, size * j, Random.Range( -size, size ) ),
          Quaternion.Euler( Random.Range(-90, 90), Random.Range(-180, 180), 0),
          settings
        );
      }
  }

  private GameObject InstantiateBird( Vector3 position, Quaternion rotation, Boid.Settings settings )
  {
    var obj = (GameObject)Instantiate( birdPrefab, position, rotation );
    obj.GetComponent<Boid>().SettingsRef = settings;
    return obj;
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
        guiTools.GuiFloatParam( ref settings.AligmentForcePart, "Fraction of flock aligment force", 0.2f );
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

    GUI.Label(new Rect(200, 0, 300, 50), Boid.ToS(cameraObj.GetComponent<Boid>().Velocity) );
  }
}
