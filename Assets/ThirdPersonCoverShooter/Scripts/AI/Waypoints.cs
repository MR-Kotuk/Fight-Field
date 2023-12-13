using UnityEngine;
using UnityEngine.Serialization;

namespace CoverShooter
{
    /// <summary>
    /// When asked to, walks the character motor around the waypoints. The AI can be made to run or wait at each position.
    /// </summary>
    public class Waypoints : MonoBehaviour
    {
        /// <summary>
        /// Points to visit.
        /// </summary>
        [Tooltip("Points to visit.")]
        [HideInInspector]
        [FormerlySerializedAs("Waypoints")]
        public Waypoint[] Points;
    }
}
