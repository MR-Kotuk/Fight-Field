namespace CoverShooter.AI
{
    [Folder("Target")]
    [Success("Done")]
    [Immediate]
    public class SetTarget : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Object;

        public SetTarget()
        {
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.SetTarget(state.Dereference(ref Object).GameObject);

            return AIResult.Finish();
        }
    }
}