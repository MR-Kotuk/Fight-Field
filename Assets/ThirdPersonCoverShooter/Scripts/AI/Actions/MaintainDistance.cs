using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class MaintainDistance : BaseAction
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

        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            values.Position = state.Actor.transform.position;
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var target = state.GetPosition(ref Position);

            var moveDirection = state.Actor.MoveDirection;

            Vector3 facing;
            if (state.GetFacing(ref Facing, moveDirection, out facing))
                actor.InputLook(actor.transform.position + facing * 100);

            var vector = target - actor.transform.position;
            var distance = vector.magnitude;
            var direction = vector / distance;

            var minDistance = state.Dereference(ref MinDistance).Float;
            var maxDistance = state.Dereference(ref MaxDistance).Float;
            var chargeDistance = state.Dereference(ref ChargeDistance).Float;

            if (distance > chargeDistance)
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
            else if (distance > maxDistance)
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
            else if (distance < minDistance)
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

            return AIResult.Hold();
        }
    }
}