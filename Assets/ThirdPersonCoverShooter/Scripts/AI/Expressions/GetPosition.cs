using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Position")]
    public class GetPosition : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        [NoFieldName]
        public Value Object;

        public override string GetText(Brain brain)
        {
            return "GetPosition(" + Object.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var o = state.Dereference(ref Object).GameObject;

            return new Value(o == null ? Vector3.zero : o.transform.position);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}