using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Position")]
    public class GetPositions : BaseExpression
    {
        [MinimumCount(1)]
        [ValueType(ValueType.GameObject, true)]
        public Value[] Objects;

        public override Value Evaluate(int id, State state)
        {
            var array = state.GetArray(id, 2);
            var count = 0;

            addUnknownArray(ref array, ref count, Objects, Objects == null ? 0 : Objects.Length, state);

            state.Arrays[id] = array;

            return new Value(array, count, ValueType.Vector3);
        }

        private void addUnknownArray(ref Value[] array, ref int count, Value[] values, int valueCount, State state)
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
                                addUnknownArray(ref array, ref count, value.Array, value.Count, state);
                                break;

                            case ValueType.GameObject:
                                for (int j = 0; j < value.Count; j++)
                                    if (value.Array[j].GameObject != null)
                                        Value.Add(ref array, ref count, new Value(value.Array[j].GameObject.transform.position));
                                break;
                        } break;

                    case ValueType.GameObject:
                        if (value.GameObject != null)
                            Value.Add(ref array, ref count, new Value(value.GameObject.transform.position));
                        break;
                }
            }
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Array;
        }
    }
}