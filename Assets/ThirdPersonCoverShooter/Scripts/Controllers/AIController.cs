using System;
using System.Collections.Generic;
using UnityEngine;

using CoverShooter.AI;

namespace CoverShooter
{
    [RequireComponent(typeof(Actor))]
    public class AIController : MonoBehaviour, ICharacterHealthListener
    {
        /// <summary>
        /// Behaviour logic used by the AI.
        /// </summary>
        [Tooltip("Behaviour logic used by the AI.")]
        public Brain Brain;

        /// <summary>
        /// State of each AI layer.
        /// </summary>
        [HideInInspector]
        public State State = new State();

        /// <summary>
        /// Visibility field of view.
        /// </summary>
        [Tooltip("Visibility field of view.")]
        public float FieldOfView = 200;

        /// <summary>
        /// Maximum distance at which objects can be seen.
        /// </summary>
        [Tooltip("Maximum distance at which objects can be seen.")]
        public float ViewDistance = 40;

        /// <summary>
        /// Maximum degrees of error the AI can make when firing.
        /// </summary>
        [Tooltip("Maximum degrees of error the AI can make when firing.")]
        public float AimError = 2f;

        private Brain _activeBrain;
        private Actor _actor;

        private static Dictionary<GameObject, AIController> _map = new Dictionary<GameObject, AIController>();
        private static List<AIController> _all = new List<AIController>();

        private bool _hasSpawned = false;

        private void Awake()
        {
            _actor = GetComponent<Actor>();

            if (Brain != null)
            {
                State.FieldOfView = FieldOfView;
                State.ViewDistance = ViewDistance;
                State.Setup(Brain, _actor);
            }
        }

        private void Update()
        {
            if (Brain != State.Brain)
                State.Setup(Brain, _actor);

            var gun = _actor.Weapon.Gun;

            if (gun != null)
                gun.AddErrorThisFrame(AimError);

            if (!_hasSpawned)
            {
                var desc = new EventDesc();
                desc.Type = AIEvent.Spawned;
                desc.Value0 = new Value(gameObject);

                for (int i = 0; i < _all.Count; i++)
                    _all[i].State.Feed(ref desc);

                _hasSpawned = true;
            }

            State.FieldOfView = FieldOfView;
            State.ViewDistance = ViewDistance;
            State.Update();
        }

        private void OnEnable()
        {
            _map[gameObject] = this;
            _all.Add(this);
        }

        private void OnDisable()
        {
            _map.Remove(gameObject);
            _all.Remove(this);
        }

        private void OnDestroy()
        {
            _map.Remove(gameObject);
            _all.Remove(this);
        }

        private void OnHit(Hit hit)
        {
            var desc = new EventDesc();
            desc.Type = AIEvent.GetHit;
            desc.Value0 = new Value(hit.Position);
            desc.Value1 = new Value(hit.Normal);
            desc.Value2 = new Value(hit.Attacker);

            State.Feed(ref desc);
        }

        public void Hear(ref GeneratedAlert alert)
        {
            State.Feed(ref alert);
        }

        public static AIController Get(GameObject gameObject)
        {
            AIController value;

            if (_map.TryGetValue(gameObject, out value))
                return value;
            else
                return null;
        }

        public void OnDead()
        {
            var desc = new EventDesc();
            desc.Type = AIEvent.Dead;
            desc.Value0 = new Value(gameObject);

            for (int i = 0; i < _all.Count; i++)
                _all[i].State.Feed(ref desc);
        }

        public void OnResurrect()
        {
            var desc = new EventDesc();
            desc.Type = AIEvent.Resurrected;
            desc.Value0 = new Value(gameObject);

            for (int i = 0; i < _all.Count; i++)
                _all[i].State.Feed(ref desc);
        }
    }
}