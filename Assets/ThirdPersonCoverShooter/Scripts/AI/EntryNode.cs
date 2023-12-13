using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class EntryNode : TriggeredNode, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Trigger executed when possible.
        /// </summary>
        public int Init;

        public EntryNode()
            : base()
        {
        }

        public EntryNode(Brain brain)
            : base()
        {
            Init = brain.AddNodeTrigger("Init");
        }

        public override bool ContainsTrigger(int id)
        {
            if (Init == id)
                return true;

            return base.ContainsTrigger(id);
        }
    }
}