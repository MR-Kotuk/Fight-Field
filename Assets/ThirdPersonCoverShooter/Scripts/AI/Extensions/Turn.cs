using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Aim)]
    public class TurnExtension : BaseExtension
    {
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        public Value Direction = new Value(CoverShooter.Direction.Forward);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;
            var direction = state.GetDirection(ref Direction);

            actor.InputLook(actor.transform.position + direction * 100);
        }
    }
}