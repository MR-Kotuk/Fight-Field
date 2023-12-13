using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public struct LayerState
    {
        public int CurrentNode;

        public ActionStateMap ActionStates;
        public ExtensionStateMap ExtensionStates;
        public IntList ActiveActions;
        public IntList ActiveExtensions;

        public bool IsFreezingAboveLayers;
        public bool HasFreezeValue;

        public bool IsEntry;
        public List<EventDesc> CurrentEvents;
        public List<EventDesc> FutureEvents;

        private bool _isUpdating;
        
        [Serializable]
        public class ActionStateMap : SerializableDictionary<int, NodeState> { }

        [Serializable]
        public class ExtensionStateMap : SerializableDictionary<int, ExtensionState> { }

        [Serializable]
        public class IntList : List<int> { }

        public NodeState GetActionState(int id)
        {
            NodeState value;

            if (ActionStates != null && ActionStates.TryGetValue(id, out value))
                return value;
            else
                return new NodeState();
        }

        public void ReInit()
        {
            IsEntry = true;
            CurrentNode = 0;
        }

        public void BeginUpdate()
        {
            _isUpdating = true;
            FutureEvents.Clear();
        }

        public void EndUpdate()
        {
            _isUpdating = false;
            CurrentEvents.Clear();
            var t = FutureEvents;
            FutureEvents = CurrentEvents;
            CurrentEvents = t;
        }

        public void Add(ref EventDesc desc)
        {
            if (_isUpdating)
                FutureEvents.Add(desc);
            else
                CurrentEvents.Add(desc);
        }

        public void SetState(int id, NodeState value)
        {
            if (ActionStates == null) ActionStates = new ActionStateMap();
            if (ActiveActions == null) ActiveActions = new IntList();

            if (!ActionStates.ContainsKey(id))
                ActiveActions.Add(id);

            ActionStates[id] = value;
        }

        public void RemoveActionState(int id)
        {
            Debug.Assert((ActionStates != null) == (ActiveActions != null));

            if (ActionStates == null)
                return;

            if (ActionStates.ContainsKey(id))
            {
                ActionStates.Remove(id);
                ActiveActions.Remove(id);
            }
        }

        public ExtensionState GetExtensionState(int id)
        {
            ExtensionState value;

            if (ExtensionStates != null && ExtensionStates.TryGetValue(id, out value))
                return value;
            else
                return new ExtensionState();
        }

        public void SetState(int id, ExtensionState value)
        {
            if (ExtensionStates == null) ExtensionStates = new ExtensionStateMap();
            if (ActiveExtensions == null) ActiveExtensions = new IntList();

            if (!ExtensionStates.ContainsKey(id))
                ActiveExtensions.Add(id);

            ExtensionStates[id] = value;
        }

        public void RemoveExtensionState(int id)
        {
            Debug.Assert((ExtensionStates != null) == (ActiveExtensions != null));

            if (ExtensionStates == null)
                return;

            if (ExtensionStates.ContainsKey(id))
            {
                ExtensionStates.Remove(id);
                ActiveExtensions.Remove(id);
            }
        }
    }
}