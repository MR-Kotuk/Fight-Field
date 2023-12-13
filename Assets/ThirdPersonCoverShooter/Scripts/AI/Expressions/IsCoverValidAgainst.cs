using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Cover")]
    public class IsCoverValidAgainst : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        public Value Cover;

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value PositionInCover = new Value(Vector3.zero);

        public Value Enemy = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(4);

        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(30);

        public override string GetText(Brain brain)
        {
            return "IsCoverValidAgainst(" + Cover.GetText(brain) + ", " + Enemy.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var coverObject = state.Dereference(ref Cover).GameObject;

            if (coverObject == null)
                return new Value(false);

            var cover = CoverSearch.GetCover(coverObject);

            if (cover == null)
                return new Value(false);

            var position = state.GetPosition(ref PositionInCover);
            var enemyPosition = state.Dereference(ref Enemy).Vector;

            var distance = Vector3.Distance(position, enemyPosition);

            if (distance >= state.Dereference(ref MinDistance).Float && distance <= state.Dereference(ref MaxDistance).Float + 0.5f)
            {
                if (!AICoverUtil.IsCoverPositionFree(cover, position, 1, state.Actor))
                    return new Value(false);

                if (AICoverUtil.IsValidCoverAgainst(cover, position, enemyPosition))
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