using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class ActionNode : TriggeredNode
    {
        public BaseAction Action { get { return (BaseAction)Attachment; } }

        /// <summary>
        /// Name of the node.
        /// </summary>
        public string Name;

        /// <summary>
        /// Does the action have a limited time to run.
        /// </summary>
        public bool HasLimitedTime = false;

        /// <summary>
        /// Max action duration if HasLimitedTime is true.
        /// </summary>
        public Value TimeLimit = new Value(1f);

        /// <summary>
        /// Trigger that is set off once the time runs out.
        /// </summary>
        public int TimeOutTrigger;

        /// <summary>
        /// Trigger that is set on a successful action result.
        /// </summary>
        public int SuccessTrigger;

        /// <summary>
        /// Trigger that is set on a failure action result.
        /// </summary>
        public int FailureTrigger;

        /// <summary>
        /// Extension IDs used by the action node.
        /// </summary>
        public int[] Extensions;

        /// <summary>
        /// Entry node of the super action.
        /// </summary>
        [HideInInspector]
        public EntryNode Entry;

        /// <summary>
        /// Node that always has priority when executing triggers in this super action group.
        /// </summary>
        [HideInInspector]
        public TriggeredNode Any;

        /// <summary>
        /// Exit node of the super node group.
        /// </summary>
        [HideInInspector]
        public TriggeredNode Exit;

        /// <summary>
        /// Exit node of the super node group.
        /// </summary>
        [HideInInspector]
        public TriggeredNode Fail;

        /// <summary>
        /// Intensity of the debug marker inside the editor for this node.
        /// </summary>
        [HideInInspector]
        [NonSerialized]
        public float EditorDebugValue = 0;

        public ActionNode()
            : base()
        {
        }

        public ActionNode(Brain brain)
            : base()
        {
            Entry = new EntryNode(brain);
            Entry.EditorPosition = new Vector2(0, 200);

            Any = new TriggeredNode();
            Any.EditorPosition = new Vector2(0, 100);

            Exit = new TriggeredNode();
            Exit.EditorPosition = new Vector2(0, 0);

            Fail = new TriggeredNode();
            Fail.EditorPosition = new Vector2(0, -100);
        }

        public ActionNode(BaseAction action)
            : base(action)
        {
            Name = Action.GetType().Name;
        }

        public override void AdjustPropertyWidth(Brain brain)
        {
            base.AdjustPropertyWidth(brain);

            if (SuccessTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(SuccessTrigger);

                if (trigger != null && trigger.PropertyWidth > PropertyWidth)
                    PropertyWidth = trigger.PropertyWidth;
            }

            if (FailureTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(FailureTrigger);

                if (trigger != null && trigger.PropertyWidth > PropertyWidth)
                    PropertyWidth = trigger.PropertyWidth;
            }

            if (TimeOutTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(TimeOutTrigger);

                if (trigger != null && trigger.PropertyWidth > PropertyWidth)
                    PropertyWidth = trigger.PropertyWidth;
            }

            if (Extensions == null)
                return;

            for (int i = 0; i < Extensions.Length; i++)
            {
                var extension = brain.GetExtension(Extensions[i]);

                if (extension != null && extension.PropertyWidth > PropertyWidth)
                    PropertyWidth = extension.PropertyWidth;
            }
        }

        public override void ClearSaves(Brain brain)
        {
            base.ClearSaves(brain);

            if (SuccessTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(SuccessTrigger);
                if (trigger != null) trigger.ClearSaves();
            }

            if (FailureTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(FailureTrigger);
                if (trigger != null) trigger.ClearSaves();
            }

            if (TimeOutTrigger > 0)
            {
                var trigger = brain.GetNodeTrigger(TimeOutTrigger);
                if (trigger != null) trigger.ClearSaves();
            }
        }

        public bool IsImmediate()
        {
            var action = Action;

            if (action == null)
                return false;
            else
            {
                var attribute = (ImmediateAttribute)Attribute.GetCustomAttribute(action.GetType(), typeof(ImmediateAttribute));
                return attribute != null && attribute.IsImmediate;
            }
        }

        public bool ContainsExtension(int id)
        {
            if (Extensions == null)
                return false;

            for (int i = 0; i < Extensions.Length; i++)
                if (Extensions[i] == id)
                    return true;

            return false;
        }

        public override bool ContainsTrigger(int id)
        {
            if (id == TimeOutTrigger) return true;
            if (id == SuccessTrigger) return true;
            if (id == FailureTrigger) return true;

            if (Entry != null && Entry.ContainsTrigger(id)) return true;
            if (Any != null && Any.ContainsTrigger(id)) return true;
            if (Any != null && Any.ContainsTrigger(id)) return true;
            if (Exit != null && Exit.ContainsTrigger(id)) return true;
            if (Fail != null && Fail.ContainsTrigger(id)) return true;

            return base.ContainsTrigger(id);
        }

        public void AddExtension(int id)
        {
            if (Extensions == null)
                Extensions = new int[1] { id };
            else
            {
                var old = Extensions;
                Extensions = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Extensions[i] = old[i];

                Extensions[old.Length] = id;
            }
        }

        public void RemoveExtensionAt(int index)
        {
            if (Extensions == null || Extensions.Length == 0)
                return;

            var old = Extensions;
            Extensions = new int[old.Length - 1];

            if (index > 0)
                for (int i = 0; i < index; i++)
                    Extensions[i] = old[i];

            if (index + 1 < old.Length)
                for (int i = index + 1; i < old.Length; i++)
                    Extensions[i - 1] = old[i];
        }

        public override void ClearValue(int id)
        {
            if (SuccessTrigger == id) SuccessTrigger = 0;
            if (FailureTrigger == id) FailureTrigger = 0;
            if (TimeOutTrigger == id) TimeOutTrigger = 0;

            if (Extensions != null && Extensions.Length > 0)
                for (int i = Extensions.Length - 1; i >= 0; i--)
                    if (Extensions[i] == id)
                        RemoveExtensionAt(i);

            base.ClearValue(id);
        }

        public void ManageSuccessTrigger(Brain brain)
        {
            if (Action != null)
            {
                var type = Action.GetType();
                var attribute = (SuccessAttribute)Attribute.GetCustomAttribute(type, typeof(SuccessAttribute));

                if (attribute == null || attribute.Name == null || attribute.Name.Length == 0)
                {
                    brain.RemoveNodeTrigger(SuccessTrigger);
                    SuccessTrigger = 0;
                }
                else
                {
                    var trigger = brain.GetNodeTrigger(SuccessTrigger);

                    if (trigger == null)
                    {
                        SuccessTrigger = brain.AddNodeTrigger(attribute.Name);
                        trigger = brain.GetNodeTrigger(SuccessTrigger);
                    }
                    else
                        trigger.Name = attribute.Name;

                    var parameters = type.GetCustomAttributes(typeof(SuccessParameterAttribute), false);

                    if (parameters == null || parameters.Length == 0)
                        trigger.ClearValues(brain);
                    else
                    {
                        int oldCount = trigger.Values == null ? 0 : trigger.Values.Length;

                        for (int i = 0; i < oldCount; i++)
                        {
                            var variable = brain.GetVariable(trigger.Values[i]);
                            var parameter = (SuccessParameterAttribute)parameters[i];
                            variable.Name = parameter.Name;
                            variable.Value.Type = parameter.Type;
                        }

                        if (parameters.Length > oldCount)
                        {
                            for (int i = oldCount; i < parameters.Length; i++)
                            {
                                var parameter = (SuccessParameterAttribute)parameters[i];
                                trigger.AddValue(brain, parameter.Name, parameter.Type);
                            }
                        }
                        else if (trigger.Values != null)
                            while (parameters.Length < trigger.Values.Length)
                                trigger.RemoveValueAt(brain, parameters.Length);

                    }
                }
            }
            else
            {
                var trigger = brain.GetNodeTrigger(SuccessTrigger);

                if (trigger == null)
                {
                    SuccessTrigger = brain.AddNodeTrigger("Done");
                    trigger = brain.GetNodeTrigger(SuccessTrigger);
                }
                else
                    trigger.Name = "Done";

                trigger.ClearValues(brain);
            }
        }

        public void ManageFailureTrigger(Brain brain)
        {
            if (Action != null)
            {
                var type = Action.GetType();
                var attribute = (FailureAttribute)Attribute.GetCustomAttribute(type, typeof(FailureAttribute));

                if (attribute == null || attribute.Name == null || attribute.Name.Length == 0)
                {
                    brain.RemoveNodeTrigger(FailureTrigger);
                    FailureTrigger = 0;
                }
                else
                {
                    var trigger = brain.GetNodeTrigger(FailureTrigger);

                    if (trigger == null)
                    {
                        FailureTrigger = brain.AddNodeTrigger(attribute.Name);
                        trigger = brain.GetNodeTrigger(FailureTrigger);
                    }
                    else
                        trigger.Name = attribute.Name;

                    var parameters = type.GetCustomAttributes(typeof(FailureParameterAttribute), false);

                    if (parameters == null || parameters.Length == 0)
                        trigger.ClearValues(brain);
                    else
                    {
                        int oldCount = trigger.Values == null ? 0 : trigger.Values.Length;

                        for (int i = 0; i < oldCount; i++)
                        {
                            var variable = brain.GetVariable(trigger.Values[i]);
                            var parameter = (FailureParameterAttribute)parameters[i];
                            variable.Name = parameter.Name;
                            variable.Value.Type = parameter.Type;
                        }

                        if (parameters.Length > oldCount)
                        {
                            for (int i = oldCount; i < parameters.Length; i++)
                            {
                                var parameter = (FailureParameterAttribute)parameters[i];
                                trigger.AddValue(brain, parameter.Name, parameter.Type);
                            }
                        }
                        else if (trigger.Values != null)
                            while (parameters.Length < trigger.Values.Length)
                                trigger.RemoveValueAt(brain, parameters.Length);

                    }
                }
            }
            else
            {
                var trigger = brain.GetNodeTrigger(FailureTrigger);

                if (trigger == null)
                {
                    FailureTrigger = brain.AddNodeTrigger("Failure");
                    trigger = brain.GetNodeTrigger(FailureTrigger);
                }
                else
                    trigger.Name = "Failure";

                trigger.ClearValues(brain);
            }
        }

        public void AddTimeOutTrigger(Brain brain)
        {
            if (brain.GetNodeTrigger(TimeOutTrigger) != null)
                return;

            TimeOutTrigger = brain.AddNodeTrigger("Time Out");
        }

        public void RemoveTimeOutTrigger(Brain brain)
        {
            brain.RemoveNodeTrigger(TimeOutTrigger);
            TimeOutTrigger = 0;
        }

        /// <summary>
        /// Called when the node is entered.
        /// </summary>
        public void Enter(State state, int layer, int id)
        {
            var nodeState = new NodeState();

            var action = Action;

            if (action != null)
                action.Enter(state, layer, ref nodeState.Values);

            state.Layers[layer].SetState(id, nodeState);

            if (Extensions != null)
                for (int i = 0; i < Extensions.Length; i++)
                {
                    var extension = state.Brain.GetExtension(Extensions[i]);

                    if (extension != null)
                        extension.Begin(state, layer, Extensions[i]);
                 }
        }

        /// <summary>
        /// Called when the node is exited.
        /// </summary>
        public void Escape(State state, int layer, int id)
        {
            var action = Action;

            if (action != null)
            {
                var nodeState = state.Layers[layer].GetActionState(id);
                action.Exit(state, layer, ref nodeState.Values);
            }

            if (Extensions != null)
                for (int i = 0; i < Extensions.Length; i++)
                {
                    var extension = state.Brain.GetExtension(Extensions[i]);

                    if (extension != null)
                        extension.End(state, layer, Extensions[i]);
                }

            state.Layers[layer].RemoveActionState(id);
        }

        /// <summary>
        /// Called every frame when the node is active.
        /// </summary>
        public AIResult Update(State state, int layer, int id, ref NodeState nodeState)
        {
            var action = Action;
            var result = new AIResult(AIResultType.Nothing);

            if (action != null && !nodeState.IsHolding)
                result = action.Update(state, layer, ref nodeState.Values);

            if (Extensions != null)
                for (int i = 0; i < Extensions.Length; i++)
                {
                    var extension = state.Brain.GetExtension(Extensions[i]);

                    if (extension != null)
                        extension.Update(state, layer, Extensions[i]);
                }

            return result;
        }
    }
}