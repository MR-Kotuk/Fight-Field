using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class Editable
    {
        /// <summary>
        /// Property label width of the node drawn inside the editor.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float LabelWidth;

        /// <summary>
        /// Property label width being adjusted during editor UI build.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float CurrentLabelWidth;

        /// <summary>
        /// Property width of the node drawn inside the editor.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float PropertyWidth;

        /// <summary>
        /// Property width being adjusted during editor UI build.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float CurrentPropertyWidth;

        public Editable()
        {
            LabelWidth = 100;
            PropertyWidth = 200;
        }

        public virtual void AdjustPropertyWidth(Brain brain)
        {
        }
    }
}