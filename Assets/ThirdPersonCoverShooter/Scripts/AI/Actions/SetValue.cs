namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    public class SetValue : BaseAction
    {
        [NoFieldName]
        public VariableReference Variable;

        public Value Value;

        public SetValue()
        {
        }

        public SetValue(int variable)
        {
            Variable = new VariableReference(variable);
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (Variable.ID > 0)
                state.Values[Variable.ID] = state.Dereference(ref Value);

            return AIResult.Finish();
        }
    }
}