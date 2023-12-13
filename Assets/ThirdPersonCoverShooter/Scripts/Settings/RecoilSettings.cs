using System;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Recoil settings for a gun.
    /// </summary>
    [Serializable]
    public struct GunRecoilSettings
    {
        /// <summary>
        /// Base degrees of error when firing.
        /// </summary>
        [Tooltip("Base degrees of error when firing.")]
        public float BaseBloom;

        /// <summary>
        /// Max degrees of error when firing.
        /// </summary>
        [Tooltip("Max degrees of error when firing.")]
        public float MaxBloom;

        /// <summary>
        /// Time in seconds to go from base to max degrees of error.
        /// </summary>
        [Tooltip("Time in seconds to go from base to max degrees of error.")]
        public float BloomTime;

        /// <summary>
        ///Time in seconds to go from max back to base degrees of error.
        /// </summary>
        [Tooltip("Time in seconds to go from max back to base degrees of error.")]
        public float BloomRecoveryTime;

        /// <summary>
        /// Intensity of the camera shake following a gun fire.
        /// </summary>
        [Tooltip("Intensity of the camera shake following a gun fire.")]
        public float ShakeIntensity;

        /// <summary>
        /// Duration of the camera shake following a gun fire.
        /// </summary>
        [Tooltip("Duration of the camera shake following a gun fire.")]
        public float ShakeTime;

        /// <summary>
        /// Default recoil settings.
        /// </summary>
        public static GunRecoilSettings Default()
        {
            var settings = new GunRecoilSettings();
            settings.BaseBloom = 0.1f;
            settings.MaxBloom = 2;
            settings.BaseBloom = 1;
            settings.BloomTime = 1;
            settings.BloomRecoveryTime = 0.5f;
            settings.ShakeIntensity = 1;
            settings.ShakeTime = 0.25f;

            return settings;
        }
    }
}