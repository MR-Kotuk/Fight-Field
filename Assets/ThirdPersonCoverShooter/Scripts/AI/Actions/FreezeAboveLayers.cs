using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Layer")]
    [Immediate]
    [Success("Done")]
    public class FreezeAboveLayers : BaseAction
    {        
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.Layers[layer].IsFreezingAboveLayers = true;
            state.Layers[layer].HasFreezeValue = true;

            return AIResult.Finish();
        }
    }
}