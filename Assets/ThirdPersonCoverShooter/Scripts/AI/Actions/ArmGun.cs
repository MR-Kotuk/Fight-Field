namespace CoverShooter.AI
{
    [Folder("Weapon")]
    [Success]
    [Failure]
    [AllowMove]
    [AllowCrouch]
    [AllowAim]
    public class ArmGun : BaseAction
    {
        [ValueType(ValueType.Weapon)]
        public Value Weapon = new Value(WeaponType.Rifle);

        [ValueType(ValueType.Boolean)]
        public Value GivenTypeOnly = new Value(false);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var preferredType = state.Dereference(ref Weapon).Weapon;
            var givenTypeOnly = state.Dereference(ref GivenTypeOnly).Bool;

            if (actor.Weapon.IsNull ||
                actor.Weapon.Gun == null ||
                actor.Weapon.Gun.Type != preferredType)
            {
                var inventory = actor.GetComponent<CharacterInventory>();
                var isSuccess = false;

                if (inventory != null && inventory.Weapons != null)
                {
                    for (int i = 0; i < inventory.Weapons.Length; i++)
                    {
                        var weapon = inventory.Weapons[i];

                        if (weapon.Gun != null && weapon.Gun.Type == preferredType)
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

                            if (weapon.Gun != null)
                            {
                                actor.InputEquip(ref weapon);

                                isSuccess = true;
                                break;
                            }
                        }

                        if (!isSuccess && !actor.Weapon.IsNull && actor.Weapon.Gun != null)
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
                    if (actor.Weapon.Gun != null && !givenTypeOnly)
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