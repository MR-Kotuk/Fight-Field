using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.AimAndFire)]
    public class FireExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;
            var gun = actor.Weapon.Gun;

            if (gun == null)
                return;

            if (!actor.IsEquipped)
                actor.InputEquip();
            else
            {
                var target = state.GetPosition(ref Target);

                actor.InputFireAvoidFriendly(target);
            }
        }
    }
}