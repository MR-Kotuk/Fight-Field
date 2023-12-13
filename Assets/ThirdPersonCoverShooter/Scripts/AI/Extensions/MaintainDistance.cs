using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Move)]
    public class MaintainDistanceExtension : BaseExtension
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(1f);

        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(2.5f);

        [ValueType(ValueType.Float)]
        public Value ChargeDistance = new Value(6f);

        [ValueType(ValueType.Speed)]
        public Value AvoidSpeed = new Value(CharacterSpeed.Walk);

        [ValueType(ValueType.Speed)]
        public Value FollowSpeed = new Value(CharacterSpeed.Run);

        [ValueType(ValueType.Speed)]
        public Value ChargeSpeed = new Value(CharacterSpeed.Sprint);

        public override void Update(State state, int layer, ref ExtensionState values)
        {
            var actor = state.Actor;
            var target = state.GetPosition(ref Position);

            var vector = target - actor.transform.position;
            var distance = vector.magnitude;
            var direction = vector / distance;

            if (distance > state.Dereference(ref ChargeDistance).Float)
            {
                float speed = 1;

                switch (state.Dereference(ref ChargeSpeed).Speed)
                {
                    case CharacterSpeed.Walk: speed = 0.5f; break;
                    case CharacterSpeed.Run: speed = 1.0f; break;
                    case CharacterSpeed.Sprint: speed = 2.0f; break;
                }

                actor.InputMoveTo(target, speed);
            }
            else if (distance > state.Dereference(ref MaxDistance).Float)
            {
                float speed = 1;

                switch (state.Dereference(ref FollowSpeed).Speed)
                {
                    case CharacterSpeed.Walk: speed = 0.5f; break;
                    case CharacterSpeed.Run: speed = 1.0f; break;
                    case CharacterSpeed.Sprint: speed = 2.0f; break;
                }

                actor.InputMoveTo(target, speed);
            }
            else if (distance < state.Dereference(ref MinDistance).Float)
            {
                float speed = 1;

                switch (state.Dereference(ref AvoidSpeed).Speed)
                {
                    case CharacterSpeed.Walk: speed = 0.5f; break;
                    case CharacterSpeed.Run: speed = 1.0f; break;
                    case CharacterSpeed.Sprint: speed = 2.0f; break;
                }

                actor.InputMovement(new CharacterMovement(-direction, speed));
            }
        }
    }
}