using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Move)]
    public class MoveFromExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Walk);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;

            var direction = (actor.transform.position - state.GetPosition(ref Position)).normalized;
            var speed = 1f;

            switch (state.Dereference(ref Speed).Speed)
            {
                case CharacterSpeed.Walk: speed = 0.5f; break;
                case CharacterSpeed.Run: speed = 1.0f; break;
                case CharacterSpeed.Sprint: speed = 2.0f; break;
            }

            actor.InputMovement(new CharacterMovement(direction, speed));
        }
    }
}