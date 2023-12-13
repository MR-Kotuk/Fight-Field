using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    public class CustomAction : BaseAction
    {
        [ValueType(ValueType.Text)]
        public Value Name = new Value("");

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            if (!values.HasStarted)
            {
                if (actor.IsPerformingCustomAction)
                    values.HasStarted = true;
                else
                    actor.InputCustomAction(state.Dereference(ref Name).Text);
            }
            else if (!actor.IsPerformingCustomAction)
                return AIResult.Finish();

            return AIResult.Hold();
        }
    }
}