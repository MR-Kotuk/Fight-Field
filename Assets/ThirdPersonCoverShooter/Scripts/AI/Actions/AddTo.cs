using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    [Folder("Array")]
    public class AddTo : BaseAction
    {
        [NoFieldName]
        public VariableReference Variable;

        public Value Value;

        public AddTo()
        {
        }

        public AddTo(int variable)
        {
            Variable = new VariableReference(variable);
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (Variable.ID > 0)
            {
                var oldValue = state.Values.ContainsKey(Variable.ID) ? state.Values[Variable.ID] : new Value();
                var array = oldValue.Array;
                var length = oldValue.Count;

                if (array == null || array.Length == length)
                {
                    if (array == null)
                    {
                        length++;
                        array = new Value[length];
                    }
                    else
                    {
                        var old = array;
                        array = new Value[length + 1];

                        for (int i = 0; i < length; i++)
                            array[i] = old[i];

                        length++;
                    }
                }
                else
                {
                    Debug.Assert(array.Length > length);
                    length++;
                }

                array[length - 1] = state.Dereference(ref Value);
                state.Values[Variable.ID] = new Value(array, length, array[length - 1].Type);
            }

            return AIResult.Finish();
        }
    }
}