using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Detect")]
    [Success("Found")]
    [SuccessParameter("Object", ValueType.GameObject)]
    [Failure("None")]
    [AllowMove]
    [AllowAimAndFire]
    [AllowCrouch]
    public class DetectSingleActor : BaseAction
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

            GameObject closest = null;
            var closestDistance = 0f;

            for (int i = 0; i < foundCount; i++)
            {
                var them = AIUtil.Actors[i];

                if (!AIUtil.CompareTeams(self, them, teamKind))
                    continue;

                if (AIUtil.IsInSight(self, them.TopPosition, viewDistance, state.FieldOfView, obstacleIgnoreDistance))
                {
                    var p = them.transform.position;

                    var distance = Vector3.Distance(selfPosition, p);

                    if (closest == null || closestDistance > distance)
                    {
                        closest = AIUtil.Actors[i].gameObject;
                        closestDistance = distance;
                    }
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(closest) });
            else
                return AIResult.Failure();
        }
    }
}