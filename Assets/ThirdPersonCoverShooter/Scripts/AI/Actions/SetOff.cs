namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    public class SetOff : BaseAction
    {
        [NoFieldName]
        public TriggerReference Trigger;

        [IgnoreNextValues]
        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;

        public SetOff()
        {
        }

        public SetOff(Brain brain, int trigger)
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
            e.ID = Trigger.ID;
            e.Type = AIEvent.Trigger;
            e.Value0 = state.Dereference(ref Value0);
            e.Value1 = state.Dereference(ref Value1);
            e.Value2 = state.Dereference(ref Value2);
            e.Value3 = state.Dereference(ref Value3);

            state.Feed(ref e);

            return AIResult.Finish();
        }
    }
}