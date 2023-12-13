namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    [Folder("Array")]
    public class RemoveFrom : BaseAction
    {
        [NoFieldName]
        public VariableReference Variable;

        public Value Value;

        public RemoveFrom()
        {
        }

        public RemoveFrom(int variable)
        {
            Variable = new VariableReference(variable);
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (Variable.ID > 0 && state.Values[Variable.ID].Type == ValueType.Array)
            {
                var oldValue = state.Values[Variable.ID];
                var array = oldValue.Array;
                int length = oldValue.Count;

                if (array != null && array.Length > 0)
                {
                    var v = state.Dereference(ref Value);

                    for (int i = length - 1; i >= 0; i--)
                    {
                        if (array[i].Equals(v))
                        {
                            for (int j = i; j < length - 1; j++)
                                array[j] = array[j + 1];

                            length--;
                        }
                    }
                }

                state.Values[Variable.ID] = new Value(array, length, state.Values[Variable.ID].SubType);
            }

            return AIResult.Finish();
        }
    }
}