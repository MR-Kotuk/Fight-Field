using UnityEngine;

namespace CoverShooter.AI
{
    [Failure("None")]
    [AllowAimAndFire]
    [AllowCrouch]
    [Folder("Complex")]
    public class FollowWaypoints : BaseAction
    {
        [ValueType(ValueType.Facing)]
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Facing = new Value(CharacterFacing.None);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            values.Index = -1;
            values.IsWaiting = false;
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            if (values.Waypoints == null)
                values.Waypoints = state.Actor.GetComponent<Waypoints>();

            var w = values.Waypoints;

            if (w == null || w.Points == null || w.Points.Length == 0)
                return AIResult.Failure();

            if (values.Index < 0 || values.Index >= w.Points.Length)
                values.IsWaiting = false;

            var actor = state.Actor;
            var position = actor.transform.position;

            if (values.IsWaiting)
            {
                values.Time += Time.deltaTime;

                if (w.Points[values.Index].Pause <= values.Time)
                {
                    values.Index = (values.Index + 1) % w.Points.Length;
                    values.IsWaiting = false;
                    values.Time = 0;
                }
            }
            else
            {
                if (values.Index < 0 || values.Index >= w.Points.Length)
                {
                    values.Index = 0;
                    var dist = Vector3.Distance(position, w.Points[0].Position);

                    for (int i = 1; i < w.Points.Length; i++)
                    {
                        var current = Vector3.Distance(position, w.Points[i].Position);

                        if (current < dist)
                        {
                            dist = current;
                            values.Index = i;
                        }
                    }
                }

                var moveTo = true;

                if (Vector3.Distance(position, w.Points[values.Index].Position) < 0.65f)
                {
                    if (w.Points[values.Index].Pause > 1f / 60f || w.Points.Length == 1)
                    {
                        values.IsWaiting = true;
                        moveTo = false;
                    }
                    else
                        values.Index = (values.Index + 1) % w.Points.Length;
                }

                if (moveTo)
                {
                    var direction = w.Points[values.Index].Position - position;
                    direction.y = 0;
                    direction.Normalize();

                    var speed = w.Points[values.Index].Run ? 1 : 0.5f;

                    Vector3 facing;      
                    if (state.GetFacing(ref Facing, direction, out facing))
                        actor.InputLook(actor.transform.position + facing * 100);

                    actor.InputMoveTo(w.Points[values.Index].Position, speed);
                }
            }

            return AIResult.Hold();
        }
    }
}