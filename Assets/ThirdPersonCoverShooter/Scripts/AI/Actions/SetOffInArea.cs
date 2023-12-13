namespace CoverShooter.AI
{
    [Success("Done")]
    [Failure("None")]
    [Immediate]
    public class SetOffInArea : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Center = Value.Builtin(BuiltinValue.SelfPosition);

        [ValueType(ValueType.Float)]
        public Value Radius = new Value(10f);

        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.Friendly);

        [ValueType(ValueType.Boolean)]
        public Value IncludeDead = new Value(false);

        [ValueType(ValueType.Boolean)]
        public Value IncludeSelf = new Value(false);

        [NoFieldName]
        public TriggerReference Trigger;

        [IgnoreNextValues]
        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;

        public SetOffInArea()
        {
        }

        public SetOffInArea(Brain brain, int trigger)
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

            var self = state.Actor;
            var position = state.GetPosition(ref Center);
            var radius = state.Dereference(ref Radius).Float;
            var teamKind = state.Dereference(ref Team).Team;
            var exclude = state.Dereference(ref IncludeSelf).Bool ? null : state.Actor;

            int foundCount;

            if (state.Dereference(ref IncludeDead).Bool)
                foundCount = AIUtil.FindActorsIncludingDead(position, radius, exclude);
            else
                foundCount = AIUtil.FindActors(position, radius, exclude);

            int count = 0;

            for (int i = 0; i < foundCount; i++)
            {
                var target = AIUtil.Actors[i];

                if (!AIUtil.CompareTeams(self, target, teamKind))
                    continue;

                var targetAI = target.GetComponent<AIController>();

                if (targetAI == null)
                    return AIResult.Failure();

                targetAI.State.Feed(ref e);
                count++;
            }

            if (count == 0)
                return AIResult.Failure();

            return AIResult.Finish();
        }
    }
}