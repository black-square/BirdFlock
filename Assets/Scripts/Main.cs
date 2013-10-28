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
    public float BirdsCount = 100;
  }

  [System.Serializable]
  public struct Size
  {
    public int width;
    public int height;

    public Size( int width, int height )
    {
      this.width = width;
      this.height = height;
    }

    public bool IsValid { get{ return width > 0 && height > 0; } }
  }

  [System.Serializable]
  public class Settings
  {
    public List<BoidSettingsEx> boidSettings = new List<BoidSettingsEx>();
    public int instancePointNum = 0;
    public Boid.DebugSettings debugSettings = new Boid.DebugSettings();

    [System.Xml.Serialization.XmlIgnore]
    public bool showSettingsWindow = false;

    [System.Xml.Serialization.XmlIgnore]
    public int settingsWindowTab = 0;

    [System.Xml.Serialization.XmlIgnore]
    public Size screenSize = new Size();
  }

  [SerializeField]
  private Settings settings = new Settings();

  //static is used to keep values after restart
  static private Settings globalSettings;

  void Start()
  {
    if( globalSettings == null )
    {
      settings = LoadSettings();
      globalSettings = settings;
    }
    else
      settings = globalSettings;

    InstantiateBirds();
    cameraControl.Enabled = !settings.showSettingsWindow;
  }

  void InstantiateBirds()
  {
    var ip = instancePoints[settings.instancePointNum];
    var sts = settings.boidSettings[settings.instancePointNum];

    sts.Trace = ip.GetComponent<Trace>();

    const float size = 0.1f;

    cameraObj = InstantiateBird( ip.position, ip.rotation, sts ).transform;
    cameraControl.Target = cameraObj;

    MathTools.FillSquareUniform( sts.BirdsCount, delegate ( int x, int y )
    {
      if( x != 0 || y != 0 )
        InstantiateBird(
          cameraObj.position + new Vector3( size * x, size * y, Random.Range(-size, size) ),
          MathTools.RandomYawPitchRotation(),
          sts
        );
    });
  }

  private readonly XmlSerializer serializer = new XmlSerializer(typeof(Settings));
  private string SettingsFileName { get{ return "Settings"; } }
  private string SettingsFilePath { get{ return Application.dataPath + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + SettingsFileName + ".xml";} }

  Settings LoadSettings()
  {
    Settings res;
    if( Application.isEditor )
    {
      using( var str = new FileStream(SettingsFilePath, FileMode.Open) )
        res = (Settings)serializer.Deserialize(str);
    }
    else
    {
      TextAsset temp = (TextAsset)Resources.Load(SettingsFileName);
      var str = new StringReader(temp.text);
      Destroy( temp );
      res = (Settings)serializer.Deserialize(str);
    }

    while( res.boidSettings.Count < instancePoints.Length )
      res.boidSettings.Add(new BoidSettingsEx());

    return res;
  }


  void SaveSettings()
  {
    if( Application.isEditor )
    {
      using( var str = new FileStream( SettingsFilePath, FileMode.Create ) )
        serializer.Serialize(str, settings);
    }
  }

  private GameObject InstantiateBird( Vector3 position, Quaternion rotation, Boid.Settings boidSettings )
  {
    var obj = (GameObject)Instantiate( birdPrefab, position, rotation );
    var boid = obj.GetComponent<Boid>();

    obj.transform.parent = birdParent;
    boid.SettingsRef = boidSettings;
    boid.DebugSettingsRef = settings.debugSettings;

    return obj;
  }

  public static Boid.Settings GetSettings( GameObject obj )
  {
    var main = Camera.main.GetComponent<Main>();
    return main.settings.boidSettings[main.settings.instancePointNum];
  }

  private GuiTools guiTools = new GuiTools();

  void GuiBoidSettings()
  {
    var sts = settings.boidSettings[settings.instancePointNum];

    GUILayout.BeginHorizontal();
      GUILayout.BeginVertical();
        var newInstancePointNum = guiTools.Switcher( settings.instancePointNum, "Instance point", new string[]{ "WayPoints", "Box", "Freedom" } );
        guiTools.FloatParam( ref sts.SpeedMultipliyer, "Speed", 20 );
        guiTools.FloatParam( ref sts.ViewRadius, "Bird's view distance", 20 );
        guiTools.FloatParam( ref sts.OptDistance, "Optimal distance between birds", 2 );
        guiTools.FloatParam( ref sts.AligmentForcePart, "Fraction of flock aligment force", 0.01f );
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
        guiTools.FloatParam( ref sts.BirdsCount, "Number of birds (Restart Required)", 1000 );
        guiTools.FloatParam( ref sts.TotalForceMultipliyer, "Reaction speed", 50 );
        guiTools.FloatParam( ref sts.Inertness, "Inertness", 1 );
        guiTools.FloatParam( ref sts.VerticalPriority, "Flock's shape deformation", 3 );
        guiTools.FloatParam( ref sts.AttractrionForce, "Waypoint's attraction force", 1.0f );
      GUILayout.EndVertical();
    GUILayout.EndHorizontal();

    if( GUILayout.Button("Load default parameters") )
    {
      var defSt = LoadSettings();
      settings.boidSettings[settings.instancePointNum] = defSt.boidSettings[settings.instancePointNum];
      Restart();
    }

    if( newInstancePointNum != settings.instancePointNum )
    {
      settings.instancePointNum = newInstancePointNum;
      cameraControl.ResetStoredSettings();
      Restart();
    }
  }

  void GuiDebugDrawSettings()
  {
    GUILayout.BeginVertical("box");
      var newFullScreen = GUILayout.Toggle( Screen.fullScreen, "Fullscreen" );
    GUILayout.EndVertical();

    if( newFullScreen != Screen.fullScreen )
      if( newFullScreen)
      {
        settings.screenSize = new Size( Screen.width, Screen.height );
        Screen.SetResolution( Screen.currentResolution.width, Screen.currentResolution.height, true );
      }
      else if( settings.screenSize.IsValid )
        Screen.SetResolution( settings.screenSize.width, settings.screenSize.height, false );
      else
        Screen.fullScreen = false;

    GUILayout.BeginVertical("box");
      guiTools.Toggle( ref settings.debugSettings.enableDrawing, "Algorithm Explanation Vectors" );
    GUILayout.EndVertical();

    if( settings.debugSettings.enableDrawing )
    {
      GUILayout.BeginVertical("box");
        guiTools.Toggle( ref settings.debugSettings.velocityDraw, "Resulting velocity" );
        guiTools.Toggle( ref settings.debugSettings.cohesionForceDraw, "Cohesion force" );
        guiTools.Toggle( ref settings.debugSettings.collisionsAvoidanceForceDraw, "Collision Avoidance force" );
        guiTools.Toggle( ref settings.debugSettings.positionForceDraw, "Cohesion + Collision Avoidance forces" );
        guiTools.Toggle( ref settings.debugSettings.obstaclesAvoidanceDraw, "Obstacles Avoidance forces" );
        guiTools.Toggle( ref settings.debugSettings.alignmentForceDraw, "Aligment force" );
        guiTools.Toggle( ref settings.debugSettings.attractionForceDraw, "Attraction force" );
        guiTools.Toggle( ref settings.debugSettings.totalForceDraw, "Resulting force" );
      GUILayout.EndVertical();
    }
  }

  void GuiInfo()
  {
    var text =
      "AUTHOR\n" +
      "   \n" +
      "   Dmitry Shesterkin\n" +
      "   http://black-square.github.io/BirdFlock/\n" +
      "   dfb@yandex.ru\n" +
      "   \n" +
      "CONTROLS\n" +
      "   \n" +
      "   Space - Toggle settings\n" +
      "   Mouse - Camera rotation\n" +
      "   Left Mouse Click - Attach camera to target\n" +
      "   Tab - Detach camera from target\n" +
      "   Mouse ScrollWheel / Up / Down - Zoom\n" +
      "   W/A/S/D - Manual camera movement\n" +
      "   Hold Right Mouse Button - Disable camera rotation";

    GUILayout.BeginVertical("box");
      GUILayout.Label( text );
    GUILayout.EndVertical();
  }

  void SettingsWindow( int windowId )
  {
    GUILayout.BeginVertical();
      settings.settingsWindowTab = GUILayout.Toolbar( settings.settingsWindowTab, new string[]{ "Birds Params", "Screen", "Info" } );

      switch(settings.settingsWindowTab)
      {
        case 0:
          GuiBoidSettings();
        break;
        case 1:
          GuiDebugDrawSettings();
        break;
        case 2:
          GuiInfo();
        break;
      }

    GUILayout.EndVertical();
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
