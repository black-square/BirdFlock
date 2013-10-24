using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
  public CameraControl cameraControl;
  public Object birdPrefab;
  public Transform birdParent;
  private Transform cameraObj;
  public Transform[] instancePoints;

  [System.Serializable]
  public class BoidSettingsEx: Boid.Settings
  {
    public float BirdsCount = 10;
  }

  [System.Serializable]
  public class Settings
  {
    public List<BoidSettingsEx> boidSettings = new List<BoidSettingsEx>();
    public int instancePointNum = 0;
    public bool showSettingsWindow = false;
  }

  [SerializeField]
  private Settings settings = new Settings();

  //static is used to keep values after restart
  static private Settings globalSettings;

  void Start()
  {
    if( globalSettings == null )
    {
      LoadSettings();
      globalSettings = settings;
    }
    else
      settings = globalSettings;

    InstantiateBirds();
    cameraControl.Enabled = true;
  }

  void InstantiateBirds()
  {
    var ip = instancePoints[settings.instancePointNum];
    var sts = settings.boidSettings[settings.instancePointNum];

    sts.Trace = ip.GetComponent<Trace>();

    float size = 0.1f;
    int birdsCount = (int)( sts.BirdsCount + 0.5f );

    int lbrd = -birdsCount / 2;
    int rbrd = lbrd + birdsCount;

    cameraObj = InstantiateBird( ip.position, ip.rotation, sts ).transform;

    cameraControl.Target = cameraObj;

    for( int i = lbrd; i < rbrd; ++i )
      for( int j = lbrd; j < rbrd; ++j )
      {
        InstantiateBird(
          cameraObj.position + new Vector3( size * i, size * j, Random.Range(-size, size) ),
          MathTools.RandomYawPitchRotation(),
          sts
        );
      }
  }

  private readonly XmlSerializer serializer = new XmlSerializer(typeof(Settings));
  private string SettingsFileName { get{ return "Settings"; } }
  private string SettingsFilePath { get{ return Application.dataPath + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + SettingsFileName + ".xml";} }

  void LoadSettings()
  {
    if( Application.isEditor )
    {
      using( var str = new FileStream(SettingsFilePath, FileMode.Open) )
        settings = (Settings)serializer.Deserialize(str);
    }
    else
    {
      TextAsset temp = (TextAsset)Resources.Load(SettingsFileName);
      var str = new StringReader(temp.text);
      Destroy( temp );
      settings = (Settings)serializer.Deserialize(str);
    }

    while( settings.boidSettings.Count < instancePoints.Length )
      settings.boidSettings.Add(new BoidSettingsEx());
  }


  void SaveSettings()
  {
    if( Application.isEditor )
    {
      using( var str = new FileStream( SettingsFilePath, FileMode.Create ) )
        serializer.Serialize(str, settings);
    }
  }

  private GameObject InstantiateBird( Vector3 position, Quaternion rotation, Boid.Settings settings )
  {
    var obj = (GameObject)Instantiate( birdPrefab, position, rotation );
    obj.transform.parent = birdParent;
    obj.GetComponent<Boid>().SettingsRef = settings;
    return obj;
  }

  public static Boid.Settings GetSettings( GameObject obj )
  {
    var main = Camera.main.GetComponent<Main>();
    return main.settings.boidSettings[main.settings.instancePointNum];
  }

  private GuiTools guiTools = new GuiTools();

  void SettingsWindow( int windowId )
  {
    var sts = settings.boidSettings[settings.instancePointNum];

    GUILayout.BeginHorizontal();
      GUILayout.BeginVertical();
        var newInstancePointNum = guiTools.Switcher( settings.instancePointNum, "Instance point", new string[]{ "Box", "WayPoints", "Freedom" } );
        guiTools.FloatParam( ref sts.SpeedMultipliyer, "Speed", 20 );
        guiTools.FloatParam( ref sts.ViewRadius, "View distance", 20 );
        guiTools.FloatParam( ref sts.OptDistance, "Optimal distance between birds", 2 );
        guiTools.FloatParam( ref sts.AligmentForcePart, "Fraction of flock aligment force", 0.01f );
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
        guiTools.FloatParam( ref sts.BirdsCount, "Total number of birds (Restart Required)", 20 );
        guiTools.FloatParam( ref sts.TotalForceMultipliyer, "Reaction speed", 50 );
        guiTools.FloatParam( ref sts.Inertness, "Inertness", 1 );
        guiTools.FloatParam( ref sts.VerticalPriority, "Flock's shape deformation", 3 );
        guiTools.FloatParam( ref sts.AttractrionForce, "Waypoint attraction force", 1.0f );
      GUILayout.EndVertical();
    GUILayout.EndHorizontal();

    if( newInstancePointNum != settings.instancePointNum )
    {
      settings.instancePointNum = newInstancePointNum;
      Restart();
    }
  }

  delegate void SimpleDlg();

  void OnSettingsClick()
  {
    settings.showSettingsWindow = !settings.showSettingsWindow;

    if(!settings.showSettingsWindow)
      SaveSettings();

    cameraControl.Enabled = !settings.showSettingsWindow;
  }

  void Restart()
  {
    guiTools.ClearCache();
    Application.LoadLevel(Application.loadedLevelName);
  }

  void Update()
  {
    if( Input.GetKeyDown("space") )
      OnSettingsClick();

    //We call it here to be sure that click on button desn't lead to camera target changing
    if( !guiTools.MouseOverGUI && Input.GetMouseButtonDown(0) )
      cameraControl.CheckForNewTarget( Input.mousePosition );

    guiTools.ManualUpdate();
  }

  void OnGUI()
  {
    var tlbLabels = new string[] { "Restart", "Settings" };
    var tlbActions = new SimpleDlg[] { Restart, OnSettingsClick };

    var tlbResult = GUILayout.Toolbar( -1, tlbLabels );
    guiTools.CheckMouseOverForLastControl();

    if( tlbResult >= 0 )
      tlbActions[tlbResult]();

    if( settings.showSettingsWindow )
      GUILayout.Window(0, new Rect(10, 30, 2, 2), SettingsWindow, "Settings");
  }
}
