using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Clamping")]
    public class Max : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Float)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Max()";
            else if (Values.Length > 2)
                return "Max(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " + " + Values[1].GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            var result = 0f;

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    var value = state.Dereference(ref Values[i]).Float;

                    if (i == 0)
                        result = value;
                    else if (value > result)
                        result = value;
                }

            return new Value(result);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}