using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Clamping")]
    public class Clamp : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Value;

        [ValueType(ValueType.Float)]
        public Value Min = new Value(0f);

        [ValueType(ValueType.Float)]
        public Value Max = new Value(1f);

        public override string GetText(Brain brain)
        {
            return "Clamp( " + Value.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var value = state.Dereference(ref Value).Float;
            var min = state.Dereference(ref Min).Float;
            var max = state.Dereference(ref Max).Float;

            if (value <= min)
                return new Value(min);
            if (value >= max)
                return new Value(max);
            else
                return new Value(value);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}