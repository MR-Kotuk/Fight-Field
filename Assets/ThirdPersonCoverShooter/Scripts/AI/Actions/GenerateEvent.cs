namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    public class GenerateEvent : BaseAction
    {
        [NoFieldName]
        public AIEvent Event = AIEvent.Investigate;

        [IgnoreNextValues]
        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;

        public GenerateEvent()
        {
        }

        public GenerateEvent(AIEvent e)
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

            state.Feed(ref e);

            return AIResult.Finish();
        }

        public override AIEvent GetEvent(Brain brain)
        {
            return Event;
        }
    }
}