using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Arithmetic")]
    public class Divide : BaseExpression
    {
        [MinimumCount(2)]
        [DefaultValueType(ValueType.Float)]
        [OnlyArithmeticType]
        public Value[] Values;

        public override string GetText(Brain brain)
        {
            if (Values == null || Values.Length == 0)
                return "Divide()";
            else if (Values.Length > 2)
                return "Divide(...)";
            else if (Values.Length == 1)
                return Values[0].GetText(brain);
            else
                return Values[0].GetText(brain) + " / " + Values[1].GetText(brain);
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
            if (Values == null || Values.Length == 0)
                return new Value(0f);
            else
            {
                var value = state.Dereference(ref Values[0]);

                for (int i = 1; i < Values.Length; i++)
                    value.Vector /= state.Dereference(ref Values[i]).Float;

                return value;
            }
        }
    }
}