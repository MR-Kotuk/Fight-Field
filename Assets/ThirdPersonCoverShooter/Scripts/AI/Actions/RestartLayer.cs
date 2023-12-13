using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Layer")]
    [Immediate]
    public class RestartLayer : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            state.Layers[layer].ReInit();
            return new AIResult(AIResultType.Triggered);
        }
    }
}