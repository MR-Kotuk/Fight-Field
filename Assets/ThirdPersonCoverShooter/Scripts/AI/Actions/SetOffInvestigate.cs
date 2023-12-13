using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Set Off")]
    [Success("Done")]
    [Immediate]
    public class SetOffInvestigate : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public SetOffInvestigate()
        {
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var e = new EventDesc();
            e.Type = AIEvent.Investigate;
            e.Value0 = new Value(state.GetPosition(ref Position));

            state.Feed(ref e);

            return AIResult.Finish();
        }
    }
}