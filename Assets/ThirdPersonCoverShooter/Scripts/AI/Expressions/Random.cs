using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Random")]
    public class Random : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Min = new Value(0f);

        [ValueType(ValueType.Float)]
        public Value Max = new Value(1f);

        public override string GetText(Brain brain)
        {
            return "Random(" + Min.GetText(brain) + ", " + Max.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var min = state.Dereference(ref Min).Float;
            var max = state.Dereference(ref Max).Float;

            return new Value(UnityEngine.Random.Range(min, max));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}