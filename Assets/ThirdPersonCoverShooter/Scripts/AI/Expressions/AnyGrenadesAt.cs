using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Any At")]
    public class AnyGrenadesAt : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "AnyGrenadesAt(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var position = state.GetPosition(ref Position);

            for (int i = 0; i < GrenadeList.Count; i++)
            {
                var grenade = GrenadeList.Get(i);

                if (Vector3.Distance(grenade.transform.position, position) < grenade.ExplosionRadius)
                    return new Value(true);
            }

            return new Value(false);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}