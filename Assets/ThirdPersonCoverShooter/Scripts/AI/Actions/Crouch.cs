using UnityEngine;

namespace CoverShooter.AI
{
    [AllowAimAndFire]
    [AllowMove]
    public class Crouch : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.Actor.InputCrouch();

            return AIResult.Hold();
        }
    }
}