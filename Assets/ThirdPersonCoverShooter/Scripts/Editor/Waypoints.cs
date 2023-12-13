using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(Waypoints))]
    [CanEditMultipleObjects]
    public class WaypointsEditor : Editor
    {
        public bool IsAdditive = false;

        private static int _editorHash = "WaypointsEditor".GetHashCode();

        private Waypoints _lastSelectedComponent;
        private int _lastSelectedWaypoint;
        private bool _dontDrawPreview = false;
        private bool _wasMouseTooFarAway = false;

        protected virtual void OnSceneGUI()
        {
            var component = (Waypoints)target;
            var controlId = GUIUtility.GetControlID(_editorHash, FocusType.Passive);

            var hasMousePosition = false;
            var mousePosition = Vector3.zero;

            Undo.RecordObject(component, "Edit waypoints");

            Handles.BeginGUI();

            GUILayout.Window(2, new Rect(20, 30, 150, 100), (id) => {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mode:");

                if (IsAdditive)
                {
                    if (GUILayout.Button("Additive"))
                        IsAdditive = false;
                }
                else
                {
                    if (GUILayout.Button("Insertive"))
                        IsAdditive = true;
                }

                GUILayout.EndHorizontal();

                if (GUILayout.Button("Reverse"))
                    reverse(component);

                if (GUILayout.Button("Delete"))
                    deleteWaypoint(component, _lastSelectedWaypoint);
            }, "Waypoints");

            Handles.EndGUI();

            if (SceneView.currentDrawingSceneView.camera.pixelRect.Contains(HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition)))
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.25f)
                    {
                        mousePosition = hit.point;
                        hasMousePosition = true;
                    }
            }

            var current = Event.current;
            var hasToSelect = false;

            switch (current.GetTypeForControl(controlId))
            {
                case EventType.KeyDown:
                    if (current.keyCode == KeyCode.Delete)
                        if (_lastSelectedComponent == component && _lastSelectedComponent.Points != null && _lastSelectedWaypoint >= 0 && _lastSelectedWaypoint < _lastSelectedComponent.Points.Length)
                        {
                            deleteWaypoint(_lastSelectedComponent, _lastSelectedWaypoint);

                            while (_lastSelectedWaypoint >= _lastSelectedComponent.Points.Length)
                                _lastSelectedWaypoint--;

                            current.Use();
                        }
                    break;

                case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (current.button == 0)
                        if ((GUIUtility.hotControl == 0 && current.GetTypeForControl(controlId) != EventType.MouseDrag && HandleUtility.nearestControl == controlId) || 
                            GUIUtility.hotControl == controlId)
                        {
                            _dontDrawPreview = false;

                            if (hasMousePosition)
                            {
                                if (!_wasMouseTooFarAway)
                                    if (current.type == EventType.MouseDown)
                                    {
                                        GUIUtility.hotControl = controlId;
                                        hasToSelect = true;
                                    }

                                current.Use();
                            }
                        }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                        GUIUtility.hotControl = 0;
                    break;

                case EventType.MouseMove:
                    if (HandleUtility.nearestControl != controlId)
                    {
                        hasMousePosition = false;
                        _dontDrawPreview = !_wasMouseTooFarAway;
                    }
                    else
                        _dontDrawPreview = false;

                    SceneView.RepaintAll();
                    break;

                case EventType.Layout:
                    if (!_wasMouseTooFarAway)
                        HandleUtility.AddDefaultControl(controlId);
                    break;
            }

            var isHoveringPoint = false;

            var hasLowestPoint = false;
            var lowestPoint = 0f;

            if (component.Points != null)
            {
                for (int i = 0; i < component.Points.Length; i++)
                    if (!hasLowestPoint || component.Points[i].Position.y < lowestPoint)
                    {
                        hasLowestPoint = true;
                        lowestPoint = component.Points[i].Position.y;
                    }

                var oldColor = Handles.color;
                Handles.color = Color.magenta;

                for (int i = 0; i < component.Points.Length; i++)
                {
                    var bottom = component.Points[i].Position;
                    bottom.y = lowestPoint;

                    Handles.DrawLine(component.Points[i].Position, bottom);
                }

                Handles.color = oldColor;
            }

            if (component.Points != null)
            {
                var oldColor = Handles.color;
                Handles.color = Color.yellow;

                const float maxDistance = 0.5f;

                for (int i = 0; i < component.Points.Length; i++)
                {
                    if (i == 0 && component.Points.Length > 1)
                    {
                        var p0 = component.Points[0].Position;
                        var p1 = component.Points[1].Position;
                        var right = Vector3.Cross(p1 - p0, Vector3.up);

                        Handles.DrawLine(p1, p1 + Vector3.Lerp(p0 - p1, right, 0.35f) * 0.75f);
                        Handles.DrawLine(p1, p1 + Vector3.Lerp(p0 - p1, -right, 0.35f) * 0.75f);
                        Handles.DrawWireDisc(component.Points[i].Position, Vector3.up, maxDistance * 0.75f);
                    }

                    var isHovered = false;
                    var wasJustSelected = false;

                    if (hasMousePosition && !isHoveringPoint && (GUIUtility.hotControl == 0 || hasToSelect))
                    {
                        if (Vector3.Distance(mousePosition, component.Points[i].Position) < maxDistance)
                        {
                            isHovered = true;
                            isHoveringPoint = true;

                            if (!_dontDrawPreview)
                            {
                                Handles.color = Color.white;
                                Handles.DrawWireDisc(component.Points[i].Position, Vector3.up, maxDistance);
                                Handles.color = Color.yellow;
                            }

                            if (hasToSelect)
                            {
                                _lastSelectedWaypoint = i;
                                _lastSelectedComponent = component;
                                hasToSelect = false;
                                wasJustSelected = true;
                                EditorUtility.SetDirty(target);
                            }
                        }
                    }

                    if (!hasToSelect && !wasJustSelected)
                    {
                        if (_lastSelectedComponent == component && _lastSelectedWaypoint == i)
                        {
                            Handles.color = Color.green;
                            Handles.DrawWireDisc(component.Points[i].Position, Vector3.up, maxDistance);

                            if (hasMousePosition)
                                if (GUIUtility.hotControl == controlId)
                                    component.Points[i].Position = mousePosition;

                            Handles.color = Color.yellow;
                        }
                        else if (!isHovered)
                            Handles.DrawWireDisc(component.Points[i].Position, Vector3.up, maxDistance * 0.5f);
                    }

                    if (component.Points.Length > 1)
                    {
                        var next = component.Points[i].Position;

                        Vector3 previous;

                        if (i == 0)
                            previous = component.Points[component.Points.Length - 1].Position;
                        else
                            previous = component.Points[i - 1].Position;

                        Handles.DrawLine(previous, next);
                    }
                }

                Handles.color = oldColor;
            }

            var canPlacePoint = false;

            if (hasMousePosition && !isHoveringPoint && (GUIUtility.hotControl == 0 || hasToSelect))
            {
                var oldColor = Handles.color;
                Handles.color = Color.white;

                var indexInBetween = -1;
                var minDist = 0f;

                if (component.Points != null)
                {
                    const float maxDistance = 8;

                    if (component.Points.Length == 0)
                    {
                        indexInBetween = -1;
                        canPlacePoint = Vector3.Distance(mousePosition, component.transform.position) < maxDistance;
                    }
                    else if (component.Points.Length == 1)
                    {
                        indexInBetween = 0;
                        canPlacePoint = Vector3.Distance(mousePosition, component.Points[0].Position) < maxDistance;

                        if (canPlacePoint && !_dontDrawPreview)
                            Handles.DrawLine(mousePosition, component.Points[0].Position);
                    }
                    else if (IsAdditive)
                    {
                        for (int i = 0; i < component.Points.Length; i++)
                        {
                            var next = (i == component.Points.Length - 1) ? 0 : i + 1;

                            var p0 = component.Points[i].Position;
                            var p1 = component.Points[next].Position;
                            var dist = Vector3.Distance(mousePosition, Util.FindClosestToPath(p0, p1, mousePosition));

                            if (dist < maxDistance)
                            {
                                canPlacePoint = true;
                                break;
                            }
                        }

                        if (canPlacePoint)
                        {
                            indexInBetween = component.Points.Length - 1;

                            Handles.DrawLine(mousePosition, component.Points[indexInBetween].Position);
                            Handles.DrawLine(mousePosition, component.Points[0].Position);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < component.Points.Length; i++)
                        {
                            var next = (i == component.Points.Length - 1) ? 0 : i + 1;

                            var p0 = component.Points[i].Position;
                            var p1 = component.Points[next].Position;
                            var dist = Vector3.Distance(mousePosition, Util.FindClosestToPath(p0, p1, mousePosition));

                            if (dist < maxDistance)
                            {
                                if (indexInBetween < 0 || dist < minDist)
                                {
                                    indexInBetween = i;
                                    minDist = dist;
                                }
                            }
                        }

                        if (indexInBetween >= 0)
                        {
                            if (component.Points.Length == 2)
                                indexInBetween = 1;

                            canPlacePoint = true;

                            if (!_dontDrawPreview)
                            {
                                var second = (indexInBetween == component.Points.Length - 1) ? 0 : indexInBetween + 1;
                                Handles.DrawLine(mousePosition, component.Points[indexInBetween].Position);
                                Handles.DrawLine(mousePosition, component.Points[second].Position);
                            }
                        }
                    }
                }

                if (canPlacePoint)
                {
                    if (!_dontDrawPreview)
                    {
                        Handles.DrawWireDisc(mousePosition, Vector3.up, 0.5f);

                        if (hasLowestPoint)
                        {
                            Handles.color = Color.grey;
                            Handles.DrawLine(mousePosition, new Vector3(mousePosition.x, lowestPoint, mousePosition.z));
                            Handles.color = Color.white;
                        }
                    }

                    if (hasToSelect)
                    {
                        var index = indexInBetween + 1;

                        if (component.Points == null)
                            component.Points = new Waypoint[1];
                        else if (component.Points.Length == 1)
                        {
                            var old = component.Points[0];
                            component.Points = new Waypoint[2];
                            component.Points[0] = old;
                        }
                        else
                        {
                            var old = component.Points;
                            component.Points = new Waypoint[old.Length + 1];

                            for (int i = 0; i < index; i++)
                                component.Points[i] = old[i];

                            for (int i = index; i < old.Length; i++)
                                component.Points[i + 1] = old[i];
                        }

                        component.Points[index].Position = mousePosition;

                        if (index > 0)
                            component.Points[index].Run = component.Points[index - 1].Run;
                        else
                            component.Points[index].Run = component.Points[component.Points.Length - 1].Run;

                        _lastSelectedComponent = component;
                        _lastSelectedWaypoint = index;
                    }
                }

                Handles.color = oldColor;
            }

            _wasMouseTooFarAway = !canPlacePoint && !isHoveringPoint;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
                return;

            var component = (Waypoints)targets[0];

            Undo.RecordObject(component, "Change waypoints");

            if (component.Points != null)
            {
                int toDelete = -1;
                int toMakeFirst = -1;

                if (GUILayout.Button("Reverse"))
                {
                    reverse(component);
                    SceneView.RepaintAll();
                }

                for (int i = 0; i < component.Points.Length; i++)
                {
                    EditorGUILayout.Space();

                    var isSelected = component == _lastSelectedComponent && i == _lastSelectedWaypoint;

                    if (isSelected)
                    {
                        var oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.green;
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        GUI.backgroundColor = oldColor;
                    }
                    else
                        EditorGUILayout.BeginHorizontal();

                    var rect = EditorGUILayout.BeginVertical();
                    GUI.Box(rect, GUIContent.none);

                    component.Points[i].Position = EditorGUILayout.Vector3Field("Position", component.Points[i].Position);
                    component.Points[i].Pause = EditorGUILayout.FloatField("Pause", component.Points[i].Pause);
                    component.Points[i].Run = EditorGUILayout.Toggle("Run", component.Points[i].Run);

                    if (!isSelected)
                        if (GUILayout.Button("Select"))
                        {
                            _lastSelectedComponent = component;
                            _lastSelectedWaypoint = i;
                            SceneView.RepaintAll();
                        }

                    if (i > 0)
                        if (GUILayout.Button("Make First"))
                            toMakeFirst = i;

                    EditorGUILayout.EndVertical();

                    {
                        var oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            toDelete = i;
                        GUI.backgroundColor = oldColor;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (toDelete >= 0)
                {
                    deleteWaypoint(component, toDelete);
                    SceneView.RepaintAll();
                }

                if (toMakeFirst >= 0)
                {
                    makeFirst(component, toMakeFirst);
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Waypoint"))
            {
                if (component.Points == null)
                    component.Points = new Waypoint[1];
                else
                {
                    var old = component.Points;
                    component.Points = new Waypoint[old.Length + 1];

                    for (int i = 0; i < old.Length; i++)
                        component.Points[i] = old[i];
                }

                var value = new Waypoint();

                if (component.Points.Length > 1)
                    value.Position = component.Points[component.Points.Length - 2].Position;
                else
                    value.Position = component.transform.position;

                component.Points[component.Points.Length - 1] = value;

                SceneView.RepaintAll();
            }
        }

        private void makeFirst(Waypoints component, int index)
        {
            if (component.Points == null || component.Points.Length < 1)
                return;

            if (index < 0 || index >= component.Points.Length)
                return;

            var old = component.Points;
            component.Points = new Waypoint[old.Length];

            for (int i = 0; i < old.Length; i++)
                component.Points[i] = old[(index + i) % old.Length];

            _lastSelectedWaypoint = 0;
        }

        private void reverse(Waypoints component)
        {
            if (component.Points == null || component.Points.Length < 2)
                return;

            if (component.Points.Length == 2)
            {
                var t = component.Points[0];
                component.Points[0] = component.Points[1];
                component.Points[1] = t;
            }
            else if (component.Points.Length > 2)
            {
                for (int i = 1; i < component.Points.Length / 2 + 1; i++)
                {
                    var t = component.Points[i];
                    component.Points[i] = component.Points[component.Points.Length - i];
                    component.Points[component.Points.Length - i] = t;
                }
            }

            if (_lastSelectedWaypoint > 0)
                _lastSelectedWaypoint = component.Points.Length - _lastSelectedWaypoint;
        }

        private void deleteWaypoint(Waypoints component, int index)
        {
            if (component.Points == null)
                return;

            if (index < 0 || index > component.Points.Length)
                return;

            var old = component.Points;
            component.Points = new Waypoint[old.Length - 1];

            for (int i = 0; i < index; i++)
                component.Points[i] = old[i];

            for (int i = index + 1; i < old.Length; i++)
                component.Points[i - 1] = old[i];

            if (_lastSelectedWaypoint >= component.Points.Length)
                _lastSelectedWaypoint = component.Points.Length - 1;
        }
    }
}
