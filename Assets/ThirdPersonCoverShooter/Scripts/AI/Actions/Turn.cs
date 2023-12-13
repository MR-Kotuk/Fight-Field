using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Aim")]
    [Success("Done")]
    [AllowMove]
    [AllowCrouch]
    public class Turn : BaseAction
    {
        [ValueType(ValueType.RelativeDirection)]
        [ValueType(ValueType.Vector3)]
        public Value Direction = new Value(CoverShooter.Direction.Forward);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var direction = state.GetDirection(ref Direction);

            if (Vector3.Dot((actor.BodyLookTarget - actor.transform.position).normalized, direction) > 0.99f)
                return AIResult.SuccessOrHold();
            else
            {
                actor.InputLook(actor.transform.position + direction * 100);
                return AIResult.Hold();
            }
        }
    }
}