using System;
using UnityEngine;

namespace CoverShooter
{
    public struct SustainedBool
    {
        public bool Value;
        public float Timer;

        public SustainedBool(bool value)
        {
            Value = value;
            Timer = 0;
        }

        public void Set(bool value, float dt, float threshold)
        {
            Timer += dt;

            if (Timer >= threshold)
            {
                Timer %= threshold;
                Value = value;
            }
        }
    }
}