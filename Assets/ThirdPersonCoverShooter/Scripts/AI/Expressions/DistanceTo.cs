using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Distance")]
    public class DistanceTo : BaseExpression
    {
        [NoFieldName]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "DistanceTo(" + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var target = state.GetPosition(ref Target);
            return new Value((target - state.Object.transform.position).magnitude);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}