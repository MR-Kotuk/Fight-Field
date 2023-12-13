using UnityEngine;

namespace CoverShooter
{
    public enum Limb
    {
        RightHand,
        LeftHand
    }

    public abstract class BaseMelee : MonoBehaviour
    {
        /// <summary>
        /// Animations and related assets to be used with this weapon.
        /// </summary>
        [Tooltip("Animations and related assets to be used with this weapon.")]
        public WeaponType Type = WeaponType.Fist;

        /// <summary>
        /// Owning object with a CharacterMotor component.
        /// </summary>
        [HideInInspector]
        public CharacterMotor Character;

        protected BaseActor Actor
        {
            get
            {
                if (_actor == null && !_triedGetActor)
                {
                    _actor = Character.GetComponent<BaseActor>();
                    _triedGetActor = true;
                }

                return _actor;
            }
        }

        private BaseActor _actor;
        private bool _triedGetActor;

        /// <summary>
        /// Return true if an attack can be started.
        /// </summary>
        public abstract bool Request();

        /// <summary>
        /// Start the attack.
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// End the attack.
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Start scanning for hits during the attack.
        /// </summary>
        public abstract void BeginScan();

        /// <summary>
        /// Stop scanning for hits during the attack.
        /// </summary>
        public abstract void EndScan();

        /// <summary>
        /// A significant moment during a melee attack, used to play effects.
        /// </summary>
        public abstract void Moment();

        protected bool IsFriend(GameObject gameObject)
        {
            if (Character == null)
                return false;

            if (Actor == null)
                return false;

            var other = Actors.Get(gameObject);

            if (other == null)
                return false;

            return Actor.Side == other.Side;
        }
    }
}