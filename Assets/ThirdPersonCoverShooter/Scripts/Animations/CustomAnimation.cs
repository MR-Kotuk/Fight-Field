using UnityEngine;

namespace CoverShooter
{
    public class CustomAnimation : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            CharacterMotor.animatorToMotorMap[animator].SendMessage("OnFinishCustomAction");
        }
    }
}
