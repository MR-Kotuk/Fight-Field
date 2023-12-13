using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Animator")]
    public class GetAnimatorFloat : BaseExpression
    {
        [ValueType(ValueType.Text)]
        public Value Name = new Value("");

        public override string GetText(Brain brain)
        {
            return "GetAnimatorFloat(" + Name.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var animator = state.Animator;

            if (animator == null)
                return new Value(0f);

            var name = state.Dereference(ref Name).Text;

            return new Value(animator.GetFloat(name));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}