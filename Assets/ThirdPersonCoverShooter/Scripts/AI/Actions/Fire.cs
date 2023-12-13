using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Attack")]
    [Failure("Can't")]
    [AllowMove]
    [AllowCrouch]
    public class Fire : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Boolean)]
        public Value AutoReload = new Value(true);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var gun = actor.Weapon.Gun;

            if (gun == null)
                return AIResult.Failure();

            var target = state.GetPosition(ref Target);

            if (gun.LoadedBulletsLeft == 0 && !actor.IsReloading)
            {
                if (state.Dereference(ref AutoReload).Bool)
                    actor.InputReload();
                else
                    return AIResult.Failure();
            }

            if (!actor.IsReloading)
                actor.InputFireAvoidFriendly(target);

            return AIResult.Hold();
        }
    }
}