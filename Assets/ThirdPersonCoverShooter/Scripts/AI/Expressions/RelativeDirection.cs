using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Direction")]
    public class RelativeDirection : BaseExpression
    {
        [ValueType(ValueType.RelativeDirection)]
        [NoFieldName]
        public Value Direction = new Value(CoverShooter.Direction.Forward);

        public override string GetText(Brain brain)
        {
            return "RelativeDirection(" + Direction.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var forward = Vector3.forward;

            if (state.Actor != null)
                forward = (state.Actor.BodyLookTarget - state.Object.transform.position).normalized;

            var right = Vector3.Cross(Vector3.up, forward);

            switch (state.Dereference(ref Direction).Direction)
            {
                case CoverShooter.Direction.Forward: return new Value(forward); 
                case CoverShooter.Direction.ForwardRight: return new Value((forward + right).normalized);
                case CoverShooter.Direction.Right: return new Value(right);
                case CoverShooter.Direction.BackwardRight: return new Value((-forward + right).normalized);
                case CoverShooter.Direction.Backward: return new Value(-forward);
                case CoverShooter.Direction.BackwardLeft: return new Value((-forward - right).normalized);
                case CoverShooter.Direction.Left: return new Value(-right);
                case CoverShooter.Direction.ForwardLeft: return new Value((forward - right).normalized);
            }

            return new Value(forward);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}