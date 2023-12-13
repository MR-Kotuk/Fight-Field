namespace CoverShooter.AI
{
    [Success("Done")]
    [AllowMove]
    [AllowAimAndFire]
    [AllowCrouch]
    public class Wait : BaseAction
    {
        [ValueType(ValueType.Float)]
        public Value Duration = new Value(1f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (!values.HasStarted)
            {
                values.HasStarted = true;
                return AIResult.Hold(state.Dereference(ref Duration).Float);
            }
            else
                return AIResult.Finish();
        }
    }
}