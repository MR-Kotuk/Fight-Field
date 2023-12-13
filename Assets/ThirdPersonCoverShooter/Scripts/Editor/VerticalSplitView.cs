using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    public class VerticalSplitView
    {
        public float Position = 100;

        private Rect _availableRect;
        private Vector2 _scrollPosition;
        private bool _isResizing;
        private float _min;
        private float _max;

        private Rect _topArea;
        private Rect _bottomArea;

        public Rect TopArea { get { return _topArea; } }
        public Rect BottomArea { get { return _bottomArea; } }

        public void Begin(float min, float max)
        {
            var rect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            _min = min;
            _max = max;

            if (rect.width > 0)
            {
                _availableRect = rect;

                if (_availableRect.height < _max)
                    _max = _availableRect.height;
            }

            Position = Mathf.Clamp(Position, _min, _max);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(Position));
        }

        public void Split(bool allowResize)
        {
            GUILayout.EndScrollView();

            var handle = new Rect(_availableRect.x, _availableRect.y + Position, _availableRect.width, 4);

            if (allowResize)
            {
                EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeVertical);

                if (Event.current.type == EventType.MouseDown && handle.Contains(Event.current.mousePosition))
                    _isResizing = true;

                if (_isResizing)
                    Position = Mathf.Clamp(Event.current.mousePosition.y, _min, _max);

                if (Event.current.type == EventType.MouseUp)
                    _isResizing = false;
            }
            else
                _isResizing = false;

            GUILayout.BeginScrollView(new Vector2(handle.x, handle.y + handle.height), GUILayout.Height(_availableRect.height - handle.y - handle.height));

            _topArea = new Rect(0, 0, _availableRect.width, handle.y);
            _bottomArea = new Rect(handle.x, handle.y + handle.height, _availableRect.width, _availableRect.height - handle.y - handle.height);
        }

        public void End()
        {
            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }
}