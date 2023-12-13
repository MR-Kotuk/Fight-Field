using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Distance")]
    public class Distance : BaseExpression
    {
        public Value A = new Value(Vector3.zero);

        public Value B = new Value(Vector3.zero);

        public override string GetText(Brain brain)
        {
            return "Distance(" + A.GetText(brain) + ", " + B.GetText(brain) + ")";
        }

        public override Value Evaluate(int id, State state)
        {
            return new Value((state.Dereference(ref A).Vector - state.Dereference(ref B).Vector).magnitude);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return ValueType.Float;
        }
    }
}