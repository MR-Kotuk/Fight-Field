using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Aim")]
    [AllowMove]
    [AllowCrouch]
    public class Aim : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var target = state.GetPosition(ref Target);

            actor.InputAim(target);

            return AIResult.Hold();
        }
    }
}