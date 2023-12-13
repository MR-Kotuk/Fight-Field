using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Find")]
    public class FindActors : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = Value.Builtin(BuiltinValue.SelfPosition);

        [ValueType(ValueType.Float)]
        public Value Radius = new Value(30f);

        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.All);

        [ValueType(ValueType.Boolean)]
        public Value IncludeDead = new Value(false);

        [ValueType(ValueType.Boolean)]
        public Value IncludeSelf = new Value(false);

        public override string GetText(Brain brain)
        {
            return "FindActors(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var position = state.GetPosition(ref Position);
            var radius = state.Dereference(ref Radius).Float;
            var ignore = state.Dereference(ref IncludeSelf).Bool ? null : state.Actor;
            var team = state.Dereference(ref Team).Team;

            int foundCount;

            if (state.Dereference(ref IncludeDead).Bool)
                foundCount = AIUtil.FindActorsIncludingDead(position, radius, ignore);
            else
                foundCount = AIUtil.FindActors(position, radius, ignore);

            var count = 0;

            if (team == ActorTeam.All)
                count = foundCount;
            else
            {
                for (int i = 0; i < foundCount; i++)
                {
                    var other = AIUtil.Actors[i];

                    if (AIUtil.CompareTeams(state.Actor, other, team))
                        count++;
                }
            }

            var array = state.GetArray(id, count);
            var index = 0;

            for (int i = 0; i < foundCount; i++)
                if (AIUtil.CompareTeams(state.Actor, AIUtil.Actors[i], team))
                {
                    array[index] = new Value(AIUtil.Actors[i].gameObject);
                    index++;
                }

            return new Value(array, count, ValueType.GameObject);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Array;
        }
    }
}