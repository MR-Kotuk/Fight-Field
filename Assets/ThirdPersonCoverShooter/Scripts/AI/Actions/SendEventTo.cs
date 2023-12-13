using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Failure("Unable")]
    [Immediate]
    public class SendEventTo : BaseAction
    {
        [ValueType(ValueType.GameObject, true)]
        public Value Target;

        [NoFieldName]
        public AIEvent Event = AIEvent.Investigate;

        [IgnoreNextValues]
        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;

        public SendEventTo()
        {
        }

        public SendEventTo(AIEvent e)
        {
            Event = e;
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var e = new EventDesc();
            e.Type = Event;
            e.Value0 = state.Dereference(ref Value0);
            e.Value1 = state.Dereference(ref Value1);
            e.Value2 = state.Dereference(ref Value2);
            e.Value3 = state.Dereference(ref Value3);

            var deref = state.Dereference(ref Target);

            if (deref.Type == ValueType.GameObject)
            {
                if (Go(deref.GameObject, ref e))
                    return AIResult.Finish();
                else
                    return AIResult.Failure();
            }
            else if (deref.Type == ValueType.Array)
            {
                var count = 0;

                for (int i = 0; i < deref.Array.Length; i++)
                    if (Go(deref.Array[i].GameObject, ref e))
                        count++;

                if (count == 0)
                    return AIResult.Failure();
                else
                    return AIResult.Finish();
            }
            else
                return AIResult.Failure();
        }

        public override AIEvent GetEvent(Brain brain)
        {
            return Event;
        }

        private bool Go(GameObject target, ref EventDesc e)
        {
            if (target == null)
                return false;

            var targetAI = target.GetComponent<AIController>();

            if (targetAI == null)
                return false;

            targetAI.State.Feed(ref e);

            return true;
        }
    }
}