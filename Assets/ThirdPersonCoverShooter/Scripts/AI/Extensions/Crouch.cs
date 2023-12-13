using UnityEngine;

namespace CoverShooter.AI
{
    [Extension(ExtensionClass.Crouch)]
    public class CrouchExtension : BaseExtension
    {
        public override void Update(State state, int layer, ref ExtensionState values)
        {
            state.Actor.InputCrouch();
        }
    }
}