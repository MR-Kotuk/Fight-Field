using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [Success("Done")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class MoveTo : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Walk);

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

            float speed = 1;

            switch (state.Dereference(ref Speed).Speed)
            {
                case CharacterSpeed.Walk: speed = 0.5f; break;
                case CharacterSpeed.Run: speed = 1.0f; break;
                case CharacterSpeed.Sprint: speed = 2.0f; break;
            }

            var direction = state.Actor.MoveDirection;

            Vector3 facing;
            if (state.GetFacing(ref Facing, direction, out facing))
                actor.InputLook(actor.transform.position + facing * 100);

            actor.InputMoveTo(target, speed);

            var vector = actor.transform.position - target;

            if (vector.y > 1 || vector.y < -1)
                return AIResult.Hold();

            vector.y = 0;

            if (vector.magnitude < 0.3f)
                return AIResult.Finish();
            else
                return AIResult.Hold();
        }
    }
}