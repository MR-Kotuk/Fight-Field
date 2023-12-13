using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class Layer
    {
        /// <summary>
        /// Name of the layer.
        /// </summary>
        public string Name;

        /// <summary>
        /// Are layers higher in the list frozen while this layer is active.
        /// </summary>
        public bool IsFreezingLayersAbove;

        /// <summary>
        /// All actions inside the layer.
        /// </summary>        
        [HideInInspector]
        public int[] Actions;

        /// <summary>
        /// All expressions inside the layer.
        /// </summary>
        [HideInInspector]
        public int[] Expressions;

        /// <summary>
        /// All save expressions inside the layer.
        /// </summary>
        [HideInInspector]
        public int[] GlobalSaves;

        /// <summary>
        /// All comment nodes inside the layer.
        /// </summary>
        [HideInInspector]
        public int[] Comments;

        /// <summary>
        /// Entry node of the layer.
        /// </summary>
        [HideInInspector]
        public EntryNode Entry;

        /// <summary>
        /// Node that always has priority when executing triggers.
        /// </summary>
        [HideInInspector]
        public TriggeredNode Any;

        public Layer()
        {
        }

        /// <summary>
        /// Populate layer basics.
        /// </summary>
        public Layer(Brain brain)
        {
            Entry = new EntryNode(brain);
            Entry.EditorPosition = new Vector2(0, -100);

            Any = new TriggeredNode();
            Any.EditorPosition = new Vector2(0, -200);
        }

        /// <summary>
        /// Is the action contained within the layer.
        /// </summary>
        public bool ContainsAction(int id)
        {
            for (int i = 0; i < Actions.Length; i++)
                if (Actions[i] == id)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds an action node id.
        /// </summary>
        public void AddAction(int id)
        {
            if (Actions == null)
                Actions = new int[1] { id };
            else
            {
                var old = Actions;
                Actions = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Actions[i] = old[i];

                Actions[old.Length] = id;
            }
        }

        /// <summary>
        /// Removes an action node id.
        /// </summary>
        public void RemoveAction(int id)
        {
            if (Actions == null)
                return;

            for (int index = 0; index < Actions.Length; index++)
                if (Actions[index] == id)
                {
                    var old = Actions;
                    Actions = new int[old.Length - 1];

                    if (index > 0)
                        for (int i = 0; i < index; i++)
                            Actions[i] = old[i];

                    if (index + 1 < old.Length)
                        for (int i = index + 1; i < old.Length; i++)
                            Actions[i - 1] = old[i];

                    return;
                }
        }

        /// <summary>
        /// Is the action contained within the layer.
        /// </summary>
        public bool ContainsComment(int id)
        {
            for (int i = 0; i < Comments.Length; i++)
                if (Comments[i] == id)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds an comment node id.
        /// </summary>
        public void AddComment(int id)
        {
            if (Comments == null)
                Comments = new int[1] { id };
            else
            {
                var old = Comments;
                Comments = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    Comments[i] = old[i];

                Comments[old.Length] = id;
            }
        }

        /// <summary>
        /// Removes an comment node id.
        /// </summary>
        public void RemoveComment(int id)
        {
            if (Comments == null)
                return;

            for (int index = 0; index < Comments.Length; index++)
                if (Comments[index] == id)
                {
                    var old = Comments;
                    Comments = new int[old.Length - 1];

                    if (index > 0)
                        for (int i = 0; i < index; i++)
                            Comments[i] = old[i];

                    if (index + 1 < old.Length)
                        for (int i = index + 1; i < old.Length; i++)
                            Comments[i - 1] = old[i];

                    return;
                }
        }

        /// <summary>
        /// Is the expression contained within the layer.
        /// </summary>
        public bool ContainsExpression(int id)
        {
            for (int i = 0; i < Expressions.Length; i++)
                if (Expressions[i] == id)
                    return true;

            return false;
        }

        /// <summary>
        /// Adds an expression node id.
        /// </summary>
        public void AddExpression(int id, bool isSave)
        {
            addToArray(ref Expressions, id);
        }

        /// <summary>
        /// Removes an expression node id.
        /// </summary>
        public void RemoveExpression(int id)
        {
            removeFromArray(ref Expressions, id);
        }

        public void RebuildSaveDatabase(Brain brain)
        {
            GlobalSaves = null;

            if (Entry != null)
                Entry.ClearSaves(brain);

            if (Any != null)
                Any.ClearSaves(brain);

            if (Actions != null)
                for (int i = 0; i < Actions.Length; i++)
                {
                    var action = brain.GetAction(Actions[i]);
                    if (action != null) action.ClearSaves(brain);
                }

            if (Expressions != null)
                for (int i = 0; i < Expressions.Length; i++)
                {
                    var expression = brain.GetExpression(Expressions[i]);

                    if (expression.Expression is SaveValue)
                    {
                        if (!evaluateParameters(brain, expression, Expressions[i]))
                        {
                            if (GlobalSaves == null)
                                GlobalSaves = new int[1] { Expressions[i] };
                            else
                            {
                                var old = GlobalSaves;
                                GlobalSaves = new int[old.Length + 1];

                                for (int j = 0; j < old.Length; j++)
                                    GlobalSaves[j] = old[j];

                                GlobalSaves[old.Length] = Expressions[i];
                            }
                        }
                    }
                }
        }

        private bool evaluateParameters(Brain brain, ExpressionNode node, int save)
        {
            if (node.Fields == null || node.Fields.Length == 0)
                return false;

            var result = false;

            for (int i = 0; i < node.Fields.Length; i++)
            {
                var field = node.Fields[i];

                if (field.FieldType == typeof(Value))
                {
                    var value = (Value)field.GetValue(node.Expression);

                    if (evaluateValue(brain, ref value, save))
                        result = true;
                }
                else if (field.FieldType == typeof(Value[]))
                {
                    var values = (Value[])field.GetValue(node.Expression);

                    for (int j = 0; j < values.Length; j++)
                        if (evaluateValue(brain, ref values[j], save))
                            result = true;
                }
            }

            return result;
        }

        private bool evaluateValue(Brain brain, ref Value value, int save)
        {
            if (value.IsConstant)
                return false;
            else
            {
                var expression = brain.GetExpression(value.ID);

                if (expression != null)
                    return evaluateParameters(brain, expression, save);
                else
                {
                    var variable = brain.GetVariable(value.ID);

                    if (variable != null && variable.Class == VariableClass.Parameter)
                    {
                        foreach (var trigger in brain.NodeTriggers.Values)
                            if (trigger.ContainsValue(value.ID))
                                trigger.AddSave(save);

                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        private void addToArray(ref int[] array, int value)
        {
            if (array == null)
                array = new int[1] { value };
            else
            {
                var old = array;
                array = new int[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    array[i] = old[i];

                array[old.Length] = value;
            }
        }

        private void removeFromArray(ref int[] array, int value)
        {
            if (array == null)
                return;

            for (int index = 0; index < array.Length; index++)
                if (array[index] == value)
                {
                    var old = array;
                    array = new int[old.Length - 1];

                    if (index > 0)
                        for (int i = 0; i < index; i++)
                            array[i] = old[i];

                    if (index + 1 < old.Length)
                        for (int i = index + 1; i < old.Length; i++)
                            array[i - 1] = old[i];

                    return;
                }
        }
    }
}