using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Raycasting")]
    public class IsRaycastNotObstructed : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Origin = new Value(Vector3.zero);

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "IsRaycastNotObstructed(" + Origin.GetText(brain) + ", " + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var origin = state.GetPosition(ref Origin);
            var target = state.GetPosition(ref Target);

            return new Value(!AIUtil.IsObstructed(origin, target));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}