using System;
using System.Collections.Generic;
using UnityEngine;

class GuiTools
{
  static bool IsValidFloat( string val )
  {
    float res;
    return float.TryParse(val, out res);
  }

  public void FloatParam( ref float value, string caption, float maxValue )
  {
    float oldValue = value;
    string text;

    if( !guiStringParamAuxData.TryGetValue(caption, out text) )
      text = value.ToString();

    if( normalTextField == null )
    {
      normalTextField = new GUIStyle("TextField");
      alertTextField = new GUIStyle(normalTextField);
      alertTextField.normal.textColor = Color.red;
      alertTextField.hover.textColor = Color.red;
      alertTextField.focused.textColor = Color.red;
    }

    GUIStyle textStyle = IsValidFloat(text) ? normalTextField: alertTextField;

    GUILayout.BeginVertical("box");
      GUILayout.Label(caption);
      GUILayout.BeginHorizontal();
        GUILayout.BeginVertical("box");
          value = GUILayout.HorizontalSlider(value, 0, maxValue, GUILayout.MinWidth(150) );
        GUILayout.EndVertical();
        text = GUILayout.TextField( text, textStyle, GUILayout.MinWidth(70) );
      GUILayout.EndHorizontal();
    GUILayout.EndVertical();

    if( value != oldValue )
      text = value.ToString();

    float res;

    if( float.TryParse(text, out res) )
      value = res;

    guiStringParamAuxData[caption] = text;
  }

  public int Switcher( int curValue, string caption, string[] texts )
  {
    GUILayout.BeginVertical("box");
      GUILayout.Label(caption);
      var newValue = GUILayout.Toolbar( curValue, texts );
    GUILayout.EndVertical();

   return newValue;
  }

  public void ClearCache()
  {
    guiStringParamAuxData.Clear();
  }

  public bool MouseOverGUI { get{ return isMouseOverGUI; } }

  public void ManualUpdate()
  {
    isMouseOverGUI = false;
  }

  public void CheckMouseOverForLastControl()
  {
    //See example:
    //http://docs.unity3d.com/Documentation/ScriptReference/GUILayoutUtility.GetLastRect.html
    if( Event.current.type != EventType.Repaint )
      return;

    if( GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) )
      isMouseOverGUI = true;
  }

  private Dictionary<string, string> guiStringParamAuxData = new Dictionary<string, string>();
  private GUIStyle normalTextField = null;
  private GUIStyle alertTextField = null;
  private bool isMouseOverGUI = false;
};
