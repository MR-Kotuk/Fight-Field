using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Raycasting")]
    public class Raycast : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Origin = new Value(Vector3.zero);

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(0.5f);

        public override Value Evaluate(int id, State state)
        {
            return new Value(Util.GetClosestStaticHit(state.GetPosition(ref Origin), state.GetPosition(ref Target), state.Dereference(ref MinDistance).Float, state.Object));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}