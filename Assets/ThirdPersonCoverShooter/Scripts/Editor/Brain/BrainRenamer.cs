using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using CoverShooter.AI;

namespace CoverShooter
{
    public enum RenameTargetType
    {
        none,
        layer,
        variable,
        trigger,
        triggerValue,
        action
    }

    public class BrainRenamer
    {
        private RenameTargetType _renameTargetType;
        private int _renameTarget;
        private int _renameTargetSub = -1;
        private string _renameValue;

        public void RenameControl(Brain brain, bool displayIfNotEditing, string name, RenameTargetType type, int target, int sub = -1)
        {
            if (_renameTargetType == type && _renameTarget == target && _renameTargetSub == sub)
            {
                var isOver = false;
                var isCancel = false;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                    isCancel = true;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                    isOver = true;

                _renameValue = GUILayout.TextField(_renameValue);

                if (isCancel)
                    EndRename(brain, false);
                else if (isOver)
                    EndRename(brain);
            }
            else if (displayIfNotEditing)
            {
                if (GUILayout.Button(name, BrainEditorUtil.Skin.label))
                    BeginRename(brain, name, type, target, sub);
            }
        }

        public void RenameLayerControl(Brain brain, ref int activeLayer, bool displayIfNotEditing, ref bool isSelected, string name, int target, int sub = -1)
        {
            if (_renameTargetType == RenameTargetType.layer && _renameTarget == target && _renameTargetSub == sub)
            {
                var isOver = false;
                var isCancel = false;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                    isCancel = true;

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                    isOver = true;

                _renameValue = GUILayout.TextField(_renameValue);

                if (isCancel)
                    EndRename(brain, false);
                else if (isOver)
                    EndRename(brain);
            }
            else if (displayIfNotEditing)
            {
                if (GUILayout.Button(name, BrainEditorUtil.Skin.label))
                {
                    if (isSelected)
                    {
                        activeLayer = target;
                        BeginRename(brain, name, RenameTargetType.layer, target, sub);
                    }
                    else
                        isSelected = true;
                }
            }
        }

        public void BeginRename(Brain brain, string value, RenameTargetType type, int target, int sub = -1)
        {
            if (_renameTargetType != RenameTargetType.none)
                EndRename(brain);

            if (brain == null)
                return;

            _renameTargetType = type;
            _renameTarget = target;
            _renameTargetSub = sub;
            _renameValue = value;
        }

        public void EndRename(Brain brain, bool isOk = true)
        {
            if (brain == null || !isOk)
            {
                stopRename();
                return;
            }

            switch (_renameTargetType)
            {
                case RenameTargetType.layer:
                    {
                        var layer = brain.GetLayer(_renameTarget);

                        if (layer != null && _renameTargetSub < 0)
                            layer.Name = _renameValue;
                    }
                    break;

                case RenameTargetType.variable:
                    {
                        var variable = brain.GetVariable(_renameTarget);

                        if (variable != null && _renameTargetSub < 0)
                            variable.Name = _renameValue;
                    }
                    break;

                case RenameTargetType.trigger:
                    {
                        var trigger = brain.GetTrigger(_renameTarget);

                        if (trigger != null && _renameTargetSub < 0)
                            trigger.Name = _renameValue;
                    }
                    break;

                case RenameTargetType.triggerValue:
                    {
                        var trigger = brain.GetTrigger(_renameTarget);

                        if (trigger != null && trigger.Values != null && _renameTargetSub >= 0 && _renameTargetSub < trigger.Values.Length)
                            trigger.Values[_renameTargetSub].Name = _renameValue;
                    }
                    break;

                case RenameTargetType.action:
                    {
                        var action = brain.GetAction(_renameTarget);

                        if (action != null && _renameTargetSub < 0)
                            action.Name = _renameValue;
                    }
                    break;
            }

            stopRename();
        }

        public void FinishRename(Brain brain, int hover)
        {
            if (_renameTargetType == RenameTargetType.action)
            {
                if (hover != _renameTarget)
                    EndRename(brain);
            }
            else
                EndRename(brain);
        }

        private void stopRename()
        {
            _renameTargetType = RenameTargetType.none;
            _renameTarget = -1;
            _renameTargetSub = -1;
        }
    }
}