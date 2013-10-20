using System;
using UnityEngine;

public class WayPoint: MonoBehaviour
{
  public int editorPriority = 0;

  public WayPoint( Trace collection )
  {
    this.collection = collection;
  }


  private Trace collection;
}

