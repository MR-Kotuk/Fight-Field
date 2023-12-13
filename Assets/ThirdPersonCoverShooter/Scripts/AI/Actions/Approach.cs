using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Move")]
    [Success("Done")]
    [AllowAimAndFire]
    [AllowCrouch]
    public class Approach : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        [ValueType(ValueType.Float)]
        public Value StopDistance = new Value(0f);

        [ValueType(ValueType.Float)]
        public Value WalkDistance = new Value(6f);

        [ValueType(ValueType.Float)]
        public Value CoverSideDistance = new Value(3f);

        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            UpdateApproachPosition(state, ref values, state.GetPosition(ref Position));
        }

        public void UpdateApproachPosition(State state, ref ActionState values, Vector3 target)
        {
            values.Position = target;
            values.ApproachPosition = values.Position;

            var foundCount = Physics.OverlapSphereNonAlloc(values.Position, 1, Util.Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;
                var cover = CoverSearch.GetCover(coverObject);

                if (cover == null)
                    continue;

                if (!cover.IsInFront(values.Position))
                    continue;

                var hasLeft = cover.LeftAdjacent == null && cover.OpenLeft;
                var hasRight = cover.RightAdjacent == null && cover.OpenRight;

                if (!hasLeft && !hasRight)
                    continue;

                var leftCorner = cover.LeftCorner(0);
                var rightCorner = cover.RightCorner(0);

                AIUtil.Path(ref state.Path, state.Actor.transform.position, leftCorner);
                var leftDistance = AIUtil.DistanceOf(state.Path);

                AIUtil.Path(ref state.Path, state.Actor.transform.position, rightCorner);
                var rightDistance = AIUtil.DistanceOf(state.Path);

                var corner = Vector3.zero;
                var vector = Vector3.zero;

                if (hasLeft && hasRight)
                {
                    if (leftDistance < rightDistance)
                    {
                        corner = leftCorner;
                        vector = cover.Left;
                    }
                    else
                    {
                        corner = rightCorner;
                        vector = cover.Right;
                    }

                }
                else if (hasLeft)
                {
                    if (leftDistance < rightDistance)
                    {
                        corner = leftCorner;
                        vector = cover.Left;
                    }
                    else
                        continue;
                }
                else if (hasRight)
                {
                    if (rightDistance < leftDistance)
                    {
                        corner = rightCorner;
                        vector = cover.Right;
                    }
                    else
                        continue;
                }

                var check = corner + vector * state.Dereference(ref CoverSideDistance).Float;
                AIUtil.GetClosestStandablePosition(ref check);

                if (!AIUtil.IsObstructed(check + Vector3.up * 2, values.Position + Vector3.up * 2))
                {
                    values.ApproachCover = cover;
                    values.ApproachPosition = check;
                    break;
                }
                else
                {
                    check = corner + vector * state.Dereference(ref CoverSideDistance).Float * 0.5f;
                    AIUtil.GetClosestStandablePosition(ref check);

                    if (!AIUtil.IsObstructed(check + Vector3.up * 2, values.Position + Vector3.up * 2))
                    {
                        values.ApproachCover = cover;
                        values.ApproachPosition = check;
                        break;
                    }
                }
            }
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var target = state.GetPosition(ref Position);

            if (Vector3.Distance(target, values.Position) >= 0.5f)
                UpdateApproachPosition(state, ref values, target);

            var actor = state.Actor;
            var distance = Vector3.Distance(actor.transform.position, values.ApproachPosition);

            float speed;

            if (distance <= state.Dereference(ref WalkDistance).Float)
                speed = 0.5f;
            else
                speed = 1.0f;

            var direction = state.Actor.MoveDirection;

            Vector3 facing;
            if (state.GetFacing(ref Facing, direction, out facing))
                actor.InputLook(actor.transform.position + facing * 100);

            actor.InputMoveTo(values.ApproachPosition, speed);

            if (values.ApproachCover != null)
            {
                if (distance <= 0.5f)
                {
                    values.ApproachCover = null;
                    values.ApproachPosition = target;
                }

                return AIResult.Hold();
            }
            else if (distance <= state.Dereference(ref StopDistance).Float + 0.1f)
                return AIResult.Finish();
            else
                return AIResult.Hold();
        }
    }
}