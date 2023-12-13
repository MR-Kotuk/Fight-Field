using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Array")]
    public class Index : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Value;

        [ValueType(ValueType.Array)]
        public Value Array;

        public override string GetText(Brain brain)
        {
            return "Index(" + Value.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var array = state.Dereference(ref Array);
            var values = array.Array;
            var index = (int)(state.Dereference(ref Value).Float + float.Epsilon);

            if (values == null || index < 0 || index >= values.Length)
            {
                var v = new Value(0f);
                v.Type = array.SubType;

                return v;
            }

            return state.Dereference(ref values[index]);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Unknown;
        }
    }
}