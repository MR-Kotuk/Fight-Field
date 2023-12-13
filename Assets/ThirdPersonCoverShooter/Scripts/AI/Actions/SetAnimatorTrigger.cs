using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    [Folder("Animator")]
    public class SetAnimatorTrigger : BaseAction
    {
        [ValueType(ValueType.Text)]
        public Value Name = new Value("");

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var animator = state.Animator;

            if (animator == null)
                return AIResult.Finish();

            var name = state.Dereference(ref Name).Text;
            animator.SetTrigger(name);

            return AIResult.Finish();
        }
    }
}