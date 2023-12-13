namespace CoverShooter.AI
{
    [Success("Done")]
    [Immediate]
    public class NextFrame : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (!values.HasStarted)
            {
                values.HasStarted = true;
                return AIResult.Hold();
            }
            else
                return AIResult.Finish();
        }
    }
}