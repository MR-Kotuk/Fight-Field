using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Roll")]
    [Success("Done")]
    public class RollTowards : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            var direction = (state.GetPosition(ref Position) - state.Object.transform.position);
            values.Angle = Util.HorizontalAngle(direction);
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
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