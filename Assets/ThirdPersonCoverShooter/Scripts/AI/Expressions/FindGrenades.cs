using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Find")]
    public class FindGrenades : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = Value.Builtin(BuiltinValue.SelfPosition);

        [ValueType(ValueType.Float)]
        public Value Radius = new Value(3f);

        public override string GetText(Brain brain)
        {
            return "FindActors(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var position = state.GetPosition(ref Position);
            var radius = state.Dereference(ref Radius).Float;

            var array = state.GetArray(id, 2);
            var count = 0;

            for (int i = 0; i < GrenadeList.Count; i++)
            {
                var grenade = GrenadeList.Get(i);

                if (Vector3.Distance(grenade.transform.position, position) <= radius + float.Epsilon)
                    Value.Add(ref array, ref count, new Value(grenade.gameObject));
            }

            state.Arrays[id] = array;

            return new Value(array, count, ValueType.GameObject);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Array;
        }
    }
}