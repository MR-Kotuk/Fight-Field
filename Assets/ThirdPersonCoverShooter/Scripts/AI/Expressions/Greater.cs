using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Comparison")]
    public class Greater : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Float, true)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Greater()";
            else if (Values.Length > 2)
                return "Greater(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " > " + Values[1].GetText(brain);
        }

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
                    else if (value > next.Float)
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