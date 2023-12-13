using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Array")]
    public class AveragePosition : BaseExpression
    {
        [MinimumCount(1)]
        [ValueType(ValueType.GameObject, true)]
        public Value[] Objects;

        public override Value Evaluate(int id, State state)
        {
            var sum = Vector3.zero;
            var count = 0;

            addUnknownArray(ref sum, ref count, Objects, Objects == null ? 0 : Objects.Length, state);

            return new Value(sum / count);
        }

        private void addUnknownArray(ref Vector3 sum, ref int count, Value[] values, int valueCount, State state)
        {
            if (values == null)
                return;

            for (int i = 0; i < valueCount; i++)
            {
                var value = state.Dereference(ref values[i]);

                switch (value.Type)
                {
                    case ValueType.Array:
                        switch (value.SubType)
                        {
                            case ValueType.Unknown:
                                addUnknownArray(ref sum, ref count, value.Array, value.Count, state);
                                break;

                            case ValueType.GameObject:
                                for (int j = 0; j < value.Count; j++)
                                    if (value.Array[j].GameObject != null)
                                    {
                                        sum += value.Array[j].GameObject.transform.position;
                                        count++;
                                    }
                                break;
                        } break;

                    case ValueType.GameObject:
                        if (value.GameObject != null)
                        {
                            sum += value.GameObject.transform.position;
                            count++;
                        }
                        break;
                }
            }
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}