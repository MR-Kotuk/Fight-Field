using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// When asked to, walks the character motor around the waypoints. The AI can be made to run or wait at each position.
    /// </summary>
    public class AIWaypoints : Waypoints
    {
        private bool _isVisiting;
        private bool _isWaiting;
        private int _waypoint;
        private float _waitTime;
        private bool _forceTake = true;

        private bool _foundWaypoints = false;

        /// <summary>
        /// Told by the brains to start visiting points in order.
        /// </summary>
        public void ToStartVisitingWaypoints()
        {
            _isVisiting = true;
            _isWaiting = false;
            _waypoint = -1;

            _foundWaypoints = Points != null && Points.Length > 0;

            if (_foundWaypoints && isActiveAndEnabled)
                SendMessage("OnWaypointsFound", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Told by the brains to stop visiting waypoints.
        /// </summary>
        public void ToStopVisitingWaypoints()
        {
            _isVisiting = false;
        }

        private void Update()
        {
            if (!_isVisiting)
                return;

            if (!_foundWaypoints && Points != null && Points.Length > 0)
            {
                _foundWaypoints = true;
                SendMessage("OnWaypointsFound", SendMessageOptions.DontRequireReceiver);
            }

            if (!_foundWaypoints)
                return;

            if (_waypoint < 0 || _waypoint >= Points.Length)
                _isWaiting = false;

            if (_isWaiting)
            {
                _waitTime += Time.deltaTime;

                if (Points[_waypoint].Pause <= _waitTime)
                {
                    _waypoint = (_waypoint + 1) % Points.Length;
                    _isWaiting = false;
                    _forceTake = true;
                    _waitTime = 0;
                }
            }
            else
            {
                var moveTo = false;

                if (_waypoint < 0 || _waypoint >= Points.Length)
                {
                    _waypoint = 0;
                    var dist = Vector3.Distance(transform.position, Points[0].Position);

                    for (int i = 1; i < Points.Length; i++)
                    {
                        var current = Vector3.Distance(transform.position, Points[i].Position);

                        if (current < dist)
                        {
                            dist = current;
                            _waypoint = i;
                        }
                    }

                    moveTo = true;
                }
                else
                    moveTo = _forceTake;

                _forceTake = false;

                if (Vector3.Distance(transform.position, Points[_waypoint].Position) < 0.65f)
                {
                    if (Points[_waypoint].Pause > 1f / 60f || Points.Length == 1)
                    {
                        _isWaiting = true;
                        moveTo = false;
                        SendMessage("ToStopMoving", SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        _waypoint = (_waypoint + 1) % Points.Length;
                        moveTo = true;
                    }
                }

                if (moveTo)
                {
                    if (Points[_waypoint].Run)
                        SendMessage("ToRunTo", Points[_waypoint].Position, SendMessageOptions.DontRequireReceiver);
                    else
                        SendMessage("ToWalkTo", Points[_waypoint].Position, SendMessageOptions.DontRequireReceiver);

                    SendMessage("ToFaceWalkDirection", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
