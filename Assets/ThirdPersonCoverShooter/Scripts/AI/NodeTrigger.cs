using System;
using UnityEngine;

namespace CoverShooter.AI
{
    public enum NodeTriggerType
    {
        Basic,
        Expression,
        Custom,
        Event
    }

    [Serializable]
    public class NodeTrigger : Editable
    {
        /// <summary>
        /// Name of the trigger.
        /// </summary>
        public string Name;

        /// <summary>
        /// Target node of the connection.
        /// </summary>
        public int Target;

        /// <summary>
        /// Owning node of the trigger.
        /// </summary>
        public int Owner;

        /// <summary>
        /// Position of the start of the curve inside the editor.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public Rect LocalEditorRect;

        /// <summary>
        /// Value used by the editor when debugging to show which triggers were recently activated.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float EditorDebugValue;

        /// <summary>
        /// Type of the trigger.
        /// </summary>
        public NodeTriggerType Type;

        /// <summary>
        /// ID of custom trigger.
        /// </summary>
        public int ID;

        /// <summary>
        /// AI event in case it's event trigger.
        /// </summary>
        public AIEvent Event;

        /// <summary>
        /// Expression used to determine if trigger succeeds.
        /// </summary>
        [NoConstant]
        public Value Expression;

        /// <summary>
        /// Value of a boolean expression for the trigger to happen.
        /// </summary>
        public bool ExpressionValue;

        /// <summary>
        /// Expression ID outputs used by the trigger.
        /// </summary>
        public int[] Values;

        /// <summary>
        /// SaveValue expressions using this trigger.
        /// </summary>
        public int[] Saves;

        public bool IsUsingVariable(int id)
        {
            if (Type == NodeTriggerType.Expression && Expression.ID == id)
                return true;
            else
                return false;
        }

        public void ClearSaves()
        {
            Saves = null;
        }

        public void AddSave(int save)
        {
            if (Saves == null)
                Saves = new int[1] { save };
            else
            {
                for (int i = 0; i < Saves.Length; i++)
                    if (Saves[i] == save)
                        return;

                var old = Saves;
                Saves = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Saves[i] = old[i];

                Saves[old.Length] = save;
            }
        }

        public bool ContainsValue(int id)
        {
            if (Values == null)
                return false;

            for (int i = 0; i < Values.Length; i++)
                if (Values[i] == id)
                    return true;

            return false;
        }

        public void AddValue(Brain brain, string name, ValueType type)
        {
            var id = brain.AddVariable(type, name, VariableClass.Parameter);

            if (Values == null)
                Values = new int[1] { id };
            else
            {
                var old = Values;
                Values = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Values[i] = old[i];

                Values[old.Length] = id;
            }
        }

        public void RemoveValueAt(Brain brain, int index)
        {
            if (Values == null || index < 0 || index >= Values.Length)
                return;

            brain.RemoveVariable(Values[index]);

            var old = Values;
            Values = new int[old.Length - 1];

            if (index > 0)
                for (int i = 0; i < index; i++)
                    Values[i] = old[i];

            if (index + 1 < old.Length)
                for (int i = index + 1; i < old.Length; i++)
                    Values[i - 1] = old[i];
        }

        public void ClearValue(int id)
        {
            if (Target == id)
                Target = 0;

            if (Expression.ID == id)
                Expression.ID = 0;
        }

        public void ClearValues(Brain brain)
        {
            if (Values != null)
                for (int i = 0; i < Values.Length; i++)
                    brain.RemoveVariable(Values[i]);

            Values = null;
        }
    }
}