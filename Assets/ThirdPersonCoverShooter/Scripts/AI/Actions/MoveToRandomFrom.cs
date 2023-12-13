using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [Success("Done")]
    [Failure("Failure")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class MoveToRandomFrom : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(2);

        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(10);

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Run);

        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public bool FindNew(ref State state, ref ActionState values)
        {
            var avoidTarget = state.GetPosition(ref Target);
            var min = state.Dereference(ref MinDistance).Float;
            var max = state.Dereference(ref MaxDistance).Float;

            var currentDistance = Vector3.Distance(avoidTarget, state.Actor.transform.position);
            var avoidDistance = min < currentDistance ? min : currentDistance;

            values.Position = state.Actor.transform.position;

            for (int i = 0; i < 32; i++)
            {
                var angle = UnityEngine.Random.Range(0, 360);
                var distance = UnityEngine.Random.Range(min, max);

                var target = state.Actor.transform.position + Util.HorizontalVector(angle) * distance;

                if (!AIUtil.GetClosestStandablePosition(ref target))
                    continue;

                if (state.Actor.CanMoveToAvoiding(target, avoidTarget, avoidDistance))
                {
                    values.Position = target;
                    return true;
                }
            }

            return false;
        }

        public bool IsBusy(BaseActor actor, Vector3 position, float avoidRange)
        {
            if (avoidRange > 0.01f && AIUtil.FindActors(position, avoidRange, actor) > 0)
                return true;
            else
                return false;
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (!values.HasStarted)
            {
                if (!FindNew(ref state, ref values))
                    return AIResult.Failure();

                values.HasStarted = true;
            }

            var actor = state.Actor;
            var target = values.Position;

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