using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Attack")]
    [Success("Attack")]
    [AllowMove]
    [AllowCrouch]    
    public class ThrowGrenade : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value Target = new Value(Vector3.zero);

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            if (!values.IsThrowing)
            {
                state.Actor.InputThrowGrenade(state.GetPosition(ref Target));
                values.IsThrowing = true;
                return AIResult.Hold();
            }
            else if (state.Actor.IsThrowing)
                return AIResult.Hold();
            else
                return AIResult.Success();
        }
    }
}