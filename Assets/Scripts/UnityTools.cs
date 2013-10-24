using UnityEngine;
using System.Collections.Generic;


public static class UnityTools
{
  public static I GetInterface<I>(this Component cmp) where I : class
  {
     return cmp.GetComponent(typeof(I)) as I;
  }
}


