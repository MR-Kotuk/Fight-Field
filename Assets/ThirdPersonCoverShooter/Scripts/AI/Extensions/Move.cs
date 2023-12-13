using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Move)]
    public class MoveExtension : BaseExtension
    {
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        public Value Direction = new Value(CoverShooter.Direction.Forward);

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Walk);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;

            var direction = state.GetDirection(ref Direction);
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