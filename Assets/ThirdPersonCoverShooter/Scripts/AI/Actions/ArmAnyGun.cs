namespace CoverShooter.AI
{
    [Folder("Weapon")]
    [Success]
    [Failure]
    [AllowMove]
    [AllowCrouch]
    [AllowAim]
    public class ArmAnyGun : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            if (actor.Weapon.IsNull ||
                actor.Weapon.Gun == null)
            {
                var inventory = actor.GetComponent<CharacterInventory>();
                var isSuccess = false;

                if (inventory != null && inventory.Weapons != null)
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

                if (!isSuccess)
                    return AIResult.Failure();
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