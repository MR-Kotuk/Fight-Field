using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Property")]
    [Success("Done")]
    [Immediate]
    public class SetViewDistance : BaseAction
    {
        [ValueType(ValueType.Float)]
        public Value Value = new Value(90f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.ViewDistance = state.Dereference(ref Value).Float;

            return AIResult.Finish();
        }
    }
}