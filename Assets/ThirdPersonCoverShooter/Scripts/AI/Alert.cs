using UnityEngine;

namespace CoverShooter
{
    public enum AlertType
    {
        Any,
        Locomotion,
        Hit,
        Death,
        GunFire,
        GunHandling,
        Melee,
        Explosion,
        Chat
    }

    /// <summary>
    /// Generates alerts.
    /// </summary>
    public class Alert : MonoBehaviour
    {
        /// <summary>
        /// Range of the alert.
        /// </summary>
        [Tooltip("Range of the alert.")]
        public float Range = 20;

        /// <summary>
        /// Should the alert be activate when enabling the object.
        /// </summary>
        [Tooltip("Should the alert be activate when enabling the object.")]
        public bool AutoActivate = true;

        /// <summary>
        /// Type of the generated alert.
        /// </summary>
        [Tooltip("Type of the generated alert.")]
        public AlertType Type;

        /// <summary>
        /// Type of the allert.
        /// </summary>
        [Tooltip("Is threat regarded as hostile by civilians.")]
        public bool IsHostile;

        [HideInInspector]
        public BaseActor Generator;

        private BaseActor _actor;

        private void Awake()
        {
            _actor = GetComponent<BaseActor>();
        }

        /// <summary>
        /// Activates the alert and resets the timer.
        /// </summary>
        public void Activate()
        {
            Alerts.Broadcast(transform.position, Range, Type, IsHostile, gameObject, _actor == null ? Generator : _actor, _actor != null);
        }

        private void OnEnable()
        {
            if (AutoActivate)
                Activate();
        }
    }

    /// <summary>
    /// Describes an alert to be picked up by an AI (AIListener). Usually treated as a sound.
    /// </summary>
    public struct GeneratedAlert
    {
        /// <summary>
        /// Position of the alert.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Range of the alert.
        /// </summary>
        public float Range;

        /// <summary>
        /// Type of the alert.
        /// </summary>
        public AlertType Type;

        /// <summary>
        /// Is threat regarded as hostile by civilians.
        /// </summary>
        public bool IsHostile;

        /// <summary>
        /// Object that generated the alert.
        /// </summary>
        public GameObject Object;

        /// <summary>
        /// BaseActor that generated the alert.
        /// </summary>
        public BaseActor Actor;

        /// <summary>
        /// Is the actor at the position of the alert.
        /// </summary>
        public bool IsDirect;

        public GeneratedAlert(Vector3 position, float range, AlertType type, GameObject object_, BaseActor actor, bool isDirect)
        {
            Position = position;
            Range = range;
            Type = type;
            IsHostile = Alerts.IsHostile(type);
            Object = object_;
            Actor = actor;
            IsDirect = isDirect;
        }

        public GeneratedAlert(Vector3 position, float range, AlertType type, bool isHostile, GameObject object_, BaseActor actor, bool isDirect)
        {
            Position = position;
            Range = range;
            Type = type;
            IsHostile = isHostile;
            Object = object_;
            Actor = actor;
            IsDirect = isDirect;
        }
    }

    public static class Alerts
    {
        /// <summary>
        /// Returns true if the given alert type is hostile.
        /// </summary>
        public static bool IsHostile(AlertType type)
        {
            switch (type)
            {
                case AlertType.GunHandling:
                case AlertType.GunFire:
                case AlertType.Melee:
                case AlertType.Explosion:
                case AlertType.Death:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Broadcasts an alert to all AIListener components in the area surrounding it.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, AlertType type, BaseActor actor, bool isDirect)
        {
            Broadcast(position, range, type, actor == null ? null : actor.gameObject, actor, isDirect);
        }

        /// <summary>
        /// Broadcasts an alert to all AIListener components in the area surrounding it.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, AlertType type, bool isHostile, BaseActor actor, bool isDirect)
        {
            Broadcast(position, range, type, isHostile, actor == null ? null : actor.gameObject, actor, isDirect);
        }

        /// <summary>
        /// Broadcasts an alert to all AIListener components in the area surrounding it.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, AlertType type, GameObject object_, BaseActor actor, bool isDirect)
        {
            if (range <= float.Epsilon)
                return;

            var alert = new GeneratedAlert(position, range, type, object_, actor, isDirect);
            var count = Physics.OverlapSphereNonAlloc(position, range, Util.Colliders, Layers.Character, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var ai = AIController.Get(Util.Colliders[i].gameObject);

                if (ai != null)
                    ai.Hear(ref alert);

                var listener = AIListeners.Get(Util.Colliders[i].gameObject);

                if (listener != null && listener.isActiveAndEnabled && Vector3.Distance(listener.transform.position, position) < range * listener.Hearing)
                    listener.Hear(ref alert);
            }
        }

        /// <summary>
        /// Broadcasts an alert to all AIListener components in the area surrounding it.
        /// </summary>
        public static void Broadcast(Vector3 position, float range, AlertType type, bool isHostile, GameObject object_, BaseActor actor, bool isDirect)
        {
            if (range <= float.Epsilon)
                return;

            var alert = new GeneratedAlert(position, range, type, isHostile, object_, actor, isDirect);
            var count = Physics.OverlapSphereNonAlloc(position, range, Util.Colliders, Layers.Character, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var ai = AIController.Get(Util.Colliders[i].gameObject);

                if (ai != null)
                    ai.Hear(ref alert);
                
                var listener = AIListeners.Get(Util.Colliders[i].gameObject);

                if (listener != null && listener.isActiveAndEnabled && Vector3.Distance(listener.transform.position, position) < range * listener.Hearing)
                    listener.Hear(ref alert);
            }
        }
    }
}
