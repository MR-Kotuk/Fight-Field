using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Array")]
    public class IndexOf : BaseExpression
    {
        public Value Value;

        [ValueType(ValueType.Array)]
        public Value Array;

        public override string GetText(Brain brain)
        {
            return "IndexOf(" + Value.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var value = state.Dereference(ref Value);
            var values = state.Dereference(ref Array).Array;

            if (values != null && values.Length > 0)
                for (int i = 0; i < values.Length; i++)
                    if (values[i].IsEqual(ref value))
                        return new Value((float)i);

            return new Value(-1f);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}