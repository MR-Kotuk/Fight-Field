using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Layer")]
    [Immediate]
    [Success("Done")]
    public class UnfreezeAboveLayers : BaseAction
    {        
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.Layers[layer].IsFreezingAboveLayers = false;
            state.Layers[layer].HasFreezeValue = true;

            return AIResult.Finish();
        }
    }
}