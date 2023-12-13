using UnityEngine;

namespace CoverShooter.AI
{
    public class SaveValue : BaseExpression
    {
        [NoFieldName]
        public VariableReference Variable;

        [NoConstant]
        [NoFieldName]
        public Value Value;

        public SaveValue()
        {
        }

        public SaveValue(Brain brain, int variable)
        {
            Variable = new VariableReference(variable);

            var v = brain.GetVariable(variable);

            if (v != null)
                Value = v.Value;
        }

        public override string GetText(Brain brain)
        {
            return "SaveValue(" + Value.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var value = state.Dereference(ref Value);

            if (Variable.ID > 0 && value.Type != ValueType.Unknown)
                state.Values[Variable.ID] = value;

            return value;
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return brain.GetValueType(ref Value);
        }
    }
}