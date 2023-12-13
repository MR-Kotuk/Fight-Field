using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Roll")]
    [Success("Done")]
    public class Roll : BaseAction
    {
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        public Value Direction = new Value(CoverShooter.Direction.Forward);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            var direction = state.GetDirection(ref Direction);
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