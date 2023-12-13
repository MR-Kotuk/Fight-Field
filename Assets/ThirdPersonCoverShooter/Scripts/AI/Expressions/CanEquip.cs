namespace CoverShooter.AI
{
    [Folder("Weapon")]
    public class CanEquip : BaseExpression
    {
        [ValueType(ValueType.Weapon)]
        public Value Weapon = new Value(WeaponType.Rifle);

        public override string GetText(Brain brain)
        {
            return "CanEquip(" + Weapon.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var actor = state.Actor;
            var preferredType = state.Dereference(ref Weapon).Weapon;

            if (!actor.Weapon.IsNull &&
                actor.Weapon.Gun != null &&
                actor.Weapon.Gun.Type == preferredType)
                return new Value(true);

            var inventory = actor.GetComponent<CharacterInventory>();

            if (inventory != null && inventory.Weapons != null)
            {
                for (int i = 0; i < inventory.Weapons.Length; i++)
                {
                    if (inventory.Weapons[i].Gun != null && inventory.Weapons[i].Gun.Type == preferredType)
                        return new Value(true);
                }
            }

            return new Value(false);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}