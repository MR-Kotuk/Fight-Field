using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Cover")]
    [Success("Done")]
    [Failure("Failed")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class TakeCover : BaseAction
    {
        [ValueType(ValueType.GameObject)]
        public Value Cover;

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Walk);

        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var coverObject = state.Dereference(ref Cover).GameObject;

            if (coverObject == null)
                return AIResult.Failure();

            var cover = CoverSearch.GetCover(coverObject);

            if (cover == null)
                return AIResult.Failure();

            float speed = 1;

            switch (state.Dereference(ref Speed).Speed)
            {
                case CharacterSpeed.Walk: speed = 0.5f; break;
                case CharacterSpeed.Run: speed = 1.0f; break;
                case CharacterSpeed.Sprint: speed = 2.0f; break;
            }

            var target = cover.ClosestPointTo(actor.transform.position, 0.1f, 0.1f);
            var distance = Vector3.Distance(actor.transform.position, target);

            if (values.HasStarted)
            {
                values.Time += Time.deltaTime;

                if (values.Time >= 1)
                    return AIResult.Failure();
            }

            if (distance < 0.7f)
            {
                if (!values.HasStarted)
                {
                    values.HasStarted = true;
                    values.Time = 0;
                }

                actor.InputTakeCover();
            }
            else
                values.HasStarted = false;

            if (actor.Cover == cover)
                return AIResult.Finish();

            actor.InputMoveTo(target, speed);

            var direction = state.Actor.MoveDirection;

            Vector3 facing;
            if (state.GetFacing(ref Facing, direction, out facing))
                actor.InputLook(actor.transform.position + facing * 100);

            return AIResult.Hold();
        }
    }
}