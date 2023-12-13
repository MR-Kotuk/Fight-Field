using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class TriggeredNode : LayerNode
    {
        /// <summary>
        /// Trigger IDs.
        /// </summary>
        public int[] Triggers;

        public TriggeredNode()
            : base()
        {
            Triggers = new int[0];
        }

        public TriggeredNode(object attachment)
            : base(attachment)
        {
            Triggers = new int[0];
        }

        public override void AdjustPropertyWidth(Brain brain)
        {
            base.AdjustPropertyWidth(brain);

            if (Triggers == null)
                return;

            for (int i = 0; i < Triggers.Length; i++)
            {
                var trigger = brain.GetNodeTrigger(Triggers[i]);

                if (trigger != null && trigger.PropertyWidth > PropertyWidth)
                    PropertyWidth = trigger.PropertyWidth;

            }
        }

        public virtual bool ContainsTrigger(int id)
        {
            if (Triggers == null)
                return false;

            for (int i = 0; i < Triggers.Length; i++)
                if (Triggers[i] == id)
                    return true;

            return false;
        }

        public virtual void ClearSaves(Brain brain)
        {
            if (Triggers != null)
                for (int i = 0; i < Triggers.Length; i++)
                {
                    var trigger = brain.GetNodeTrigger(Triggers[i]);

                    if (trigger != null)
                        trigger.ClearSaves();
                }
        }

        /// <summary>
        /// Add a trigger ID to the array of triggers. Returns it's index.
        /// </summary>
        public int AddTrigger(int id)
        {
            if (Triggers == null)
                Triggers = new int[1] { id };
            else
            {
                var old = Triggers;
                Triggers = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Triggers[i] = old[i];

                Triggers[old.Length] = id;
            }

            return Triggers.Length - 1;
        }

        /// <summary>
        /// Removes trigger at a certain index.
        /// </summary>
        public void RemoveTriggerAt(int index)
        {
            if (Triggers == null || Triggers.Length == 0)
                return;

            var old = Triggers;
            Triggers = new int[old.Length - 1];

            if (index > 0)
                for (int i = 0; i < index; i++)
                    Triggers[i] = old[i];

            if (index + 1 < old.Length)
                for (int i = index + 1; i < old.Length; i++)
                    Triggers[i - 1] = old[i];
        }

        /// <summary>
        /// Clears value references to the certain id.
        /// </summary>
        public override void ClearValue(int id)
        {
            base.ClearValue(id);

            var isStillRemoving = true;

            while (isStillRemoving)
            {
                isStillRemoving = false;

                if (Triggers != null)
                    for (int i = 0; i < Triggers.Length; i++)
                        if (Triggers[i] == id)
                        {
                            RemoveTriggerAt(i);
                            isStillRemoving = true;
                            break;
                        }
            }
        }
    }
}