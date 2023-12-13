using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Attack")]
    [Success("Done")]
    [Failure("Can't")]
    [AllowMove]
    [AllowCrouch]
    public class BurstFire : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value Count = new Value(3);

        [ValueType(ValueType.Float)]
        public Value Wait = new Value(0.25f);

        [ValueType(ValueType.Float)]
        public Value Duration = new Value(1f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            var target = state.GetPosition(ref Target);
            actor.InputAim(target);

            var gun = actor.Weapon.Gun;

            if (gun == null)
                return AIResult.Failure();

            if (gun.LoadedBulletsLeft == 0 && !gun.CanLoad && !actor.IsReloading)
                return AIResult.Failure();

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

            if (values.Count / 2 > state.Dereference(ref Count).Float)
                return AIResult.Finish();
            else
                return AIResult.Hold();
        }
    }
}