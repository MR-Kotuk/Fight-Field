using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    [Folder("Animator")]
    public class SetAnimatorValue : BaseAction
    {
        [ValueType(ValueType.Text)]
        public Value Name = new Value("");

        public Value Value = new Value(0f);


        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var animator = state.Animator;

            if (animator == null)
                return AIResult.Finish();

            var name = state.Dereference(ref Name).Text;
            var value = state.Dereference(ref Value);

            switch (value.Type)
            {
                case ValueType.Float: animator.SetFloat(name, value.Float); break;
                case ValueType.Boolean: animator.SetBool(name, value.Bool); break;
            }

            return AIResult.Finish();
        }
    }
}