using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Any At")]
    public class AnyInvisibleCoverAt : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value Radius = new Value(2);

        public override string GetText(Brain brain)
        {
            return "AnyInvisibleCoverAt(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var position = state.GetPosition(ref Position);
            var radius = state.Dereference(ref Radius).Float;

            var foundCount = Physics.OverlapSphereNonAlloc(position, radius, Util.Colliders, CoverShooter.Layers.Cover, QueryTriggerInteraction.Collide);

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;
                var cover = CoverSearch.GetCover(coverObject);

                if (cover == null)
                    continue;

                if (!cover.IsInFront(state.Actor.transform.position, false))
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