using UnityEngine;

namespace CoverShooter.AI
{
    [Success("True")]
    [Failure("False")]
    [Immediate]
    public class If : BaseAction
    {
        [ValueType(ValueType.Boolean)]
        [NoFieldName]
        public Value Value = new Value(true);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (state.Dereference(ref Value).Bool)
                return AIResult.SuccessOrHold();
            else
                return AIResult.FailureOrHold();
        }
    }
}