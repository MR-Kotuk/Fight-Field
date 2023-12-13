using UnityEngine;
using CoverShooter.AI;

namespace CoverShooter
{
    public struct CurveEnd
    {
        public Vector2 Position;
        public Vector2 Tangent;

        public CurveEnd(Vector2 position, Vector2 tangent)
        {
            Position = position;
            Tangent = tangent;
        }
    }

    public class BrainLocator
    {
        public const float Tangent = 100;

        /// <summary>
        /// Grid position inside the editor.
        /// </summary>
        public Vector2 Position;

        private Rect _totalArea;
        private bool _hasTotalArea;

        public void ClampPosition(Rect screen)
        {
            if (!_hasTotalArea)
                return;

            if (Position.x + screen.width < _totalArea.x) Position.x = _totalArea.x - screen.width;
            if (Position.y + screen.height < _totalArea.y) Position.y = _totalArea.y - screen.height;

            if (Position.x > _totalArea.x + _totalArea.width) Position.x = _totalArea.x + _totalArea.width;
            if (Position.y > _totalArea.y + _totalArea.height) Position.y = _totalArea.y + _totalArea.height;
        }

        public void UpdateTotalArea(Brain brain, int activeLayer, int superNode)
        {
            _totalArea = new Rect(-200, 200, 400, 400);
            _hasTotalArea = true;

            var layer = (brain != null && brain.Layers != null && activeLayer >= 0 && activeLayer < brain.Layers.Length) ? brain.Layers[activeLayer] : null;

            if (layer == null)
                return;

            var entry = brain.GetEntry(layer, superNode);
            if (entry != null) addToTotalArea(ToWorld(entry));

            var any = brain.GetAny(layer, superNode);
            if (any != null) addToTotalArea(ToWorld(any));

            var exit = brain.GetExit(superNode);
            if (exit != null) addToTotalArea(ToWorld(entry));

            var fail = brain.GetFail(superNode);
            if (fail != null) addToTotalArea(ToWorld(entry));

            if (layer.Actions != null)
                for (int i = 0; i < layer.Actions.Length; i++)
                {
                    var id = layer.Actions[i];
                    var node = brain.GetAction(id);

                    if (node != null)
                        addToTotalArea(ToWorld(node));
                }

            if (layer.Expressions != null)
                for (int i = 0; i < layer.Expressions.Length; i++)
                {
                    var id = layer.Expressions[i];
                    var node = brain.GetExpression(id);

                    if (node != null)
                        addToTotalArea(ToWorld(node));
                }

            _hasTotalArea = true;
        }

        private void addToTotalArea(Rect r)
        {
            if (r.x < _totalArea.x)
            {
                _totalArea.width += _totalArea.x - r.x;
                _totalArea.x = r.x;
            }

            if (r.y < _totalArea.y)
            {
                _totalArea.height += _totalArea.y - r.y;
                _totalArea.y = r.y;
            }

            if (r.x + r.width > _totalArea.x + _totalArea.width)
                _totalArea.width = r.x + r.width - _totalArea.x;

            if (r.y + r.height > _totalArea.y + _totalArea.height)
                _totalArea.height = r.y + r.height - _totalArea.y;
        }

        public CurveEnd LocateUp(Vector2 position, Vector2 end)
        {
            var tangent = Tangent;
            optimizeTangent(position, end, ref tangent);

            return new CurveEnd(position, position - Vector2.up * tangent);
        }

        public CurveEnd LocateLeft(Vector2 position, Vector2 end)
        {
            var tangent = Tangent;
            optimizeTangent(position, end, ref tangent);

            return new CurveEnd(position, position + Vector2.left * tangent);
        }

        public CurveEnd LocateRight(Vector2 position, Vector2 end)
        {
            var tangent = Tangent;
            optimizeTangent(position, end, ref tangent);

            return new CurveEnd(position, position + Vector2.right * tangent);
        }

        public CurveEnd LocateDown(Vector2 position, Vector2 end)
        {
            var tangent = Tangent;
            optimizeTangent(position, end, ref tangent);

            return new CurveEnd(position, position - Vector2.down * tangent);
        }

        public CurveEnd Locate(ExpressionNode expression, Vector2 end)
        {
            if (expression == null)
                return new CurveEnd();

            var rect = ToScreen(expression);
            var size = BrainEditorUtil.ExpressionOutputSize;

            var right = GetRightOutput(GetRightSlot(rect, size));
            var top = GetTopOutput(GetTopSlot(rect, size));
            var bottom = GetBottomOutput(GetBottomSlot(rect, size));

            var rightd = Vector2.Distance(end, right);
            var topd = Vector2.Distance(end, top);
            var bottomd = Vector2.Distance(end, bottom);

            if (end.x + size > right.x && rightd <= topd && rightd <= bottomd)
            {
                expression.HasEditorRightConnection = true;
                return LocateRight(right, end);
            }
            else if (topd <= bottomd)
            {
                expression.HasEditorTopConnection = true;
                return LocateUp(top, end);
            }
            else
            {
                expression.HasEditorBottomConnection = true;
                return LocateDown(bottom, end);
            }
        }

        public CurveEnd Locate(Brain brain, Layer layer, int superNode, Variable variable, Vector2 end)
        {
            if (variable == null)
                return new CurveEnd();

            variable.HasConnection = true;

            var window = new Rect();
            if (!ToScreenAction(brain, layer, superNode, variable.OwningNode, ref window))
                return new CurveEnd();

            var r = variable.LocalEditorRect;
            var position = new Vector2(window.x + r.x + r.width,
                                       window.y + r.y + r.height * 0.5f);
            return new CurveEnd(position, position + Vector2.right * Tangent);
        }

        public CurveEnd Locate(Brain brain, Layer layer, int superNode, Location location, Vector2 end)
        {
            switch (location.Type)
            {
                case LocationType.Action:
                    {
                        var rect = new Rect();
                        if (!ToScreenAction(brain, layer, superNode, location.Id, ref rect))
                            return new CurveEnd();

                        var action = brain.GetAction(location.Id);

                        LayerNode node = action;

                        if (node == null && location.Id < 0)
                        {
                            if (location.Id == State.EntryID)
                                node = brain.GetEntry(layer, superNode);
                            else if (location.Id == State.AnyID)
                                node = brain.GetAny(layer, superNode);
                            else if (location.Id == State.ExitID)
                                node = brain.GetExit(superNode);
                            else if (location.Id == State.FailID)
                                node = brain.GetFail(superNode);
                        }

                        var size = BrainEditorUtil.ActionInputSize;

                        var left = GetLeftOutput(action != null ? GetLeftSlot(action, rect, size) : GetLeftSlot(rect, size));
                        var top = GetTopOutput(GetTopSlot(rect, size));
                        var bottom = GetBottomOutput(GetBottomSlot(rect, size));

                        var leftd = Vector2.Distance(end, left);
                        var topd = Vector2.Distance(end, top);
                        var bottomd = Vector2.Distance(end, bottom);

                        if (end.x < left.x + size && leftd <= topd && leftd <= bottomd)
                        {
                            node.HasEditorLeftConnection = true;
                            return LocateLeft(left, end);
                        }
                        else if (topd <= bottomd)
                        {
                            node.HasEditorTopConnection = true;
                            return LocateUp(top, end);
                        }
                        else
                        {
                            node.HasEditorBottomConnection = true;
                            return LocateDown(bottom, end);
                        }
                    }

                case LocationType.Expression:
                    return Locate(brain.GetExpression(location.Id), end);

                case LocationType.ActionValue:
                    {
                        var action = brain.GetAction(location.Id);

                        if (action == null)
                            return new CurveEnd();

                        var window = ToScreen(action);
                        var value = action.GetValue(location.Index);
                        var r = value.LocalEditorRect;
                        r.x += window.x;
                        r.y += window.y;
                        var position = new Vector2(r.x,
                                                   r.y + r.height * 0.5f);
                        return LocateLeft(position, end);
                    }

                case LocationType.ExtensionValue:
                    {
                        var extension = brain.GetExtension(location.Id);

                        if (extension == null)
                            return new CurveEnd();

                        var node = brain.GetAction(extension.Owner);

                        if (node == null)
                            return new CurveEnd();

                        var window = ToScreen(node);

                        var value = extension.GetValue(location.Index);
                        var r = value.LocalEditorRect;
                        r.x += window.x;
                        r.y += window.y;
                        var position = new Vector2(r.x,
                                                   r.y + r.height * 0.5f);
                        return LocateLeft(position, end);
                    }

                case LocationType.Trigger:
                    {
                        var trigger = brain.GetNodeTrigger(location.Id);

                        if (trigger == null)
                            return new CurveEnd();

                        var window = new Rect();
                        if (!ToScreenAction(brain, layer, superNode, trigger.Owner, ref window))
                            return new CurveEnd();

                        var r = trigger.LocalEditorRect;
                        r.x += window.x;
                        r.y += window.y;
                        var position = new Vector2(r.x + r.width,
                                                   r.y + r.height * 0.5f);
                        return LocateRight(position, end);
                    }

                case LocationType.TriggerInput:
                    {
                        var trigger = brain.GetNodeTrigger(location.Id);

                        if (trigger == null)
                            return new CurveEnd();

                        var window = new Rect();
                        if (!ToScreenAction(brain, layer, superNode, trigger.Owner, ref window))
                            return new CurveEnd();

                        var r = trigger.Expression.LocalEditorRect;
                        r.x += window.x;
                        r.y += window.y;
                        var position = new Vector2(r.x,
                                                   r.y + r.height * 0.5f);
                        return LocateLeft(position, end);
                    }

                case LocationType.TriggerVariable:
                    return Locate(brain, layer, superNode, brain.GetVariable(location.Id), end);

                case LocationType.ExpressionValue:
                    {
                        var expression = brain.GetExpression(location.Id);

                        if (expression == null)
                            return new CurveEnd();

                        var window = ToScreen(expression);

                        var value = expression.GetValue(location.Index);
                        var r = value.LocalEditorRect;
                        r.x += window.x;
                        r.y += window.y;
                        var position = new Vector2(r.x, r.y + r.height * 0.5f);
                        return LocateLeft(position, end);
                    }

                default:
                    Debug.Assert(false);
                    return new CurveEnd();
            }
        }

        private void optimizeTangent(Vector3 a, Vector3 b, ref float tangent)
        {
            var dist = Vector3.Distance(a, b);

            if (dist < tangent * 3)
                tangent = dist / 3;
        }

        public bool ToScreenAction(Brain brain, Layer layer, int superNode, int id, ref Rect result)
        {
            if (id == State.EntryID)
            {
                var entry = brain.GetEntry(layer, superNode);
                if (entry == null) return false;
                result = ToScreen(entry);
                return true;
            }
            else if (id == State.AnyID)
            {
                var any = brain.GetAny(layer, superNode);
                if (any == null) return false;
                result = ToScreen(any);
                return true;
            }
            else if (id == State.ExitID)
            {
                var exit = brain.GetExit(superNode);
                if (exit == null) return false;
                result = ToScreen(exit);
                return true;
            }
            else if (id == State.FailID)
            {
                var fail = brain.GetFail(superNode);
                if (fail == null) return false;
                result = ToScreen(fail);
                return true;
            }
            else
            {
                var action = brain.GetAction(id);
                if (action == null) return false;
                result = ToScreen(action);
                return true;
            }
        }

        public Rect ToWorld(LayerNode node)
        {
            return ToWorld(node.EditorPosition, node.EditorWidth, node.EditorHeight);
        }

        public Rect ToWorld(Vector2 position, float width, float height)
        {
            var origin = new Vector2(position.x - width * 0.5f, position.y);
            return new Rect(origin.x, origin.y, width, height);
        }

        public Rect ToScreen(LayerNode node)
        {
            return ToScreen(node.EditorPosition, node.EditorWidth, node.EditorHeight);
        }

        public Rect GetLeftSlot(ActionNode node, Rect rect, float size)
        {
            return new Rect(rect.x - size, rect.y + size * 0.5f, size, size);
        }

        public Rect GetLeftSlot(Rect rect, float size)
        {
            return new Rect(rect.x - size, rect.y + rect.height * 0.5f - size * 0.5f, size, size);
        }

        public Rect GetTopSlot(Rect rect, float size)
        {
            return new Rect(rect.x + rect.width * 0.5f - size * 0.5f, rect.y - size, size, size);
        }

        public Rect GetRightSlot(Rect rect, float size)
        {
            return new Rect(rect.x + rect.width, rect.y + rect.height * 0.5f - size * 0.5f, size, size);
        }

        public Rect GetBottomSlot(Rect rect, float size)
        {
            return new Rect(rect.x + rect.width * 0.5f - size * 0.5f, rect.y + rect.height, size, size);
        }

        public Vector2 GetTopOutput(Rect rect)
        {
            return new Vector2(rect.x + rect.width * 0.5f, rect.y);
        }

        public Vector2 GetLeftOutput(Rect rect)
        {
            return new Vector2(rect.x, rect.y + rect.height * 0.5f);
        }

        public Vector2 GetRightOutput(Rect rect)
        {
            return new Vector2(rect.x + rect.width, rect.y + rect.height * 0.5f);
        }

        public Vector2 GetBottomOutput(Rect rect)
        {
            return new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height);
        }

        public Rect ToScreen(Vector2 position, float width, float height)
        {
            var origin = ToScreen(position.x - width * 0.5f, position.y);
            return new Rect(origin.x, origin.y, width, height);
        }

        public Vector2 ToScreen(float x, float y)
        {
            return new Vector2(x - Position.x, y - Position.y);
        }

        public Vector2 ToWorld(float x, float y)
        {
            return new Vector2(x + Position.x, y + Position.y);
        }
    }
}