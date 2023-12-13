using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Raycasting")]
    public class IsPositionVisible : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "IsPositionVisible(" + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var target = state.GetPosition(ref Target);

            return new Value(AIUtil.IsInSight(state.Actor, target, state.ViewDistance + 0.5f, state.FieldOfView));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}