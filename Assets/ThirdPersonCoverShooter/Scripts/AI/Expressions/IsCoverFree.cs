using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Cover")]
    public class IsCoverFree : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        public Value Cover;

        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value PositionInCover = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value Threshold = new Value(2);

        public override string GetText(Brain brain)
        {
            return "IsCoverFree(" + Cover.GetText(brain) + ")";
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
            var threshold = state.Dereference(ref Threshold).Float;

            if (AICoverUtil.IsCoverPositionFree(cover, position, threshold, state.Actor))
                return new Value(true);
            else
                return new Value(false);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}