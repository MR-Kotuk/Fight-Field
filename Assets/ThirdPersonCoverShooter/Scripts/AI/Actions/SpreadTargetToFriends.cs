namespace CoverShooter.AI
{
    [Folder("Target")]
    [Immediate]
    [Success("Success")]
    [Failure("Nobody/no target")]
    public class SpreadTargetToFriends : BaseAction
    {
        [ValueType(ValueType.Float)]
        public Value Distance = new Value(20);

        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.Friendly);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (state.SpreadTargetToFriends(state.Dereference(ref Distance).Float))
                return AIResult.Success();
            else
                return AIResult.Failure();
        }
    }
}