using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Arithmetic")]
    public class Add : BaseExpression
    {
        [MinimumCount(2)]
        [DefaultValueType(ValueType.Float)]
        [OnlyArithmeticType]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Add()";
            else if (Values.Length > 2)
                return "Add(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " + " + Values[1].GetText(brain);
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
            var result = Vector3.zero;
            var resultType = ValueType.Float;

            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                {
                    var value = state.Dereference(ref Values[i]);

                    switch (value.Type)
                    {
                        case ValueType.Float:
                            result.x += value.Vector.x;
                            result.y += value.Vector.x;
                            result.z += value.Vector.x;
                            break;

                        case ValueType.Vector2:
                            result.x += value.Vector.x;
                            result.y += value.Vector.y;

                            if (resultType == ValueType.Float)
                                resultType = ValueType.Vector2;

                            break;

                        case ValueType.Vector3:
                            result += value.Vector;

                            if (resultType == ValueType.Float ||
                                resultType == ValueType.Vector2)
                                resultType = ValueType.Vector3;
                            break;
                    }
                }

            switch (resultType)
            {
                case ValueType.Float:
                    return new Value(result.x);

                case ValueType.Vector2:
                    return new Value(new Vector2(result.x, result.y));

                default:
                    return new Value(result);
            }
        }
    }
}