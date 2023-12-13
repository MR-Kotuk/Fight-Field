using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [Success("Done")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class MoveFrom : BaseAction
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

            var direction = (actor.transform.position - state.GetPosition(ref Position)).normalized;
            values.Covered += Vector3.Dot(direction, actor.transform.position - values.Position);
            values.Position = actor.transform.position;

            float speed = 1;

            switch (state.Dereference(ref Speed).Speed)
            {
                case CharacterSpeed.Walk: speed = 0.5f; break;
                case CharacterSpeed.Run: speed = 1.0f; break;
                case CharacterSpeed.Sprint: speed = 2.0f; break;
            }

            Vector3 facing;
            if (state.GetFacing(ref Facing, direction, out facing))
                actor.InputLook(actor.transform.position + facing * 100);

            actor.InputMovement(new CharacterMovement(direction, speed));

            return AIResult.Hold();
        }
    }
}