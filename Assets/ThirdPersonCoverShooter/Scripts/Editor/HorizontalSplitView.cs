using UnityEngine;
using UnityEditor;

namespace CoverShooter
{
    public class HorizontalSplitView
    {
        public float Position = 300;

        private Rect _availableRect;
        private Vector2 _sscrollPosition;
        private bool _isResizing;
        private float _min;
        private float _max;

        private Rect _leftArea;
        private Rect _rightArea;

        public Rect LeftArea { get { return _leftArea; } }
        public Rect RightArea { get { return _rightArea; } }

        public void Begin(float min, float max)
        {
            var rect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            _min = min;
            _max = max;

            if (rect.width > 0)
            {
                _availableRect = rect;

                if (_availableRect.width < _max)
                    _max = _availableRect.width;
            }

            Position = Mathf.Clamp(Position, _min, _max);

            _sscrollPosition = GUILayout.BeginScrollView(_sscrollPosition, GUILayout.Width(Position));
        }

        public void Split(bool allowResize)
        {
            GUILayout.EndScrollView();

            var handle = new Rect(_availableRect.x + Position, _availableRect.y, 4, _availableRect.height);

            if (allowResize)
            {
                EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeHorizontal);

                if (Event.current.type == EventType.MouseDown && handle.Contains(Event.current.mousePosition))
                    _isResizing = true;

                if (_isResizing)
                    Position = Mathf.Clamp(Event.current.mousePosition.x, _min, _max);

                if (Event.current.type == EventType.MouseUp)
                    _isResizing = false;
            }
            else
                _isResizing = false;

            GUILayout.BeginScrollView(new Vector2(handle.x + handle.width, handle.y), GUILayout.Width(_availableRect.width - handle.x - handle.width));

            _leftArea = new Rect(0, 0, handle.x, _availableRect.height);
            _rightArea = new Rect(handle.x + handle.width, handle.y, _availableRect.width - handle.x - handle.width, _availableRect.height);
        }

        public void End()
        {
            GUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
        }
    }
}