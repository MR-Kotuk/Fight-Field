using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(AIController))]
    public class AIControllerEditor : Editor
    {
        private List<int> _toBeRemoved = new List<int>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var controller = (AIController)target;

            if (controller.Brain == null)
                return;

            Undo.RecordObject(controller, "Edit AI values");

            if (!Application.isPlaying)
            {
                if (controller.State != null &&
                    ((controller.State.Brain != null && controller.State.Brain != controller.Brain) ||
                     (controller.State.Layers != null && controller.State.Layers.Length > 0)))
                    controller.State = new AI.State();
            }

            if (_toBeRemoved == null)
                _toBeRemoved = new List<int>();
            else
                _toBeRemoved.Clear();

            foreach (var id in controller.State.Values.Keys)
                if (!controller.Brain.Variables.ContainsKey(id) ||
                    controller.Brain.Variables[id].Class != AI.VariableClass.Visible)
                    _toBeRemoved.Add(id);

            foreach (var id in controller.Brain.Variables.Keys)
            {
                var variable = controller.Brain.Variables[id];

                if (variable.Class != AI.VariableClass.Visible)
                    continue;

                var wasAlreadyContained = controller.State.Values.ContainsKey(id);
                var value = wasAlreadyContained ? controller.State.Values[id] : variable.Value;

                var newValue = value;

                GUILayout.BeginHorizontal();
                GUILayout.Label(variable.Name, GUILayout.Width(EditorGUIUtility.labelWidth));
                BrainEditorUtil.EditValue(ref newValue, true);
                GUILayout.EndHorizontal();

                if ((!wasAlreadyContained && !newValue.IsEqual(ref variable.Value)) || !newValue.IsEqual(ref value))
                    controller.State.Values[id] = newValue;
            }

            if (!Application.isPlaying)
                for (int i = 0; i < _toBeRemoved.Count; i++)
                    controller.State.Values.Remove(_toBeRemoved[i]);
        }
    }
}
