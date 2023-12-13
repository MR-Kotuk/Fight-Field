using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Melee")]
    [Failure("Can't")]
    [AllowMove]
    [AllowAim]
    [AllowCrouch]
    public class Block : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            if (!actor.Weapon.HasMelee)
                return AIResult.Failure();

            actor.InputBlock();

            return AIResult.Hold();
        }
    }
}