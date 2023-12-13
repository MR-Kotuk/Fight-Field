using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter.AI
{
    public class Brain : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool TriggerInvestigation = true;
        public float CoverInvestigationTimer = 8;
        public float UncoveredInvestigationTimer = 1;
        public float CoverCheckRange = 2f;

        public bool HandleReceivedHits = true;
        public bool SetAttackerAsTarget = true;
        public bool SearchIfUnknownEnemies = true;

        public bool SetVisibleTargetIfNone = true;
        public bool UnsetDeadTargets = true;

        public bool SetVeryCloseTargets = true;
        public float SetCloseTargetThreshold = 8;

        public bool ReactToEnemySounds = true;
        public bool HearSoundSetTargetIfCurrentInvisible = true;
        public bool HearSoundSearchIfNoEnemies = true;

        public bool TriggerTargetTooClose = true;
        public float TargetTooCloseThreshold = 4;

        public bool ReactToFriendTargets = true;
        public bool SetFriendTargetIfNone = true;
        public bool SetNewFriendTargetIfOwnInvisible = true;
        public bool UpdateFriendTargetIfSameAndVisible = true;

        public bool TellFriendsAboutTarget = true;
        public float CommunicationDistance = 20;
        public float CommunicateDelay = 1;

        public bool OnlyVisibleDeath = true;
        public bool OnlyVisibleResurrection = true;
        public bool OnlyVisibleSpawn = true;

        public bool GenerateSpecificDeathEvents = true;
        public bool GenerateSpecificResurrectionEvents = true;
        public bool GenerateSpecificSpawnEvents = true;

        public bool StopOnDeath = true;
        public bool ContinueOnResurrection = true;

        public bool SearchIfFindDeathWithNoTarget = true;

        /// <summary>
        /// All of the layers inside the brain.
        /// </summary>
        [HideInInspector]
        public Layer[] Layers = new Layer[0];

        /// <summary>
        /// All of the nodes inside the brain.
        /// </summary>
        [HideInInspector]
        public ActionMap Actions = new ActionMap();

        /// <summary>
        /// All of the variables inside the brain.
        /// </summary>
        [HideInInspector]
        public VariableMap Variables = new VariableMap();

        /// <summary>
        /// All of the expressions inside the brain.
        /// </summary>
        [HideInInspector]
        public ExpressionMap Expressions = new ExpressionMap();

        /// <summary>
        /// All of the extensions inside the brain.
        /// </summary>
        [HideInInspector]
        public ExtensionMap Extensions = new ExtensionMap();

        /// <summary>
        /// All of the comments inside the brain.
        /// </summary>
        [HideInInspector]
        public CommentMap Comments = new CommentMap();

        /// <summary>
        /// All of the triggers inside the brain.
        /// </summary>
        [HideInInspector]
        public TriggerMap Triggers = new TriggerMap();

        /// <summary>
        /// All of the node triggers inside the brain.
        /// </summary>
        [HideInInspector]
        public NodeTriggerMap NodeTriggers = new NodeTriggerMap();

        [Serializable]
        public class ActionMap : SerializableDictionary<int, ActionNode> { }

        [Serializable]
        public class VariableMap : SerializableDictionary<int, Variable> { }

        [Serializable]
        public class ExpressionMap : SerializableDictionary<int, ExpressionNode> { }

        [Serializable]
        public class ExtensionMap : SerializableDictionary<int, ExtensionNode> { }

        [Serializable]
        public class CommentMap : SerializableDictionary<int, CommentNode> { }

        [Serializable]
        public class TriggerMap : SerializableDictionary<int, Trigger> { }

        [Serializable]
        public class NodeTriggerMap : SerializableDictionary<int, NodeTrigger> { }

        [SerializeField]
        [HideInInspector]
        private int _id;

        [NonSerialized]
        private List<int> _temp = new List<int>();

        private int generateId()
        {
            return ++_id;
        }

        public ValueType GetValueType(ref Value value)
        {
            if (value.IsConstant)
                return value.Type;
            else
            {
                ExpressionNode expression;
                Variable variable;

                if (Expressions.TryGetValue(value.ID, out expression))
                    return expression.Expression.GetReturnType(this);
                else if (Variables.TryGetValue(value.ID, out variable))
                    return variable.Value.Type;

                return ValueType.Unknown;
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            foreach (var action in Actions.Values)
            {
                action.ManageSuccessTrigger(this);
                action.ManageFailureTrigger(this);
            }
        }

        public void RebuildSaveDatabase()
        {
            if (Layers != null)
                for (int i = 0; i < Layers.Length; i++)
                    Layers[i].RebuildSaveDatabase(this);
        }

        public bool IsActionPlaced(int id)
        {
            if (Layers == null)
                return false;

            for (int i = 0; i < Layers.Length; i++)
                if (Layers[i].ContainsAction(id))
                    return true;

            return false;
        }

        public bool IsExpressionPlaced(int id)
        {
            if (Layers == null)
                return false;

            for (int i = 0; i < Layers.Length; i++)
                if (Layers[i].ContainsExpression(id))
                    return true;

            return false;
        }

        public bool IsExtensionPlaced(int id, bool checkActionsUsed = false)
        {
            if (Layers == null)
                return false;

            foreach (var key in Actions.Keys)
                if (!checkActionsUsed || IsActionPlaced(key))
                    if (Actions[key].ContainsExtension(id))
                        return true;

            return false;
        }

        public bool IsNodeTriggerPlaced(int id, bool checkActionsUsed = false)
        {
            if (Layers == null)
                return false;

            foreach (var key in Actions.Keys)
                if (!checkActionsUsed || IsActionPlaced(key))
                    if (Actions[key].ContainsTrigger(id))
                        return true;

            for (int i = 0; i < Layers.Length; i++)
                if (Layers[i].Entry.ContainsTrigger(id))
                    return true;
                else if (Layers[i].Any.ContainsTrigger(id))
                    return true;

            return false;
        }

        public bool CollectGarbage()
        {
            bool result = false;

            _temp.Clear();

            foreach (var id in Actions.Keys)
                if (!IsActionPlaced(id))
                    _temp.Add(id);

            foreach (var id in _temp)
                RemoveAction(id);

            result |= _temp.Count != 0;
            _temp.Clear();

            foreach (var id in Expressions.Keys)
                if (!IsExpressionPlaced(id))
                    _temp.Add(id);

            foreach (var id in _temp)
                RemoveExpression(id);

            result |= _temp.Count != 0;
            _temp.Clear();

            foreach (var id in Extensions.Keys)
                if (!IsExtensionPlaced(id))
                    _temp.Add(id);

            foreach (var id in _temp)
                RemoveExtension(id);

            result |= _temp.Count != 0;
            _temp.Clear();

            foreach (var id in NodeTriggers.Keys)
                if (!IsNodeTriggerPlaced(id))
                    _temp.Add(id);

            foreach (var id in _temp)
                RemoveNodeTrigger(id);

            result |= _temp.Count != 0;

            return result;
        }

        public bool IsUsingVariable(int id)
        {
            foreach (var item in Actions.Values)
                if (item.IsUsingVariable(id))
                    return true;

            foreach (var item in Expressions.Values)
                if (item.IsUsingVariable(id))
                    return true;

            foreach (var item in NodeTriggers.Values)
                if (item.IsUsingVariable(id))
                    return true;

            foreach (var item in Extensions.Values)
                if (item.IsUsingVariable(id))
                    return true;

            return false;
        }

        public bool IsUsingTrigger(int id)
        {
            foreach (var item in NodeTriggers.Values)
                if (item.Type == NodeTriggerType.Custom && item.ID == id)
                    return true;

            return false;
        }

        public int AddLayer(string name)
        {
            var layer = new Layer(this);

            layer.Name = name;

            if (Layers != null)
            {
                var hasTheSame = false;
                int index = 0;

                do
                {
                    hasTheSame = false;

                    for (int i = 0; i < Layers.Length; i++)
                        if (Layers[i].Name == layer.Name)
                        {
                            hasTheSame = true;
                            index++;
                            layer.Name = name + " (" + index.ToString() + ")";
                            break;
                        }
                } while (hasTheSame);
            }

            if (Layers == null)
                Layers = new Layer[1] { layer };
            else
            {
                var old = Layers;
                Layers = new Layer[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Layers[i] = old[i];

                Layers[old.Length] = layer;
            }

            return Layers.Length - 1;
        }

        public void RemoveLayer(int index)
        {
            if (Layers == null || Layers.Length == 0)
                return;

            var layer = Layers[index];

            if (layer != null && layer.Actions != null)
                for (int i = 0; i < layer.Actions.Length; i++)
                    Actions.Remove(layer.Actions[i]);

            var old = Layers;
            Layers = new Layer[old.Length - 1];

            if (index > 0)
                for (int i = 0; i < index; i++)
                    Layers[i] = old[i];

            if (index + 1 < old.Length)
                for (int i = index + 1; i < old.Length; i++)
                    Layers[i - 1] = old[i];
        }

        public void MoveLayerUp(int index)
        {
            if (Layers == null || Layers.Length == 0 || index <= 0 || index >= Layers.Length)
                return;

            var t = Layers[index - 1];
            Layers[index - 1] = Layers[index];
            Layers[index] = t;
        }

        public void MoveLayerDown(int index)
        {
            if (Layers == null || Layers.Length == 0 || index < 0 || index >= Layers.Length - 1)
                return;

            var t = Layers[index + 1];
            Layers[index + 1] = Layers[index];
            Layers[index] = t;
        }

        public string GetText(int id)
        {
            if (id < 0)
                return ((BuiltinValue)(-id)).ToString();

            Variable variable;
            if (Variables.TryGetValue(id, out variable))
                return variable.Name;

            ExpressionNode node;
            if (Expressions.TryGetValue(id, out node))
                if (node.Expression != null)
                    return node.Expression.GetText(this);

            return "None";
        }

        public int GetRootActionId(int id)
        {
            var root = 0;

            while (id != 0)
            {
                root = id;
                id = GetAction(id).Parent;
            }

            return root;
        }

        public int GetRootActionIdTill(int id, int till)
        {
            var root = 0;

            while (id != till)
            {
                root = id;

                var action = GetAction(id);

                if (action == null)
                    return 0;

                id = action.Parent;
            }

            return root;
        }

        public int AddSuperAction(int layerIndex, int parent, Vector2 editorPosition)
        {
            var node = new ActionNode(this);
            node.Name = "Super Action";
            node.EditorPosition = editorPosition;
            node.Parent = parent;

            var id = generateId();

            Actions.Add(id, node);

            var layer = GetLayer(layerIndex);

            if (layer != null)
                layer.AddAction(id);

            node.ManageSuccessTrigger(this);
            node.ManageFailureTrigger(this);

            return id;
        }

        public int AddAction(int layerIndex, int parent, BaseAction action, Vector2 editorPosition)
        {
            var node = new ActionNode(action);
            node.EditorPosition = editorPosition;
            node.Parent = parent;

            var id = generateId();

            Actions.Add(id, node);

            var layer = GetLayer(layerIndex);

            if (layer != null)
                layer.AddAction(id);

            node.ManageSuccessTrigger(this);
            node.ManageFailureTrigger(this);

            return id;
        }

        public void RemoveAction(int id)
        {
            _temp.Clear();

            foreach (var e in Expressions.Keys)
                if (Expressions[e].Parent == id)
                    _temp.Add(e);

            for (int i = 0; i < _temp.Count; i++)
                RemoveExpression(_temp[i]);

            var continueTheLoop = true;

            while (continueTheLoop)
            {
                continueTheLoop = false;

                foreach (var a in Actions.Keys)
                    if (Actions[a].Parent == id)
                    {
                        RemoveAction(a);
                        continueTheLoop = true;
                        break;
                    }
            }

            ActionNode action;
            if (Actions.TryGetValue(id, out action))
            {
                if (action.Triggers != null)
                    while (action.Triggers.Length > 0)
                        RemoveNodeTrigger(action.Triggers[action.Triggers.Length - 1]);

                if (action.Extensions != null)
                    while (action.Extensions.Length > 0)
                        RemoveExtension(action.Extensions[action.Extensions.Length - 1]);

                Actions.Remove(id);
            }

            ClearValue(id);

            if (Layers != null)
                for (int i = 0; i < Layers.Length; i++)
                    Layers[i].RemoveAction(id);
        }

        public int AddExpression(int layerIndex, int parent, BaseExpression expression, Vector2 editorPosition)
        {
            var node = new ExpressionNode(expression);
            node.EditorPosition = editorPosition;
            node.Parent = parent;

            var id = generateId();

            Expressions.Add(id, node);

            var layer = GetLayer(layerIndex);

            if (layer != null)
                layer.AddExpression(id, expression is SaveValue);

            return id;
        }

        public void RemoveExpression(int id)
        {
            if (Expressions.ContainsKey(id))
                Expressions.Remove(id);

            ClearValue(id);

            if (Layers != null)
                for (int i = 0; i < Layers.Length; i++)
                    Layers[i].RemoveExpression(id);
        }

        public int AddComment(int layerIndex, int parent, Vector2 editorPosition)
        {
            var node = new CommentNode();
            node.EditorPosition = editorPosition;
            node.Parent = parent;

            var id = generateId();

            Comments.Add(id, node);

            var layer = GetLayer(layerIndex);

            if (layer != null)
                layer.AddComment(id);

            return id;
        }

        public void RemoveComment(int id)
        {
            if (Comments.ContainsKey(id))
                Comments.Remove(id);

            ClearValue(id);

            if (Layers != null)
                for (int i = 0; i < Layers.Length; i++)
                    Layers[i].RemoveComment(id);
        }

        public int AddExtension(int layerIndex, int parent, BaseExtension extension)
        {
            var node = new ExtensionNode(extension);
            node.Parent = parent;

            var id = generateId();
            Extensions.Add(id, node);

            return id;
        }

        public void RemoveExtension(int id)
        {
            if (Extensions.ContainsKey(id))
                Extensions.Remove(id);

            ClearValue(id);
        }

        public int AddNodeTrigger(string name)
        {
            return addNodeTrigger(name, NodeTriggerType.Basic, 0, false, AIEvent.Trigger);
        }

        public int AddNodeTrigger(string name, NodeTriggerType type)
        {
            return addNodeTrigger(name, type, 0, false, AIEvent.Trigger);
        }

        public int AddNodeCustomTrigger(string name, int id)
        {
            return addNodeTrigger(name, NodeTriggerType.Custom, id, false, AIEvent.Trigger);
        }

        public int AddNodeTrigger(string name, AIEvent eventType)
        {
            return addNodeTrigger(name, NodeTriggerType.Event, 0, false, eventType);
        }

        public int AddNodeExpressionTrigger(string name, bool value)
        {
            return addNodeTrigger(name, NodeTriggerType.Expression, 0, value, AIEvent.Trigger);
        }

        public static bool GetInfo(AIEvent e, int index, out string name, out ValueType type)
        {
            name = null;
            type = ValueType.Unknown;

            switch (e)
            {
                case AIEvent.GetHit:
                    if (index == 0)
                    {
                        name = "Position";
                        type = ValueType.Vector3;
                        return true;
                    }

                    if (index == 1)
                    {
                        name = "Normal";
                        type = ValueType.Vector3;
                        return true;
                    }

                    if (index == 2)
                    {
                        name = "Attacker";
                        type = ValueType.GameObject;
                        return true;
                    }

                    return false;

                case AIEvent.HearEnemySound:
                    if (index == 0)
                    {
                        name = "Position";
                        type = ValueType.Vector3;
                        return true;
                    }

                    if (index == 1)
                    {
                        name = "Object";
                        type = ValueType.GameObject;
                        return true;
                    }

                    return false;

                case AIEvent.Investigate:
                    if (index == 0)
                    {
                        name = "Position";
                        type = ValueType.Vector3;
                        return true;
                    }

                    return false;

                case AIEvent.Search:
                    if (index == 0)
                    {
                        name = "Position";
                        type = ValueType.Vector3;
                        return true;
                    }

                    return false;

                case AIEvent.HearFriendTarget:
                    if (index == 0)
                    {
                        name = "Target";
                        type = ValueType.GameObject;
                        return true;
                    }

                    if (index == 1)
                    {
                        name = "Position";
                        type = ValueType.Vector3;
                        return true;
                    }

                    if (index == 2)
                    {
                        name = "Is Visible";
                        type = ValueType.Boolean;
                        return true;
                    }

                    if (index == 3)
                    {
                        name = "Sender";
                        type = ValueType.GameObject;
                        return true;
                    }

                    return false;

                case AIEvent.Dead:
                case AIEvent.EnemyDead:
                case AIEvent.FriendDead:
                case AIEvent.Resurrected:
                case AIEvent.EnemyResurrected:
                case AIEvent.FriendResurrected:
                case AIEvent.Spawned:
                case AIEvent.SelfSpawned:
                case AIEvent.FriendSpawned:
                case AIEvent.EnemySpawned:
                    if (index == 0)
                    {
                        name = "Object";
                        type = ValueType.GameObject;
                        return true;
                    }

                    return false;
            }

            return false;
        }

        private int addNodeTrigger(string name, NodeTriggerType type, int id, bool value, AIEvent eventType)
        {
            var trigger = new NodeTrigger();
            trigger.Name = name;
            trigger.Type = type;
            trigger.ID = id;
            trigger.ExpressionValue = value;
            trigger.Event = eventType;

            if (type == NodeTriggerType.Event)
            {
                for (int i = 0; i < 4; i++)
                {
                    string en;
                    ValueType et;

                    if (GetInfo(eventType, i, out en, out et))
                        trigger.AddValue(this, en, et);
                }
            }

            var triggerId = generateId();
            NodeTriggers.Add(triggerId, trigger);

            return triggerId;
        }

        public void RemoveNodeTrigger(int id)
        {
            NodeTrigger trigger;

            if (NodeTriggers.TryGetValue(id, out trigger))
            {
                trigger.ClearValues(this);
                NodeTriggers.Remove(id);
            }

            ClearValue(id);
        }

        public int AddVariable(ValueType type, string name, VariableClass class_)
        {
            var variable = new Variable();
            variable.Value.Type = type;
            variable.Class = class_;
            variable.Name = name;

            if (Variables != null && class_ != VariableClass.Parameter)
            {
                var hasTheSame = false;
                var index = 0;

                do
                {
                    hasTheSame = false;

                    foreach (Variable previous in Variables.Values)
                        if (previous.Name == variable.Name)
                        {
                            hasTheSame = true;
                            index++;
                            variable.Name = name + " (" + index.ToString() + ")";
                            break;
                        }
                } while (hasTheSame);
            }

            var id = generateId();
            Variables.Add(id, variable);

            return id;
        }

        public void RemoveVariable(int id)
        {
            if (Variables.ContainsKey(id))
                Variables.Remove(id);

            ClearValue(id);
        }

        public int AddTrigger(string name)
        {
            var trigger = new Trigger();
            trigger.Name = name;

            if (Triggers != null)
            {
                var hasTheSame = false;
                var index = 0;

                do
                {
                    hasTheSame = false;

                    foreach (Trigger previous in Triggers.Values)
                        if (previous.Name == trigger.Name)
                        {
                            hasTheSame = true;
                            index++;
                            trigger.Name = name + " (" + index.ToString() + ")";
                            break;
                        }
                } while (hasTheSame);
            }

            var id = generateId();
            Triggers.Add(id, trigger);

            return id;
        }

        public void RemoveTrigger(int id)
        {
            if (Triggers.ContainsKey(id))
                Triggers.Remove(id);

            ClearValue(id);
        }

        public void ClearValue(int id)
        {
            if (Layers != null)
                for (int i = 0; i < Layers.Length; i++)
                {
                    Layers[i].Entry.ClearValue(id);
                    Layers[i].Any.ClearValue(id);
                }

            foreach (var action in Actions.Values)
                action.ClearValue(id);

            foreach (var expression in Expressions.Values)
                expression.ClearValue(id);

            foreach (var trigger in NodeTriggers.Values)
                trigger.ClearValue(id);

            foreach (var extension in Extensions.Values)
                extension.ClearValue(id);
        }

        public Layer GetLayer(int index)
        {
            if (Layers == null || Layers.Length <= index || index < 0)
                return null;

            return Layers[index];
        }

        public ActionNode GetAction(int id)
        {
            ActionNode node;

            if (Actions.TryGetValue(id, out node))
                return node;
            else
                return null;
        }

        public Variable GetVariable(int id)
        {
            Variable variable;

            if (Variables.TryGetValue(id, out variable))
                return variable;
            else
                return null;
        }

        public ExpressionNode GetExpression(int id)
        {
            ExpressionNode expression;

            if (Expressions.TryGetValue(id, out expression))
                return expression;
            else
                return null;
        }

        public CommentNode GetComment(int id)
        {
            CommentNode comment;

            if (Comments.TryGetValue(id, out comment))
                return comment;
            else
                return null;
        }

        public ExtensionNode GetExtension(int id)
        {
            ExtensionNode extension;

            if (Extensions.TryGetValue(id, out extension))
                return extension;
            else
                return null;
        }

        public Trigger GetTrigger(int id)
        {
            Trigger trigger;

            if (Triggers.TryGetValue(id, out trigger))
                return trigger;
            else
                return null;
        }

        public NodeTrigger GetNodeTrigger(int id)
        {
            NodeTrigger trigger;

            if (NodeTriggers.TryGetValue(id, out trigger))
                return trigger;
            else
                return null;
        }

        public EntryNode GetEntry(Layer layer, int superNode)
        {
            var parent = GetAction(superNode);

            return parent == null ? layer.Entry : parent.Entry;
        }

        public TriggeredNode GetAny(Layer layer, int superNode)
        {
            var parent = GetAction(superNode);

            return parent == null ? layer.Any : parent.Any;
        }

        public LayerNode GetExit(int superNode)
        {
            var parent = GetAction(superNode);

            return parent == null ? null : parent.Exit;
        }

        public LayerNode GetFail(int superNode)
        {
            var parent = GetAction(superNode);

            return parent == null ? null : parent.Fail;
        }
    }
}