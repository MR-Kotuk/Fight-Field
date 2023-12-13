namespace CoverShooter.AI
{
    [Folder("Weapon")]
    [Success("Done")]
    [AllowMove]
    [AllowCrouch]
    [AllowAim]
    public class Disarm : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            actor.InputUnequip();

            if (actor.IsUnequipped)
                return AIResult.Finish();
            else
                return AIResult.Hold();
        }
    }
}