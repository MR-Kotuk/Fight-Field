using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(Collider))]
    public class Melee : BaseMelee
    {
        /// <summary>
        /// Damage done by a melee attack.
        /// </summary>
        [Tooltip("Damage done by a melee attack.")]
        public float Damage = 20;

        /// <summary>
        /// Time in seconds for to wait for another melee hit.
        /// </summary>
        [Tooltip("Time in seconds for to wait for another melee hit.")]
        public float Cooldown = 0.5f;

        /// <summary>
        /// Time in seconds between hits that the character will respond to with hurt animations.
        /// </summary>
        [Tooltip("Time in seconds between hits that the character will respond to with hurt animations.")]
        public float DamageResponseWaitTime = 0;

        private bool _isAttacking;
        private bool _isScanning;
        private float _cooldown;

        private List<GameObject> _receptors = new List<GameObject>();
        private IMeleeListener[] _listeners;

        private Collider _collider;
        
        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _listeners = Util.GetInterfaces<IMeleeListener>(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isAttacking || !_isScanning)
                return;

            if (Character != null && other.gameObject == Character.gameObject)
                return;

            var shield = BulletShield.Get(other.gameObject);

            if (shield != null)
                return;

            var part = other.GetComponent<BodyPartHealth>();
            var obj = (part == null || part.Target == null) ? other.gameObject : part.Target.gameObject;

            if (part != null && part.Target != null && IsFriend(part.Target.gameObject))
                return;

            if (_receptors.Contains(obj))
                return;

            _receptors.Add(obj);

            var normal = (Character.transform.position - obj.transform.position).normalized;

            HitType type;

            switch (Type)
            {
                case WeaponType.Pistol: type = HitType.PistolMelee; break;
                case WeaponType.Rifle: type = HitType.RifleMelee; break;
                case WeaponType.Shotgun: type = HitType.ShotgunMelee; break;
                case WeaponType.Sniper: type = HitType.SniperMelee; break;
                case WeaponType.Fist: type = HitType.Fist; break;
                case WeaponType.Machete: type = HitType.Machete; break;

                default:
                    type = HitType.Fist;
                    Debug.Assert(false, "Invalid melee type");
                    break;
            }

            var hit = new Hit(other.ClosestPointOnBounds(transform.position), normal, Damage, Character.gameObject, other.gameObject, type, DamageResponseWaitTime);
            other.SendMessage("OnHit", hit, SendMessageOptions.DontRequireReceiver);

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnMeleeHit();
        }

        /// <summary>
        /// Return true if an attack can be started.
        /// </summary>
        public override bool Request()
        {
            return !_isAttacking && _cooldown <= float.Epsilon;
        }

        /// <summary>
        /// Start the attack. 
        /// </summary>
        public override void Begin()
        {
            _isAttacking = true;
            _isScanning = false;
            _cooldown = Cooldown;
            _collider.enabled = false;
        }

        /// <summary>
        /// End the attack.
        /// </summary>
        public override void End()
        {
            _isAttacking = false;
        }

        /// <summary>
        /// Start scanning for hits during the attack.
        /// </summary>
        public override void BeginScan()
        {
            if (_isScanning)
                return;

            _isScanning = true;
            _collider.enabled = true;
            _receptors.Clear();

            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnMeleeAttack();
        }

        /// <summary>
        /// Stop scanning for hits during the attack.
        /// </summary>
        public override void EndScan()
        {
            if (_isScanning)
            {
                _collider.enabled = false;
                _isScanning = false;
            }
        }

        /// <summary>
        /// Notify all listeners of a melee moment.
        /// </summary>
        public override void Moment()
        {
            for (int i = 0; i < _listeners.Length; i++)
                _listeners[i].OnMeleeMoment();
        }

        private void Update()
        {
            if (_cooldown > float.Epsilon)
                _cooldown -= Time.deltaTime;
        }
    }
}