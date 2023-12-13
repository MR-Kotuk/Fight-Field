using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Array")]
    public class IsNotEmpty : BaseExpression
    {
        [MinimumCount(1)]
        public Value[] Values;

        public override Value Evaluate(int id, State state)
        {
            var count = 0f;

            addUnknownArray(ref count, Values, Values == null ? 0 : Values.Length, state);

            return new Value(count > 0);
        }

        private void addUnknownArray(ref float count, Value[] values, int valueCount, State state)
        {
            if (values == null)
                return;

            for (int i = 0; i < valueCount; i++)
            {
                var value = state.Dereference(ref values[i]);

                if (value.Type == ValueType.Array)
                    addUnknownArray(ref count, value.Array, value.Count, state);
                else if (value.Type != ValueType.Unknown)
                    count++;
            }
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}