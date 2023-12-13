using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using CoverShooter.AI;

namespace CoverShooter
{
    public class BrainEditorPanel
    {
        private string[] _tabs = new string[] { "Layers", "Variables", "Triggers", "Instincts" };
        private int _tab;

        private int _potentialLayer;

        public bool Display(Brain brain, AIController controller, BrainRenamer renamer, ref int activeLayer)
        {
            if (controller != null)
                GUILayout.Box(controller.gameObject.name);

            if (brain != null)
            {
                if (GUILayout.Button("Save"))
                {
                    var path = AssetDatabase.GetAssetPath(brain);

                    if (path != null && path.Length > 0)
                    {
                        var text = JsonUtility.ToJson(brain, true);
                        File.WriteAllText(path, text);
                    }
                }
            }

            _tab = GUILayout.Toolbar(_tab, _tabs);

            switch (_tab)
            {
                case 0: return layersTab(brain, renamer, ref activeLayer);
                case 1: return variablesTab(brain, renamer, ref activeLayer);
                case 2: return triggersTab(brain, renamer, ref activeLayer);
                case 3: return instinctsTab(brain);
            }

            return false;
        }

        private bool layersTab(Brain brain, BrainRenamer renamer, ref int activeLayer)
        {
            var needsRepaint = false;

            if (brain != null && brain.Layers != null)
            {
                int layerToDelete = -1;
                int layerToMoveUp = -1;
                int layerToMoveDown = -1;

                if (Event.current.type == EventType.Repaint)
                    _potentialLayer = -1;

                for (int i = 0; i < brain.Layers.Length; i++)
                {
                    var layer = brain.Layers[i];

                    var oldBackground = GUI.backgroundColor;

                    if (activeLayer == i)
                        GUI.backgroundColor = new Color32(89, 137, 207, 255);

                    GUILayout.BeginVertical(BrainEditorUtil.Skin.box);

                    GUI.backgroundColor = oldBackground;

                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical();

                    var isSelected = activeLayer == i;
                    renamer.RenameLayerControl(brain, ref activeLayer, true, ref isSelected, layer.Name, i);

                    if (isSelected)
                        activeLayer = i;

                    GUI.enabled = i > 0;

                    GUILayout.BeginHorizontal();
                    layer.IsFreezingLayersAbove = GUILayout.Toggle(layer.IsFreezingLayersAbove, "Freeze abve layers");
                    GUILayout.Label("", GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    GUI.enabled = i > 0;

                    if (BrainEditorUtil.UpButton())
                        layerToMoveUp = i;

                    GUI.enabled = i < brain.Layers.Length - 1;
                    if (BrainEditorUtil.DownButton())
                        layerToMoveDown = i;

                    GUI.enabled = brain.Layers.Length > 1;
                    if (BrainEditorUtil.DeleteButton())
                        layerToDelete = i;

                    GUI.enabled = true;

                    GUILayout.EndHorizontal();
                    BrainEditorUtil.HorizontalSpace();
                    GUILayout.EndVertical();

                    if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                        _potentialLayer = i;

                    if (Event.current.type == EventType.MouseDown && i == _potentialLayer)
                    {
                        needsRepaint = true;
                        activeLayer = i;
                    }
                }

                if (layerToMoveUp >= 0)
                {
                    if (layerToMoveUp == activeLayer)
                        activeLayer--;

                    brain.MoveLayerUp(layerToMoveUp);
                    needsRepaint = true;
                }

                if (layerToMoveDown >= 0)
                {
                    if (layerToMoveUp == activeLayer)
                        activeLayer++;

                    brain.MoveLayerDown(layerToMoveDown);
                    needsRepaint = true;
                }

                if (layerToDelete >= 0)
                {
                    brain.RemoveLayer(layerToDelete);
                    if (activeLayer >= brain.Layers.Length && brain.Layers.Length > 0)
                        activeLayer = brain.Layers.Length - 1;
                    needsRepaint = true;
                }
            }

            if (brain != null)
                if (GUILayout.Button("Add"))
                    brain.AddLayer("New Layer");

            return needsRepaint;
        }

        private bool variablesTab(Brain brain, BrainRenamer renamer, ref int activeLayer)
        {
            var needsRepaint = false;

            if (brain != null && brain.Variables != null)
            {
                int variableToDelete = 0;

                foreach (int id in brain.Variables.Keys)
                {
                    var variable = brain.Variables[id];

                    if (variable.Class == VariableClass.Parameter)
                        continue;

                    GUILayout.BeginVertical(BrainEditorUtil.Skin.box);
                    GUILayout.BeginHorizontal();

                    renamer.RenameControl(brain, true, variable.Name, RenameTargetType.variable, id);

                    BrainEditorUtil.EditValueWithStyle(ref variable.Value, false);

                    if (BrainEditorUtil.DeleteButton())
                        variableToDelete = id;

                    GUILayout.EndHorizontal();

                    variable.Class = GUILayout.Toggle(variable.Class == VariableClass.Visible, "Display in inspector") ? VariableClass.Visible : VariableClass.Listed;

                    GUILayout.EndVertical();
                }

                if (variableToDelete > 0)
                {
                    if (!brain.IsUsingVariable(variableToDelete) ||
                        EditorUtility.DisplayDialog("Delete Variable",
                                                    "There are nodes using the variable. Do you really want to delete it?",
                                                    "Yes", "No"))
                    {
                        brain.RemoveVariable(variableToDelete);
                        needsRepaint = true;
                    }
                }
            }

            if (brain != null)
                if (GUILayout.Button("Add"))
                {
                    var menu = new GenericMenu();

                    foreach (AI.ValueType type in Enum.GetValues(typeof(AI.ValueType)))
                        if (type != AI.ValueType.Unknown)
                            menu.AddItem(new GUIContent(type.ToString()), false, () => brain.AddVariable(type, "New " + type.ToString(), VariableClass.Listed));

                    menu.ShowAsContext();
                }

            return needsRepaint;
        }

        private bool triggersTab(Brain brain, BrainRenamer renamer, ref int activeLayer)
        {
            var needsRepaint = false;

            if (brain != null && brain.Triggers != null)
            {
                int triggerToDelete = 0;

                foreach (int id in brain.Triggers.Keys)
                {
                    var trigger = brain.Triggers[id];

                    GUILayout.BeginVertical(BrainEditorUtil.Skin.box);
                    GUILayout.BeginHorizontal();

                    renamer.RenameControl(brain, true, trigger.Name, RenameTargetType.trigger, id);

                    if (BrainEditorUtil.DeleteButton())
                        triggerToDelete = id;

                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(BrainEditorUtil.Skin.box);

                    int valueToDelete = -1;

                    if (trigger.Values != null)
                    {
                        for (int i = 0; i < trigger.Values.Length; i++)
                        {
                            BrainEditorUtil.HorizontalSpace();
                            GUILayout.BeginHorizontal();

                            if (BrainEditorUtil.DeleteButton())
                                valueToDelete = i;

                            renamer.RenameControl(brain, true, trigger.Values[i].Name, RenameTargetType.triggerValue, id, i);
                            trigger.Values[i].Type = (AI.ValueType)EditorGUILayout.EnumPopup(trigger.Values[i].Type, BrainEditorUtil.GetStyle("DropDown"));

                            GUILayout.EndHorizontal();
                        }

                        if (trigger.Values.Length > 0)
                            BrainEditorUtil.HorizontalSpace();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (valueToDelete >= 0)
                    {
                        trigger.RemoveValueAt(valueToDelete);
                        needsRepaint = true;
                    }

                    if (BrainEditorUtil.AddButton())
                    {
                        var menu = new GenericMenu();

                        foreach (AI.ValueType type in Enum.GetValues(typeof(AI.ValueType)))
                            if (type != AI.ValueType.Unknown)
                                menu.AddItem(new GUIContent(type.ToString()), false, () => trigger.AddValue("New " + type.ToString(), type));

                        menu.ShowAsContext();
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();

                    BrainEditorUtil.HorizontalSpace();

                    GUILayout.EndVertical();
                }

                if (triggerToDelete > 0)
                {
                    if (!brain.IsUsingTrigger(triggerToDelete) ||
                        EditorUtility.DisplayDialog("Delete Trigger",
                                                    "There are nodes using the trigger. Do you really want to delete it?",
                                                    "Yes", "No"))
                    {
                        brain.RemoveTrigger(triggerToDelete);
                        needsRepaint = true;
                    }
                }
            }

            if (brain != null)
                if (GUILayout.Button("Add"))
                    brain.AddTrigger("New Trigger");

            return needsRepaint;
        }

        private bool instinctsTab(Brain brain)
        {
            if (brain == null)
                return false;

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 270;

            brain.TriggerInvestigation = EditorGUILayout.Toggle("Trigger timed investigations: ", brain.TriggerInvestigation);

            if (brain.TriggerInvestigation)
            {
                brain.CoverInvestigationTimer = EditorGUILayout.FloatField("Cover investigation timer: ", brain.CoverInvestigationTimer);
                brain.UncoveredInvestigationTimer = EditorGUILayout.FloatField("Uncovered nvestigation timer: ", brain.UncoveredInvestigationTimer);
                brain.CoverCheckRange = EditorGUILayout.FloatField("Cover check range: ", brain.CoverCheckRange);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.HandleReceivedHits = EditorGUILayout.Toggle("Handle received hits: ", brain.HandleReceivedHits);

            if (brain.HandleReceivedHits)
            {
                brain.SetAttackerAsTarget = EditorGUILayout.Toggle("Set attacker as target: ", brain.SetAttackerAsTarget);
                brain.SearchIfUnknownEnemies = EditorGUILayout.Toggle("Search if unknown enemies: ", brain.SearchIfUnknownEnemies);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.SetVisibleTargetIfNone = EditorGUILayout.Toggle("Set visible target if own none: ", brain.SetVisibleTargetIfNone);
            brain.UnsetDeadTargets = EditorGUILayout.Toggle("Unset dead targets: ", brain.UnsetDeadTargets);

            BrainEditorUtil.HorizontalSpace();

            brain.SetVeryCloseTargets = EditorGUILayout.Toggle("Set very close enemies as targets: ", brain.SetVeryCloseTargets);

            if (brain.SetVeryCloseTargets)
                brain.SetCloseTargetThreshold = EditorGUILayout.FloatField("Very close enemy threshold: ", brain.SetCloseTargetThreshold);

            BrainEditorUtil.HorizontalSpace();

            brain.ReactToEnemySounds = EditorGUILayout.Toggle("React to enemy sounds: ", brain.ReactToEnemySounds);

            if (brain.HandleReceivedHits)
            {
                brain.HearSoundSetTargetIfCurrentInvisible = EditorGUILayout.Toggle("Set target if current invisible: ", brain.HearSoundSetTargetIfCurrentInvisible);
                brain.HearSoundSearchIfNoEnemies = EditorGUILayout.Toggle("Search if no previous enemies: ", brain.HearSoundSearchIfNoEnemies);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.TriggerTargetTooClose = EditorGUILayout.Toggle("Trigger too close event: ", brain.TriggerTargetTooClose);

            if (brain.TriggerTargetTooClose)
            {
                brain.TargetTooCloseThreshold = EditorGUILayout.FloatField("Distance threshold: ", brain.TargetTooCloseThreshold);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.ReactToFriendTargets = EditorGUILayout.Toggle("React to friend's targets: ", brain.ReactToFriendTargets);

            if (brain.ReactToFriendTargets)
            {
                brain.SetFriendTargetIfNone = EditorGUILayout.Toggle("Set target if own none: ", brain.SetFriendTargetIfNone);
                brain.SetNewFriendTargetIfOwnInvisible = EditorGUILayout.Toggle("Set visible friend target if own invisible: ", brain.SetNewFriendTargetIfOwnInvisible);
                brain.UpdateFriendTargetIfSameAndVisible = EditorGUILayout.Toggle("Update same target position if friend's visible: ", brain.UpdateFriendTargetIfSameAndVisible);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.TellFriendsAboutTarget = EditorGUILayout.Toggle("Tell friends about target: ", brain.TellFriendsAboutTarget);

            if (brain.TellFriendsAboutTarget)
            {
                brain.CommunicationDistance = EditorGUILayout.FloatField("Communication distance: ", brain.CommunicationDistance);
                brain.CommunicateDelay = EditorGUILayout.FloatField("Communication delay (seconds): ", brain.CommunicateDelay);
            }

            BrainEditorUtil.HorizontalSpace();

            brain.StopOnDeath = EditorGUILayout.Toggle("Stop on death: ", brain.StopOnDeath);
            brain.ContinueOnResurrection = EditorGUILayout.Toggle("Continue on resurrection: ", brain.ContinueOnResurrection);

            BrainEditorUtil.HorizontalSpace();

            brain.OnlyVisibleDeath = EditorGUILayout.Toggle("Only take visible death events: ", brain.OnlyVisibleDeath);
            brain.OnlyVisibleResurrection = EditorGUILayout.Toggle("Only take visible resurrection events: ", brain.OnlyVisibleResurrection);
            brain.OnlyVisibleSpawn = EditorGUILayout.Toggle("Only take visible spawn events: ", brain.OnlyVisibleSpawn);

            BrainEditorUtil.HorizontalSpace();

            brain.GenerateSpecificDeathEvents = EditorGUILayout.Toggle("Generate specific death events: ", brain.GenerateSpecificDeathEvents);
            brain.GenerateSpecificResurrectionEvents = EditorGUILayout.Toggle("Generate specific death events: ", brain.GenerateSpecificResurrectionEvents);
            brain.GenerateSpecificSpawnEvents = EditorGUILayout.Toggle("Generate specific spawn events: ", brain.GenerateSpecificSpawnEvents);

            BrainEditorUtil.HorizontalSpace();

            brain.SearchIfFindDeathWithNoTarget = EditorGUILayout.Toggle("Search if no target and find death: ", brain.SearchIfFindDeathWithNoTarget);

            BrainEditorUtil.HorizontalSpace();

            EditorGUIUtility.labelWidth = oldLabelWidth;

            return false;
        }
    }
}