using UnityEngine;

namespace CoverShooter.AI
{
    public class CanThrowAt : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value ExplosionRadius = new Value(2);

        public override string GetText(Brain brain)
        {
            return "CanThrowAt(" + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var target = state.GetPosition(ref Target);
            var explosionRadius = state.Dereference(ref ExplosionRadius).Float;
            var motor = state.Actor.Motor;

            GrenadeDescription desc;
            desc.Gravity = motor.Grenade.Gravity;
            desc.Duration = motor.PotentialGrenade.Timer;
            desc.Bounciness = motor.PotentialGrenade.Bounciness;

            Vector3[] grenadePath = new Vector3[128];

            int pathLength = GrenadePath.Calculate(GrenadePath.Origin(motor, Util.HorizontalAngle(target - motor.transform.position)),
                                                   target,
                                                   motor.Grenade.MaxVelocity,
                                                   desc,
                                                   grenadePath,
                                                   motor.Grenade.Step);

            if (pathLength == 0)
                return new Value(false);

            return new Value(Vector3.Distance(grenadePath[pathLength - 1], target) < explosionRadius);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}