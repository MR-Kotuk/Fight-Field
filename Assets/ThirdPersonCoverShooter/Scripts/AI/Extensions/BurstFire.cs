using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.AimAndFire)]
    public class BurstFireExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value Wait = new Value(0.25f);

        [ValueType(ValueType.Float)]
        public Value Duration = new Value(1f);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;

            var target = state.GetPosition(ref Target);
            actor.InputAim(target);

            var gun = actor.Weapon.Gun;

            if (gun == null)
                return;

            if (!actor.IsEquipped)
            {
                actor.InputEquip();
                return;
            }

            if (gun.LoadedBulletsLeft == 0 && !gun.CanLoad && !actor.IsReloading)
                return;

            if (values.Count % 2 == 0)
            {
                values.Time += Time.deltaTime;

                if (gun.LoadedBulletsLeft == 0 && !actor.IsReloading)
                    actor.InputReload();

                if (values.Time > state.Dereference(ref Wait).Float && !actor.IsReloading)
                {
                    values.Time = 0;
                    values.Count++;
                }
            }
            else
            {
                values.Time += Time.deltaTime;

                if (gun.LoadedBulletsLeft == 0)
                    values.Count++;
                else
                {
                    if (values.Time > state.Dereference(ref Duration).Float)
                    {
                        values.Time = 0;
                        values.Count++;
                    }

                    actor.InputFireAvoidFriendly();
                }
            }
        }
    }
}