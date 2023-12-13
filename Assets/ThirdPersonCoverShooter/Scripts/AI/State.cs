using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter.AI
{
    public enum BuiltinValue
    {
        Self = 1,
        SelfPosition,
        AliveTime,
        ViewDistance,
        FieldOfView,
        Target,
        TargetPosition,
        AimTarget,
        IsTargetVisible,
        HasTarget,
        HasNoTarget,
        TargetInvisibleTime,
        TargetVisibleTime,
        HealthFraction,
        IsArmed,
        EquipppedWeaponType,
        TotalBulletsLeft,
        LoadedBulletsLeft,
        IsAlive,
        HasMelee,
        IsInCover
    }

    [Serializable]
    public class State
    {
        public const int EntryID = -1;
        public const int AnyID = -2;
        public const int ExitID = -3;
        public const int FailID = -4;

        [NonSerialized]
        public static State EditorEditedState;

        public float FieldOfView = 200;
        public float ViewDistance = 40;

        public GameObject Target;
        public BaseActor TargetActor;
        public Vector3 TargetPosition;
        public bool IsTargetVisible;
        public bool WasTargetLastSeenNearCover;
        public float TargetVisibleTime;
        public float TargetInvisibleTime;
        public bool WasInvestigationTriggered;
        public bool WasSearchTriggered;
        public bool WasTargetTooClose;

        public GameObject TargetSenderThisFrame;
        public GameObject TargetSenderLastFrame;
        public float TargetSendTimer;

        public Brain Brain { get { return _brain; } }
        public Actor Actor { get { return _actor; } }
        public Animator Animator { get { return _animator; } }
        public GameObject Object { get { return _actor.gameObject; } }
        public float TimeAlive { get { return _timeAlive; } }

        public NavMeshPath Path;

        /// <summary>
        /// State of each AI layer.
        /// </summary>
        [HideInInspector]
        public LayerState[] Layers;

        [HideInInspector]
        public ValueMap Values = new ValueMap();

        [HideInInspector]
        public ArrayMap Arrays = new ArrayMap();

        [Serializable]
        public class ValueMap : SerializableDictionary<int, Value> { }

        [Serializable]
        public class ArrayMap : SerializableDictionary<int, Value[]> { }

        private Brain _brain;
        private Actor _actor;
        private Animator _animator;
        private bool _isAlreadyMovingToANode;

        private float _timeAlive;

        private int[] _keyTemp = new int[64];

        private bool _isDead = true;

        private List<GameObject> _deathCheck = new List<GameObject>();
        private List<GameObject> _spawnCheck = new List<GameObject>();
        private List<GameObject> _checkRemove = new List<GameObject>();

        private float _deathCheckWait = 0;

        private AIController[] _friends;
        private BaseActor[] _closeEnemies;
        private int _friendCount;
        private int _closeEnemyCount;
        private float _closeUpdateDelay;

        private float _targetSearchDelay = 0;
        private float _visibilityDelay = 0;

        public Vector3 GetPosition(ref Value value)
        {
            var dereferenced = this.Dereference(ref value);

            if (dereferenced.Type == ValueType.GameObject)
            {
                if (dereferenced.GameObject != null)
                    return dereferenced.GameObject.transform.position;
                else
                    return _actor.transform.position;
            }
            else
                return dereferenced.Vector3;
        }

        public bool GetFacing(ref Value value, Vector3 forward, out Vector3 result)
        {
            var dereferenced = Dereference(ref value);

            if (dereferenced.Type == ValueType.Facing)
            {
                switch (dereferenced.Facing)
                {
                    case CharacterFacing.Front: result = forward; return true;
                    case CharacterFacing.Back: result = -forward; return true;
                    case CharacterFacing.Left: result = new Vector3(forward.z, forward.y, -forward.x); return true;
                    case CharacterFacing.Right: result = new Vector3(-forward.z, forward.y, forward.x); return true;
                }

                result = Vector3.zero;
                return false;
            }
            else if (dereferenced.Type == ValueType.RelativeDirection)
            {
                var right = Vector3.Cross(Vector3.up, forward);

                switch (dereferenced.Direction)
                {
                    case CoverShooter.Direction.Forward: result = forward; return true;
                    case CoverShooter.Direction.ForwardRight: result = (forward + right).normalized; return true;
                    case CoverShooter.Direction.Right: result = right; return true;
                    case CoverShooter.Direction.BackwardRight: result = (-forward + right).normalized; return true;
                    case CoverShooter.Direction.Backward: result = -forward; return true;
                    case CoverShooter.Direction.BackwardLeft: result = (-forward - right).normalized; return true;
                    case CoverShooter.Direction.Left: result = -right; return true;
                    case CoverShooter.Direction.ForwardLeft: result = (forward - right); return true;
                }

                result = Vector3.zero;
                return false;
            }
            else if (dereferenced.Type == ValueType.Vector3)
            {
                if (_actor != null)
                {
                    result = (dereferenced.Vector3 - _actor.transform.position).normalized;
                    return true;
                }
                else
                {
                    result = Vector3.zero;
                    return false;
                }
            }
            else if (dereferenced.Type == ValueType.GameObject)
            {
                if (_actor != null)
                {
                    result = (GetPosition(ref dereferenced) - _actor.transform.position).normalized;
                    return true;
                }
                else
                {
                    result = Vector3.zero;
                    return false;
                }
            }
            else
            {
                result = Vector3.zero;
                return false;
            }
        }

        public Vector3 GetDirection(ref Value value)
        {
            var dereferenced = this.Dereference(ref value);

            if (dereferenced.Type == ValueType.RelativeDirection)
            {
                var forward = Vector3.forward;

                if (_actor != null)
                    forward = _actor.transform.forward;

                var right = Vector3.Cross(Vector3.up, forward);

                switch (dereferenced.Direction)
                {
                    case CoverShooter.Direction.Forward: return forward;
                    case CoverShooter.Direction.ForwardRight: return (forward + right).normalized;
                    case CoverShooter.Direction.Right: return right;
                    case CoverShooter.Direction.BackwardRight: return (-forward + right).normalized;
                    case CoverShooter.Direction.Backward: return -forward;
                    case CoverShooter.Direction.BackwardLeft: return (-forward - right).normalized;
                    case CoverShooter.Direction.Left: return -right;
                    case CoverShooter.Direction.ForwardLeft: return (forward - right);
                }

                return forward;
            }
            else if (dereferenced.Type == ValueType.GameObject)
            {
                if (_actor != null)
                    return (GetPosition(ref value) - _actor.transform.position).normalized;
                else
                    return GetPosition(ref value).normalized;
            }
            else
                return dereferenced.Vector3; 
        }

        public static ValueType GetBuiltinValueType(BuiltinValue id)
        {
            switch (id)
            {
                case BuiltinValue.Self: return ValueType.GameObject;
                case BuiltinValue.SelfPosition: return ValueType.Vector3;
                case BuiltinValue.AliveTime: return ValueType.Float;
                case BuiltinValue.ViewDistance: return ValueType.Float;
                case BuiltinValue.FieldOfView: return ValueType.Float;
                case BuiltinValue.Target: return ValueType.GameObject;
                case BuiltinValue.TargetPosition: return ValueType.Vector3;
                case BuiltinValue.IsTargetVisible: return ValueType.Boolean;
                case BuiltinValue.HasTarget: return ValueType.Boolean;
                case BuiltinValue.HasNoTarget: return ValueType.Boolean;
                case BuiltinValue.AimTarget: return ValueType.Vector3;
                case BuiltinValue.TargetVisibleTime: return ValueType.Float;
                case BuiltinValue.TargetInvisibleTime: return ValueType.Float;
                case BuiltinValue.HealthFraction: return ValueType.Float;
                case BuiltinValue.IsArmed: return ValueType.Boolean;
                case BuiltinValue.EquipppedWeaponType: return ValueType.Weapon;
                case BuiltinValue.LoadedBulletsLeft: return ValueType.Float;
                case BuiltinValue.TotalBulletsLeft: return ValueType.Float;
                case BuiltinValue.IsAlive: return ValueType.Boolean;
                case BuiltinValue.HasMelee: return ValueType.Boolean;
                case BuiltinValue.IsInCover: return ValueType.Boolean;
            }

            return ValueType.Unknown;
        }

        public Value GetBuiltin(BuiltinValue id)
        {
            switch (id)
            {
                case BuiltinValue.Self: return new Value(_actor.gameObject);
                case BuiltinValue.SelfPosition: return new Value(_actor.transform.position);
                case BuiltinValue.AliveTime: return new Value(_timeAlive);
                case BuiltinValue.ViewDistance: return new Value(ViewDistance);
                case BuiltinValue.FieldOfView: return new Value(FieldOfView);
                case BuiltinValue.Target: return new Value(Target);
                case BuiltinValue.TargetPosition: return new Value(TargetPosition);
                case BuiltinValue.IsTargetVisible: return new Value(Target != null && IsTargetVisible);
                case BuiltinValue.HasTarget: return new Value(Target != null);
                case BuiltinValue.HasNoTarget: return new Value(Target == null);

                case BuiltinValue.AimTarget:
                    {
                        var ground = TargetPosition;

                        if (!IsTargetVisible && !WasTargetLastSeenNearCover)
                            ground += _actor.transform.forward * 4;

                        if (TargetActor != null)
                        {
                            var fraction = 0.75f;

                            if (_actor != null && _actor.Weapon.Gun == null)
                                fraction = 0.5f;

                            if (TargetActor.Cover != null && !TargetActor.Cover.IsInFront(_actor.transform.position))
                                return new Value(ground + TargetActor.RelativeStandingTopPosition * fraction);
                            else
                                return new Value(ground + TargetActor.RelativeTopPosition * fraction);
                        }
                        else
                            return new Value(ground + Vector3.up * 1.5f);
                    }

                case BuiltinValue.TargetVisibleTime: return new Value(TargetVisibleTime);
                case BuiltinValue.TargetInvisibleTime: return new Value(TargetInvisibleTime);
                case BuiltinValue.HealthFraction: return new Value(Actor.HealthFraction);
                case BuiltinValue.IsArmed: return new Value(Actor.IsEquipped);
                case BuiltinValue.EquipppedWeaponType:
                    if (Actor.Weapon.Gun != null)
                        return new Value(Actor.Weapon.Gun.Type);
                    else
                        return new Value(WeaponType.Fist);

                case BuiltinValue.LoadedBulletsLeft:
                    if (Actor.Weapon.Gun != null)
                        return new Value(Actor.Weapon.Gun.LoadedBulletsLeft);
                    else
                        return new Value(0f);

                case BuiltinValue.TotalBulletsLeft:
                    if (Actor.Weapon.Gun != null)
                        return new Value(Actor.Weapon.Gun.TotalBulletsLeft);
                    else
                        return new Value(0f);

                case BuiltinValue.IsAlive:
                    if (_actor != null)
                        return new Value(_actor.IsAlive);
                    else
                        return new Value(false);

                case BuiltinValue.HasMelee:
                    if (_actor != null)
                        return new Value(_actor.Weapon.HasMelee);
                    else
                        return new Value(false);

                case BuiltinValue.IsInCover:
                    if (_actor != null)
                        return new Value(_actor.Cover != null);
                    else
                        return new Value(false);
            }

            return new Value();
        }

        public void UnsetTarget()
        {
            var hadTarget = Target != null;

            Target = null;
            TargetActor = null;
            IsTargetVisible = false;
            TargetVisibleTime = 0;
            TargetInvisibleTime = 0;
            WasInvestigationTriggered = false;
            WasTargetTooClose = false;
            WasTargetLastSeenNearCover = false;

            if (hadTarget)
                Feed(AIEvent.NoTarget);
        }

        public void SetTarget(GameObject value)
        {
            bool wasVisible = IsTargetVisible;

            if (Target != value && value != null)
            {
                Target = value;
                TargetActor = value.GetComponent<BaseActor>();
                WasTargetTooClose = false;
                IsTargetVisible = false;
                Feed(AIEvent.NewTarget);
                wasVisible = true;
            }
            else if (value == null && Target != null)
            {
                Target = null;
                TargetActor = null;
                WasTargetTooClose = false;
                IsTargetVisible = false;
                Feed(AIEvent.NoTarget);
            }

            WasInvestigationTriggered = false;

            if (value != null)
                WasSearchTriggered = false;

            if (value != null)
                if (Vector3.Distance(TargetPosition, value.transform.position) > 0.1f)
                {
                    Feed(AIEvent.NewTargetPosition);
                    TargetPosition = value.transform.position;
                }

            UpdateTargetVisibility(wasVisible);

            if (_brain.TellFriendsAboutTarget)
                SpreadTargetToFriends(_brain.CommunicationDistance);
        }

        public void SetTargetAt(GameObject value, Vector3 position)
        {
            bool wasVisible = IsTargetVisible;

            if (Target != value && value != null)
            {
                Target = value;
                TargetActor = value.GetComponent<BaseActor>();
                WasTargetTooClose = false;
                IsTargetVisible = false;
                Feed(AIEvent.NewTarget);
                wasVisible = true;
            }
            else if (value == null && Target != null)
            {
                Target = null;
                TargetActor = null;
                WasTargetTooClose = false;
                IsTargetVisible = false;
                Feed(AIEvent.NoTarget);
            }

            WasInvestigationTriggered = false;

            if (value != null)
                WasSearchTriggered = false;

            if (Vector3.Distance(TargetPosition, position) > 0.01f)
            {
                Feed(AIEvent.NewTargetPosition);
                TargetPosition = position;
            }

            UpdateTargetVisibility(wasVisible);

            if (_brain.TellFriendsAboutTarget)
                SpreadTargetToFriends(_brain.CommunicationDistance);
        }

        public void SetTargetPosition(Vector3 position)
        {
            WasInvestigationTriggered = false;

            if (Target != null)
                WasSearchTriggered = false;

            if (Vector3.Distance(TargetPosition, position) > 0.01f)
            {
                Feed(AIEvent.NewTargetPosition);
                TargetPosition = position;

                UpdateTargetVisibility(IsTargetVisible);
            }
        }

        public void UpdateTargetVisibility(bool previousValue)
        {
            if (Target == null)
                IsTargetVisible = false;
            else if (TargetActor != null)
                IsTargetVisible = AIUtil.IsInSight(_actor, TargetActor.TopPosition, ViewDistance, FieldOfView);
            else
                IsTargetVisible = AIUtil.IsInSight(_actor, Target.transform.position + Vector3.up * 1.5f, ViewDistance, FieldOfView);

            if (IsTargetVisible != previousValue)
            {
                if (Target != null)
                    WasSearchTriggered = false;

                WasInvestigationTriggered = false;
                TargetVisibleTime = 0;
                TargetInvisibleTime = 0;

                if (IsTargetVisible)
                    Feed(AIEvent.TargetVisible);
                else
                {
                    if (TargetActor != null && TargetActor.Cover != null)
                        WasTargetLastSeenNearCover = true;
                    else
                    {
                        var foundCount = Physics.OverlapSphereNonAlloc(TargetPosition, _brain.CoverCheckRange, Util.Colliders, CoverShooter.Layers.Cover, QueryTriggerInteraction.Collide);
                        WasTargetLastSeenNearCover = false;

                        for (int i = 0; i < foundCount; i++)
                        {
                            var coverObject = Util.Colliders[i].gameObject;
                            var cover = CoverSearch.GetCover(coverObject);

                            if (cover == null)
                                continue;

                            if (!cover.IsInFront(_actor.transform.position, false))
                            {
                                WasTargetLastSeenNearCover = true;
                                break;
                            }
                        }
                    }

                    Feed(AIEvent.TargetInvisible);
                }
            }
        }

        public void CheckDeathsAndSpawns()
        {
            _checkRemove.Clear();

            for (int i = 0; i < _deathCheck.Count; i++)
            {
                var obj = _deathCheck[i];

                if (isVisible(obj))
                {
                    var actor = Actors.Get(obj);

                    if (actor != null)
                    {
                        if (actor.IsAlive)
                            Feed(AIEvent.Resurrected, new Value(obj));
                        else
                            Feed(AIEvent.Dead, new Value(obj));
                    }

                    _checkRemove.Add(obj);
                }
            }

            for (int i = 0; i < _checkRemove.Count; i++)
                _deathCheck.Remove(_checkRemove[i]);

            _checkRemove.Clear();

            for (int i = 0; i < _spawnCheck.Count; i++)
            {
                var obj = _spawnCheck[i];

                if (isVisible(obj))
                {
                    var actor = Actors.Get(obj);

                    if (actor != null)
                        Feed(AIEvent.Spawned, new Value(obj));

                    _checkRemove.Add(obj);
                }
            }

            for (int i = 0; i < _checkRemove.Count; i++)
                _spawnCheck.Remove(_checkRemove[i]);
        }

        public int GetCurrentNode(int layer)
        {
            if (Layers == null || layer < 0 || layer >= Layers.Length)
                return 0;

            return Layers[layer].CurrentNode;
        }

        public void Setup(Brain value, Actor actor)
        {
            _actor = actor;
            _animator = actor.GetComponent<Animator>();

            if (value != null)
                if (Layers == null || Layers.Length != value.Layers.Length)
                {
                    if (Layers != null && _brain != null)
                        for (int i = 0; i < Layers.Length; i++)
                            if (Layers[i].CurrentNode > 0)
                            {
                                var node = Layers[i].CurrentNode;
                                Layers[i] = new LayerState();
                                Layers[i].IsEntry = true;
                                Layers[i].CurrentEvents = new List<EventDesc>();
                                Layers[i].FutureEvents = new List<EventDesc>();

                                if (_brain != null)
                                {
                                    var n = _brain.GetAction(node);

                                    if (n != null)
                                        n.Escape(this, i, node);
                                }

                                Debug.Assert(Layers[i].CurrentNode == 0);
                            }

                    Layers = new LayerState[value.Layers.Length];
                }

            _brain = value;

            if (_brain != null)
                for (int i = 0; i < _brain.Layers.Length; i++)
                    if (Layers[i].CurrentNode == 0 || !_brain.Layers[i].ContainsAction(Layers[i].CurrentNode))
                    {
                        Layers[i] = new LayerState();
                        Layers[i].IsEntry = true;
                        Layers[i].CurrentEvents = new List<EventDesc>();
                        Layers[i].FutureEvents = new List<EventDesc>();
                    }
        }

        public void Feed(ref GeneratedAlert alert)
        {
            if (alert.Type == AlertType.Death && alert.Actor != null)
                Feed(AIEvent.Dead, new Value(alert.Actor), true);

            if (alert.Actor != null && _actor != null && alert.Actor.Side != _actor.Side)
            {
                Feed(AIEvent.HearEnemySound,
                     new Value(alert.Position),
                     new Value(alert.Object),
                     true);
            }
        }

        public void Feed(AIEvent type, bool isHeard = false)
        {
            var e = new EventDesc();
            e.Type = type;

            Feed(ref e, isHeard);
        }

        public void Feed(AIEvent type, Value value0, bool isHeard = false)
        {
            var e = new EventDesc();
            e.Type = type;
            e.Value0 = value0;

            Feed(ref e, isHeard);
        }

        public void Feed(AIEvent type, Value value0, Value value1, bool isHeard = false)
        {
            var e = new EventDesc();
            e.Type = type;
            e.Value0 = value0;
            e.Value1 = value1;

            Feed(ref e, isHeard);
        }

        public void Feed(AIEvent type, Value value0, Value value1, Value value2, bool isHeard = false)
        {
            var e = new EventDesc();
            e.Type = type;
            e.Value0 = value0;
            e.Value1 = value1;
            e.Value2 = value2;

            Feed(ref e, isHeard);
        }

        public void Feed(AIEvent type, Value value0, Value value1, Value value2, Value value3, bool isHeard = false)
        {
            var e = new EventDesc();
            e.Type = type;
            e.Value0 = value0;
            e.Value1 = value1;
            e.Value2 = value2;
            e.Value3 = value3;

            Feed(ref e, isHeard);
        }

        private bool isVisible(GameObject obj, float height = 1)
        {
            if (_actor != null && _actor.gameObject == obj)
                return true;
            else
                return AIUtil.IsInSight(_actor, obj.transform.position + Vector3.up * height, ViewDistance, FieldOfView);
        }

        public void Feed(ref EventDesc desc, bool isHeard = false)
        {
            if (_brain == null)
                return;

            if (desc.Type == AIEvent.Dead)
            {
                var obj = desc.Value0.GameObject;

                if (obj == null)
                    return;

                if (_brain.OnlyVisibleDeath && !isHeard && !isVisible(obj, 1.25f))
                {
                    if (!_deathCheck.Contains(obj))
                        _deathCheck.Add(obj);

                    return;
                }
                else if (_deathCheck.Contains(obj))
                    _deathCheck.Remove(obj);
            }

            if (desc.Type == AIEvent.Resurrected)
            {
                var obj = desc.Value0.GameObject;

                if (obj == null)
                    return;

                if (_brain.OnlyVisibleResurrection && !isHeard && !isVisible(obj))
                {
                    if (!_deathCheck.Contains(obj))
                        _deathCheck.Add(obj);

                    return;
                }
                else if (_deathCheck.Contains(obj))
                    _deathCheck.Remove(obj);
            }

            if (desc.Type == AIEvent.Spawned)
            {
                var obj = desc.Value0.GameObject;

                if (obj == null)
                    return;

                if (_brain.OnlyVisibleSpawn && !isHeard && !isVisible(obj))
                {
                    if (!_spawnCheck.Contains(obj))
                        _spawnCheck.Add(obj);

                    return;
                }
                else if (_spawnCheck.Contains(obj))
                    _spawnCheck.Remove(obj);
            }

            if (Layers != null && Layers.Length > 0)
                for (int layer = Layers.Length - 1; layer >= 0; layer--)
                    Layers[layer].Add(ref desc);

            if (desc.Type == AIEvent.Investigate)
                WasInvestigationTriggered = true;

            if (desc.Type == AIEvent.Search)
                WasSearchTriggered = true;

            if (desc.Type == AIEvent.TargetVisible && _brain.UnsetDeadTargets)
            {
                if (TargetActor != null && !TargetActor.IsAlive)
                    UnsetTarget();
            }

            if (desc.Type == AIEvent.GetHit && _brain.HandleReceivedHits)
            {
                var attacker = desc.Value2.GameObject;
                var attackerActor = attacker != null ? Actors.Get(attacker) : null;

                if (attackerActor == null)
                {
                    if (_brain.SearchIfUnknownEnemies && Target == null)
                        Feed(AIEvent.Search, desc.Value0);
                }
                else if (attackerActor.Side != _actor.Side)
                {
                    if (_brain.SetAttackerAsTarget)
                        SetTarget(attacker);
                }
            }

            if (desc.Type == AIEvent.HearEnemySound && _brain.ReactToEnemySounds)
            {
                if (!IsTargetVisible)
                {
                    var attacker = desc.Value1.GameObject;

                    if (attacker == null || Actors.Get(attacker) == null)
                    {
                        if (WasSearchTriggered)
                            Feed(AIEvent.Search, desc.Value0);
                    }
                    else
                    {
                        SetTarget(attacker);
                    }
                }
                else if (Brain.SetVeryCloseTargets && _actor != null)
                {
                    var attacker = desc.Value1.GameObject;

                    if (attacker != null &&
                        (Target == null || Vector3.Distance(_actor.transform.position, Target.transform.position) > Brain.SetCloseTargetThreshold))
                    {
                        if (Vector3.Distance(attacker.transform.position, _actor.transform.position) < Brain.SetCloseTargetThreshold)
                            SetTarget(attacker);
                    }
                }
            }

            if (desc.Type == AIEvent.HearFriendTarget && _brain.ReactToFriendTargets)
            {
                if (Target == null)
                {
                    if (_brain.SetFriendTargetIfNone)
                        SetTargetAt(desc.Value0.GameObject, desc.Value1.Vector3);
                }
                else if (!IsTargetVisible)
                {
                    if (Target == desc.Value0.GameObject)
                    {
                        if (!IsTargetVisible && desc.Value2.Bool && _brain.UpdateFriendTargetIfSameAndVisible)
                            SetTargetPosition(desc.Value1.Vector3);
                    }
                    else if (_brain.SetNewFriendTargetIfOwnInvisible && !IsTargetVisible)
                        SetTargetAt(desc.Value0.GameObject, desc.Value1.Vector3);
                }
            }

            if (desc.Type == AIEvent.Dead && Target == null && _brain.SearchIfFindDeathWithNoTarget)
            {
                var obj = desc.Value0.GameObject;

                if (obj != null)
                    Feed(AIEvent.Search, new Value(obj.transform.position));
            }

            if (desc.Type == AIEvent.Dead)
            {
                var obj = desc.Value0.GameObject;

                if (obj != null && Target == obj)
                    Feed(AIEvent.TargetDead);
            }

            if (desc.Type == AIEvent.TargetDead && _brain.UnsetDeadTargets)
                UnsetTarget();

            if (desc.Type == AIEvent.Dead && _brain.GenerateSpecificDeathEvents)
            {
                var obj = desc.Value0.GameObject;

                if (obj != null && _actor != null)
                {
                    if (_actor.gameObject == obj)
                        Feed(AIEvent.SelfDead);
                    else
                    {
                        var actor = Actors.Get(obj);

                        if (actor != null)
                        {
                            if (actor.Side == _actor.Side)
                                Feed(AIEvent.FriendDead, desc.Value0);
                            else
                                Feed(AIEvent.EnemyDead, desc.Value0);
                        }
                    }
                }
            }

            if (desc.Type == AIEvent.Resurrected && _brain.GenerateSpecificResurrectionEvents)
            {
                var obj = desc.Value0.GameObject;

                if (obj != null)
                {
                    if (_actor != null)
                    {
                        if (_actor.gameObject == obj)
                            Feed(AIEvent.SelfResurrected);
                        else
                        {
                            var actor = Actors.Get(obj);

                            if (actor != null)
                            {
                                if (actor.Side == _actor.Side)
                                    Feed(AIEvent.FriendResurrected, desc.Value0);
                                else
                                    Feed(AIEvent.EnemyResurrected, desc.Value0);
                            }
                        }
                    }
                }
            }

            if (desc.Type == AIEvent.Spawned && _brain.GenerateSpecificSpawnEvents)
            {
                var obj = desc.Value0.GameObject;

                if (obj != null)
                {
                    if (_actor != null)
                    {
                        if (_actor.gameObject == obj)
                            Feed(AIEvent.SelfSpawned);
                        else
                        {
                            var actor = Actors.Get(obj);

                            if (actor != null)
                            {
                                if (actor.Side == _actor.Side)
                                    Feed(AIEvent.FriendSpawned, desc.Value0);
                                else
                                    Feed(AIEvent.EnemySpawned, desc.Value0);
                            }
                        }
                    }
                }
            }
        }

        public bool SpreadTargetToFriends(float distance)
        {
            if (Target == null)
                return false;

            var any = false;

            var value0 = new Value(Target);
            var value1 = new Value(TargetPosition);
            var value2 = new Value(IsTargetVisible);
            var value3 = new Value(_actor.gameObject);

            for (int i = 0; i < _friendCount; i++)
            {
                var them = _friends[i];

                if (them.gameObject == TargetSenderLastFrame ||
                    them.gameObject == TargetSenderThisFrame)
                    continue;

                if (them != null)
                {
                    var canSend = true;

                    if (them.State.Target == Target && them.State.IsTargetVisible)
                        canSend = false;
                    else if (them.State.Target != Target ||
                            (them.State.TargetPosition - TargetPosition).sqrMagnitude > 0.1f)
                    {
                        canSend = true;
                    }

                    if (canSend)
                    {
                        var e = new EventDesc();
                        e.Type = AIEvent.HearFriendTarget;
                        e.Value0 = value0;
                        e.Value1 = value1;
                        e.Value2 = value2;
                        e.Value3 = value3;

                        them.State.Feed(ref e);
                    }

                    any = true;
                }
            }

            if (any)
            {
                if (_actor != null)
                    TargetSenderThisFrame = _actor.gameObject;

                TargetSendTimer = 0;
            }

            return any;
        }

        public void Update()
        {
            if (_brain == null)
                return;

            if (_actor != null)
            {
                if (_actor.IsAlive)
                {
                    if (_isDead)
                    {
                        if (!_brain.ContinueOnResurrection)
                        {
                            Target = null;
                            TargetActor = null;
                            TargetPosition = Vector3.zero;
                            IsTargetVisible = false;
                            WasTargetLastSeenNearCover = false;
                            TargetVisibleTime = 0;
                            TargetInvisibleTime = 0;
                            WasInvestigationTriggered = false;
                            WasSearchTriggered = false;
                            WasTargetTooClose = false;

                            TargetSenderThisFrame = null;
                            TargetSenderLastFrame = null;
                            TargetSendTimer = 0;

                            if (Layers != null)
                                for (int i = 0; i < Layers.Length; i++)
                                {
                                    if (_brain != null && Layers[i].CurrentNode > 0)
                                    {
                                        var node = _brain.GetAction(Layers[i].CurrentNode);

                                        if (node != null)
                                            node.Escape(this, i, Layers[i].CurrentNode);
                                    }

                                    var n = new LayerState();
                                    n.IsEntry = true;
                                    n.CurrentEvents = new List<EventDesc>();
                                    n.FutureEvents = new List<EventDesc>();

                                    for (int j = 0; j < Layers[i].CurrentEvents.Count; j++)
                                        n.CurrentEvents.Add(Layers[i].CurrentEvents[j]);

                                    Layers[i] = n;
                                }
                        }

                        _isDead = false;
                    }
                }
                else
                {
                    _isDead = true;

                    if (_brain.StopOnDeath)
                    {
                        _timeAlive = 0;
                        return;
                    }
                }
            }

            if (_deathCheckWait >= 0)
                _deathCheckWait -= Time.deltaTime;

            if (_deathCheck.Count > 0 && _deathCheckWait <= 0)
            {
                CheckDeathsAndSpawns();
                _deathCheckWait = 0.5f;
            }

            if (_actor == null || _actor.IsAlive)
                _timeAlive += Time.deltaTime;

            _closeUpdateDelay -= Time.deltaTime;

            if (_closeUpdateDelay <= 0)
            {
                updateFriendsAndCloseEnemies();
                _closeUpdateDelay = UnityEngine.Random.Range(0.4f, 0.8f);
            }

            if (_targetSearchDelay >= 0)
                _targetSearchDelay -= Time.deltaTime;

            if (_visibilityDelay >= 0)
                _visibilityDelay -= Time.deltaTime;

            if (_visibilityDelay <= 0)
            {
                UpdateTargetVisibility(IsTargetVisible);
                _visibilityDelay = UnityEngine.Random.Range(0.1f, 0.2f);
            }

            if (Brain.SetVeryCloseTargets)
                checkVeryCloseEnemies();

            if (IsTargetVisible)
            {
                if (Vector3.Distance(TargetPosition, Target.transform.position) > 0.01f)
                {
                    Feed(AIEvent.NewTargetPosition);
                    TargetPosition = Target.transform.position;
                }

                TargetVisibleTime += Time.deltaTime;
                TargetInvisibleTime = 0;

                if (_brain.TellFriendsAboutTarget)
                    if (TargetSendTimer > _brain.CommunicateDelay)
                        SpreadTargetToFriends(_brain.CommunicationDistance);

                TargetSendTimer += Time.deltaTime;
            }
            else if (Target != null)
            {
                TargetVisibleTime = 0;
                TargetInvisibleTime += Time.deltaTime;

                if (_brain.TriggerInvestigation && !WasInvestigationTriggered)
                {
                    if ((TargetInvisibleTime > _brain.CoverInvestigationTimer && WasTargetLastSeenNearCover) ||
                        (TargetInvisibleTime > _brain.UncoveredInvestigationTimer && !WasTargetLastSeenNearCover))
                    {
                        Feed(AIEvent.Investigate, new Value(TargetPosition));
                    }
                }

                if (_brain.TriggerTargetTooClose)
                    if (!WasTargetTooClose && Vector3.Distance(_actor.transform.position, TargetPosition) <= _brain.TargetTooCloseThreshold)
                    {
                        Feed(AIEvent.TargetTooClose);
                        WasTargetTooClose = true;
                    }
            }
            else
            {
                TargetVisibleTime = 0;
                TargetInvisibleTime = 0;

                if (_brain.SetVisibleTargetIfNone && _targetSearchDelay <= 0)
                {
                    GameObject closest = null;
                    var closestDistance = 0f;

                    var selfPosition = _actor.transform.position;
                    var viewDistance = _actor.GetViewDistance(ViewDistance, false);
                    var foundCount = AIUtil.FindActors(_actor.transform.position, viewDistance, _actor);

                    for (int i = 0; i < foundCount; i++)
                    {
                        var them = AIUtil.Actors[i];

                        if (them.Side == _actor.Side)
                            continue;

                        if (AIUtil.IsInSight(_actor, them.TopPosition, viewDistance, FieldOfView))
                        {
                            var p = them.transform.position;

                            var distance = Vector3.Distance(selfPosition, p);

                            if (closest == null || closestDistance > distance)
                            {
                                closest = AIUtil.Actors[i].gameObject;
                                closestDistance = distance;
                            }
                        }
                    }

                    if (closest != null)
                        SetTarget(closest);

                    _targetSearchDelay = UnityEngine.Random.Range(0.4f, 0.6f);
                }
            }

#if UNITY_EDITOR
            if (EditorEditedState == this)
            {
                foreach (var trigger in _brain.NodeTriggers.Values)
                    if (trigger.EditorDebugValue > float.Epsilon)
                        trigger.EditorDebugValue = Mathf.Clamp01(trigger.EditorDebugValue - Time.deltaTime);

                foreach (var node in _brain.Actions.Values)
                    if (node.EditorDebugValue > float.Epsilon)
                        node.EditorDebugValue = Mathf.Clamp01(node.EditorDebugValue - Time.deltaTime);
            }
#endif

            if (Layers != null && Layers.Length > 0)
                for (int layer = Layers.Length - 1; layer >= 0; layer--)
                {
                    if (Layers[layer].CurrentNode != 0)
                    {
                        var action = _brain.GetAction(Layers[layer].CurrentNode);

                        if (action == null)
                            Layers[layer].ReInit();
                    }

                    var activeStates = Layers[layer].ActiveActions;
                    var nodeStates = Layers[layer].ActionStates;

                    if (activeStates != null && nodeStates != null)
                    {
                        var count = activeStates.Count;

                        for (int i = 0; i < count;i++)
                        {
                            var id = activeStates[i];
                            var state = nodeStates[id];

                            if (state.IsHolding)
                            {
                                state.Hold -= Time.deltaTime;

                                if (state.Hold < float.Epsilon)
                                    state.IsHolding = false;

                                nodeStates[id] = state;
                            }
                        }
                    }

                    EvaluateGlobalSaves(layer);

                    Layers[layer].BeginUpdate();

                    updateSpecificLayer(layer, 0);

                    var root = _brain.GetRootActionId(Layers[layer].CurrentNode);
                    var hasTriggeredAny = false;

                    while (root != 0)
                    {
                        var action = _brain.GetAction(root);

                        if (action.Action != null)
                            break;

                        if (updateSpecificLayer(layer, root))
                            hasTriggeredAny = true;

                        if (hasTriggeredAny)
                            break;
                        else
                            root = _brain.GetRootActionIdTill(Layers[layer].CurrentNode, root);
                    }

                    Layers[layer].EndUpdate();

                    if (hasTriggeredAny)
                        continue;

                    if (Layers[layer].HasFreezeValue)
                    {
                        if (Layers[layer].IsFreezingAboveLayers)
                            break;
                    }
                    else if (Brain.Layers[layer].IsFreezingLayersAbove && _brain.GetAction(Layers[layer].CurrentNode) != null)
                        break;
                }

            TargetSenderLastFrame = TargetSenderThisFrame;
            TargetSenderThisFrame = null;
        }

        private bool updateSpecificLayer(int layer, int parentId)
        {
            int iteration = 0;
            bool hadAny = false;
            var hadAnyTrigger = false;

            while (iteration == 0 || (hadAny && Layers[layer].CurrentEvents.Count > 0))
            {
                hadAny = false;
                bool isFirst = true;
                int iterations = 0;

                while (iterations < 100 && updateSpecificLayer(layer, parentId, isFirst, ref hadAny))
                {
                    iterations++;
                    hadAnyTrigger = true;
                    isFirst = false;
                }

                iteration++;
            }

            return hadAnyTrigger;
        }

        private void checkVeryCloseEnemies()
        {
            if (Target != null && Vector3.Distance(Target.transform.position, _actor.transform.position) < Brain.SetCloseTargetThreshold)
                return;

            if (_closeEnemyCount == 0)
                return;

            if (_closeEnemyCount == 1)
            {
                SetTarget(_closeEnemies[0].gameObject);
                return;
            }

            var closest = _closeEnemies[0].gameObject;
            var closestDistance = Vector3.Distance(closest.transform.position, _actor.transform.position);

            for (int i = 1; i < _closeEnemyCount; i++)
            {
                var dist = Vector3.Distance(_closeEnemies[i].transform.position, _actor.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = _closeEnemies[i].gameObject;
                }
            }

            SetTarget(closest);
        }

        private bool updateSpecificLayer(int layer, int parentId, bool updateAny, ref bool hadAny)
        {
            var parent = _brain.GetAction(parentId);
            var currentNode = Layers[layer].CurrentNode;

            var any = parent == null ? _brain.Layers[layer].Any : parent.Any;
            var entry = parent == null ? _brain.Layers[layer].Entry : parent.Entry;

            bool isEntry;

            if (parent == null)
                isEntry = currentNode <= 0 && Layers[layer].IsEntry;
            else
                isEntry = entry != null && currentNode == parentId;

            if (updateAny && evaluateTriggers(layer, parent, any.Triggers))
            {
                isEntry = false;
                hadAny = true;
            }

            if (isEntry)
                if (!evaluateTriggers(layer, parent, entry.Triggers))
                    Trigger(layer, null, entry.Init);

            var nodeId = Layers[layer].CurrentNode;
            var node = _brain.GetAction(nodeId);

            while (node != null)
            {
                if (node.Parent == parentId)
                {
                    if (evaluateTriggers(layer, parent, node.Triggers))
                        return true;
                    else
                    {
                        node.EditorDebugValue = 1;

                        NodeState state;

                        if (Layers[layer].ActionStates != null)
                        {
                            if (Layers[layer].ActionStates.TryGetValue(nodeId, out state))
                                state.Duration += Time.deltaTime;
                            else
                                state = new NodeState();
                        }
                        else
                            state = new NodeState();

                        var result = new AIResult(state.HasFinished ? AIResultType.Finish : AIResultType.Nothing);

                        if (!state.HasFinished)
                        {
                            result = node.Update(this, layer, nodeId, ref state);

                            if (result.Type == AIResultType.Hold)
                            {
                                state.IsHolding = true;
                                state.Hold = result.Time;
                            }
                            else if (result.Type == AIResultType.Finish)
                                state.HasFinished = true;
                        }

                        Layers[layer].SetState(nodeId, state);

                        switch (result.Type)
                        {
                            case AIResultType.Hold:
                            case AIResultType.Nothing:
                                break;

                            case AIResultType.Triggered:
                                return true;

                            case AIResultType.Success:
                            case AIResultType.Finish:
                                if (resultTrigger(layer, node, node.SuccessTrigger, result.Values))
                                    return true;
                                else if (result.CanHold)
                                {
                                    state.IsHolding = true;
                                    state.Hold = result.Time;
                                    state.HasFinished = false;
                                    Layers[layer].SetState(nodeId, state);
                                }

                                break;

                            case AIResultType.Failure:
                                if (resultTrigger(layer, node, node.FailureTrigger, result.Values))
                                    return true;
                                else
                                    break;

                            case AIResultType.TimeOut:
                                if (resultTrigger(layer, node, node.TimeOutTrigger, result.Values))
                                    return true;
                                else
                                    break;
                        }

                        if (node.HasLimitedTime)
                            if (state.Duration >= Dereference(ref node.TimeLimit).Float)
                                if (resultTrigger(layer, node, node.TimeOutTrigger, result.Values))
                                    return true;
                    }

                    break;
                }

                nodeId = node.Parent;
                node = _brain.GetAction(nodeId);
            }

            return false;
        }

        private bool resultTrigger(int layer, ActionNode source, int id, Value[] values)
        {
            var trigger = Brain.GetNodeTrigger(id);

            int min;

            if (trigger.Values == null || values == null)
                min = 0;
            else if (trigger.Values.Length < values.Length)
                min = trigger.Values.Length;
            else
                min = values.Length;

            for (int i = 0; i < min; i++)
                Values[trigger.Values[i]] = Dereference(ref values[i]);

            if (source == null)
                return false;

            return Trigger(layer, _brain.GetAction(source.Parent), trigger);
        }

        public bool Trigger(int layer, int id)
        {
            ActionNode parent = null;

            var source = _brain.GetAction(Layers[layer].CurrentNode);

            if (source != null)
                parent = _brain.GetAction(source.Parent);

            return Trigger(layer, parent, _brain.GetNodeTrigger(id));
        }

        public bool Trigger(int layer, ActionNode source, int id)
        {
            ActionNode parent = null;

            if (source != null)
                parent = _brain.GetAction(source.Parent);

            return Trigger(layer, parent, _brain.GetNodeTrigger(id));
        }

        public bool Trigger(int layer, NodeTrigger trigger)
        {
            ActionNode parent = null;

            var source = _brain.GetAction(Layers[layer].CurrentNode);

            if (source != null)
                parent = _brain.GetAction(source.Parent);

            return Trigger(layer, parent, trigger);
        }

        public bool Trigger(int layer, ActionNode parent, NodeTrigger trigger)
        {
            if (trigger == null)
                return false;

            if (trigger.Target != 0)
            {
                if (trigger.Saves != null)
                    for (int i = 0; i < trigger.Saves.Length; i++)
                        EvaluateSave(trigger.Saves[i]);
            }

            if (trigger.Target == ExitID)
            {
                if (parent == null)
                    return false;

                var parentTrigger = _brain.GetNodeTrigger(parent.SuccessTrigger);

                if (parentTrigger == null || parentTrigger.Target == 0)
                    return false;

                trigger.EditorDebugValue = 1;
                parentTrigger.EditorDebugValue = 1;
                return Trigger(layer, parent, parentTrigger);
            }
            else if (trigger.Target == FailID)
            {
                if (parent == null)
                    return false;

                var parentTrigger = _brain.GetNodeTrigger(parent.FailureTrigger);

                if (parentTrigger == null || parentTrigger.Target == 0)
                    return false;

                trigger.EditorDebugValue = 1;
                parentTrigger.EditorDebugValue = 1;
                return Trigger(layer, parent, parentTrigger);
            }
            else if (trigger.Target > 0)
            {
#if UNITY_EDITOR
                trigger.EditorDebugValue = 1;
#endif
                Go(layer, trigger.Target);
                return true;
            }
            else
                return false;
        }

        public void Go(int layer, int value)
        {
            if (_isAlreadyMovingToANode)
            {
                Layers[layer].CurrentNode = value;
                return;
            }

            EvaluateGlobalSaves(layer);

            Debug.Assert(_brain != null && _brain.Layers != null);
            Debug.Assert(layer >= 0 && layer <= Brain.Layers.Length);

            var old = Layers[layer].CurrentNode;
            Layers[layer].CurrentNode = value;
            Layers[layer].IsEntry = value == 0 && Layers[layer].IsEntry;

            if (old > 0)
            {
                var next = _brain.GetAction(value);

                if (next == null || old != next.Parent)
                {
                    var n = _brain.GetAction(old);

                    if (n != null)
                    {
                        _isAlreadyMovingToANode = true;
                        n.Escape(this, layer, old);
                        _isAlreadyMovingToANode = false;
                    }
                }
            }

            {
                var id = Layers[layer].CurrentNode;
                var n = _brain.GetAction(id);

                if (n != null)
                {
#if UNITY_EDITOR
                    if (n != null)
                        n.EditorDebugValue = 1;
#endif

                    n.Enter(this, layer, id);
                }
            }
        }

        public Value GetValue(int id)
        {
            Value value;

            if (Values.TryGetValue(id, out value))
                return value;
            else
                return new Value();
        }

        public Value Evaluate(int id)
        {
            if (id < 0)
                return GetBuiltin((BuiltinValue)(-id));

            Variable variable;
            ExpressionNode expression;
            Value dereferenced;

            if (Brain.Expressions.TryGetValue(id, out expression))
            {
                var evalued = expression.Evaluate(id, this);
                return Dereference(ref evalued);
            }
            else if (Values.TryGetValue(id, out dereferenced))
                return dereferenced;
            else if (Brain.Variables.TryGetValue(id, out variable))
                return variable.Value;
            else
                return new Value();
        }

        public Value Dereference(ref Value value)
        {
            if (value.IsConstant)
                return value;
            else
                return Evaluate(value.ID);
        }

        public Value[] GetArray(int id, int reserved)
        {
            Value[] array;

            if (Arrays.TryGetValue(id, out array))
            {
                if (array == null || array.Length < reserved)
                {
                    array = new Value[reserved];
                    Arrays[id] = array;
                }
            }
            else
            {
                array = new Value[reserved];
                Arrays[id] = array;
            }

            return array;
        }

        public void EvaluateSave(int id)
        {
            var expression = Brain.GetExpression(id);
            expression.Evaluate(id, this);
        }

        public void EvaluateGlobalSaves(int layerIndex)
        {
            var layer = Brain.Layers[layerIndex];

            if (layer.GlobalSaves == null)
                return;

            for (int i = 0; i < layer.GlobalSaves.Length; i++)
                EvaluateSave(layer.GlobalSaves[i]);
        }

        private bool searchForEvent(int layer, ref EventDesc desc, AIEvent type, int id = 0)
        {
            var list = Layers[layer].CurrentEvents;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type && (id == 0 ||
                                             list[i].ID == id ||
                                             (list[i].ID == 0 && Brain.GetTrigger(id).Name == list[i].Name)))
                {
                    desc = list[i];
                    list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private bool evaluateTriggers(int layer, ActionNode parent, int[] triggers)
        {
            var desc = new EventDesc();

            if (triggers != null)
                for (int i = 0; i < triggers.Length; i++)
                {
                    var trigger = Brain.GetNodeTrigger(triggers[i]);

                    if (trigger == null || trigger.Target == 0)
                        continue;

                    switch (trigger.Type)
                    {
                        case NodeTriggerType.Expression:
                            if (Dereference(ref trigger.Expression).Bool == trigger.ExpressionValue)
                            {
                                Trigger(layer, parent, trigger);
                                return true;
                            }
                            break;

                        case NodeTriggerType.Custom:
                            if (searchForEvent(layer, ref desc, AIEvent.Trigger, trigger.ID))
                            {
                                if (trigger.Values != null)
                                    for (int j = 0; j < trigger.Values.Length; j++)
                                    {
                                        switch (j)
                                        {
                                            case 0: Values[trigger.Values[j]] = desc.Value0; break;
                                            case 1: Values[trigger.Values[j]] = desc.Value1; break;
                                            case 2: Values[trigger.Values[j]] = desc.Value2; break;
                                            case 3: Values[trigger.Values[j]] = desc.Value3; break;
                                        }
                                    }

                                Trigger(layer, parent, trigger);
                                return true;
                            }
                            break;

                        case NodeTriggerType.Event:
                            if (searchForEvent(layer, ref desc, trigger.Event))
                            {
                                if (trigger.Values != null)
                                    for (int j = 0; j < trigger.Values.Length; j++)
                                    {
                                        switch (j)
                                        {
                                            case 0: Values[trigger.Values[j]] = desc.Value0; break;
                                            case 1: Values[trigger.Values[j]] = desc.Value1; break;
                                            case 2: Values[trigger.Values[j]] = desc.Value2; break;
                                            case 3: Values[trigger.Values[j]] = desc.Value3; break;
                                        }
                                    }

                                Trigger(layer, parent, trigger);
                                return true;
                            }
                            break;

                        default: break;
                    }
                }

            return false;
        }

        private void updateFriendsAndCloseEnemies()
        {
            if (_actor == null)
            {
                _friendCount = 0;
                _closeEnemyCount = 0;
                return;
            }

            var checkVeryClose = Brain.SetVeryCloseTargets && (Target == null || Vector3.Distance(Target.transform.position, _actor.transform.position) > Brain.SetCloseTargetThreshold);

            var maxDistance = Brain.CommunicationDistance;
            var veryCloseIsFurther = false;

            if (!checkVeryClose)
                _closeEnemyCount = 0;
            else if (Brain.SetCloseTargetThreshold > maxDistance)
            {
                maxDistance = Brain.SetCloseTargetThreshold;
                veryCloseIsFurther = true;
            }

            var count = AIUtil.FindActors(_actor.transform.position, Brain.CommunicationDistance, true, _actor);

            if (_friends == null || _friends.Length < count)
                _friends = new AIController[count];

            if (_closeEnemies == null || _closeEnemies.Length < count)
                _closeEnemies = new BaseActor[count];

            _friendCount = 0;
            _closeEnemyCount = 0;

            for (int i = 0; i < count; i++)
                if (AIUtil.Actors[i].Side == _actor.Side)
                {
                    if (veryCloseIsFurther && Vector3.Distance(AIUtil.Actors[i].transform.position, _actor.transform.position) > Brain.CommunicationDistance)
                        continue;

                    var ai = AIUtil.Actors[i].GetComponent<AIController>();

                    if (ai != null)
                    {
                        _friends[_friendCount] = ai;
                        _friendCount++;
                    }
                }
                else if (checkVeryClose)
                {
                    if (!veryCloseIsFurther && Vector3.Distance(AIUtil.Actors[i].transform.position, _actor.transform.position) > Brain.SetCloseTargetThreshold)
                        continue;

                    if (isVisible(AIUtil.Actors[i].gameObject))
                    {
                        _closeEnemies[_closeEnemyCount] = AIUtil.Actors[i];
                        _closeEnemyCount++;
                    }
                }
        }
    }
}