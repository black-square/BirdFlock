using System;
using UnityEngine;

public static class BoidTools
{
  //Force prevents birds from collapsing into the point. Works with cohesion force.
  //Formula bases on assumption that cohesion force is the difference between bird's
  //position and geometric center of visible birds
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
      var sqrDist = revDir.sqrMagnitude;

      force = Vector3.zero;

      if( sqrDist < MathTools.sqrEpsilon ) // Do not take into account oneself
        return false;

      //simplify( revDir / dist * (optFactor / dist) );
      force = revDir * ( optFactor / sqrDist );
      return true;
    }

    public float Calc( float dist )
    {
      return optFactor / dist;
    }
    
    readonly float optFactor;
  };


  //There was a delegate instead this define, but it was unoptimal because
  //delegates create garbage:
  //http://stackoverflow.com/questions/1582754/does-using-a-delegate-create-garbage
  //#define COLLISION_AVOIDANCE_SQUARE


  //Force between birds and obstacles
  public struct CollisionAvoidanceForce
  {
    public CollisionAvoidanceForce( Boid.Settings sts, float sepForceAtOptDistance )
    {
      //We make an asumption that between an obstacle and a bird on the distance OptDistance should exists same
      //force as between two birds on the same distance

      optDistance = sts.OptDistance;

      // Maple:
      // restart;
      // f := x-> factor2*(factor1/x^2 - 1);
      // Mult := 2 * SpeedMultipliyer; #When collision occurs between birds each bird has a force vector and total force is twise bigger than between wall and bird. That's why we're  multiplying force
      // F := { f(ViewRadius) = 0, f(OptDistance) = Mult * sepForceAtOptDistance }:
      // Res := solve( F, {factor1, factor2} );
      // RealConsts := {ViewRadius = 0.5, OptDistance = 0.1, sepForceAtOptDistance = 0.05, SpeedMultipliyer = 3};
      // plot( eval(f(x), eval(Res, RealConsts) ), x = 0..eval(ViewRadius, RealConsts) );

      #if COLLISION_AVOIDANCE_SQUARE
        var ViewRadius2 = sts.ViewRadius * sts.ViewRadius;
        var OptDistance2 = sts.OptDistance * sts.OptDistance;
        factor1 = ViewRadius2;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * OptDistance2 / ( OptDistance2 - ViewRadius2 );
      #else
        factor1 = sts.ViewRadius;
        factor2 = -2 * sts.SpeedMultipliyer * sepForceAtOptDistance * sts.OptDistance / ( sts.OptDistance - sts.ViewRadius );
      #endif
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

      if( dist <= MathTools.epsilon )
      {
        //Let's setup the direction to outside of colider
        revDir = (pointOnBounds - cld.transform.position).normalized;

        //and distance to N percent of OptDistance
        dist = 0.1f * optDistance;
      }
      else
        revDir /= dist;

      //Force depends on direction of bird: no need to turn a bird if it is flying in opposite direction
      force.dir = revDir * ( CalcImpl(dist) * MathTools.AngleToFactor(revDir, birdDir) );
      force.pos = pointOnBounds;
      return true;
    }

    #if COLLISION_AVOIDANCE_SQUARE
      float CalcImpl( float dist )
      {
        return factor2 * (factor1 / (dist * dist) - 1);
      }
    #else
      float CalcImpl( float dist )
      {
        return factor2 * (factor1 / dist - 1);
      }
    #endif

    delegate float ForceDlg(float dist);
    readonly float factor1;
    readonly float factor2;
    readonly float optDistance;
  };
}