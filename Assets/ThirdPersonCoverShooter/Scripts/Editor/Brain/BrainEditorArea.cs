using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using CoverShooter.AI;

namespace CoverShooter
{
    public class BrainEditorArea
    {
        /// <summary>
        /// Size of the elements inside the editor view.
        /// </summary>
        public float Scale = 1.0f;

        private bool _hasInitialPosition = false;
        private BrainLocator _locator = new BrainLocator();

        private int _lastClickNode;
        private float _lastClickTime;
        private Vector2 _lastClickPosition;

        private Connector _connector;

        private int _nodeBelow;

        private int _futureSetSuperNode = -1;

        struct NodeFunContext
        {
            public BrainRenamer Renamer;
            public AIController Controller;
            public Brain Brain;
            public int ActiveLayer;
            public int SuperNode;
            public bool NeedsRepaint;
            public int NodeToRemove;
        }

        struct VariableTarget
        {
            public int ID;
            public int Index;
            public int Value;
            public bool HasValue;
        }

        private NodeFunContext _funContext;
        private VariableTarget _variableTarget;

        public void UpdateArea(Brain brain, int activeLayer, int superNode, Rect screen)
        {
            _locator.UpdateTotalArea(brain, activeLayer, superNode);
            _locator.ClampPosition(screen);
        }

        public bool Display(EditorWindow editor, AIController controller, BrainRenamer renamer, Rect screen, Brain brain, int activeLayer, ref int superNode)
        {
            if (!_hasInitialPosition)
            {
                _locator.Position = new Vector2(-screen.width * 0.5f, -screen.height * 0.5f) * Scale;
                _hasInitialPosition = true;
            }

            {
                var oldColor = GUI.backgroundColor;
                GUI.color = new Color32(36, 36, 36, 255);
                GUI.DrawTexture(screen, EditorGUIUtility.whiteTexture);
                GUI.color = oldColor;
            }

            drawGrid(screen, 20, new Color(0.3f, 0.3f, 0.3f, 0.1f));
            drawGrid(screen, 100, new Color(0.4f, 0.4f, 0.4f, 0.2f));

            var needsRepaint = nodesInput(renamer, screen, Event.current, brain, activeLayer, ref superNode);

            if (_futureSetSuperNode != -1)
            {
                superNode = _futureSetSuperNode;
                _futureSetSuperNode = -1;
            }

            if (brain == null)
                return needsRepaint;

            var layer = getLayer(brain, activeLayer);

            if (layer == null)
                return needsRepaint;

            _locator.UpdateTotalArea(brain, activeLayer, superNode);

            _funContext.Renamer = renamer;
            _funContext.Controller = controller;
            _funContext.Brain = brain;
            _funContext.ActiveLayer = activeLayer;
            _funContext.SuperNode = superNode;

            editor.BeginWindows();

            {
                var entry = brain.GetEntry(layer, superNode);

                if (entry != null)
                {
                    var box = GUI.Window(State.EntryID, _locator.ToScreen(entry), layerNodeFun, GUIContent.none, BrainEditorUtil.GetStyle("EntryWindow"));
                    entry.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                    editOrDrawEntryTriggers(controller, brain, activeLayer, superNode, State.EntryID, entry, box, false);
                    editOrDrawCustomTriggers(controller, brain, activeLayer, superNode, State.EntryID, entry, box, false, false);
                }

                var any = brain.GetAny(layer, superNode);

                if (any != null)
                {
                    var box = GUI.Window(State.AnyID, _locator.ToScreen(any), layerNodeFun, GUIContent.none, BrainEditorUtil.GetStyle("AnyWindow"));
                    any.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                    editOrDrawCustomTriggers(controller, brain, activeLayer, superNode, State.AnyID, any, box, false, false);
                }

                var exit = brain.GetExit(superNode);

                if (exit != null)
                {
                    var box = GUI.Window(State.ExitID, _locator.ToScreen(exit), layerNodeFun, "Exit", BrainEditorUtil.GetStyle("ExitWindow"));
                    exit.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                    editOrDrawActionSlots(brain, exit, State.ExitID, box);
                }

                var fail = brain.GetFail(superNode);

                if (fail != null)
                {
                    var box = GUI.Window(State.FailID, _locator.ToScreen(fail), layerNodeFun, "Fail", BrainEditorUtil.GetStyle("FailWindow"));
                    fail.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                    editOrDrawActionSlots(brain, fail, State.FailID, box);
                }
            }

            if (layer.Comments != null)
            {
                for (int i = 0; i < layer.Comments.Length; i++)
                {
                    var id = layer.Comments[i];
                    var node = brain.GetComment(id);

                    if (node != null && node.Parent == superNode)
                    {
                        var box = GUI.Window(id, _locator.ToScreen(node), commentFun, GUIContent.none, BrainEditorUtil.GetStyle("CommentWindow"));
                        node.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);
                    }
                }
            }

            if (layer.Actions != null)
            {
                for (int i = 0; i < layer.Actions.Length; i++)
                {
                    var id = layer.Actions[i];
                    var node = brain.GetAction(id);

                    if (node != null && node.Parent == superNode)
                    {
                        var box = GUI.Window(id, _locator.ToScreen(node), actionFun, GUIContent.none, node.Action == null ? BrainEditorUtil.GetStyle("SuperWindow") : BrainEditorUtil.Skin.window);
                        node.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                        var valueIndex = 0;

                        editOrDrawActionSlots(brain, node, id, box);
                        editOrDrawActionValues(renamer, brain, activeLayer, superNode, id, node, box, ref valueIndex, false);
                        editOrDrawActionExtensions(brain, activeLayer, superNode, id, node, box, false);
                        editOrDrawActionTriggers(controller, brain, activeLayer, superNode, id, node, box, false);
                        editOrDrawCustomTriggers(controller, brain, activeLayer, superNode, id, node, box, false, false);

                        if (Application.isPlaying && controller != null)
                        {
                            var color = new Color(0, 0, 0, 0);

                            var activeNode = controller.State.GetCurrentNode(activeLayer);
                            var isActive = activeNode == id;

                            if (!isActive && activeNode > 0)
                            {
                                do
                                {
                                    activeNode = brain.GetAction(activeNode).Parent;
                                    isActive = activeNode == id;
                                } while (!isActive && activeNode > 0);
                            }

                            if (isActive)
                            {
                                color = Color.yellow;
                            }
                            else if (node.EditorDebugValue > float.Epsilon)
                            {
                                color = new Color(0.25f, 0.25f, 1.0f, node.EditorDebugValue);
                            }

                            if (color.a > 0.01f)
                            {
                                var margin = 2;
                                EditorGUI.DrawRect(new Rect(box.x - margin, box.y - margin, box.width + margin * 2, box.height + margin * 2), color);
                            }
                        }
                        else
                            node.EditorDebugValue = 0;
                    }
                }
            }

            if (layer.Expressions != null)
            {
                for (int i = 0; i < layer.Expressions.Length; i++)
                {
                    var id = layer.Expressions[i];
                    var node = brain.GetExpression(id);

                    if (node != null && node.Parent == superNode)
                    {
                        if (node.Expression != null)
                        {
                            var box = GUI.Window(id, _locator.ToScreen(node), expressionFun, GUIContent.none, BrainEditorUtil.GetStyle("ExpressionWindow"));
                            node.EditorPosition = _locator.ToWorld(box.x + box.width * 0.5f, box.y);

                            var sourceOffset = new Vector2(box.x, box.y);
                            editOrDrawExpressionSlots(brain, id, node, box);
                            editOrDrawExpression(brain, activeLayer, superNode, id, node, false, box);
                        }
                    }
                }
            }

            editor.EndWindows();

            if (_connector != null)
                drawConnector(brain, layer, superNode, Event.current.mousePosition);

            if (_funContext.NeedsRepaint)
            {
                needsRepaint = true;
                _funContext.NeedsRepaint = false;
            }

            if (_funContext.NodeToRemove > 0)
            {
                brain.RemoveExpression(_funContext.NodeToRemove);
                brain.RemoveAction(_funContext.NodeToRemove);
                brain.RemoveComment(_funContext.NodeToRemove);
                _funContext.NodeToRemove = 0;
            }

            return needsRepaint;
        }

        private bool nodesInput(BrainRenamer renamer, Rect screen, Event e, Brain brain, int activeLayer, ref int superNode)
        {
            var needsRepaint = false;

            switch (e.type)
            {
                case EventType.ScrollWheel:
                    {
                        var middle = new Vector2(e.mousePosition.x, e.mousePosition.y) / Scale;

                        var previousScale = Scale;
                        Scale = Mathf.Clamp(Scale - 0.01f * e.delta.y, 0.2f, 2f);

                        var newMiddle = new Vector2(e.mousePosition.x, e.mousePosition.y) / Scale;
                        _locator.Position -= (newMiddle - middle) * Scale;
                        _locator.ClampPosition(screen);

                        needsRepaint = true;
                    }
                    break;

                case EventType.MouseMove:
                    if (_connector != null)
                    {
                        _connector.IsValid = false;
                        needsRepaint = true;

                        var hover = getNodeBelow(brain, activeLayer, superNode, Event.current.mousePosition);

                        if (hover != 0)
                        {
                            var action = brain.GetAction(hover);

                            if (action != null)
                                _connector.Check(LocationType.Action);
                            else
                            {
                                var expression = brain.GetExpression(hover);

                                if (expression != null)
                                    _connector.Check(LocationType.Expression);
                            }
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (e.button == 2)
                    {
                        _locator.Position -= e.delta;
                        needsRepaint = true;

                        _locator.ClampPosition(screen);
                    }
                    break;

                case EventType.MouseDown:
                    {
                        var hover = getNodeBelow(brain, activeLayer, superNode, Event.current.mousePosition);

                        if (e.button == 0)
                        {
                            if (_lastClickNode == hover && 
                                Time.time - _lastClickTime < 0.4f &&
                                Vector2.Distance(Event.current.mousePosition, _lastClickPosition) < 2)
                            {
                                var node = brain != null ? brain.GetAction(hover) : null;

                                if (node != null && node.Action == null)
                                    superNode = hover;
                            }

                            _lastClickNode = hover;
                            _lastClickTime = Time.time;
                            _lastClickPosition = Event.current.mousePosition;

                            if (_connector != null && !_connector.IsValid)
                            {
                                _connector.Clear(brain);
                                _connector = null;
                            }
                            else if (_connector != null && hover != 0)
                            {
                                _connector.AcceptId(brain, hover);
                                _connector = null;
                            }
                        }

                        if (e.button == 1)
                            _nodeBelow = hover;

                        renamer.FinishRename(brain, hover);

                        if (e.button == 1)
                        {
                            var mousePosition = Event.current.mousePosition;

                            if (_nodeBelow > 0)
                            {
                                var currentNode = _nodeBelow;
                                var action = brain.GetAction(currentNode);

                                var menu = new GenericMenu();

                                if (action != null)
                                    menu.AddItem(new GUIContent("Rename"), false, () => { renamer.BeginRename(brain, action.Name, RenameTargetType.action, currentNode); });

                                menu.AddItem(new GUIContent("Remove"), false, () =>
                                {
                                    brain.RemoveExpression(currentNode);
                                    brain.RemoveAction(currentNode);
                                    brain.RemoveComment(currentNode);
                                });

                                show(menu);
                            }
                            else if (_nodeBelow == 0 && brain != null)
                            {
                                var menu = new GenericMenu();

                                if (superNode > 0)
                                {
                                    var node = brain.GetAction(superNode);

                                    if (node != null)
                                        menu.AddItem(new GUIContent("Go Up"), false, () =>
                                        {
                                            _futureSetSuperNode = node.Parent;
                                        });
                                    else
                                        superNode = 0;
                                }

                                var parent = superNode;

                                menu.AddItem(new GUIContent("Add Comment"), false, () => brain.AddComment(activeLayer, parent, _locator.ToWorld(mousePosition.x, mousePosition.y)));

                                menu.AddItem(new GUIContent("Add Action/Super Node"), false, () => brain.AddSuperAction(activeLayer, parent, _locator.ToWorld(mousePosition.x, mousePosition.y)));

                                foreach (var id in brain.Variables.Keys)
                                {
                                    var variable = brain.Variables[id];

                                    if (variable.Class != VariableClass.Parameter)
                                    {
                                        menu.AddItem(new GUIContent("Add Action/Set/" + variable.Name), false, () => brain.AddAction(activeLayer, parent, new SetValue(id), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                        menu.AddItem(new GUIContent("Add Expression/Save/" + variable.Name), false, () => brain.AddExpression(activeLayer, parent, new SaveValue(brain, id), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                    }
                                }

                                foreach (var id in brain.Triggers.Keys)
                                {
                                    menu.AddItem(new GUIContent("Add Action/Set Off/" + brain.Triggers[id].Name), false, () => brain.AddAction(activeLayer, parent, new SetOff(brain, id), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                    menu.AddItem(new GUIContent("Add Action/Set Off for other/" + brain.Triggers[id].Name), false, () => brain.AddAction(activeLayer, parent, new SetOffFor(brain, id), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                    menu.AddItem(new GUIContent("Add Action/Set Off in area/" + brain.Triggers[id].Name), false, () => brain.AddAction(activeLayer, parent, new SetOffInArea(brain, id), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                }

                                foreach (AIEvent ev in Enum.GetValues(typeof(AIEvent)))
                                {
                                    if (ev == AIEvent.Trigger)
                                        continue;

                                    var text = ev.ToString();

                                    menu.AddItem(new GUIContent("Add Action/Generate Event/" + text), false, () => brain.AddAction(activeLayer, parent, new GenerateEvent(ev), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                    menu.AddItem(new GUIContent("Add Action/Send Event to/" + text), false, () => brain.AddAction(activeLayer, parent, new SendEventTo(ev), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                    menu.AddItem(new GUIContent("Add Action/Send Event in area/" + text), false, () => brain.AddAction(activeLayer, parent, new SendEventInArea(ev), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                }

                                foreach (Type type in Assembly.GetAssembly(typeof(BaseAction)).GetTypes())
                                    if (type.IsClass && !type.IsAbstract && 
                                        type != typeof(SetOff) && 
                                        type != typeof(SetOffFor) &&
                                        type != typeof(SetOffInArea) &&
                                        type != typeof(GenerateEvent) &&
                                        type != typeof(SendEventTo) &&
                                        type != typeof(SendEventInArea) &&
                                        type != typeof(SetValue) && 
                                        type != typeof(SaveValue))
                                    {
                                        if (type.IsSubclassOf(typeof(BaseAction)))
                                        {
                                            var folder = (FolderAttribute)Attribute.GetCustomAttribute(type, typeof(FolderAttribute));

                                            string name;

                                            if (folder != null)
                                                name = "Add Action/" + folder.Folder + "/" + type.Name;
                                            else
                                                name = "Add Action/" + type.Name;

                                            menu.AddItem(new GUIContent(name), false, () => brain.AddAction(activeLayer, parent, (BaseAction)Activator.CreateInstance(type), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                        }
                                        else if (type.IsSubclassOf(typeof(BaseExpression)))
                                        {
                                            var folder = (FolderAttribute)Attribute.GetCustomAttribute(type, typeof(FolderAttribute));

                                            string name;

                                            if (folder != null)
                                                name = "Add Expression/" + folder.Folder + "/" + type.Name;
                                            else
                                                name = "Add Expression/" + type.Name;

                                            menu.AddItem(new GUIContent(name), false, () => brain.AddExpression(activeLayer, parent, (BaseExpression)Activator.CreateInstance(type), _locator.ToWorld(mousePosition.x, mousePosition.y)));
                                        }
                                    }

                                show(menu);
                            }
                        }
                    }
                    break;
            }

            return needsRepaint;
        }

        private void layerNodeFun(int id)
        {
            var controller = _funContext.Controller;
            var brain = _funContext.Brain;
            var activeLayer = _funContext.ActiveLayer;
            var superNode = _funContext.SuperNode;

            var layer = getLayer(brain, activeLayer);

            LayerNode node;

            switch (id)
            {
                case State.EntryID:
                    BrainEditorUtil.WindowTitle("Entry");
                    node = brain.GetEntry(layer, superNode);
                    break;

                case State.AnyID:
                    BrainEditorUtil.WindowTitle("Any");
                    node = brain.GetAny(layer, superNode);
                    break;

                case State.ExitID:
                    node = brain.GetExit(superNode);
                    break;

                case State.FailID:
                    node = brain.GetFail(superNode);
                    break;

                default:
                    Debug.Assert(false);
                    return;
            }

            if (id == State.ExitID || id == State.FailID)
            {
                BrainEditorUtil.HorizontalSpace();
            }
            else
            {
                var rect = new Rect();

                if (_locator.ToScreenAction(brain, layer, superNode, id, ref rect))
                {
                    if (node is EntryNode)
                        editOrDrawEntryTriggers(controller, brain, activeLayer, superNode, id, (EntryNode)node, rect, true);

                    if (node is TriggeredNode)
                        editOrDrawCustomTriggers(controller, brain, activeLayer, superNode, id, (TriggeredNode)node, rect, true, true);
                }
            }

            if (id == State.ExitID)
            {
                node.EditorWidth = BrainEditorUtil.ExitWidth;
                node.EditorHeight = BrainEditorUtil.ExitHeight;
            }
            else if (id == State.FailID)
            {
                node.EditorHeight = BrainEditorUtil.FailWidth;
                node.EditorHeight = BrainEditorUtil.FailHeight;
            }
            else
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    var height = rect.y + rect.height + 10;

                    node.AdjustPropertyWidth(brain);
                    var width = Mathf.Max(200, node.PropertyWidth);

                    if (width != node.EditorWidth)
                    {
                        node.EditorWidth = width;
                        _funContext.NeedsRepaint = true;
                    }

                    if (height != node.EditorHeight)
                    {
                        node.EditorHeight = height;
                        _funContext.NeedsRepaint = true;
                    }
                }
            }

            GUI.DragWindow();
        }

        private void actionFun(int id)
        {
            var controller = _funContext.Controller;
            var brain = _funContext.Brain;
            var activeLayer = _funContext.ActiveLayer;
            var superNode = _funContext.SuperNode;
            var renamer = _funContext.Renamer;

            var node = _funContext.Brain.GetAction(id);
            var action = node.Action;

            var rect = _locator.ToScreen(node);
            var valueIndex = 0;

            if (!BrainEditorUtil.WindowTitle(node.Name, true))
                _funContext.NodeToRemove = id;

            editOrDrawActionValues(renamer, brain, activeLayer, superNode, id, node, rect, ref valueIndex, true);
            editOrDrawActionExtensions(brain, activeLayer, superNode, id, node, rect, true);
            editOrDrawActionTriggers(controller, brain, activeLayer, superNode, id, node, rect, true);
            editOrDrawCustomTriggers(controller, brain, activeLayer, superNode, id, node, rect, action == null ? true : !node.IsImmediate(), true);

            if (Event.current.type == EventType.Repaint)
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                var height = lastRect.y + lastRect.height + 10;

                node.AdjustPropertyWidth(brain);
                adjustPropertyWidthByName(node, node.Name);
                var width = Mathf.Max(200, node.PropertyWidth);

                if (width != node.EditorWidth)
                {
                    node.EditorWidth = width;
                    _funContext.NeedsRepaint = true;
                }

                if (height != node.EditorHeight)
                {
                    node.EditorHeight = height;
                    _funContext.NeedsRepaint = true;
                }
            }

            GUI.DragWindow();
        }

        private void expressionFun(int id)
        {
            var expression = _funContext.Brain.GetExpression(id);
            var name = expression.Expression.GetType().Name;

            if (!BrainEditorUtil.WindowTitle(name, true))
                _funContext.NodeToRemove = id;   

            editOrDrawExpression(_funContext.Brain, _funContext.ActiveLayer, _funContext.SuperNode, id, expression, true, _locator.ToScreen(expression));

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                var height = rect.y + rect.height + 10;

                adjustPropertyWidthByName(expression, name);
                var width = Mathf.Max(200, expression.PropertyWidth);

                if (width != expression.EditorWidth)
                {
                    expression.EditorWidth = width;
                    _funContext.NeedsRepaint = true;
                }

                if (height != expression.EditorHeight)
                {
                    expression.EditorHeight = height;
                    _funContext.NeedsRepaint = true;
                }
            }

            GUI.DragWindow();
        }

        private void editOrDrawExpressionSlots(Brain brain, int id, ExpressionNode node, Rect window)
        {
            {
                var r = _locator.GetTopSlot(window, BrainEditorUtil.ExpressionOutputSize);
                var outPosition = _locator.GetTopOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Expression);

                if (BrainEditorUtil.ExpressionOutput(r, node.HasEditorTopConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToValue(Location.Expression(id));
                }

                node.HasEditorTopConnection = false;
            }

            {
                var r = _locator.GetRightSlot(window, BrainEditorUtil.ExpressionOutputSize);
                var outPosition = _locator.GetRightOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Expression);

                if (BrainEditorUtil.ExpressionOutput(r, node.HasEditorRightConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToValue(Location.Expression(id));
                }

                node.HasEditorRightConnection = false;
            }

            {
                var r = _locator.GetBottomSlot(window, BrainEditorUtil.ExpressionOutputSize);
                var outPosition = _locator.GetBottomOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Expression);

                if (BrainEditorUtil.ExpressionOutput(r, node.HasEditorBottomConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToValue(Location.Expression(id));
                }

                node.HasEditorBottomConnection = false;
            }
        }

        private void editOrDrawExpression(Brain brain, int activeLayer, int superNode, int id, ExpressionNode node, bool isInWindow, Rect window)
        {
            var rect = _locator.ToScreen(node);

            bool hadLines;
            int valueIndex = 0;

            node.CurrentLabelWidth = 0;
            node.CurrentPropertyWidth = 0;

            editOrDrawValues(brain, activeLayer, superNode, id, node, rect, isInWindow, ref valueIndex, out hadLines);

            if (isInWindow)
            {
                node.LabelWidth = node.CurrentLabelWidth;
                node.PropertyWidth = node.CurrentPropertyWidth;
            }

            if (!hadLines)
                GUILayout.Label("");
        }

        private void commentFun(int id)
        {
            var comment = _funContext.Brain.GetComment(id);

            comment.Text = GUILayout.TextField(comment.Text, GUILayout.ExpandWidth(true), GUILayout.MinHeight(40));

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                var height = rect.y + rect.height + 10;
                var width = 235;

                if (width != comment.EditorWidth)
                {
                    comment.EditorWidth = width;
                    _funContext.NeedsRepaint = true;
                }

                if (height != comment.EditorHeight)
                {
                    comment.EditorHeight = height;
                    _funContext.NeedsRepaint = true;
                }
            }

            GUI.DragWindow();
        }

        private void editOrDrawActionSlots(Brain brain, LayerNode node, int id, Rect window)
        {
            {
                var r = _locator.GetTopSlot(window, BrainEditorUtil.ActionInputSize);
                var outPosition = _locator.GetTopOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Action);

                if (BrainEditorUtil.ActionInputTop(r, node.HasEditorTopConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToTrigger(Location.Action(id));
                }

                node.HasEditorTopConnection = false;
            }

            {
                var action = brain.GetAction(id);
                var r = action == null ? _locator.GetLeftSlot(window, BrainEditorUtil.ActionInputSize) : _locator.GetLeftSlot(action, window, BrainEditorUtil.ActionInputSize);
                var outPosition = _locator.GetLeftOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Action);

                if (BrainEditorUtil.ActionInputLeft(r, node.HasEditorLeftConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToTrigger(Location.Action(id));
                }

                node.HasEditorLeftConnection = false;
            }

            {
                var r = _locator.GetBottomSlot(window, BrainEditorUtil.ActionInputSize);
                var outPosition = _locator.GetBottomOutput(r);

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.Action);

                if (BrainEditorUtil.ActionInputBottom(r, node.HasEditorBottomConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptId(brain, id);
                        _connector = null;
                    }
                    else
                        _connector = new ToTrigger(Location.Action(id));
                }

                node.HasEditorBottomConnection = false;
            }
        }

        private void editOrDrawActionValues(BrainRenamer renamer, Brain brain, int activeLayer, int superNode, int id, ActionNode node, Rect window, ref int valueIndex, bool isInWindow)
        {
            var action = node.Action;
            var trigger = action == null ? null : action.GetTrigger(brain);
            var event_ = action == null ? AIEvent.Trigger : action.GetEvent(brain);

            if (isInWindow)
            {
                renamer.RenameControl(brain, false, node.Name, RenameTargetType.action, id);

                if (action != null)
                {
                    var text = action.GetType().Name;

                    if (text != node.Name)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        GUILayout.Label(text);

                        adjustPropertyWidthByName(node, text);

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }

            node.CurrentLabelWidth = 0;
            node.CurrentPropertyWidth = 0;

            if (action != null)
            {
                bool hadLines;
                editOrDrawValues(brain, activeLayer, superNode, id, node, window, isInWindow, ref valueIndex, out hadLines);
            }

            if (trigger != null)
            {
                if (trigger != null && trigger.Values != null)
                    for (int i = 0; i < trigger.Values.Length; i++)
                    {
                        var field = node.Fields[i + valueIndex];
                        var value = (Value)field.GetValue(action);

                        value.Type = trigger.Values[i].Type;

                        editOrDrawValue(brain, activeLayer, superNode, node, id, trigger.Values[i].Name, ref valueIndex, ref value, false, true, null, false, isInWindow, window);

                        field.SetValue(action, value);
                    }
            }

            if (event_ != AIEvent.Trigger)
            {
                var start = valueIndex;

                while (valueIndex < node.Fields.Length)
                {
                    var field = node.Fields[valueIndex];
                    var value = (Value)field.GetValue(action);

                    string valueName;

                    if (!Brain.GetInfo(event_, valueIndex - start, out valueName, out value.Type))
                        break;

                    editOrDrawValue(brain, activeLayer, superNode, node, id, valueName, ref valueIndex, ref value, false, true, null, false, isInWindow, window);

                    field.SetValue(action, value);
                }
            }

            if (isInWindow)
            {
                node.LabelWidth = node.CurrentLabelWidth;
                node.PropertyWidth = node.CurrentPropertyWidth;
            }

            if (isInWindow)
                BrainEditorUtil.HorizontalSpace();
        }

        private void editOrDrawActionExtensions(Brain brain, int activeLayer, int superNode, int owningNodeId, ActionNode node, Rect window, bool isInWindow)
        {
            var hasMove = false;
            var hasAim = false;
            var hasFire = false;
            var hasCrouch = false;

            var hadAny = false;
            var itemToDelete = -1;

            if (node.Extensions != null)
                for (int i = 0; i < node.Extensions.Length; i++)
                {
                    var extension = brain.GetExtension(node.Extensions[i]);
                    extension.Owner = owningNodeId;

                    switch (extension.Extension.Class)
                    {
                        case ExtensionClass.Aim: hasAim = true; break;
                        case ExtensionClass.Fire: hasFire = true; break;
                        case ExtensionClass.AimAndFire: hasAim = true; hasFire = true; break;
                        case ExtensionClass.Move: hasMove = true; break;
                        case ExtensionClass.Crouch: hasCrouch = true; break;
                    }

                    if (hasValues(extension))
                    {
                        if (isInWindow)
                        {
                            GUILayout.BeginVertical(BrainEditorUtil.GetTitledBox());

                            if (!BrainEditorUtil.BoxTitle(extension.Extension.Name, true))
                                itemToDelete = i;
                        }

                        var valueIndex = 0;
                        bool hadLines;

                        extension.CurrentLabelWidth = 0;
                        extension.CurrentPropertyWidth = 0;

                        editOrDrawValues(brain, activeLayer, superNode, node.Extensions[i], extension, window, isInWindow, ref valueIndex, out hadLines);

                        if (isInWindow)
                        {
                            extension.LabelWidth = extension.CurrentLabelWidth;
                            extension.PropertyWidth = extension.CurrentPropertyWidth;

                            adjustPropertyWidthByName(extension, extension.Extension.Name);
                        }

                        if (isInWindow)
                            GUILayout.EndVertical();
                    }
                    else
                    {
                        if (isInWindow)
                            if (!BrainEditorUtil.NoContentBox(extension.Extension.Name, true))
                                itemToDelete = i;
                    }

                    hadAny = true;
                }

            if (itemToDelete >= 0)
                brain.RemoveExtension(node.Extensions[itemToDelete]);

            var canAddMove = !hasMove && node.Action != null && node.Action.AllowMoveExtension;
            var canAddAim = !hasAim && node.Action != null && node.Action.AllowAimExtension;
            var canAddFire = !hasFire && node.Action != null && node.Action.AllowFireExtension;
            var canAddCrouch = !hasCrouch && node.Action != null && node.Action.AllowCrouchExtension;

            if (!canAddAim && !canAddFire && !canAddMove && !canAddCrouch)
            {
                if (hadAny && isInWindow)
                    BrainEditorUtil.HorizontalSpace();

                return;
            }

            if (isInWindow)
            {
                if (GUILayout.Button("Extend"))
                {
                    var menu = new GenericMenu();

                    foreach (Type type in Assembly.GetAssembly(typeof(BaseExtension)).GetTypes())
                        if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseExtension)))
                        {
                            var class_ = BaseExtension.GetClass(type);
                            var isOk = true;

                            switch (class_)
                            {
                                case ExtensionClass.AimAndFire: isOk = canAddAim && canAddFire; break;
                                case ExtensionClass.Aim: isOk = canAddAim; break;
                                case ExtensionClass.Fire: isOk = canAddFire; break;
                                case ExtensionClass.Move: isOk = canAddMove; break;
                                case ExtensionClass.Crouch: isOk = canAddCrouch; break;
                            }

                            if (isOk)
                                menu.AddItem(new GUIContent(BaseExtension.GetName(type)), false, () => node.AddExtension(brain.AddExtension(activeLayer, owningNodeId, (BaseExtension)Activator.CreateInstance(type))));
                        }

                    show(menu);
                }

                BrainEditorUtil.HorizontalSpace();
            }
        }

        private bool hasValues(LayerNode node)
        {
            for (int i = 0; i < node.Fields.Length; i++)
            {
                var field = node.Fields[i];

                if (field.FieldType == typeof(Value))
                    return true;
                else if (field.FieldType == typeof(VariableReference))
                    return true;
                else if (field.FieldType == typeof(TriggerReference))
                    return true;
                else if (field.FieldType == typeof(Value[]))
                    return true;
            }

            return false;
        }

        private void editOrDrawValues(Brain brain, int activeLayer, int superNode, int id, LayerNode node, Rect window, bool isInWindow, ref int valueIndex, out bool hadLines)
        {
            SetValue setValue = null;

            if (node.Attachment is SetValue)
                setValue = (SetValue)node.Attachment;

            hadLines = false;

            for (int i = 0; i < node.Fields.Length; i++)
            {
                var field = node.Fields[i];

                var ignore = (IgnoreNextValuesAttribute)Attribute.GetCustomAttribute(field, typeof(IgnoreNextValuesAttribute));
                if (ignore != null)
                {
                    valueIndex++;
                    break;
                }

                var typeAttributes = Attribute.GetCustomAttributes(field, typeof(ValueTypeAttribute));
                var noFieldNamesAttribute = (NoFieldNameAttribute)Attribute.GetCustomAttribute(field, typeof(NoFieldNameAttribute));
                var noConstantsAttribute = (NoConstantAttribute)Attribute.GetCustomAttribute(field, typeof(NoConstantAttribute));
                var defaultTypeAttribute = (DefaultValueTypeAttribute)Attribute.GetCustomAttribute(field, typeof(DefaultValueTypeAttribute));
                var onlyArithmeticTypesAttribute = (OnlyArithmeticTypeAttribute)Attribute.GetCustomAttribute(field, typeof(OnlyArithmeticTypeAttribute));

                var hasTypes = typeAttributes != null && typeAttributes.Length > 0;
                var hasSingleType = hasTypes && typeAttributes.Length == 1;
                var hasMultipleTypes = hasTypes && typeAttributes.Length > 1; 
                var hasNoFieldNames = noFieldNamesAttribute == null ? false : noFieldNamesAttribute.Value;
                var hasNoConstants = noConstantsAttribute == null ? false : noConstantsAttribute.Value;
                var defaultType = defaultTypeAttribute == null ? AI.ValueType.Float : defaultTypeAttribute.Type;
                var hasOnlyArithmeticTypes = onlyArithmeticTypesAttribute == null ? false : onlyArithmeticTypesAttribute.Value;

                if (field.FieldType == typeof(Value))
                {
                    var value = (Value)field.GetValue(node.Attachment);
                    var hasSetType = false;

                    if (setValue != null && !hasTypes)
                    {
                        var variable = brain.GetVariable(setValue.Variable.ID);

                        if (variable != null)
                        {
                            hasSetType = true;
                            value.Type = variable.Value.Type;
                        }
                    }

                    if (hasSetType)
                        editOrDrawValue(brain, activeLayer, superNode, node, id, hasNoFieldNames ? null : field.Name, ref valueIndex, ref value, hasNoConstants, true, null, hasOnlyArithmeticTypes, isInWindow, window);
                    else
                        editOrDrawValue(brain, activeLayer, superNode, node, id, hasNoFieldNames ? null : field.Name, ref valueIndex, ref value, hasNoConstants, hasSingleType, typeAttributes, hasOnlyArithmeticTypes, isInWindow, window);

                    field.SetValue(node.Attachment, value);
                    hadLines = true;
                }
                else if (field.FieldType == typeof(VariableReference))
                {
                    if (isInWindow)
                        editVariableReference(brain, node, node.Attachment, field, hasNoFieldNames);
                }
                else if (field.FieldType == typeof(TriggerReference))
                {
                    if (isInWindow)
                        editTriggerReference(brain, node, node.Attachment, field, hasNoFieldNames);
                }
                else if (field.FieldType == typeof(AIEvent))
                {
                    if (isInWindow)
                        editEvent(brain, node, node.Attachment, field, hasNoFieldNames);
                }
                else if (field.FieldType == typeof(Value[]))
                {
                    var array = (Value[])field.GetValue(node.Attachment);

                    var countAttribute = (MinimumCountAttribute)Attribute.GetCustomAttribute(field, typeof(MinimumCountAttribute));
                    var minimum = countAttribute == null ? 1 : countAttribute.Count;

                    if (array == null)
                    {
                        array = new Value[minimum];

                        for (int j = 0; j < minimum; j++)
                            array[j].Type = defaultType;
                    }
                    else if (array.Length < minimum)
                    {
                        var old = array;
                        array = new Value[minimum];

                        for (int j = 0; j < old.Length; j++)
                            array[j] = old[j];

                        for (int j = old.Length; j < minimum; j++)
                            array[j].Type = defaultType;
                    }

                    var itemToDelete = -1;

                    for (int j = 0; j < array.Length; j++)
                    {
                        if (j > 0 && !hasTypes)
                            EditorGUILayout.Space();

                        if (array.Length > minimum && isInWindow)
                        {
                            GUILayout.BeginHorizontal();
                            editOrDrawValue(brain, activeLayer, superNode, node, id, null, ref valueIndex, ref array[j], hasNoConstants, hasSingleType, typeAttributes, hasOnlyArithmeticTypes, isInWindow, window);

                            if (BrainEditorUtil.DeleteButton())
                                itemToDelete = j;

                            GUILayout.EndHorizontal();
                        }
                        else
                            editOrDrawValue(brain, activeLayer, superNode, node, id, null, ref valueIndex, ref array[j], hasNoConstants, hasSingleType, typeAttributes, hasOnlyArithmeticTypes, isInWindow, window);
                    }

                    if (itemToDelete >= 0)
                    {
                        var old = array;
                        array = new Value[old.Length - 1];

                        if (itemToDelete > 0)
                            for (int j = 0; j < itemToDelete; j++)
                                array[j] = old[j];

                        if (itemToDelete + 1 < old.Length)
                            for (int j = itemToDelete + 1; j < old.Length; j++)
                                array[j - 1] = old[j];
                    }

                    if (isInWindow)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (BrainEditorUtil.AddButton())
                        {
                            var old = array;
                            array = new Value[old.Length + 1];

                            for (int j = 0; j < old.Length; j++)
                                array[j] = old[j];

                            array[old.Length] = new Value();
                            array[old.Length].Type = defaultType;
                        }

                        GUILayout.EndHorizontal();
                    }

                    field.SetValue(node.Attachment, array);
                    hadLines = true;
                }
            }
        }

        private void editOrDrawValue(Brain brain, int activeLayer, int superNode, Editable editable, int sourceId, string name, ref int index, ref Value value, bool isValueNonConstant, bool isTypeConstant, Attribute[] types, bool isTypeArithmetic, bool isInWindow, Rect window)
        {
            if (_variableTarget.HasValue && _variableTarget.ID == sourceId && _variableTarget.Index == index)
            {
                value.ID = _variableTarget.Value;
                _variableTarget.HasValue = false;
            }

            if (!isInWindow)
            {
                var r = new Rect(value.LocalEditorRect.x + window.x + 3,
                                 value.LocalEditorRect.y + window.y,
                                 value.LocalEditorRect.width,
                                 value.LocalEditorRect.height);

                var variable = value.ID == 0 ? null : brain.GetVariable(value.ID);
                var hasConnection = value.ID > 0 && (variable == null || variable.Class == VariableClass.Parameter);

                if (_connector != null && _connector.Source.Id == sourceId && _connector.Source.Index == index)
                    hasConnection = false;

                if (_connector != null && r.Contains(Event.current.mousePosition))
                    _connector.Check(LocationType.ExpressionValue);

                if (BrainEditorUtil.ValueInput(r, hasConnection))
                {
                    if (_connector != null)
                    {
                        _connector.AcceptValue(brain, ref value);
                        _connector = null;
                    }
                    else
                    {
                        if (editable is ExpressionNode)
                            _connector = new ToExpression(Location.ExpressionValue(sourceId, index));
                        else if (editable is ActionNode)
                            _connector = new ToExpression(Location.ActionValue(sourceId, index));
                        else if (editable is ExtensionNode)
                            _connector = new ToExpression(Location.ExtensionValue(sourceId, index));
                        else if (editable is NodeTrigger)
                            _connector = new ToExpression(Location.TriggerInput(sourceId));
                    }
                }

                if (hasConnection)
                    drawValueConnection(brain, getLayer(brain, activeLayer), superNode, _locator.GetLeftOutput(r), value.ID, types, isTypeArithmetic);

                index++;
                return;
            }

            GUILayout.BeginHorizontal();

            if (name != null && name.Length > 0)
            {
                if (editable != null)
                {
                    var width = GUI.skin.label.CalcSize(new GUIContent(name)).x + 10;

                    GUILayout.Label(name, GUILayout.Width(editable.LabelWidth));

                    if (width > editable.CurrentLabelWidth)
                        editable.CurrentLabelWidth = width;
                }
                else
                    GUILayout.Label(name, GUILayout.Width(EditorGUIUtility.labelWidth));
            }
            else
            {
            }

            if (value.IsConstant && !isValueNonConstant)
            {
                if (types != null && types.Length == 1)
                    value.Type = ((ValueTypeAttribute)types[0]).Type;

                if (isTypeConstant)
                {
                    BrainEditorUtil.EditValueWithStyle(ref value, false);
                    adjustPropertyWidth(editable, ref value);
                }
                else
                {
                    GUILayout.BeginVertical();
                    BrainEditorUtil.EditValueWithStyle(ref value, false);
                    adjustPropertyWidth(editable, ref value);

                    GUILayout.BeginHorizontal();

                    GUILayout.Label(new GUIContent(), GUILayout.Width(45));

                    var dropDownStyle = BrainEditorUtil.GetStyle("DropDown");

                    if (types != null && types.Length > 1)
                    {
                        var strings = new string[types.Length];
                        var selectedIndex = 0;

                        for (int i = 0; i < strings.Length; i++)
                        {
                            var t = ((ValueTypeAttribute)types[i]).Type;

                            if (value.Type == t)
                                selectedIndex = i;

                            strings[i] = t.ToString();
                        }

                        var newIndex = EditorGUILayout.Popup(selectedIndex, strings, dropDownStyle);

                        if (newIndex != selectedIndex)
                            value.Type = ((ValueTypeAttribute)types[newIndex]).Type;
                    }
                    else if (isTypeArithmetic)
                        value.Type = (AI.ValueType)EditorGUILayout.EnumPopup((ArithmeticValueType)value.Type, dropDownStyle);
                    else
                        value.Type = (AI.ValueType)EditorGUILayout.EnumPopup(value.Type, dropDownStyle);

                    var typeWidth = dropDownStyle.CalcSize(new GUIContent(value.Type.ToString())).x;

                    adjustPropertyWidth(editable, typeWidth + 50);

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
            }
            else
            {
                string text;

                if (value.ID >= 0)
                {
                    var variable = brain.GetVariable(value.ID);

                    if (variable != null)
                    {
                        GUILayout.Label(variable.Name);
                        text = variable.Name;
                    }
                    else
                    {
                        text = brain.GetText(value.ID);
                        GUILayout.Label(text);
                    }
                }
                else
                {
                    text = ((BuiltinValue)(-value.ID)).ToString();
                    GUILayout.Label(text);
                }

                if (editable != null)
                {
                    var width = GUI.skin.label.CalcSize(new GUIContent(text)).x;
                    adjustPropertyWidth(editable, width);
                }
            }

            variableButton(brain, sourceId, index, types, isTypeArithmetic);

            if (Event.current.type == EventType.Repaint)
            {
                var last = GUILayoutUtility.GetLastRect();
                var size = BrainEditorUtil.ValueInputSize;
                value.LocalEditorRect = new Rect(-size, last.y + (last.height - size) * 0.5f, size, size);
            }

            GUILayout.EndHorizontal();

            index++;
        }

        private void adjustPropertyWidth(Editable editable, ref Value value)
        {
            var propertyWidth = editable.LabelWidth + BrainEditorUtil.GetValueWidth(ref value) + BrainEditorUtil.PropertyExtraWidth;

            if (propertyWidth > editable.CurrentPropertyWidth)
                editable.CurrentPropertyWidth = propertyWidth;
        }

        private void adjustPropertyWidth(Editable editable, float valueWidth)
        {
            var propertyWidth = editable.LabelWidth + valueWidth + BrainEditorUtil.PropertyExtraWidth;

            if (propertyWidth > editable.CurrentPropertyWidth)
                editable.CurrentPropertyWidth = propertyWidth;
        }

        private void adjustPropertyWidthByName(Editable editable, string name)
        {
            var width = GUI.skin.label.CalcSize(new GUIContent(name)).x + BrainEditorUtil.PropertyExtraWidth;

            if (width > editable.PropertyWidth)
                editable.PropertyWidth = width;
        }

        private void editVariableReference(Brain brain, Editable editable, object attachment, FieldInfo field, bool noFieldName)
        {
            var reference = (VariableReference)field.GetValue(attachment);
            var variable = brain.GetVariable(reference.ID);

            GUILayout.BeginHorizontal();

            if (!noFieldName)
                GUILayout.Label(field.Name, GUILayout.Width(editable.LabelWidth));

            if (GUILayout.Button(variable == null ? "" : variable.Name))
            {
                var menu = new GenericMenu();

                foreach (var variableId in brain.Variables.Keys)
                {
                    var v = brain.Variables[variableId];

                    if (v.Class != VariableClass.Parameter)
                        menu.AddItem(new GUIContent(v.Name), false, () => { field.SetValue(attachment, new VariableReference(variableId)); });
                }

                show(menu);
            }

            GUILayout.EndHorizontal();
        }

        private void editTriggerReference(Brain brain, Editable editable, object attachment, FieldInfo field, bool noFieldName)
        {
            var reference = (TriggerReference)field.GetValue(attachment);
            var trigger = brain.GetTrigger(reference.ID);

            GUILayout.BeginHorizontal();

            if (!noFieldName)
                GUILayout.Label(field.Name, GUILayout.Width(editable.LabelWidth));

            if (GUILayout.Button(trigger == null ? "" : trigger.Name))
            {
                if (brain.Triggers.Count > 0)
                {
                    var menu = new GenericMenu();

                    foreach (var variableId in brain.Triggers.Keys)
                        menu.AddItem(new GUIContent(brain.Triggers[variableId].Name), false, () => { field.SetValue(attachment, new TriggerReference(variableId)); });

                    show(menu);
                }
            }

            var width = GUI.skin.button.CalcSize(new GUIContent(trigger == null ? "none" : trigger.Name)).x + BrainEditorUtil.PropertyExtraWidth;

            if (width > editable.CurrentPropertyWidth)
                editable.CurrentPropertyWidth = width;

            GUILayout.EndHorizontal();
        }

        private void editEvent(Brain brain, Editable editable, object attachment, FieldInfo field, bool noFieldName)
        {
            var e = (AIEvent)field.GetValue(attachment);

            GUILayout.BeginHorizontal();

            if (!noFieldName)
                GUILayout.Label(field.Name, GUILayout.Width(editable.LabelWidth));

            BrainEditorUtil.EditEvent(ref e);
            field.SetValue(attachment, e);

            var width = Mathf.Max(100, editable.LabelWidth) + BrainEditorUtil.GetEventWidth(e) + BrainEditorUtil.PropertyExtraWidth;

            if (width > editable.CurrentPropertyWidth)
                editable.CurrentPropertyWidth = width;

            GUILayout.EndHorizontal();
        }

        private bool matchType(Attribute[] types, bool isTypeArithmetic, CoverShooter.AI.ValueType type)
        {
            if (type == AI.ValueType.Unknown)
                return true;

            if (types != null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] is ValueTypeAttribute)
                        if (((ValueTypeAttribute)types[i]).Type == type)
                            return true;
                }

                if (types.Length > 0)
                    return false;
            }

            if (isTypeArithmetic)
                return type == AI.ValueType.Float ||
                       type == AI.ValueType.Vector2 ||
                       type == AI.ValueType.Vector3;

            return true;
        }

        private void variableButton(Brain brain, int id, int index, Attribute[] types, bool isTypeArithmetic)
        {
            if (BrainEditorUtil.VariableButton())
            {
                var menu = new GenericMenu();

                foreach (BuiltinValue builtin in Enum.GetValues(typeof(BuiltinValue)))
                {
                    if (!matchType(types, isTypeArithmetic, State.GetBuiltinValueType(builtin)))
                        continue;

                    menu.AddItem(new GUIContent("None"), false, () =>
                    {
                        _variableTarget.ID = id;
                        _variableTarget.Index = index;
                        _variableTarget.HasValue = true;
                        _variableTarget.Value = 0;
                    });

                    menu.AddItem(new GUIContent("Builtin/" + builtin.ToString()), false, () =>
                    {
                        _variableTarget.ID = id;
                        _variableTarget.Index = index;
                        _variableTarget.HasValue = true;
                        _variableTarget.Value = -(int)builtin;
                    });

                }

                foreach (var variableId in brain.Variables.Keys)
                {
                    var v = brain.Variables[variableId];

                    if (v.Class != VariableClass.Parameter && matchType(types, isTypeArithmetic, v.Value.Type))
                        menu.AddItem(new GUIContent(v.Name), false, () =>
                        {
                            _variableTarget.ID = id;
                            _variableTarget.Index = index;
                            _variableTarget.HasValue = true;
                            _variableTarget.Value = variableId;
                        });
                }

                show(menu);
            }
        }

        private void editOrDrawEntryTriggers(AIController controller, Brain brain, int activeLayer, int superNode, int id, EntryNode node, Rect window, bool isInWindow)
        {
            if (node.Init == 0)
                node.Init = brain.AddNodeTrigger("Init");

            editTrigger(controller, brain, activeLayer, superNode, null, id, false, node.Init, brain.GetNodeTrigger(node.Init), window, isInWindow);
        }

        private void editOrDrawActionTriggers(AIController controller, Brain brain, int activeLayer, int superNode, int id, ActionNode node, Rect window, bool isInWindow)
        {
            if (node.SuccessTrigger != 0)
                editTrigger(controller, brain, activeLayer, superNode, node, id, false, node.SuccessTrigger, brain.GetNodeTrigger(node.SuccessTrigger), window, isInWindow);

            if (node.FailureTrigger != 0)
                editTrigger(controller, brain, activeLayer, superNode, node, id, false, node.FailureTrigger, brain.GetNodeTrigger(node.FailureTrigger), window, isInWindow);

            if (node.HasLimitedTime)
            {
                var tid = node.TimeOutTrigger;

                if (!editTrigger(controller, brain, activeLayer, superNode, node, id, true, tid, brain.GetNodeTrigger(tid), window, isInWindow))
                {
                    node.HasLimitedTime = false;
                    node.RemoveTimeOutTrigger(brain);
                }
            }
        }

        private void editOrDrawCustomTriggers(AIController controller, Brain brain, int activeLayer, int superNode, int id, TriggeredNode node, Rect window, bool canAddTriggers, bool isInWindow)
        {
            var triggerToRemove = -1;

            if (node.Triggers != null)
            {
                var action = node as ActionNode;

                for (int i = 0; i < node.Triggers.Length; i++)
                    if (!editTrigger(controller, brain, activeLayer, superNode, action, id, true, node.Triggers[i], brain.GetNodeTrigger(node.Triggers[i]), window, isInWindow))
                        triggerToRemove = i;
            }

            if (triggerToRemove >= 0)
                brain.RemoveNodeTrigger(node.Triggers[triggerToRemove]);

            if (canAddTriggers)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (BrainEditorUtil.AddButton())
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Is True"), false, () => node.AddTrigger(brain.AddNodeExpressionTrigger("Is True", true)));
                    menu.AddItem(new GUIContent("Is False"), false, () => node.AddTrigger(brain.AddNodeExpressionTrigger("Is False", false)));

                    var action = node as ActionNode;

                    if (action != null)
                        if (!action.HasLimitedTime && !action.IsImmediate() && !(action.Action is Wait))
                        {
                            menu.AddItem(new GUIContent("Time Out"), false, () => 
                            {
                                action.HasLimitedTime = true;
                                action.AddTimeOutTrigger(brain);
                            }); 
                        }

                    foreach (var customId in brain.Triggers.Keys)
                    {
                        var alreadyHas = false;

                        if (node.Triggers != null)
                            for (int i = 0; i < node.Triggers.Length; i++)
                            {
                                var t = brain.GetNodeTrigger(node.Triggers[i]);

                                if (t.Type == NodeTriggerType.Custom && t.ID == customId)
                                {
                                    alreadyHas = true;
                                    break;
                                }
                            }

                        if (!alreadyHas)
                            menu.AddItem(new GUIContent(brain.Triggers[customId].Name), false, () => node.AddTrigger(brain.AddNodeCustomTrigger(brain.Triggers[customId].Name, customId)));
                    }

                    foreach (AIEvent type in Enum.GetValues(typeof(AIEvent)))
                    {
                        var alreadyHas = false;

                        if (node.Triggers != null)
                            for (int i = 0; i < node.Triggers.Length; i++)
                            {
                                var t = brain.GetNodeTrigger(node.Triggers[i]);

                                if (t.Type == NodeTriggerType.Event && t.Event == type)
                                {
                                    alreadyHas = true;
                                    break;
                                }
                            }

                        if (!alreadyHas)
                            menu.AddItem(new GUIContent("Event/" + type.ToString()), false, () => node.AddTrigger(brain.AddNodeTrigger(type.ToString(), type)));
                    }

                    show(menu);
                }

                GUILayout.EndHorizontal();
            }
        }

        private bool editTrigger(AIController controller, Brain brain, int activeLayer, int superNode, ActionNode owningActionNode, int ownerId, bool canRemove, int id, NodeTrigger trigger, Rect window, bool isInWindow)
        {
            if (trigger == null)
                return false;

            trigger.Owner = ownerId;
            int valueIndex = 0;

            if (!isInWindow)
            {
                var outputSlot = new Rect(window.x + trigger.LocalEditorRect.x,
                                          window.y + trigger.LocalEditorRect.y,
                                          trigger.LocalEditorRect.width,
                                          trigger.LocalEditorRect.height);

                {
                    if (_connector != null && outputSlot.Contains(Event.current.mousePosition))
                        _connector.Check(LocationType.Trigger);

                    if (BrainEditorUtil.ActionOutput(outputSlot, trigger.Target != 0))
                    {
                        if (_connector != null)
                        {
                            _connector.AcceptId(brain, id);
                            _connector = null;
                        }
                        else
                            _connector = new ToAction(Location.Trigger(id));
                    }
                }

                if (owningActionNode != null && id == owningActionNode.TimeOutTrigger)
                    editOrDrawValue(brain, activeLayer, superNode, trigger, id, "Time Limit", ref valueIndex, ref owningActionNode.TimeLimit, false, true, null, true, isInWindow, window);

                if (trigger.Type == NodeTriggerType.Expression)
                    editOrDrawValue(brain, activeLayer, superNode, trigger, id, null, ref valueIndex, ref trigger.Expression, true, true, null, false, isInWindow, window);

                if (trigger.Target != 0 && (_connector == null || _connector.Source.Id != id))
                    drawTriggerConnection(brain, getLayer(brain, activeLayer), superNode, _locator.GetRightOutput(outputSlot), trigger.Target);

                if (trigger.Values != null && trigger.Values.Length > 0)
                {
                    for (int i = 0; i < trigger.Values.Length; i++)
                    {
                        var variable = brain.GetVariable(trigger.Values[i]);
                        var r = new Rect(window.x + variable.LocalEditorRect.x,
                                         window.y + variable.LocalEditorRect.y,
                                         variable.LocalEditorRect.width,
                                         variable.LocalEditorRect.height);

                        if (_connector != null && r.Contains(Event.current.mousePosition))
                            _connector.Check(LocationType.TriggerVariable);

                        if (BrainEditorUtil.ExpressionOutput(r, variable.HasConnection))
                        {
                            if (_connector != null)
                            {
                                _connector.AcceptId(brain, trigger.Values[i]);
                                _connector = null;
                            }
                            else
                                _connector = new ToValue(Location.TriggerVariable(trigger.Values[i]));
                        }

                        variable.HasConnection = false;
                    }
                }

                return true;
            }

            var isTimeOut = owningActionNode != null && id == owningActionNode.TimeOutTrigger;

            var hasValues = isTimeOut || 
                            trigger.Type == NodeTriggerType.Expression ||
                            (trigger.Values != null && trigger.Values.Length > 0);

            var toBeRemoved = false;

            if (hasValues)
            {
                GUILayout.BeginVertical(BrainEditorUtil.GetTitledBox());

                if (!BrainEditorUtil.BoxTitle(trigger.Name, canRemove))
                    toBeRemoved = true;
            }
            else
            {
                if (!BrainEditorUtil.NoContentBox(trigger.Name, canRemove))
                    toBeRemoved = true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUILayoutUtility.GetLastRect();
                var size = BrainEditorUtil.ActionOutputSize;
                trigger.LocalEditorRect = new Rect(window.width - 3, rect.y + (rect.height - size) * 0.5f, size, size);
            }

            trigger.CurrentLabelWidth = 0;
            trigger.CurrentPropertyWidth = 0;

            if (isTimeOut)
                editOrDrawValue(brain, activeLayer, superNode, trigger, id, "Time Limit", ref valueIndex, ref owningActionNode.TimeLimit, false, true, null, true, isInWindow, window);

            if (trigger.Type == NodeTriggerType.Expression)
                editOrDrawValue(brain, activeLayer, superNode, trigger, id, null, ref valueIndex, ref trigger.Expression, true, true, null, false, isInWindow, window);

            if (trigger.Values != null && trigger.Values.Length > 0)
            {
                for (int i = 0; i < trigger.Values.Length; i++)
                {
                    var variable = brain.GetVariable(trigger.Values[i]);

                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();

                    var text = variable != null ? variable.Name : "!unknown parameter!";
                    GUILayout.Label(text);

                    var width = GUI.skin.label.CalcSize(new GUIContent(text)).x + BrainEditorUtil.PropertyExtraWidth;

                    if (width > trigger.CurrentPropertyWidth)
                        trigger.CurrentPropertyWidth = width;

                    if (Event.current.type == EventType.Repaint)
                    {
                        var rect = GUILayoutUtility.GetLastRect();
                        var size = BrainEditorUtil.ExpressionOutputSize;
                        variable.LocalEditorRect = new Rect(window.width - 3, rect.y + (rect.height - size) * 0.5f, size, size);
                        variable.ScreenEditorRect = variable.LocalEditorRect;
                        variable.ScreenEditorRect.x += window.x;
                        variable.ScreenEditorRect.y += window.y;
                        variable.OwningNode = ownerId;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            trigger.LabelWidth = trigger.CurrentLabelWidth;
            trigger.PropertyWidth = trigger.CurrentPropertyWidth;
            adjustPropertyWidthByName(trigger, trigger.Name);

            if (hasValues)
                GUILayout.EndVertical();

            return !toBeRemoved;
        }

        private void drawValueConnection(Brain brain, Layer layer, int superNode, Vector2 input, int target, Attribute[] types, bool isTypeArithmetic)
        {
            CurveEnd end;

            var isOk = true;
            var expression = brain.GetExpression(target);

            if (expression != null)
            {
                end = _locator.Locate(expression, input);
                isOk = matchType(types, isTypeArithmetic, expression.Expression.GetReturnType(brain));
            }
            else
            {
                var variable = brain.GetVariable(target);

                if (variable != null && variable.Class == VariableClass.Parameter)
                {
                    end = _locator.Locate(brain, layer, superNode, variable, input);
                    isOk = matchType(types, isTypeArithmetic, variable.Value.Type);
                }
                else
                    return;
            }

            var start = _locator.LocateLeft(input, end.Position);
            var color = isOk ? new Color(82, 0, 255, 1) : Color.red;

            Handles.DrawBezier(start.Position, end.Position, start.Tangent, end.Tangent, color, null, 4);

            if (!isOk)
            {
                var width = 100;
                var height = 20;
                GUI.Label(new Rect(start.Position.x - width, start.Position.y - height * 0.5f, width, height), "Type mismatch", BrainEditorUtil.Skin.label);
            }
        }

        private void drawTriggerConnection(Brain brain, Layer layer, int superNode, Vector2 output, int target)
        {
            var end = _locator.Locate(brain, layer, superNode, Location.Action(target), output);
            var start = _locator.LocateRight(output, end.Position);

            Handles.DrawBezier(start.Position, end.Position, start.Tangent, end.Tangent, Color.white, null, 4);
        }

        private void drawConnector(Brain brain, Layer layer, int superNode, Vector2 position)
        {
            if (_connector == null)
                return;

            var start = _locator.Locate(brain, layer, superNode, _connector.Source, position);
            var endPosition = position;

            var color = Color.red;

            if (_connector.IsValid)
                switch (_connector.Source.Type)
                {
                    case LocationType.Action:
                    case LocationType.Trigger:
                        color = Color.white;
                        break;

                    default:
                        color = new Color(82, 0, 255, 1);
                        break;
                }

            Handles.DrawBezier(start.Position, endPosition, start.Tangent, endPosition, color, null, 4);
        }

        private int getNodeBelow(Brain brain, int activeLayer, int superNode, Vector2 position)
        {
            int node = 0;
            var layer = getLayer(brain, activeLayer);

            if (layer != null)
            {
                var superAction = brain.GetAction(superNode);

                if (superAction != null && superAction.Exit != null)
                    if (_locator.ToScreen(superAction.Exit).Contains(position))
                        node = State.ExitID;

                if (superAction != null && superAction.Fail != null)
                    if (_locator.ToScreen(superAction.Fail).Contains(position))
                        node = State.FailID;

                if (layer.Comments != null)
                    for (int i = 0; i < layer.Comments.Length; i++)
                    {
                        var comment = brain.GetComment(layer.Comments[i]);

                        if (comment != null && comment.Parent == superNode)
                            if (_locator.ToScreen(comment).Contains(position))
                            {
                                node = layer.Comments[i];
                                break;
                            }
                    }

                if (layer.Actions != null)
                    for (int i = 0; i < layer.Actions.Length; i++)
                    {
                        var action = brain.GetAction(layer.Actions[i]);

                        if (action != null && action.Parent == superNode)
                            if (_locator.ToScreen(action).Contains(position))
                            {
                                node = layer.Actions[i];
                                break;
                            }
                    }

                if (layer.Expressions != null)
                    for (int i = 0; i < layer.Expressions.Length; i++)
                    {
                        var expression = brain.GetExpression(layer.Expressions[i]);

                        if (expression != null && expression.Parent == superNode)
                            if (_locator.ToScreen(expression).Contains(position))
                            {
                                node = layer.Expressions[i];
                                break;
                            }
                    }
            }

            return node;
        }

        private void drawGrid(Rect screen, float spacing, Color color)
        {
            var width = screen.width / Scale;
            var height = screen.height / Scale;
            var columnCount = Mathf.CeilToInt(width / spacing);
            var rowCount = Mathf.CeilToInt(height / spacing);

            Handles.color = color;
            Handles.BeginGUI();

            var worldOrigin = _locator.ToWorld(0, 0);
            worldOrigin.x = Mathf.Floor(worldOrigin.x / spacing) * spacing;
            worldOrigin.y = Mathf.Floor(worldOrigin.y / spacing) * spacing;

            var origin = _locator.ToScreen(worldOrigin.x, worldOrigin.y);

            for (int x = 0; x < columnCount; x++)
            {
                var a = new Vector3(origin.x + spacing * x, -spacing, 0);
                var b = new Vector3(origin.x + spacing * x, height + spacing, 0);
                Handles.DrawLine(a, b);
            }

            for (int y = 0; y < rowCount; y++)
            {
                var a = new Vector3(-spacing, origin.y + spacing * y, 0);
                var b = new Vector3(width + spacing, origin.y + spacing * y, 0);
                Handles.DrawLine(a, b);
            }

            Handles.EndGUI();
        }

        private Layer getLayer(Brain brain, int activeLayer)
        {
            return (brain != null && brain.Layers != null && activeLayer >= 0 && activeLayer < brain.Layers.Length) ? brain.Layers[activeLayer] : null;
        }

        private void show(GenericMenu menu)
        {
            var m = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;

            menu.DropDown(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0));

            GUI.matrix = m;
        }
    }
}