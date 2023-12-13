using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Boolean")]
    public class And : BaseExpression
    {
        [MinimumCount(2)]
        [ValueType(ValueType.Boolean)]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "And()";
            else if (Values.Length > 2)
                return "And(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " and " + Values[1].GetText(brain);
        }
        public override Value Evaluate(int id, State state)
        {
            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    if (!state.Dereference(ref Values[i]).Bool)
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