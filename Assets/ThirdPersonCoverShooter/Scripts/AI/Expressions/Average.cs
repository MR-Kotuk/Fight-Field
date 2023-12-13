using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Array")]
    public class Average : BaseExpression
    {
        [MinimumCount(1)]
        [DefaultValueType(ValueType.Float)]
        [OnlyArithmeticType]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Average()";
            else if (Values.Length > 2)
                return "Average(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return "Average(" + Values[0].GetText(brain) + ", " + Values[1].GetText(brain) + ")";
        }

        public override ValueType GetReturnType(Brain brain)
        {
            var resultType = ValueType.Float;

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    switch (brain.GetValueType(ref Values[i]))
                    {
                        case ValueType.Vector2:
                            if (resultType == ValueType.Float)
                                resultType = ValueType.Vector2;

                            break;

                        case ValueType.Vector3:
                            if (resultType == ValueType.Float ||
                                resultType == ValueType.Vector2)
                                resultType = ValueType.Vector3;
                            break;
                    }
                }

            return resultType;
        }

        public override Value Evaluate(int id, State state)
        {
            var resultType = ValueType.Float;
            var sum = Vector3.zero;
            var count = 0;

            addUnknownArray(ref sum, ref count, ref resultType, Values, Values == null ? 0 : Values.Length, state);

            switch (resultType)
            {
                case ValueType.Vector3: return new Value(sum / count);
                case ValueType.Vector2: return new Value(new Vector2(sum.x, sum.y) / count);
                default: return new Value(sum.x / count);
            }            
        }

        private void addUnknownArray(ref Vector3 sum, ref int count, ref ValueType resultType, Value[] values, int valueCount, State state)
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
                                addUnknownArray(ref sum, ref count, ref resultType, value.Array, value.Count, state);
                                break;

                            case ValueType.Float:
                                for (int j = 0; j < value.Count; j++)
                                {
                                    var v = value.Array[j].Vector;
                                    sum.x += v.x;
                                    sum.y += v.x;
                                    sum.z += v.x;
                                }

                                count += value.Count;
                                break;

                            case ValueType.Vector2:
                                for (int j = 0; j < value.Count; j++)
                                {
                                    var v = value.Array[j].Vector;
                                    sum.x += v.x;
                                    sum.y += v.y;
                                }

                                if (resultType == ValueType.Float)
                                    resultType = ValueType.Vector2;

                                count += value.Count;
                                break;

                            case ValueType.Vector3:
                                for (int j = 0; j < value.Count; j++)
                                    sum += value.Array[j].Vector;

                                if (resultType != ValueType.Vector3)
                                    resultType = ValueType.Vector3;

                                count += value.Count;
                                break;
                        } break;

                    case ValueType.Float:
                        sum.x += value.Vector.x;
                        sum.y += value.Vector.x;
                        sum.z += value.Vector.x;
                        count++;
                        break;

                    case ValueType.Vector2:
                        sum.x += value.Vector.x;
                        sum.y += value.Vector.y;

                        if (resultType == ValueType.Float)
                            resultType = ValueType.Vector2;

                        count++;
                        break;

                    case ValueType.Vector3:
                        sum += value.Vector;

                        if (resultType != ValueType.Vector3)
                            resultType = ValueType.Vector3;

                        count++;
                        break;
                }
            }
        }
    }
}