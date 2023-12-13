namespace CoverShooter.AI
{
    [Folder("Weapon")]
    [Success]
    [Failure]
    [AllowMove]
    [AllowCrouch]
    [AllowAim]
    public class ArmMelee : BaseAction
    {
        [ValueType(ValueType.Weapon)]
        public Value Weapon = new Value(WeaponType.Fist);

        [ValueType(ValueType.Boolean)]
        public Value GivenTypeOnly = new Value(false);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var preferredType = state.Dereference(ref Weapon).Weapon;
            var givenTypeOnly = state.Dereference(ref GivenTypeOnly).Bool;

            if (actor.Weapon.IsNull ||
                !actor.Weapon.HasMelee || 
                !((actor.Weapon.LeftMelee != null && actor.Weapon.LeftMelee.Type == preferredType) ||
                  (actor.Weapon.RightMelee != null && actor.Weapon.RightMelee.Type == preferredType)))
            {
                var inventory = actor.GetComponent<CharacterInventory>();
                var isSuccess = false;

                if (inventory != null && inventory.Weapons != null)
                {
                    for (int i = 0; i < inventory.Weapons.Length; i++)
                    {
                        var weapon = inventory.Weapons[i];

                        if (weapon.HasMelee && 
                            ((weapon.LeftMelee != null && weapon.LeftMelee.Type == preferredType) ||
                             (weapon.RightMelee != null && weapon.RightMelee.Type == preferredType)))
                        {
                            actor.InputEquip(ref weapon);

                            isSuccess = true;
                            break;
                        }
                    }

                    if (!isSuccess && !givenTypeOnly)
                    {
                        for (int i = 0; i < inventory.Weapons.Length; i++)
                        {
                            var weapon = inventory.Weapons[i];

                            if (weapon.HasMelee)
                            {
                                actor.InputEquip(ref weapon);

                                isSuccess = true;
                                break;
                            }
                        }

                        if (!isSuccess && !actor.Weapon.IsNull && actor.Weapon.HasMelee)
                        {
                            actor.InputEquip();
                            isSuccess = true;
                        }
                    }


                    if (isSuccess)
                    {
                        if (actor.IsEquipped)
                            return AIResult.Finish();
                    }
                }

                if (!isSuccess)
                {
                    if (actor.Weapon.HasMelee && !givenTypeOnly)
                    {
                        actor.InputEquip();

                        if (actor.IsEquipped)
                            return AIResult.Finish();
                    }
                    else
                        return AIResult.Failure();
                }
            }
            else
            {
                actor.InputEquip();

                if (actor.IsEquipped)
                    return AIResult.Finish();
            }

            return AIResult.Hold();
        }
    }
}