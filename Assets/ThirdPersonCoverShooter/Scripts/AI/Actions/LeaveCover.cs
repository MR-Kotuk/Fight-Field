using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Cover")]
    [Success("Done")]
    [Immediate]
    public class LeaveCover : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.Actor.InputLeaveCover();
            return AIResult.Finish();
        }
    }
}