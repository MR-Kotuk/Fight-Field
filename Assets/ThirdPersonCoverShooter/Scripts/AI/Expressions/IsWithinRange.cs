using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Distance")]
    public class IsWithinRange : BaseExpression
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(new Vector3(0, 0, 0));

        [ValueType(ValueType.Float)]
        public Value Min = new Value(0f);

        [ValueType(ValueType.Float)]
        public Value Max = new Value(10f);

        public override string GetText(Brain brain)
        {
            return "IsWithinRange(" + Position.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            var distance = Vector3.Distance(state.Object.transform.position, state.GetPosition(ref Position));
            var min = state.Dereference(ref Min).Float;
            var max = state.Dereference(ref Max).Float;

            return new Value(distance >= min && distance <= max);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Boolean;
        }
    }
}