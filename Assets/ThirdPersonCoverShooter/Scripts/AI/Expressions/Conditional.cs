using UnityEngine;

namespace CoverShooter.AI
{
    public class Conditional : BaseExpression
    {
        [ValueType(ValueType.Boolean)]
        public Value Condition;

        public Value IfTrue;
        public Value IfFalse;

        public override string GetText(Brain brain)
        {
            return "Conditional(" + Condition.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            return state.Dereference(ref Condition).Bool ? state.Dereference(ref IfTrue) : state.Dereference(ref IfFalse);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return brain.GetValueType(ref IfTrue);
        }
    }
}