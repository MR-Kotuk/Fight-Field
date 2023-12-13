using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("BaseActor")]
    public class GetStandingTopPosition : BaseExpression
    {
        [ValueType(ValueType.GameObject)]
        [NoFieldName]
        public Value Object;

        public override string GetText(Brain brain)
        {
            return "GetStandingTopPosition(" + Object.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var o = state.Dereference(ref Object).GameObject;

            if (o == null)
                return new Value(Vector3.zero);

            var a = Actors.Get(o);

            return new Value(a == null ? o.transform.position : a.StandingTopPosition);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Vector3;
        }
    }
}