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
    public class DetectActors : BaseAction
    {
        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.All);

        [ValueType(ValueType.Boolean)]
        public Value IsAlerted = new Value(false);

        [ValueType(ValueType.Boolean)]
        public Value IncludeDead = new Value(false);

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
            var teamKind = state.Dereference(ref Team).Team;
            var obstacleIgnoreDistance = state.Dereference(ref ObstacleIgnoreDistance).Float;
            var viewDistance = self.GetViewDistance(state.ViewDistance, state.Dereference(ref IsAlerted).Bool);

            int foundCount;

            if (state.Dereference(ref IncludeDead).Bool)
                foundCount = AIUtil.FindActorsIncludingDead(actor.transform.position, viewDistance, state.Actor);
            else
                foundCount = AIUtil.FindActors(actor.transform.position, viewDistance, state.Actor);

            var array = state.GetArray(state.Layers[layer].CurrentNode, 8);
            var count = 0;

            for (int i = 0; i < foundCount; i++)
            {
                var them = AIUtil.Actors[i];

                if (!AIUtil.CompareTeams(self, them, teamKind))
                    continue;

                if (AIUtil.IsInSight(self, them.TopPosition, viewDistance, state.FieldOfView, obstacleIgnoreDistance))
                {
                    Value.Add(ref array, ref count, new Value(them.gameObject));

                    var p = them.transform.position;
                    sum += p;

                    var distance = Vector3.Distance(selfPosition, p);

                    if (closest == null || closestDistance > distance)
                    {
                        closest = AIUtil.Actors[i].gameObject;
                        closestDistance = distance;
                    }
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(array, count, ValueType.GameObject), new Value(closest), new Value(sum / foundCount) });
            else
                return AIResult.Failure();
        }
    }
}