using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Aim)]
    public class AimExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;
            var target = state.GetPosition(ref Target);

            actor.InputAim(target);
        }
    }
}