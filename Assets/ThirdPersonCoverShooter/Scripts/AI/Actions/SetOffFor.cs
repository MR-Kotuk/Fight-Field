using UnityEngine;

namespace CoverShooter.AI
{
    [Success("Done")]
    [Failure("Unable")]
    [Immediate]
    public class SetOffFor : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Target;

        [NoFieldName]
        public TriggerReference Trigger;

        [IgnoreNextValues]
        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;

        public SetOffFor()
        {
        }

        public SetOffFor(Brain brain, int trigger)
        {
            Trigger = new TriggerReference(trigger);

            var t = brain.GetTrigger(trigger);

            if (t != null && t.Values != null)
            {
                if (t.Values.Length > 0) Value0.Type = t.Values[0].Type;
                if (t.Values.Length > 1) Value1.Type = t.Values[1].Type;
                if (t.Values.Length > 2) Value2.Type = t.Values[2].Type;
                if (t.Values.Length > 3) Value3.Type = t.Values[3].Type;
            }
        }

        public override Trigger GetTrigger(Brain brain)
        {
            return brain.GetTrigger(Trigger.ID);
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var e = new EventDesc();
            e.ID = 0;
            e.Name = state.Brain.GetTrigger(Trigger.ID).Name;
            e.Type = AIEvent.Trigger;
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