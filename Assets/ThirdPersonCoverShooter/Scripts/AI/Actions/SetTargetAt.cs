using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Target")]
    [Success("Done")]
    [Immediate]
    public class SetTargetAt : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Object;

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(new Vector3(0, 0, 0));

        public SetTargetAt()
        {
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.SetTargetAt(state.Dereference(ref Object).GameObject,
                              state.GetPosition(ref Position));

            return AIResult.Finish();
        }
    }
}