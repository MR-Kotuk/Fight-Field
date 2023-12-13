using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Boolean")]
    public class Or : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Boolean, true)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Or()";
            else if (Values.Length > 2)
                return "Or(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " or " + Values[1].GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                    if (state.Dereference(ref Values[i]).Bool)
                        return new Value(true);

            return new Value(false);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}