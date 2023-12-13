using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Detect")]
    [Success("Found")]
    [SuccessParameter("All", ValueType.GameObject, true)]
    [SuccessParameter("Closest", ValueType.GameObject)]
    [SuccessParameter("Average Position", ValueType.Vector3)]
    [Failure("None")]
    [AllowMove]
    [AllowAimAndFire]
    [AllowCrouch]
    public class DetectGrenades : BaseAction
    {
        [ValueType(ValueType.Boolean)]
        public Value OnlyThreatening = new Value(true);

        [ValueType(ValueType.Float)]
        public Value TimeThreshold = new Value(2);

        [ValueType(ValueType.Boolean)]
        public Value IsAlerted = new Value(false);

        [ValueType(ValueType.Float)]
        public Value ObstacleIgnoreDistance = new Value(1f);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            GameObject closest = null;
            var closestDistance = 0f;
            var sum = Vector3.zero;

            var self = state.Actor;
            var selfPosition = actor.transform.position;
            var obstacleIgnoreDistance = state.Dereference(ref ObstacleIgnoreDistance).Float;
            var viewDistance = self.GetViewDistance(state.ViewDistance, state.Dereference(ref IsAlerted).Bool);
            var onlyThreatening = state.Dereference(ref OnlyThreatening).Bool;
            var timeThreshold = state.Dereference(ref TimeThreshold).Float;

            var array = state.GetArray(state.Layers[layer].CurrentNode, 8);
            var count = 0;

            for (int i = 0; i < GrenadeList.Count; i++)
            {
                var grenade = GrenadeList.Get(i);

                if (!grenade.IsActivated)
                    continue;

                if (grenade.SecondsLeft > timeThreshold)
                    continue;

                var p = grenade.transform.position;
                var distance = Vector3.Distance(selfPosition, p);

                if (onlyThreatening && distance > grenade.ExplosionRadius)
                    continue;

                if (AIUtil.IsInSight(self, grenade.transform.position, viewDistance, state.FieldOfView, obstacleIgnoreDistance))
                {
                    sum += p;
                    Value.Add(ref array, ref count, new Value(grenade.gameObject));

                    if (closest == null || closestDistance > distance)
                    {
                        closest = grenade.gameObject;
                        closestDistance = distance;
                    }
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(array, count, ValueType.GameObject), new Value(closest), new Value(sum / count) });
            else
                return AIResult.Failure();
        }
    }
}