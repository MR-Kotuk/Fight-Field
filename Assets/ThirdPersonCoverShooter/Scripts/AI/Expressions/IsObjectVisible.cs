using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Raycasting")]
    public class IsObjectVisible : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        public Value Target;

        public override string GetText(Brain brain)
        {
            return "IsObjectVisible(" + Target.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var obj = state.Dereference(ref Target).GameObject;

            if (obj == null)
                return new Value(false);

            Vector3 target;

            var actor = Actors.Get(obj);

            if (actor == null)
                target = obj.transform.position;
            else
                target = actor.TopPosition;

            return new Value(AIUtil.IsInSight(state.Actor, target, state.ViewDistance + 0.5f, state.FieldOfView));
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}