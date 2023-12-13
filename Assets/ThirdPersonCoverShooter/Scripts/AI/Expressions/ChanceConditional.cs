using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Random")]
    public class ChanceConditional : BaseExpression
    {
        [ValueType(ValueType.Float)]
        public Value Chance = new Value(0.5f);

        public Value IfGreater;
        public Value IfLesser;

        public override string GetText(Brain brain)
        {
            return Chance.GetText(brain) + " ? " + IfGreater.GetText(brain) + " : " + IfLesser.GetText(brain);
        }

        public override Value Evaluate(int id, State state)
        {
            return (UnityEngine.Random.Range(0f, 1f) + float.Epsilon <= state.Dereference(ref Chance).Float) ? state.Dereference(ref IfGreater) : state.Dereference(ref IfLesser);
        }

        public override ValueType GetReturnType(Brain brain)
        {
            return brain.GetValueType(ref IfGreater);
        }
    }
}