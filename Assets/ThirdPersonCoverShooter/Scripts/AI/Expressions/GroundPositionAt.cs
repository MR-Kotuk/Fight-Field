using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Position")]
    public class GroundPositionAt : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        [NoFieldName]
        public Value Position = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "GroundPositionAt(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var p = state.GetPosition(ref Position);

            AIUtil.GetClosestStandablePosition(ref p);

            return new Value(p);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}