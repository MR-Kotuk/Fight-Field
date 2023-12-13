using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Keeps character on top of a moving platform. It doesn’t require a Character Motor or any other component and therefore can be used on any object even if it’s not physical.
    /// </summary>
    public class CharacterPlatform : MonoBehaviour
    {
        private GameObject _platform;
        private GameObject _previousPlatform;
        private Vector3 _lastLocalPointOnPlatform;
        private Vector3 _lastPlatformRelevantPosition;

        private CharacterVertical _vertical;

        private void LateUpdate()
        {
            findPlatform();

            if (_platform != null && _platform == _previousPlatform)
            {
                transform.Rotate(0, _platform.transform.eulerAngles.y - _previousPlatform.transform.eulerAngles.y, 0);
                transform.position += _platform.transform.TransformPoint(_lastLocalPointOnPlatform) - _lastPlatformRelevantPosition;

                updatePlatformPoints();
            }

            _previousPlatform = _platform;
        }

        private void updatePlatformPoints()
        {
            _lastPlatformRelevantPosition = transform.position;
            _lastLocalPointOnPlatform = _platform.transform.InverseTransformPoint(transform.position);
        }

        private void findPlatform()
        {
            GameObject newPlatform = null;

            if (_vertical == null)
                _vertical = gameObject.AddComponent<CharacterVertical>();

            for (int i = 0; i < _vertical.Count; i++)
            {
                var hit = _vertical.Hits[i];

                if (!hit.collider.isTrigger)
                    if (hit.collider.gameObject != gameObject)
                        newPlatform = hit.collider.gameObject;
            }

            if (newPlatform != _platform && newPlatform != null)
            {
                _platform = newPlatform;
                updatePlatformPoints();
            }
            else
                _platform = newPlatform;
        }
    }
}