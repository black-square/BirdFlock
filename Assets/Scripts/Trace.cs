using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Trace: MonoBehaviour
{
  public WayPoint[] wayPoints;
  private WayPoint curWP = null;

  public Trace()
  {
  }

  public void Start()
  {
    if( wayPoints.Length > 0 )
      curWP = wayPoints[0];
  }

  static Trace FindDefault()
  {
    return (Trace)UnityEngine.Object.FindObjectOfType( typeof(Trace) );
  }

  [MenuItem ("GameEditor/Collect route from priority of waypoints")]
  static void DoSomething ()
  {
    var list = new List<WayPoint>();
    var rgx = new Regex(@"\d+$");

    //Selection.gameObjects doesn't hold selection order
    foreach( var obj in FindObjectsOfType(typeof(WayPoint)) )
    {
      var wp = (WayPoint)obj;

      list.Add(wp);
      wp.name = rgx.Split(obj.name)[0] + wp.editorPriority.ToString("D2");
    }

    list.Sort( (t1, t2) => t1.editorPriority - t2.editorPriority );
    //ths.wayPoints = list.Select( (v) => v.gameObject ).ToArray();
    FindDefault().wayPoints = list.ToArray();
  }

  public Vector3 GetAtractionPoint( Boid.Settings sts )
  {
    return curWP.transform.position;
  }
}


