using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
  public Object birdPrefab;
  private Transform cameraObj;
  public Transform[] instancePoints;
  private List<Boid.Settings> settings = new List<Boid.Settings>();

  public int auxBirdsCount = 10;
  public int instancePointNum = 0;

  void Start()
  {
    LoadSettings();

    var ip = instancePoints[instancePointNum];
    var sts = settings[instancePointNum];

    sts.Trace = ip.GetComponent<Trace>();

    float size = 0.1f;
    int lbrd = -auxBirdsCount / 2;
    int rbrd = lbrd + auxBirdsCount;

    cameraObj = InstantiateBird( ip.position, ip.rotation, sts ).transform;

    GetComponent<CameraControl>().target = cameraObj;

    for( int i = lbrd; i < rbrd; ++i )
      for( int j = lbrd; j < rbrd; ++j )
      {
        InstantiateBird(
          cameraObj.position + new Vector3( size * i, size * j, Random.Range( -size, size ) ),
          Quaternion.Euler( Random.Range(-90, 90), Random.Range(-180, 180), 0),
          sts
        );
      }
  }

  void LoadSettings()
  {
    var xml = PlayerPrefs.GetString( "Settings" );

    if( !string.IsNullOrEmpty(xml) )
    {
      var str = new StringReader(xml);
      var ser = new XmlSerializer(typeof(List<Boid.Settings>));
      settings = (List<Boid.Settings>)ser.Deserialize(str);
    }

    while( settings.Count < instancePoints.Length )
      settings.Add(new Boid.Settings());
  }


  void SaveSettings()
  {
    var str = new StringWriter();
    var ser = new XmlSerializer(typeof(List<Boid.Settings>));

    ser.Serialize(str, settings);
    PlayerPrefs.SetString( "Settings", str.ToString() );
  }

  private GameObject InstantiateBird( Vector3 position, Quaternion rotation, Boid.Settings settings )
  {
    var obj = (GameObject)Instantiate( birdPrefab, position, rotation );
    obj.GetComponent<Boid>().SettingsRef = settings;
    return obj;
  }

  public static Boid.Settings GetSettings( GameObject obj )
  {
    var main = Camera.main.GetComponent<Main>();
    return main.settings[main.instancePointNum];
  }
 
  // Update is called once per frame
  void Update()
  {
 
  }

  private GuiTools guiTools = new GuiTools();

  void SettingsWindow( int windowId )
  {
    var sts = settings[instancePointNum];

    GUILayout.BeginHorizontal();
      GUILayout.BeginVertical();
        guiTools.GuiFloatParam( ref sts.SpeedMultipliyer, "Speed", 20 );
        guiTools.GuiFloatParam( ref sts.ViewRadius, "View distance", 20 );
        guiTools.GuiFloatParam( ref sts.OptDistance, "Optimal distance between birds", 2 );
        guiTools.GuiFloatParam( ref sts.AligmentForcePart, "Fraction of flock aligment force", 0.01f );
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
        guiTools.GuiFloatParam( ref sts.TotalForceMultipliyer, "Reaction speed", 50 );
        guiTools.GuiFloatParam( ref sts.Inertness, "Inertness", 1 );
        guiTools.GuiFloatParam( ref sts.AttractrionForce, "Waypoint attraction force", 0.1f );
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
      () => {showSettingsWindow = !showSettingsWindow; if(!showSettingsWindow) SaveSettings(); }
    };

    var tlbResult = GUILayout.Toolbar( -1, tlbLabels );

    if( tlbResult >= 0 )
      tlbActions[tlbResult]();

    if( showSettingsWindow )
      GUILayout.Window(0, new Rect(10, 30, 2, 2), SettingsWindow, "Settings");

    //GUI.Label(new Rect(200, 0, 300, 50), MathTools.ToS(cameraObj.GetComponent<Boid>().Velocity) );
  }
}
