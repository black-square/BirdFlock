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

  public void GuiFloatParam( ref float value, string caption, float maxValue )
  {
    float oldValue = value;
    string text;

    if( !guiFloatParamAuxData.TryGetValue(caption, out text) )
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
        GUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
          GUILayout.FlexibleSpace();
          value = GUILayout.HorizontalSlider(value, 0, maxValue, GUILayout.MinWidth(150) );
          GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        text = GUILayout.TextField( text, textStyle, GUILayout.MinWidth(70) );
      GUILayout.EndHorizontal();
    GUILayout.EndVertical();

    if( value != oldValue )
      text = value.ToString();

    float res;

    if( float.TryParse(text, out res) )
      value = res;

    guiFloatParamAuxData[caption] = text;
  }


  private Dictionary<string, string> guiFloatParamAuxData = new Dictionary<string, string>();
  private GUIStyle normalTextField = null;
  private GUIStyle alertTextField = null;
};
