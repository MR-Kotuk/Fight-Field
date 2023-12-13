using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Query")]
    [Success("Found")]
    [SuccessParameter("Array", ValueType.GameObject, true)]
    [SuccessParameter("Closest", ValueType.GameObject)]
    [SuccessParameter("Average Position", ValueType.Vector3)]
    [Failure("None")]
    [AllowMove]
    [AllowAimAndFire]
    [AllowCrouch]
    public class QueryCovers : BaseAction
    {
        [ValueType(ValueType.Float)]
        public Value Distance = new Value(30);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            GameObject closest = null;
            var closestDistance = 0f;
            var sum = Vector3.zero;

            var self = state.Actor;
            var selfPosition = actor.transform.position;
            var maxDistance = state.Dereference(ref Distance).Float;      

            var foundCount = Physics.OverlapSphereNonAlloc(selfPosition, maxDistance, Util.Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            var array = state.GetArray(state.Layers[layer].CurrentNode, 8);
            var count = 0;

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;

                if (CoverSearch.GetCover(coverObject) == null)
                    continue;

                Value.Add(ref array, ref count, new Value(coverObject));

                var p = coverObject.transform.position;
                sum += p;

                var distance = Vector3.Distance(selfPosition, p);

                if (closest == null || closestDistance > distance)
                {
                    closest = coverObject;
                    closestDistance = distance;
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(array, count, ValueType.GameObject), new Value(closest), new Value(sum / foundCount) });
            else
                return AIResult.Failure();
        }
    }
}