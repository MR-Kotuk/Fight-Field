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
    public class QueryActors : BaseAction
    {
        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.All);

        [ValueType(ValueType.Float)]
        public Value Distance = new Value(30);

        [ValueType(ValueType.Boolean)]
        public Value IncludeDead = new Value(false);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            GameObject closest = null;
            var closestDistance = 0f;
            var sum = Vector3.zero;

            var self = state.Actor;
            var selfPosition = actor.transform.position;
            var maxDistance = state.Dereference(ref Distance).Float;
            var teamKind = state.Dereference(ref Team).Team;            

            int foundCount;

            if (state.Dereference(ref IncludeDead).Bool)
                foundCount = AIUtil.FindActorsIncludingDead(actor.transform.position, maxDistance, state.Actor);
            else
                foundCount = AIUtil.FindActors(actor.transform.position, maxDistance, state.Actor);

            var array = state.GetArray(state.Layers[layer].CurrentNode, 8);
            var count = 0;

            for (int i = 0; i < foundCount; i++)
            {
                var them = AIUtil.Actors[i];

                if (!AIUtil.CompareTeams(self, them, teamKind))
                    continue;

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

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(array, count, ValueType.GameObject), new Value(closest), new Value(sum / foundCount) });
            else
                return AIResult.Failure();
        }
    }
}