using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Clamping")]
    public class WrapOver : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Value;

        [ValueType(ValueType.Float)]
        [ValueType(ValueType.Array)]
        public Value Count;

        public override string GetText(Brain brain)
        {
            return "WrapOver( " + Count.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var value = state.Dereference(ref Value).Float;
            var array = state.Dereference(ref Count);

            float max;

            if (array.Type == ValueType.Array)
                max = array.Array.Length;
            else
                max = array.Float;

            if (value >= max)
                return new Value(0f);
            else
                return new Value(value);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}