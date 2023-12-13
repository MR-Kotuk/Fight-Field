using System;
using UnityEngine;

namespace CoverShooter.AI
{
    public enum VariableClass
    {
        Listed,
        Visible,
        Parameter
    }

    [Serializable]
    public class Variable
    {
        /// <summary>
        /// Name of the variable.
        /// </summary>
        public string Name;

        /// <summary>
        /// Value of the variable.
        /// </summary>
        public Value Value;

        /// <summary>
        /// Variable class, is it listed in the variable list or visible in the inspector.
        /// </summary>
        public VariableClass Class;

        /// <summary>
        /// Position inside the editor if the variable is a trigger parameter.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public Rect LocalEditorRect;

        /// <summary>
        /// Position inside the editor if the variable is a trigger parameter.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public Rect ScreenEditorRect;

        /// <summary>
        /// Action node ID that owns this variable.
        /// </summary>
        [HideInInspector]
        public int OwningNode;

        /// <summary>
        /// Cached value whether the variable used as a parameter has a connection output. Used for visualization.
        /// </summary>
        [HideInInspector]
        public bool HasConnection;
    }

    [Serializable]
    public struct VariableReference
    {
        /// <summary>
        /// ID of the referenced variable.
        /// </summary>
        public int ID;

        public VariableReference(int id)
        {
            ID = id;
        }
    }
}