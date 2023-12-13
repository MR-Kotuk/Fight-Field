using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [Success("Done")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class MoveToRandomAgainst : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value SightGround;

        [ValueType(ValueType.Boolean)]
        public Value MaintainSight = new Value(true);

        [ValueType(ValueType.Float)]
        public Value SightDistance = Value.Variable(ValueType.Float, -(int)BuiltinValue.ViewDistance);

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(2);

        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(10);

        [ValueType(ValueType.Float)]
        public Value AvoidRange = new Value(2);

        [ValueType(ValueType.Speed)]
        public Value Speed = new Value(CharacterSpeed.Walk);

        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            FindNew(ref state, ref values);
        }

        public bool FindNew(ref State state, ref ActionState values)
        {
            var min = state.Dereference(ref MinDistance).Float;
            var max = state.Dereference(ref MaxDistance).Float;
            var avoidRange = state.Dereference(ref AvoidRange).Float;
            var sight = state.GetPosition(ref SightGround);
            var maintainSight = state.Dereference(ref MaintainSight).Bool;
            var sightDistance = state.Dereference(ref SightDistance).Float;

            values.Position = state.Actor.transform.position;

            for (int i = 0; i < 32; i++)
            {
                var angle = UnityEngine.Random.Range(0, 360);
                var distance = UnityEngine.Random.Range(min, max);

                var target = state.Actor.transform.position + Util.HorizontalVector(angle) * distance;

                if (maintainSight && Vector3.Distance(target, sight) >= sightDistance)
                    continue;

                if (!AIUtil.GetClosestStandablePosition(ref target))
                    continue;

                if (maintainSight && AIUtil.IsObstructed(target + Vector3.up * 2, sight + Vector3.up * 2))
                    continue;

                if (IsBusy(state.Actor, target, avoidRange))
                    continue;

                if (state.Actor.CanMoveTo(target))
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
            var actor = state.Actor;
            var target = values.Position;
            var avoidRange = state.Dereference(ref AvoidRange).Float;

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