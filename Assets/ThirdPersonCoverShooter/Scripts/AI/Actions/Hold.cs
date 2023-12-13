namespace CoverShooter.AI
{
    [AllowMove]
    [AllowAimAndFire]
    [AllowCrouch]
    public class Hold : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            return AIResult.Hold(10);
        }
    }
}