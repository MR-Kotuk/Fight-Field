using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Finds nearby friends and makes connections to them. Other components then pass information about the threat to these nearby friends.
    /// </summary>
    [RequireComponent(typeof(BaseActor))]
    public class AICommunication : AIBase
    {
        /// <summary>
        /// All friends in contact.
        /// </summary>
        public IEnumerable<BaseActor> Friends
        {
            get { return _friends; }
        }

        /// <summary>
        /// Number of nearby friends.
        /// </summary>
        public int FriendCount
        {
            get { return _friends.Count; }
        }

        /// <summary>
        /// Distance in any direction for AI to communicate between each other.
        /// </summary>
        [Tooltip("Distance in any direction for AI to communicate between each other.")]
        public float Distance = 12;

        /// <summary>
        /// Time in seconds between each contact update.
        /// </summary>
        [Tooltip("Time in seconds between each contact update.")]
        public float UpdateDelay = 0.2f;

        /// <summary>
        /// Should lines between friends be drawn in the editor.
        /// </summary>
        [Tooltip("Should lines between friends be drawn in the editor.")]
        public bool DebugFriends = false;

        private BaseActor _actor;
        private HashSet<BaseActor> _stayFriends = new HashSet<BaseActor>();
        private HashSet<BaseActor> _friends = new HashSet<BaseActor>();
        private HashSet<BaseActor> _oldFriends = new HashSet<BaseActor>();

        private float _wait = 0;

        private static Dictionary<BaseActor, AICommunication> _components = new Dictionary<BaseActor, AICommunication>();

        private void Awake()
        {
            _actor = GetComponent<BaseActor>();
        }

        private void Update()
        {
            if (!_actor.IsAlive)
                return;

            _wait -= Time.deltaTime;

            if (DebugFriends)
                foreach (var friend in _friends)
                    if (friend != null)
                        Debug.DrawLine(friend.transform.position, transform.position, Color.yellow);

            if (_wait > float.Epsilon)
                return;

            _wait = Random.Range(UpdateDelay * 0.8f, UpdateDelay * 1.2f);

            _oldFriends.Clear();

            foreach (var friend in _friends)
                if (friend != null)
                    _oldFriends.Add(friend);

            _friends.Clear();

            foreach (var friend in _oldFriends)
            {
                var distance = Vector3.Distance(_actor.transform.position, friend.transform.position);

                if (distance < Distance && friend.IsAlive)
                    _friends.Add(friend);
                else
                {
                    Message("OnLostFriend", friend);

                    var comm = get(friend);

                    if (comm != null)
                    {
                        if (comm._friends.Contains(_actor))
                        {
                            comm._friends.Remove(_actor);
                            comm.Message("OnLostFriend", _actor);
                        }
                    }
                }
            }

            var count = Physics.OverlapSphereNonAlloc(_actor.transform.position, Distance, Util.Colliders, Layers.Character, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var friend = Actors.Get(Util.Colliders[i].gameObject);

                if (friend != null && friend != _actor && friend.IsAlive && friend.Side == _actor.Side && !_oldFriends.Contains(friend))
                {
                    var distance = Vector3.Distance(_actor.transform.position, friend.transform.position);

                    if (distance < Distance)
                    {
                        var comm = get(friend);

                        if (comm != null)
                        {
                            Message("OnFoundFriend", friend);
                            _friends.Add(friend);

                            comm._stayFriends.Add(_actor);

                            if (!comm._friends.Contains(_actor))
                            {
                                comm._friends.Add(_actor);
                                comm.Message("OnFoundFriend", _actor);
                            }
                        }
                    }
                }
            }

            _stayFriends.Clear();
        }

        private AICommunication get(BaseActor actor)
        {
            if (!_components.ContainsKey(actor))
                _components[actor] = actor.GetComponent<AICommunication>();

            return _components[actor];
        }
    }
}
