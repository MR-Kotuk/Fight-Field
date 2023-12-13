using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    /// <summary>
    /// Each character inside the level must have this component as the AI only regards objects with BaseActor as characters.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BaseActor : MonoBehaviour, ICharacterHeightListener, ICharacterHealthListener
    {
        #region Properties

        /// <summary>
        /// Is the object alive.
        /// </summary>
        public bool IsAlive
        {
            get { return _isAlive; }
        }

        /// <summary>
        /// Collider attached to the object.
        /// </summary>
        public Collider Collider
        {
            get { return _collider; }
        }

        /// <summary>
        /// Physical body of the object. Can be null.
        /// </summary>
        public Rigidbody Body
        {
            get { return _body; }
        }

        /// <summary>
        /// Cover the threat is hiding behind. Null if there isn't any.
        /// </summary>
        public Cover Cover
        {
            get { return _takenCover; }
        }

        /// <summary>
        /// Top position when the actor is standing.
        /// </summary>
        public Vector3 RelativeStandingTopPosition
        {
            get
            {
                if (_hasStandingHeight)
                    return Vector3.up * _standingHeight;
                else
                    return Vector3.up * _height;
            }
        }

        /// <summary>
        /// Current top position.
        /// </summary>
        public Vector3 RelativeTopPosition
        {
            get { return Vector3.up * _height; }
        }

        /// <summary>
        /// Top position when the actor is standing.
        /// </summary>
        public Vector3 StandingTopPosition
        {
            get
            {
                if (_hasStandingHeight)
                    return transform.position + Vector3.up * _standingHeight;
                else
                    return transform.position + Vector3.up * _height;
            }
        }

        /// <summary>
        /// Current top position.
        /// </summary>
        public Vector3 TopPosition
        {
            get { return transform.position + Vector3.up * _height; }
        }

        /// <summary>
        /// Fractional health value.
        /// </summary>
        public float HealthFraction
        {
            get
            {
                if (_health == null)
                    return 1;
                else
                    return _health.Health / _health.MaxHealth;
            }
        }

        /// <summary>
        /// Fractional health value.
        /// </summary>
        public float Health
        {
            get
            {
                if (_health == null)
                    return 999999999;
                else
                    return _health.Health;
            }
        }

        /// <summary>
        /// Is the AI attached to the actor alerted.
        /// </summary>
        public bool IsAlerted
        {
            get { return _isAlerted; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Team number used by the AI.
        /// </summary>
        [Tooltip("Team number used by the AI.")]
        public int Side = 0;

        /// <summary>
        /// Is the actor aggresive. Value used by the AI. Owning AI usually overwrites the value if present.
        /// </summary>
        [Tooltip("Is the actor aggresive. Value used by the AI. Owning AI usually overwrites the value if present.")]
        public bool IsAggressive = true;

        #endregion

        #region Private fields

        private bool _hasStandingHeight;
        private float _standingHeight;
        private float _height;

        private bool _isAlive = true;

        private Cover _takenCover;
        private Cover _potentialFutureCover;
        private Vector3 _potentialFutureCoverCheckPosition;
        private Cover _registeredCover;

        private Collider _collider;
        private Rigidbody _body;
        private CharacterHealth _health;

        private bool _isAlerted;

        #endregion

        #region Events

        /// <summary>
        /// Notify the component of the standing height (used when in cover).
        /// </summary>
        public virtual void OnStandingHeight(float value)
        {
            _hasStandingHeight = true;
            _standingHeight = value;
        }

        public virtual void OnCurrentHeight(float value)
        {
        }

        /// <summary>
        /// Notified by components that the actor is no longer alive.
        /// </summary>
        public virtual void OnDead()
        {
            _isAlive = false;
        }

        /// <summary>
        /// Notified that the actor has been resurrected.
        /// </summary>
        public virtual void OnResurrect()
        {
            _isAlive = true;
        }

        /// <summary>
        /// Tell the threat to mark itself as being behind the given cover.
        /// </summary>
        public virtual void OnEnterCover(Cover cover)
        {
            _takenCover = cover;
            updateRegisteredCover(transform.position);
        }

        /// <summary>
        /// Tell the threat to mark itself as out of cover.
        /// </summary>
        public virtual void OnLeaveCover()
        {
            _takenCover = null;
            updateRegisteredCover(transform.position);
        }

        /// <summary>
        /// Notified by an AI that the actor is alerted.
        /// </summary>
        public virtual void OnAlerted()
        {
            _isAlerted = true;
        }

        #endregion

        #region Behaviour

        protected virtual void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            _health = GetComponent<CharacterHealth>();

            _height = _collider.bounds.extents.y * 2;
        }

        protected virtual void OnEnable()
        {
            Actors.Register(this);
        }

        protected virtual void OnDisable()
        {
            Actors.Unregister(this);
        }

        protected virtual void OnDestroy()
        {
            Actors.Unregister(this);
        }

        protected virtual void Update()
        {
            _height = _collider.bounds.extents.y * 2;
        }

        #endregion

        #region Protected methods

        protected void updateRegisteredCover(Vector3 position)
        {
            var cover = _takenCover;

            if (cover == null)
                cover = _potentialFutureCover;

            if (cover != _registeredCover)
            {
                if (_registeredCover != null)
                    _registeredCover.UnregisterUser(this);

                _registeredCover = cover;

                if (_registeredCover != null)
                    _registeredCover.RegisterUser(this, position);
            }
            else if (_registeredCover != null)
                _registeredCover.RegisterUser(this, position);
        }

        protected void updatePotentialFutureCover(Vector3 position, bool force)
        {
            if (!force && Vector3.Distance(position, _potentialFutureCoverCheckPosition) < 1)
                return;

            _potentialFutureCover = null;
            _potentialFutureCoverCheckPosition = position;

            var foundCount = Physics.OverlapSphereNonAlloc(position, 0.5f, Util.Colliders, CoverShooter.Layers.Cover, QueryTriggerInteraction.Collide);

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;
                var cover = CoverSearch.GetCover(coverObject);

                if (cover == null)
                    continue;

                _potentialFutureCover = cover;
                break;
            }
        }

        #endregion
    }

    public static class Actors
    {
        public static IEnumerable<BaseActor> All
        {
            get { return _list; }
        }

        public static int Count
        {
            get { return _list.Count; }
        }

        private static List<BaseActor> _list = new List<BaseActor>();
        private static Dictionary<GameObject, BaseActor> _map = new Dictionary<GameObject, BaseActor>();

        public static BaseActor Get(int index)
        {
            return _list[index];
        }

        public static BaseActor Get(GameObject gameObject)
        {
            if (_map.ContainsKey(gameObject))
                return _map[gameObject];
            else
                return null;
        }

        public static void Register(BaseActor actor)
        {
            if (!_list.Contains(actor))
                _list.Add(actor);

            _map[actor.gameObject] = actor;
        }

        public static void Unregister(BaseActor actor)
        {
            if (_list.Contains(actor))
                _list.Remove(actor);

            if (_map.ContainsKey(actor.gameObject))
                _map.Remove(actor.gameObject);
        }
    }
}
