using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Spawns effects prefabs on various gun events like reloads or gunfire.
    /// </summary>
    [RequireComponent(typeof(BaseGun))]
    public class GunEffects : BaseEffects, IGunListener
    {
        /// <summary>
        /// Object to instantiate when ejecting a magazine.
        /// </summary>
        [Tooltip("Object to instantiate when ejecting a magazine.")]
        public GameObject Eject;

        /// <summary>
        /// Object to instantiate when a magazine is put inside the gun.
        /// </summary>
        [Tooltip("Object to instantiate when a magazine is put inside the gun.")]
        public GameObject Rechamber;

        /// <summary>
        /// Object to instantiate on each bullet fire.
        /// </summary>
        [Tooltip("Object to instantiate on each bullet fire.")]
        public GameObject Fire;

        /// <summary>
        /// Object to instantiate on each shotgun pump.
        /// </summary>
        [Tooltip("Object to instantiate on each shotgun pump.")]
        public GameObject Pump;

        /// <summary>
        /// Object to instantiate on each fire attempt with an empty magazine.
        /// </summary>
        [Tooltip("Object to instantiate on each fire attempt with an empty magazine.")]
        public GameObject EmptyFire;

        /// <summary>
        /// Object to instantiate to simulate shell ejection.
        /// </summary>
        [Tooltip("Object to instantiate to simulate shell ejection.")]
        public GameObject Shell;

        private BaseGun _gun;

        private void Awake()
        {
            _gun = GetComponent<BaseGun>();
        }

        protected override void OnDisable()
        {
            Disable(Eject);
            Disable(Rechamber);
            Disable(Fire);
            Disable(Pump);
            Disable(EmptyFire);
            Disable(Shell);
        }

        /// <summary>
        /// Play magazine eject effect.
        /// </summary>
        public void OnEject()
        {
            Instantiate(Eject, transform.position);
        }

        /// <summary>
        /// Play magazine load effect.
        /// </summary>
        public void OnRechamber()
        {
            Instantiate(Rechamber, transform.position);
        }

        /// <summary>
        /// Play shotgun pump effect.
        /// </summary>
        public void OnPump()
        {
            Instantiate(Pump, transform.position);
        }

        /// <summary>
        /// Play fire effects delayed by the given amount of time in seconds.
        /// </summary>
        /// <param name="delay">Time to delay the creation of effects.</param>
        public void OnFire(float delay)
        {
            if (_gun != null && _gun.Aim != null)
                InstantiateLocallyIn(delay, Fire, _gun.Aim.transform, Vector3.zero, Quaternion.identity);

            InstantiateIn(delay, Shell, transform.position);
        }

        /// <summary>
        /// Play an effect when the gun fails to fire.
        /// </summary>
        public void OnEmptyFire()
        {
            if (EmptyFire == null)
                return;

            Instantiate(EmptyFire, transform.position);
        }

        public void OnFullyLoaded() { }
        public void OnBulletLoad() { }
        public void OnPumpStart() { }
        public void OnMagazineLoadStart() { }
        public void OnBulletLoadStart() { }
    }
}