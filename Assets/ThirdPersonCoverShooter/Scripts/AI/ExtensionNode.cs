using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class ExtensionNode : LayerNode
    {
        public BaseExtension Extension { get { return (BaseExtension)Attachment; } }

        [HideInInspector]
        public int Owner;

        public ExtensionNode()
            : base()
        {
        }

        public ExtensionNode(BaseExtension extension)
            : base(extension)
        {
        }

        /// <summary>
        /// Called when the node is entered.
        /// </summary>
        public void Begin(State state, int layer, int id)
        {
            var extensionState = new ExtensionState();

            var extension = Extension;

            if (extension != null)
                extension.Begin(state, layer, ref extensionState);

            state.Layers[layer].SetState(id, extensionState);
        }

        /// <summary>
        /// Called when the node is exited.
        /// </summary>
        public void End(State state, int layer, int id)
        {
            var extension = Extension;

            if (extension != null)
            {
                var extensionState = state.Layers[layer].GetExtensionState(id);
                extension.End(state, layer, ref extensionState);
                state.Layers[layer].RemoveExtensionState(id);
            }
        }

        /// <summary>
        /// Called every frame when the node is active.
        /// </summary>
        public void Update(State state, int layer, int id)
        {
            var extension = Extension;

            if (extension != null)
            {
                var extensionState = state.Layers[layer].GetExtensionState(id);
                extension.Update(state, layer, ref extensionState);
                state.Layers[layer].SetState(id, extensionState);
            }
        }
    }
}