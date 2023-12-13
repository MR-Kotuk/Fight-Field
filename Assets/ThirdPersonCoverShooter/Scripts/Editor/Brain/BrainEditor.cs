using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using CoverShooter.AI;

namespace CoverShooter
{
    [CustomEditor(typeof(Brain))]
    public class BrainEditor : EditorWindow
    {
        /// <summary>
        /// Currently edited brain.
        /// </summary>
        public Brain Brain;

        /// <summary>
        /// Currently selected controller.
        /// </summary>
        public AIController Controller;

        private bool _needsRepaint;
        private HorizontalSplitView _horizontalSplit = new HorizontalSplitView();
        private VerticalSplitView _verticalSplit = new VerticalSplitView();

        private int _activeLayer;
        private int _superNode;

        private BrainEditorArea _area = new BrainEditorArea();
        private BrainEditorPanel _panel = new BrainEditorPanel();
        private BrainRenamer _renamer = new BrainRenamer();

        [MenuItem("Window/Brain")]
        static void Init()
        {
            var window = GetWindow<BrainEditor>(false, "Brain", true);
            window.minSize = new Vector2(600.0f, 300.0f);

            window.Show();
        }

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            string assetPath = AssetDatabase.GetAssetPath(instanceID);
            Brain brain = AssetDatabase.LoadAssetAtPath<Brain>(assetPath);

            if (brain != null)
            {
                BrainEditor window = (BrainEditor)GetWindow(typeof(BrainEditor));
                window.Brain = brain;
                window.Show();

                return true;
            }

            return false;
        }

        [MenuItem("Assets/Create/Brain")]
        private static void Create()
        {
            var folder = AssetDatabase.GetAssetPath(Selection.activeObject);

            string path;

            if (folder == null)
                path = "New Brain.brain";
            else
                path = Path.Combine(folder, "New Brain.brain");

            int counter = 0;

            while (File.Exists(path))
            {
                counter++;

                var name = "New Brain (" + counter + ").brain";

                if (folder == null)
                    path = name;
                else
                    path = Path.Combine(folder, name);
            }

            var brain = new Brain();
            brain.AddLayer("Base");

            File.WriteAllText(path, JsonUtility.ToJson(brain, true));
            AssetDatabase.ImportAsset(path);
        }

        private void OnEnable()
        {
            OnSelectionChange();
        }

        private void OnDisable()
        {
            State.EditorEditedState = null;
        }

        private void OnDestroy()
        {
            State.EditorEditedState = null;
        }

        private void OnGUI()
        {
            var previousSkin = GUI.skin;
            GUI.skin = BrainEditorUtil.Skin;

            var totalArea = new Rect(-400, -400, 800, 800);

            if (Controller != null && Controller.Brain != Brain)
                Controller = null;

            State.EditorEditedState = Controller != null ? Controller.State : null;

            //wantsMouseMove = _connectionMode != ConnectionMode.none;
            wantsMouseMove = true;

            if (Brain != null)
            {
                Undo.RecordObject(Brain, "Edit brain");

                //Brain.CollectGarbage();

                while (true)
                {
                    if (_superNode <= 0)
                    {
                        _superNode = 0;
                        break;
                    }

                    var node = Brain.GetAction(_superNode);

                    if (node == null)
                    {
                        _superNode = 0;
                        break;
                    }
                    else if (node.Action != null)
                        _superNode = node.Parent;
                    else
                        break;
                }

                foreach (var trigger in Brain.NodeTriggers.Values)
                    if (trigger.Type == NodeTriggerType.Custom)
                        checkCustomTrigger(trigger);
            }

            if (_activeLayer < 0)
                _activeLayer = 0;
            else if (Brain != null && _activeLayer >= Brain.Layers.Length && Brain.Layers.Length > 0)
                _activeLayer = Brain.Layers.Length - 1;

            _horizontalSplit.Begin(350, float.MaxValue);
            {
                var previousActiveLayer = _activeLayer;

                if (_panel.Display(Brain, Controller, _renamer, ref _activeLayer))
                    _needsRepaint = true;

                if (Event.current.type == EventType.MouseDown || focusedWindow != this)
                {
                    _renamer.EndRename(Brain);
                    _needsRepaint = true;
                }

                if (previousActiveLayer != _activeLayer)
                {
                    _superNode = 0;
                    _area.UpdateArea(Brain, _activeLayer, _superNode, position);
                }

                _horizontalSplit.Split(true);

                _verticalSplit.Begin(24, 24);
                {
                    GUILayout.BeginHorizontal();

                    if (Brain != null)
                    {
                        superNodeButton(0, Brain.Layers[_activeLayer].Name);

                        var id = Brain.GetRootActionId(_superNode);

                        while (id > 0)
                        {
                            GUILayout.Label(">", GUILayout.Width(16));

                            superNodeButton(id, Brain.GetAction(id).Name);
                            id = Brain.GetRootActionIdTill(_superNode, id);
                        }
                    }

                    GUILayout.EndHorizontal();

                    _verticalSplit.Split(false);
                }
                _verticalSplit.End();
            }
            _horizontalSplit.End();

            var previousSuperNode = _superNode;

            {
                GUI.EndGroup();
                var clippedArea = new Rect(_horizontalSplit.RightArea.x,
                                           _verticalSplit.BottomArea.y,
                                           _horizontalSplit.RightArea.width,
                                           _verticalSplit.BottomArea.height);

                {
                    var pivot = new Vector2(clippedArea.x, clippedArea.y);
                    var scaleInv = 1.0f / _area.Scale;

                    clippedArea.x -= pivot.x;
                    clippedArea.y -= pivot.y;
                    clippedArea.xMin *= scaleInv;
                    clippedArea.xMax *= scaleInv;
                    clippedArea.yMin *= scaleInv;
                    clippedArea.yMax *= scaleInv;
                    clippedArea.x += pivot.x;
                    clippedArea.y += pivot.y;
                }

                clippedArea.y += 21;
                GUI.BeginGroup(clippedArea);

                var previousMatrix = GUI.matrix;
                Matrix4x4 translation = Matrix4x4.TRS(new Vector3(clippedArea.xMin, clippedArea.yMin, 0), Quaternion.identity, Vector3.one);
                Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(_area.Scale, _area.Scale, 1.0f));
                GUI.matrix = translation * scaleMatrix * translation.inverse * GUI.matrix;

                var controller = Controller;

                if (controller != null && controller.Brain != Brain)
                    controller = null;

                if (_area.Display(this, controller, _renamer, new Rect(0, 0, clippedArea.width, clippedArea.height), Brain, _activeLayer, ref _superNode))
                    _needsRepaint = true;

                GUI.matrix = previousMatrix;
                GUI.EndGroup();
                GUI.BeginClip(new Rect(0, 0, Screen.width, Screen.height));
            }

            if (_superNode != previousSuperNode)
                _area.UpdateArea(Brain, _activeLayer, _superNode, position);

            if (Brain != null)
                Brain.RebuildSaveDatabase();

            if (_needsRepaint)
            {
                GUI.changed = true;
                Repaint();
            }

            GUI.skin = previousSkin;
        }

        private void superNodeButton(int id, string name)
        {
            float min;
            float max;
            GUI.skin.button.CalcMinMaxWidth(new GUIContent(name), out min, out max);

            if (GUILayout.Button(name, GUILayout.Width(max + 15)))
            {
                _superNode = id;
                _area.UpdateArea(Brain, _activeLayer, _superNode, position);
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject != null)
            {
                if (Selection.activeObject is AIController)
                {
                    Controller = (AIController)Selection.activeObject;
                }
                else if (Selection.activeObject is GameObject)
                {
                    var ai = ((GameObject)Selection.activeObject).GetComponent<AIController>();

                    if (ai != null)
                        Controller = ai;
                    else
                        Controller = null;
                }
                else
                    Controller = null;
            }
            else
                Controller = null;

            Repaint();
        }

        private void checkCustomTrigger(NodeTrigger trigger)
        {
            if (trigger == null || trigger.Type != NodeTriggerType.Custom)
                return;

            var source = Brain.GetTrigger(trigger.ID);

            if (source == null)
                return;

            if (source.Values == null || source.Values.Length == 0)
            {
                if (trigger.Values != null)
                    for (int i = 0; i < trigger.Values.Length; i++)
                        Brain.RemoveVariable(trigger.Values[i]);

                trigger.Values = null;
                return;
            }

            if (trigger.Values == null || trigger.Values.Length != source.Values.Length)
            {
                var old = trigger.Values;
                trigger.Values = new int[source.Values.Length];

                if (old != null)
                {
                    if (old.Length > trigger.Values.Length)
                    {
                        for (int i = trigger.Values.Length; i < old.Length; i++)
                            Brain.RemoveVariable(old[i]);

                        for (int i = 0; i < trigger.Values.Length; i++)
                            trigger.Values[i] = old[i];
                    }
                    else
                        for (int i = 0; i < old.Length; i++)
                            trigger.Values[i] = old[i];
                }
            }

            for (int i = 0; i < trigger.Values.Length; i++)
            {
                var variable = Brain.GetVariable(trigger.Values[i]);

                if (variable == null)
                    trigger.Values[i] = Brain.AddVariable(source.Values[i].Type, source.Values[i].Name, VariableClass.Parameter);
                else
                {
                    variable.Value.Type = source.Values[i].Type;
                    variable.Name = source.Values[i].Name;
                }
            }
        }
}
}