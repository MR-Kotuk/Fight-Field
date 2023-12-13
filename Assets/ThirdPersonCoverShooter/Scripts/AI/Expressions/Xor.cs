using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Boolean")]
    public class Xor : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Boolean, true)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Xor()";
            else if (Values.Length > 2)
                return "Xor(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " xor " + Values[1].GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            var result = false;

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    var b = state.Dereference(ref Values[i]).Bool;

                    if (b && result)
                        return new Value(false);
                    else if (b)
                        result = true;
                }

            return new Value(result);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}