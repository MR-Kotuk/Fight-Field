using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Comparison")]
    public class Equal : BaseExpression
    {
        [MinimumCount(2)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Equal()";
            else if (Values.Length > 2)
                return "Equal(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " = " + Values[1].GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            var value = new Value();

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    var next = state.Dereference(ref Values[i]);

                    if (i == 0)
                        value = next;
                    else if (!value.IsEqual(ref next))
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