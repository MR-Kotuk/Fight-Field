using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Random")]
    public class Chance : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Fraction = new Value(0.5f);

        public override string GetText(Brain brain)
        {
            return "Chance(" + Fraction.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var fraction = state.Dereference(ref Fraction).Float;

            return new Value(UnityEngine.Random.Range(0f, 1f) + float.Epsilon <= fraction);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}