using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Target")]
    [Success("Done")]
    [Immediate]
    public class SetTargetPosition : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(new Vector3(0, 0, 0));

        public SetTargetPosition()
        {
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.TargetPosition = state.GetPosition(ref Position);

            return AIResult.Finish();
        }
    }
}