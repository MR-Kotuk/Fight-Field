using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace CoverShooter
{
    /// <summary>
    /// Each character inside the level must have this component as the AI only regards objects with BaseActor as characters.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CharacterMotor))]
    public class Actor : BaseActor, ICharacterCoverListener
    {
        #region Properties

        /// <summary>
        /// Does the character have a weapon in their hands.
        /// </summary>
        public bool IsArmed
        {
            get { return _motor != null && _motor.EquippedWeapon.Gun != null; }
        }

        /// <summary>
        /// Returns true if the motor is currently throwing a grenade.
        /// </summary>
        public bool IsThrowing
        {
            get { return _motor != null && _motor.IsThrowingGrenade; }
        }

        /// <summary>
        /// Current look direction of the actor's head.
        /// </summary>
        public Vector3 HeadDirection
        {
            get
            {
                if (_motor == null)
                    return transform.forward;
                else
                    return _motor.HeadForward;
            }
        }

        /// <summary>
        /// Returns true if motor is performing a custom action.
        /// </summary>
        public bool IsPerformingCustomAction
        {
            get { return _motor.IsPerformingCustomAction; }
        }

        /// <summary>
        /// Current threat of the actor. Can be set by the AI. Otherwise a last attacked enemy is used.
        /// </summary>
        public BaseActor Threat
        {
            get
            {
                if (_brain != null)
                    return _brain.Threat;
                else
                    return _possibleThreat;
            }
        }

        /// <summary>
        /// Character motor attached to the object.
        /// </summary>
        public CharacterMotor Motor
        {
            get { return _motor; }
        }

        public WeaponDescription Weapon
        {
            get
            {
                _motor.Weapon.CheckCache();
                return _motor.Weapon;
            }
        }

        public bool IsEquipped
        {
            get { return _motor.WeaponEquipState == WeaponEquipState.equipped && _motor.EquippedWeapon.IsTheSame(ref _motor.Weapon); }
        }

        public bool IsUnequipped
        {
            get { return _motor.WeaponEquipState == WeaponEquipState.unequipped; }
        }

        public bool IsRolling
        {
            get { return _motor.IsRolling; }
        }

        public bool IsCrouching
        {
            get { return _motor.IsCrouching; }
        }

        public bool IsInLowCover
        {
            get { return _motor.IsInLowCover; }
        }

        public bool IsReloading
        {
            get { return _motor.IsReloading; }
        }

        public Vector3 BodyLookTarget
        {
            get { return _motor.BodyLookTarget; }
        }

        public Vector3 MoveDirection
        {
            get { return _moveDirection; }
        }

        public Vector3 MoveTarget
        {
            get { return _moveTarget; }
        }

        public float MovePathLength
        {
            get { return _movePathLength; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// The actor will make the motor avoid other actors.
        /// </summary>
        [Tooltip("The actor will make the motor avoid other actors.")]
        public bool AvoidOtherActors = true;

        /// <summary>
        /// The AI will move out of the way of other actors that are closer than the threshold.
        /// </summary>
        [Tooltip("The AI will move out of the way of other actors that are closer than the threshold.")]
        public float ObstructionRadius = 1.5f;

        #endregion

        #region Private fields

        private CharacterMotor _motor;
        private BaseBrain _brain;

        private Vector3 _moveDirection;

        private BaseActor _possibleThreat;

        private List<DarkZone> _darkZones = new List<DarkZone>();
        private List<LightZone> _lightZones = new List<LightZone>();
        private List<GrassZone> _grassZones = new List<GrassZone>();

        private bool _hasMoveTarget;
        private bool _keepMovingToTarget;
        private Vector3 _moveTarget;
        private float _moveSpeed;
        private float _movePathLength;

        private NavMeshPath _path;
        private NavMeshPath _tempPath;
        private Vector3[] _pathPoints = new Vector3[64];
        private Vector3[] _tempPoints = new Vector3[64];
        private int _pathLength;
        private int _currentPathIndex;

        private bool _hasCheckedIfReachable = false;
        private Vector3 _positionToCheckIfReachable;

        private Vector3 _moveVelocity = Vector3.zero;

        private bool _isAvoidingMover;
        private float _avoidingTimer;
        private Vector3 _avoidDirection;
        private float _avoidSpeed;

        private Actor[] _nearbyActors;
        private int _nearbyActorCount;
        private float _nearbyActorTime;
        private float _nearbyActorDelay;

        private float _throwTimer;
        private Vector3 _throwTarget;
        private bool _isThrowingGrenade;

        #endregion

        #region Events

        /// <summary>
        /// The actor enters a flashlight or any similar object.
        /// </summary>
        public void OnEnterGrass(GrassZone zone)
        {
            if (!_grassZones.Contains(zone))
                _grassZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a lighted area.
        /// </summary>
        public void OnLeaveGrass(GrassZone zone)
        {
            if (_grassZones.Contains(zone))
                _grassZones.Remove(zone);
        }

        /// <summary>
        /// The actor enters a flashlight or any similar object.
        /// </summary>
        public void OnEnterLight(LightZone zone)
        {
            if (!_lightZones.Contains(zone))
                _lightZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a lighted area.
        /// </summary>
        public void OnLeaveLight(LightZone zone)
        {
            if (_lightZones.Contains(zone))
                _lightZones.Remove(zone);
        }

        /// <summary>
        /// The actor enters a dark area.
        /// </summary>
        public void OnEnterDarkness(DarkZone zone)
        {
            if (!_darkZones.Contains(zone))
                _darkZones.Add(zone);
        }

        /// <summary>
        /// The actor leaves a dark area.
        /// </summary>
        public void OnLeaveDarkness(DarkZone zone)
        {
            if (_darkZones.Contains(zone))
                _darkZones.Remove(zone);
        }

        /// <summary>
        /// Notified that an enemy actor has been hit. May be set as the Threat.
        /// </summary>
        public void OnSuccessfulHit(Hit hit)
        {
            var actor = hit.Target.GetComponent<BaseActor>();

            if (actor != null && actor.Side != Side)
                _possibleThreat = actor;
        }

        #endregion

        #region Commands

        public void InputCustomAction(string name)
        {
            _motor.SendMessage("OnCustomAction", name);
        }

        public bool CanMoveTo(Vector3 target)
        {
            if (_tempPath == null)
                _tempPath = new NavMeshPath();

            AIUtil.Path(ref _tempPath, transform.position, target);

            return _tempPath.status == NavMeshPathStatus.PathComplete;
        }

        public bool CanMoveToAvoiding(Vector3 target, Vector3 avoidTarget, float distance)
        {
            if (_tempPath == null)
                _tempPath = new NavMeshPath();

            AIUtil.Path(ref _tempPath, transform.position, target);

            if (_tempPath.status == NavMeshPathStatus.PathComplete)
            {
                var count = _tempPath.GetCornersNonAlloc(_tempPoints);

                for (int i = 0; i < count - 1; i++)
                {
                    var a = _tempPoints[i];
                    var b = _tempPoints[i + 1];

                    if (Util.DistanceToSegment(avoidTarget, a, b) <= distance)
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        public void InputMoveTo(Vector3 target, float speed)
        {
            _moveSpeed = speed;
            _movePathLength = Vector3.Distance(target, transform.position);
            _keepMovingToTarget = true;

            if (Vector3.Distance(_moveTarget, target) > 0.3f ||
                (!_hasMoveTarget && Vector3.Distance(transform.position, target) > 0.3f))
            {
                _hasMoveTarget = true;
                _moveTarget = target;
                updatePath();

                updatePotentialFutureCover(target, true);
            }

            if (_hasMoveTarget)
                Debug.DrawLine(transform.position, _moveTarget, Color.magenta);
        }

        public void InputLook(Vector3 position)
        {
            _motor.SetBodyTarget(position, 1);
        }

        public void InputAim(Vector3 position)
        {
            _motor.SetBodyTarget(position, 1);
            _motor.SetAimTarget(position);
            _motor.InputMeleeTarget(position);
            _motor.InputAim();
        }

        public void InputAim()
        {
            _motor.InputAim();
        }

        public void InputCrouch()
        {
            _motor.InputCrouch();
        }

        public void InputFire(Vector3 position)
        {
            _motor.SetBodyTarget(position, 1);
            _motor.SetAimTarget(position);
            _motor.InputFire();
        }

        public void InputFire()
        {
            _motor.InputFire();
        }

        public void InputFireAvoidFriendly(Vector3 position)
        {
            _motor.SetBodyTarget(position, 1);
            _motor.SetAimTarget(position);
            _motor.InputFireOnCondition(Side);
        }

        public void InputFireAvoidFriendly()
        {
            _motor.InputFireOnCondition(Side);
        }

        public void InputMelee(Vector3 position)
        {
            _motor.InputMelee(position);
        }

        public void InputMelee()
        {
            _motor.InputMelee();
        }

        public void InputBlock()
        {
            _motor.InputBlock();
        }

        public void InputMovement(CharacterMovement movement)
        {
            _motor.InputMovement(movement);
        }

        public void InputRoll(float angle)
        {
            _motor.InputRoll(angle);
        }

        public void InputReload()
        {
            _motor.InputReload();
        }

        public void InputUnequip()
        {
            _motor.InputUnequip();
        }

        public void InputEquip()
        {
            _motor.IsEquipped = true;
        }

        public void InputEquip(ref WeaponDescription weapon)
        {
            _motor.Weapon = weapon;
            _motor.IsEquipped = true;
        }

        public void InputThrowGrenade(Vector3 target)
        {
            if (!_isThrowingGrenade)
                _throwTimer = 0.5f;

            _isThrowingGrenade = true;
            _throwTarget = target;

            _motor.InputTakeGrenade();
        }

        public void InputTakeCover()
        {
            _motor.InputImmediateCoverSearch();
            _motor.InputForceTakeCover();
        }

        public void InputLeaveCover()
        {
            _motor.InputLeaveCover();
        }

        #endregion

        #region Behaviour

        protected override void Update()
        {
            base.Update();

            GlobalSearchCache.Update();

            if (_isThrowingGrenade)
            {
                _throwTimer -= Time.deltaTime;

                if (_throwTimer <= 0)
                {
                    _isThrowingGrenade = false;
                    _motor.InputThrowGrenade(_throwTarget);
                }
            }
            else
                _throwTimer = 0;

            if (_isAvoidingMover)
            {
                if (_avoidingTimer > float.Epsilon)
                {
                    _avoidingTimer -= Time.deltaTime;
                    move(_avoidDirection, false);
                }
                else
                    _isAvoidingMover = false;
            }

            updatePotentialFutureCover(_hasMoveTarget ? _moveTarget : transform.position, false);
            updateRegisteredCover(_hasMoveTarget ? _moveTarget : transform.position);

            if (_hasMoveTarget)
            {
                var vectorToPath = Vector3.zero;
                var isCloseToThePath = false;
                var distanceToPath = 0f;

                if (_currentPathIndex <= _pathLength - 1)
                {
                    vectorToPath = Util.VectorToSegment(transform.position, _pathPoints[_currentPathIndex], _pathPoints[_currentPathIndex + 1]);
                    distanceToPath = vectorToPath.magnitude;
                    isCloseToThePath = distanceToPath < 0.5f;
                }

                if (!isCloseToThePath)
                    updatePath();

                var isLastStepOnPartialPath = _currentPathIndex >= _pathLength - 2 && _path.status == NavMeshPathStatus.PathPartial;

                if (_path.status != NavMeshPathStatus.PathInvalid && !_hasCheckedIfReachable)
                {
                    //if (_pathLength == 0 || Vector3.Distance(_pathPoints[_pathLength - 1], _positionToCheckIfReachable) >= 0.2f)
                    //Message("OnPositionUnreachable", _positionToCheckIfReachable);

                    _hasCheckedIfReachable = true;
                }

                var distanceToTarget = Util.HorizontalDistance(transform.position, _moveTarget);

                if (distanceToTarget >= 0.5f && !isLastStepOnPartialPath)
                {
                    if (_path.status == NavMeshPathStatus.PathInvalid || _pathLength == 0)
                        updatePath();

                    var previousPoint = _currentPathIndex < _pathLength ? _pathPoints[_currentPathIndex] : transform.position;
                    var point = _currentPathIndex + 1 < _pathLength ? _pathPoints[_currentPathIndex + 1] : _moveTarget;

                    updateMovePathLength();

                    if (!AIUtil.IsPositionOnNavMesh(point))
                        updatePath();

                    if (Util.HorizontalDistance(point, _moveTarget) > 0.5f)
                    {
                        var forward = point - previousPoint;
                        forward.y = 0;
                        forward.Normalize();

                        var offset = 1f;
                        NavMeshHit hit;

                        var right = Vector3.Cross(forward, Vector3.up);
                        var hasLeft = !NavMesh.Raycast(point, point - right * offset, out hit, NavMesh.AllAreas);
                        var hasRight = !NavMesh.Raycast(point, point + right * offset, out hit, NavMesh.AllAreas);
                        var original = point;

                        if (hasLeft && !hasRight)
                        {
                            point -= right * offset;

                            if (!AIUtil.GetClosestStandablePosition(ref point))
                                point = original;
                        }
                        else if (hasRight && !hasLeft)
                        {
                            point += right * offset;

                            if (!AIUtil.GetClosestStandablePosition(ref point))
                                point = original;
                        }
                    }

                    var direction = point - transform.position;
                    direction.y = 0;

                    var distanceToPoint = direction.magnitude;

                    if (distanceToPoint > float.Epsilon)
                        direction /= distanceToPoint;

                    var velocity = Vector3.zero;
                    var speed = _moveSpeed;

                    if (distanceToTarget < 2)
                        speed = 0.5f;

                    if (!checkIncomingCollisions(direction, _moveSpeed))
                    {
                        if (distanceToPoint < 0.2f && _currentPathIndex + 1 < _pathLength)
                        {
                            var index = _currentPathIndex;

                            if (distanceToPoint > 0.07f && _currentPathIndex + 2 < _pathLength)
                            {
                                if (Vector3.Dot(point - transform.position, _pathPoints[_currentPathIndex + 2] - transform.position) <= 0.1f)
                                    _currentPathIndex++;
                            }
                            else
                                _currentPathIndex++;
                        }

                        if (distanceToPath > 0.12f)
                            direction = (direction + vectorToPath).normalized;

                        velocity = direction * _moveSpeed;

                        move(velocity, false);
                    }
                }
                else
                {
                    _movePathLength = Vector3.Distance(transform.position, _moveTarget);

                    if (distanceToTarget > 0.03f)
                    {
                        var vector = _moveTarget - transform.position;
                        vector.y = 0;

                        var direction = vector.normalized;

                        if (!checkIncomingCollisions(direction, _moveSpeed))
                        {
                            if (vector.magnitude < 0.2f)
                            {
                                transform.position = Util.Lerp(transform.position, _moveTarget, 6);
                            }
                            else
                            {
                                if (_motor.IsInCover)
                                    move(direction, false);
                                else
                                    move(direction * 0.5f, false);
                            }
                        }
                    }
                    else
                    {
                        _motor.transform.position = _moveTarget;
                        _hasMoveTarget = false;
                        updatePotentialFutureCover(transform.position, true);
                    }
                }
            }
            else
                checkIncomingCollisions(Vector3.zero, 1);

            if (!_keepMovingToTarget && _hasMoveTarget)
            {
                _hasMoveTarget = false;
                updatePotentialFutureCover(transform.position, true);
            }

            _keepMovingToTarget = false;

            var moveDirection = _motor.MovementDirection;

            if (Vector3.Dot(_moveDirection, moveDirection) < -0.75f)
                _moveDirection = moveDirection;
            else
                Util.Lerp(ref _moveDirection, moveDirection, 4);
        }

        protected override void Awake()
        {
            base.Awake();

            _motor = GetComponent<CharacterMotor>();
            _brain = GetComponent<BaseBrain>();
        }

        protected override void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
            _moveDirection = transform.forward;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            GlobalSearchCache.Restart();
        }

        #endregion

        #region Private methods



        /// <summary>
        /// Sets up the actor to move to the target position.
        /// </summary>
        private void updatePath()
        {
            AIUtil.Path(ref _path, transform.position, _moveTarget);

            _pathLength = _path.GetCornersNonAlloc(_pathPoints);
            _currentPathIndex = 0;

            if (_pathLength > _pathPoints.Length)
                _pathLength = _pathPoints.Length;

            _hasCheckedIfReachable = false;
            _positionToCheckIfReachable = _moveTarget;

            updateMovePathLength();
        }

        private void updateMovePathLength()
        {
            var point = _currentPathIndex + 1 < _pathLength ? _pathPoints[_currentPathIndex + 1] : _moveTarget;

            _movePathLength = Vector3.Distance(transform.position, point);

            for (int i = _currentPathIndex + 1; i < _pathLength - 1; i++)
                _movePathLength += Vector3.Distance(_pathPoints[i], _pathPoints[i + 1]);
        }

        private void move(Vector3 velocity, bool noPath)
        {
            if (_moveVelocity.magnitude > velocity.magnitude)
                _moveVelocity = velocity;
            else
                Util.Lerp(ref _moveVelocity, velocity, 3);

            var speed = _moveVelocity.magnitude;

            if (speed < 0.49f)
                return;

            var direction = _moveVelocity / speed;

            var origin = transform.position + Vector3.up * 0.5f;
            var target = origin + direction;

            const float threshold = 0.5f;

            if (!Util.IsFree(gameObject, origin, direction, threshold, false, true))
            {
                if (noPath)
                {
                    var right = Vector3.Cross(direction, Vector3.up);

                    if (Util.IsFree(gameObject, origin, right, 0.5f, false, true))
                        direction = right;
                    else if (Util.IsFree(gameObject, origin, -right, threshold, false, true))
                        direction = -right;
                }
                else
                    updatePath();
            }

            _motor.InputMovement(new CharacterMovement(direction, speed));
        }

        private bool checkIncomingCollisions(Vector3 ownMovement, float speed)
        {
            if (!AvoidOtherActors)
                return false;

            if (Time.realtimeSinceStartup - _nearbyActorTime >= _nearbyActorDelay)
            {
                _nearbyActorDelay = 0.5f + Random.Range(-0.2f, 0.2f);
                _nearbyActorTime = Time.realtimeSinceStartup;

                var count = AIUtil.FindActors(transform.position, ObstructionRadius, this);

                if (count > 0 && (_nearbyActors == null || _nearbyActors.Length < count))
                    _nearbyActors = new Actor[count];

                _nearbyActorCount = 0;

                for (int i = 0; i < count; i++)
                {
                    var a = AIUtil.Actors[i] as Actor;

                    if (a != null)
                        _nearbyActors[_nearbyActorCount++] = a;
                }
            }

            var ownMovementMagnitude = ownMovement.magnitude;
            var hasOwnMovement = ownMovementMagnitude > 0.25f;
            var ownDirection = ownMovement / ownMovementMagnitude;

            var closestDot = 0f;
            var closestVector = Vector3.zero;
            var closestDirection = Vector3.zero;
            var closestPosition = Vector3.zero;
            var isClosestPrimary = false;
            var isClosestStatic = false;
            var hasClosest = false;

            for (int i = 0; i < _nearbyActorCount; i++)
            {
                var other = _nearbyActors[i];

                Vector3 velocity;
                float magnitude;

                var isStatic = false;

                if (other._isAvoidingMover)
                {
                    velocity = other._avoidDirection;
                    magnitude = 1;
                }
                else
                {
                    velocity = other.Motor.MovementDirection;
                    magnitude = velocity.magnitude;

                    if (magnitude < 0.1f)
                    {
                        var body = other.Body;

                        if (body == null)
                            continue;

                        velocity = body.velocity;
                        magnitude = velocity.magnitude;

                        if (magnitude < 0.1f)
                        {
                            if (hasOwnMovement)
                            {
                                isStatic = true;
                                velocity = -ownMovement;
                                magnitude = 1;
                            }
                            else
                                continue;
                        }
                    }
                }

                var direction = velocity / magnitude;

                if (hasOwnMovement && Vector3.Dot(ownDirection, direction) > -0.5f)
                    continue;

                var vector = (transform.position - other.transform.position).normalized;
                var dot = Vector3.Dot(direction, vector);

                if (dot < 0.7f)
                    continue;

                var isPrimary = !other._isAvoidingMover && !isStatic;

                if (!hasClosest || (isClosestStatic && !isStatic) || (isPrimary && !isClosestPrimary) || (isPrimary && dot > closestDot) || (!isPrimary && !isClosestPrimary && dot > closestDot))
                {
                    hasClosest = true;
                    isClosestPrimary = isPrimary;
                    isClosestStatic = isStatic;
                    closestPosition = other.transform.position;
                    closestDirection = direction;
                    closestVector = vector;
                    closestDot = dot;
                }
            }

            if (hasClosest)
            {
                if (!isClosestPrimary && !isClosestStatic && !hasOwnMovement && canMoveInDirection(closestVector))
                    return avoid(closestVector, speed);

                var point = Util.FindClosestToPath(closestPosition, closestPosition + closestDirection * 100, transform.position);
                var vector = transform.position - point;
                var distance = vector.magnitude;

                if (distance < 0.1f)
                {
                    var right = Vector3.Cross(closestVector, Vector3.up);
                    var left = -right;

                    if (hasOwnMovement && isClosestStatic)
                    {
                        right = (right + ownMovement).normalized;
                        left = (left + ownMovement).normalized;
                    }

                    if (canMoveInDirection(right))
                        return avoid(right, speed);

                    if (canMoveInDirection(left))
                        return avoid(left, speed);
                }
                else
                {
                    var direction = vector / distance;

                    if (hasOwnMovement && isClosestStatic)
                        direction = (direction + ownMovement).normalized;

                    if (canMoveInDirection(direction))
                        return avoid(direction, speed);
                }

                if (isClosestPrimary && !isClosestStatic && !hasOwnMovement && canMoveInDirection(closestVector))
                    return avoid(closestVector, speed);
            }

            return false;
        }

        private bool avoid(Vector3 direction, float speed)
        {
            _avoidingTimer = 0.15f;
            _isAvoidingMover = true;
            _avoidDirection = direction;
            _avoidSpeed = speed;
            move(direction * speed, true);

            return true;
        }

        private bool canMoveInDirection(Vector3 vector)
        {
            if (AIUtil.IsNavigationBlocked(transform.position, transform.position + vector))
                return false;

            return true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Calculates the view distance when looking at this actor.
        /// </summary>
        public float GetViewDistance(float viewDistance, bool isAlerted)
        {
            return Util.GetViewDistance(viewDistance, _darkZones, _lightZones, _motor.IsCrouching ? _grassZones : null, isAlerted);
        }

        #endregion
    }
}
