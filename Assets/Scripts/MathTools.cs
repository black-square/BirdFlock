using UnityEngine;

public static class MathTools
{
  public const float epsilon = 1e-10f;

  public static Vector3 StretchAlongAxis( Vector3 point, Vector3 stretchAxis, float stretchFactor )
  {
    var upVector = Vector3.up;

    //Check if vectors are colliniar
    if( Vector3.Dot(upVector, stretchAxis) >= 1.0f - epsilon )
      upVector = Vector3.left;

    var right = Vector3.Cross(upVector, stretchAxis);
    var up = Vector3.Cross(stretchAxis, right);
    var forward = stretchAxis;

    right.Normalize();
    up.Normalize();
    forward.Normalize();

    Matrix4x4 rotate = new Matrix4x4();
    rotate.SetColumn(0, right);
    rotate.SetColumn(1, up);
    rotate.SetColumn(2, forward);
    rotate[3, 3] = 1.0F;

    Matrix4x4 scale = new Matrix4x4();
    scale[0, 0] = 1.0f;
    scale[1, 1] = 1.0f;
    scale[2, 2] = stretchFactor;
    scale[3, 3] = 1.0f;

    Matrix4x4 trans = rotate * scale * rotate.transpose;

    return trans.MultiplyPoint3x4( point );
  }

  public delegate Vector3 TransformPointDlg( Vector3 point );

  public static void DrawTestCube( Vector3 center, TransformPointDlg transform )
  {
    var count = 15;
    var dist = 0.1f;

    for( int x = 0; x < count; ++x )
      for( int y = 0; y < count; ++y )
        for( int z = 0; z < count; ++z )
        {
          if( x == 0 || x == count - 1 || y == 0 || y == count - 1|| z == 0 || z == count - 1 )
          {
            var pos = new Vector3( dist * (x - count / 2), dist * (y - count / 2), dist * (z - count / 2) );
            pos = transform( pos );
            Debug.DrawLine( center + pos * 0.9f, center + pos, Color.red );
          }
        }
  }

  public static float ClampAngle ( float angle, float min, float max )
  {
    if (angle < -360)
      angle += 360;
  
    if (angle > 360)
      angle -= 360;
  
    return Mathf.Clamp (angle, min, max);
  }

}
