using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Boolean")]
    public class Not : BaseExpression
    {
        [ValueType(ValueType.Boolean)]
        [NoFieldName]
        public Value Value;

        public override string GetText(Brain brain)
        {
            return "Not " + Value.GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            return new Value(!state.Dereference(ref Value).Bool);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}