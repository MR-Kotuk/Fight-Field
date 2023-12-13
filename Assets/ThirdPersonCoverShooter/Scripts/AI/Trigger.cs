using System;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public struct TriggerValue
    {
        /// <summary>
        /// Name of the trigger value.
        /// </summary>
        public string Name;

        /// <summary>
        /// Type of the trigger value.
        /// </summary>
        public ValueType Type;

        public TriggerValue(string name, ValueType type)
        {
            Name = name;
            Type = type;
        }
    }

    [Serializable]
    public struct TriggerReference
    {
        /// <summary>
        /// ID of the referenced trigger.
        /// </summary>
        public int ID;

        public TriggerReference(int id)
        {
            ID = id;
        }
    }

    [Serializable]
    public class Trigger
    {
        /// <summary>
        /// Name of the trigger.
        /// </summary>
        public string Name;

        /// <summary>
        /// Types of values used by the trigger.
        /// </summary>
        public TriggerValue[] Values;

        public void AddValue(string name, ValueType type)
        {
            if (Values == null)
                Values = new TriggerValue[1] { new TriggerValue(name, type) };
            else
            {
                var old = Values;
                Values = new TriggerValue[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Values[i] = old[i];

                Values[old.Length] = new TriggerValue(name, type);
            }
        }

        public void RemoveValueAt(int index)
        {
            if (Values == null || index < 0 || index >= Values.Length)
                return;

            var old = Values;
            Values = new TriggerValue[old.Length - 1];

            if (index > 0)
                for (int i = 0; i < index; i++)
                    Values[i] = old[i];

            if (index + 1 < old.Length)
                for (int i = index + 1; i < old.Length; i++)
                    Values[i - 1] = old[i];

            return;
        }
    }
}