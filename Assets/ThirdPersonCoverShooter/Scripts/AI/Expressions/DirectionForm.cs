using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Direction")]
    public class DirectionFrom : BaseExpression
    {
        [NoFieldName]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "DirectionFrom(" + Target.GetText(brain) + ")";
        }
        public override Value Evaluate(int id, State state)
        {
            var target = state.GetPosition(ref Target);
            return new Value((state.Object.transform.position - target).normalized);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}