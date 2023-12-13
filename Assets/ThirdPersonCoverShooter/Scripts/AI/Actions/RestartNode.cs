using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Layer")]
    [Immediate]
    public class RestartNode : BaseAction
    {
        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var node = state.Brain.GetAction(state.Layers[layer].CurrentNode);

            if (node.Parent <= 0)
                state.Layers[layer].ReInit();
            else
                state.Go(layer, node.Parent);

            return new AIResult(AIResultType.Triggered);
        }
    }
}