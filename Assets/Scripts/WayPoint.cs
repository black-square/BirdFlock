using System;
using UnityEngine;

public class WayPoint: MonoBehaviour, Boid.ITrigger
{
  public int editorPriority = 0;
  public Trace trace;

  void OnTriggerEnter(Collider other)
  {
    trace.NextWayPoint();
  }

  public void OnTouch(Boid boid)
  {
    if( collider.isTrigger )
      trace.NextWayPoint();
  }
}

