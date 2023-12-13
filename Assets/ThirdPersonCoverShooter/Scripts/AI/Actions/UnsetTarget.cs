namespace CoverShooter.AI
{
    [Folder("Target")]
    [Success("Done")]
    [Immediate]
    public class UnsetTarget : BaseAction
    {
        public UnsetTarget()
        {
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.UnsetTarget();

            return AIResult.Finish();
        }
    }
}