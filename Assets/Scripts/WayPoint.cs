using System;
using UnityEngine;

public class WayPoint: MonoBehaviour
{
  public int editorPriority = 0;
  public Trace trace;

  void OnTriggerEnter(Collider other)
  {
    trace.NextWayPoint();
  }
}

