using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Comparison")]
    public class GreaterEqual : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Float, true)]
        public Value[] Values;

        public override Value Evaluate(int id, State state)
        {
            float value = 0;

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    var next = state.Dereference(ref Values[i]);

                    if (next.Type != ValueType.Float)
                        return new Value(false);

                    if (i == 0)
                        value = next.Float;
                    else if (value >= next.Float)
                        continue;
                    else
                        return new Value(false);
                }

            return new Value(Values != null && Values.Length > 0);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}