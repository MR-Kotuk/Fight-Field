using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Animator")]
    public class GetAnimatorBool : BaseExpression
    {
        [ValueType(ValueType.Text)]
        public Value Name = new Value("");

        public override string GetText(Brain brain)
        {
            return "GetAnimatorBool(" + Name.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var animator = state.Animator;

            if (animator == null)
                return new Value(0f);

            var name = state.Dereference(ref Name).Text;

            return new Value(animator.GetBool(name));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}