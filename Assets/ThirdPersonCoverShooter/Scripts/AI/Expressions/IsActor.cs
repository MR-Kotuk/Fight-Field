using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Object")]
    public class IsActor : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        [NoFieldName]
        public Value Object;

        public override string GetText(Brain brain)
        {
            return "IsActor(" + Object.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            return new Value(Actors.Get(state.Dereference(ref Object).GameObject) != null);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}