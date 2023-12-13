using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Aim)]
    public class TurnFromExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;
            var direction = (actor.transform.position - state.GetPosition(ref Position)).normalized;

            actor.InputLook(actor.transform.position + direction * 100);
        }
    }
}