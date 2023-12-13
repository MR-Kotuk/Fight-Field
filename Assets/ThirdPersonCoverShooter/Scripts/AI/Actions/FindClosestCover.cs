using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Find")]
    [Success("Found")]
    [SuccessParameter("Object", ValueType.GameObject, false)]
    [SuccessParameter("Position", ValueType.GameObject)]
    [Failure("Failed")]
    [Immediate]
    public class FindClosestCover : BaseAction
    {
        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(30);

        [ValueType(ValueType.Float)]
        public Value TakenThreshold = new Value(2);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            var self = state.Actor;
            var selfPosition = actor.transform.position;
            var maxDistance = state.Dereference(ref MaxDistance).Float;
            var takenThreshold = state.Dereference(ref TakenThreshold).Float;

            var foundCount = Physics.OverlapSphereNonAlloc(selfPosition, maxDistance, Util.Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            GameObject closest = null;
            var closestPosition = Vector3.zero;
            var closestDistance = 0f;

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;
                var cover = CoverSearch.GetCover(coverObject);

                if (cover == null)
                    continue;

                var position = Vector3.zero;

                if (AICoverUtil.Consider(cover, ref position, selfPosition, maxDistance, takenThreshold, state.Actor))
                {
                    var distance = Vector3.Distance(selfPosition, position);

                    if (closest == null || closestDistance > distance)
                    {
                        closestPosition = position;
                        closest = coverObject;
                        closestDistance = distance;
                    }
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(closest), new Value(closestPosition) });
            else
                return AIResult.Failure();
        }
    }
}