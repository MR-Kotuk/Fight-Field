using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Any At")]
    public class AnyActorsAt : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value Radius = new Value(2);

        [ValueType(ValueType.Team)]
        public Value Team = new Value(ActorTeam.All);

        [ValueType(ValueType.Boolean)]
        public Value IncludeSelf = new Value(false);

        public override string GetText(Brain brain)
        {
            return "AnyActorsAt(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var position = state.GetPosition(ref Position);
            var radius = state.Dereference(ref Radius).Float;
            var includeSelf = state.Dereference(ref IncludeSelf).Bool;
            var team = state.Dereference(ref Team).Team;

            var count = AIUtil.FindActors(position, radius, includeSelf ? null : state.Actor);

            for (int i = 0; i < count; i++)
            {
                var other = AIUtil.Actors[i];

                if (AIUtil.CompareTeams(state.Actor, other, team))
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