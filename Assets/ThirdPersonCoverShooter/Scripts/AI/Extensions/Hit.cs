using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Fire)]
    public class HitExtension : BaseExtension
    {
        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;

            if (!actor.Weapon.HasMelee)
                return;

            if (!actor.IsEquipped)
                actor.InputEquip();
            else
                actor.InputMelee();
        }
    }
}