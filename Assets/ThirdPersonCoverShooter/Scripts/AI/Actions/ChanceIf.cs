using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Yes")]
    [Failure("No")]
    [Immediate]
    public class ChanceIf : BaseAction
    {
        [ValueType(ValueType.Float)]
        [NoFieldName]
        public Value Fraction = new Value(0.5f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (UnityEngine.Random.Range(0f, 1f) + float.Epsilon <= state.Dereference(ref Fraction).Float)
                return AIResult.SuccessOrHold();
            else
                return AIResult.FailureOrHold();
        }
    }
}