using System;
using UnityEngine;

public static class BoidTools
{
  public struct SeparationForce
  {
    public SeparationForce( Boid.Settings sts )
    {
      //We have to compensate cohesion force which in the OptDistance point
      //equals OptDistance / 2
      //solve( {optFactor / OptDistance = OptDistance / 2}, {optFactor} );
      optFactor = sts.OptDistance * sts.OptDistance / 2;
    }

    public bool Calc( Vector3 cur, Vector3 other, out Vector3 force )
    {
      var revDir = cur - other;
      var dist = revDir.magnitude;

      force = Vector3.zero;

      if( dist < MathTools.epsilon ) // Do not take into account oneself
        return false;

      //simplify( revDir / dist * (optFactor / dist) );
      force = revDir * optFactor / ( dist * dist );
      return true;
    }

    public float Calc( float dist )
    {
      return optFactor/ dist;
    }
    
    readonly float optFactor;
  };

  public struct CollisionAvoidanceForce
  {
    public CollisionAvoidanceForce( Boid.Settings sts, float sepForceAtOptDistance, bool useSquareFunction )
    {
      // Maple:
      // restart;
      // f := x-> factor2*(factor1/x^2 - 1);
      // Mult := 2 * SpeedMultipliyer; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
      // F := { f(ViewRadius) = 0, f(OptDistance) = Mult * sepForceAtOptDistance }:
      // Res := solve( F, {factor1, factor2} );
      // RealConsts := {ViewRadius = 0.5, OptDistance = 0.1, sepForceAtOptDistance = 0.05, SpeedMultipliyer = 3};
      // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(ViewRadius, RealConsts) );
      forceDlg = null;

      if( useSquareFunction )
      {
        var ViewRadius2 = sts.ViewRadius * sts.ViewRadius;
        var OptDistance2 = sts.OptDistance * sts.OptDistance;
        factor1 = ViewRadius2;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * OptDistance2 / ( OptDistance2 - ViewRadius2 );
        forceDlg = CalcImplSquared;
      }
      else
      {
        factor1 = sts.ViewRadius;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * sts.OptDistance / ( sts.OptDistance - sts.ViewRadius );
        forceDlg = CalcImplLinear;
      }
    }

    public struct Force
    {
      public Vector3 dir;
      public Vector3 pos;
    };

    public bool Calc( Vector3 cur, Vector3 birdDir, Collider cld, out Force force )
    {
      var pointOnBounds = MathTools.CalcPointOnBounds( cld, cur );
      var revDir = cur - pointOnBounds;
      var dist = revDir.magnitude;

      revDir /= dist;

      //Force depends on direction of bird: no need to turn a bird if it is flying in opposite direction
      force.dir = revDir * ( forceDlg(dist) * MathTools.AngleToFactor(revDir, birdDir) );
      force.pos = pointOnBounds;
      return true;
    }

    float CalcImplLinear( float dist )
    {
      return factor2 * (factor1 / dist - 1);
    }

    float CalcImplSquared( float dist )
    {
      return factor2 * (factor1 / (dist * dist) - 1);
    }

    delegate float ForceDlg(float dist);
    readonly float factor1;
    readonly float factor2;
    readonly ForceDlg forceDlg;
  };
}