using UnityEngine;

namespace CoverShooter.AI
{
    [Immediate]
    [Success("Done")]
    public class Log : BaseAction
    {
        [ValueType(ValueType.Text)]
        public Value Value;

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (!values.HasStarted)
            {
                values.HasStarted = true;
                Debug.Log(state.Dereference(ref Value).Text);
            }

            return AIResult.Finish();
        }
    }
}