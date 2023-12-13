using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Direction")]
    public class DirectionTo : BaseExpression
    {
        [NoFieldName]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "DirectionTo(" + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var target = state.GetPosition(ref Target);
            return new Value((target - state.Object.transform.position).normalized);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}