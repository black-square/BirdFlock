using UnityEngine;

public static class MathTools
{
  public const float epsilon = 1e-10f;
  public const float sqrEpsilon = epsilon * epsilon;

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

  public static float ClampAngle( float angle, float min, float max )
  {
    if (angle < -360)
      angle += 360;
  
    if (angle > 360)
      angle -= 360;
  
    return Mathf.Clamp (angle, min, max);
  }

  //Returns the closiest point to cur position on bounds of cld
  public static Vector3 CalcPointOnBounds( Collider cld, Vector3 cur )
  {
    SphereCollider sphc = cld as SphereCollider;

    if( !sphc )
      return cld.ClosestPointOnBounds( cur );
    else
    {
      //cld.ClosestPointOnBounds returns not precise values for spheres
      //Fortunately they could be calculated easily
      var realPos = sphc.transform.position + sphc.center;
      var dir = cur - realPos;
      var realScale = sphc.transform.lossyScale;
      var realRadius = sphc.radius * Mathf.Max( realScale.x, realScale.y, realScale.z );
      var dirLength = dir.magnitude;

      //BoxCollider.ClosestPointOnBounds returns cur if points are inside the volume
      if( dirLength < realRadius )
        return cur;

      var dirFraction = realRadius / dirLength;
      return realPos + dirFraction * dir;
    }
  }

  public static string ToS( Vector3 vec )
  {
    return System.String.Format("{0:0.00000}:[{1:0.00000}, {2:0.00000}, {3:0.00000}]", vec.magnitude, vec.x, vec.y, vec.z );
  }

  //Projects vectors on plane XZ and calculate angle between them
  public static float AngleXZProjected( Vector3 vec1, Vector3 vec2 )
  {
    vec1.y = 0;
    vec2.y = 0;

    return Vector3.Angle( vec1, vec2 );
  }

  //Projects vectors on plane XZ and turn it right on 90deg
  public static Vector3 RightVectorXZProjected( Vector3 vec )
  {
    vec.y = 0;
    return Quaternion.AngleAxis(90, Vector3.up) * vec;
  }

  //Returns magnitude of vector vec projected on vecNormal
  public static float VecProjectedLength( Vector3 vec, Vector3 vecNormal )
  {
    var proj = Vector3.Project( vec, vecNormal );
    return proj.magnitude * Mathf.Sign( Vector3.Dot(proj, vecNormal) );
  }

  //Check that Quaternion is not NaN
  public static bool IsValid ( Quaternion q )
  {
    #pragma warning disable 1718
    return q == q; //Comparisons to NaN always return false, no matter what the value of the float is.
    #pragma warning restore 1718
  }

  //Check that Vector3 is not NaN
  public static bool IsValid ( Vector3 v )
  {
    #pragma warning disable 1718
    return v == v; //Comparisons to NaN always return false, no matter what the value of the float is.
    #pragma warning restore 1718
  }

  //Map interval of angles between vectors [0..Pi] to interval [0..1]
  //Vectors a and b must be normalized
  public static float AngleToFactor( Vector3 a, Vector3 b )
  {
    //plot((1-cos(x))/2, x = 0..Pi);
    return ( 1 - Vector3.Dot(a, b) ) / 2;
  }

  public static Quaternion RandomYawPitchRotation()
  {
    return Quaternion.Euler( Random.Range(-90, 90), Random.Range(-180, 180), 0 );
  }

  public static Vector3 RandomVectorInBox( float boxSize )
  {
    return new Vector3( Random.Range(-boxSize, boxSize), Random.Range(-boxSize, boxSize), Random.Range(-boxSize, boxSize) );
  }
}
