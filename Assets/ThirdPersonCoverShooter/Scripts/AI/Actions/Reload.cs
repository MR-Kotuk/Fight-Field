using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Attack")]
    [Success("Done")]
    [Failure("Can't")]
    [AllowMove]
    [AllowCrouch]
    public class Reload : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var gun = actor.Weapon.Gun;

            if (gun == null)
                return AIResult.Failure();

            if (gun.LoadedBulletsLeft == 0 && !actor.IsReloading)
                actor.InputReload();

            if (!actor.IsReloading)
            {
                if (gun.LoadedBulletsLeft > 0)
                    return AIResult.Success();
                else
                    return AIResult.Failure();
            }

            return AIResult.Hold();
        }
    }
}