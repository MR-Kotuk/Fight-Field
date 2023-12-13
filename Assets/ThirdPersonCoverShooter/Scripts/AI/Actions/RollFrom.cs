using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Roll")]
    [Success("Done")]
    public class RollFrom : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            var origin = state.Actor.transform.position;
            var direction = (origin - state.GetPosition(ref Position));
            values.Angle = Util.HorizontalAngle(direction);            

            if (AIUtil.IsNavigationBlocked(origin, origin + direction))
            {
                var steps = 6;
                var step = 180f / steps;

                for (int i = 1; i < steps; i++)
                {
                    var a = values.Angle - i * step;
                    var b = values.Angle + i * step;
                    var av = Util.HorizontalVector(a);
                    var bv = Util.HorizontalVector(b);

                    if (!AIUtil.IsNavigationBlocked(origin, origin + av))
                    {
                        values.Angle = a;
                        return;
                    }

                    if (!AIUtil.IsNavigationBlocked(origin, origin + bv))
                    {
                        values.Angle = b;
                        return;
                    }
                }

                values.IsInvalid = true;
            }
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (values.IsInvalid)
                return AIResult.Finish();

            var actor = state.Actor;

            if (!values.HasStarted)
            {
                if (actor.IsRolling)
                    values.HasStarted = true;
                else
                    actor.InputRoll(values.Angle);
            }
            else if (!actor.IsRolling)
                return AIResult.Finish();

            return AIResult.Hold();
        }
    }
}