using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Object")]
    public class IsNull : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        [NoFieldName]
        public Value Object;

        public override string GetText(Brain brain)
        {
            return "IsNull(" + Object.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            return new Value(state.Dereference(ref Object).GameObject == null);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}