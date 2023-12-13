using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Aim")]
    [Success("Done")]
    [AllowMove]
    [AllowCrouch]
    public class TurnTowards : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Position = new Value(Vector3.zero);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var direction = (state.GetPosition(ref Position) - actor.transform.position).normalized;

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