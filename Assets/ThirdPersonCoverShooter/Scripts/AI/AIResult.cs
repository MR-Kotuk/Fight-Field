using System;
using UnityEngine;

namespace CoverShooter.AI
{
    public enum AIResultType
    {
        Nothing,
        Hold,
        Success,
        Failure,
        TimeOut,
        Finish,
        Triggered
    }

    public struct AIResult
    {
        public AIResultType Type;
        public float Time;
        public Value[] Values;
        public bool CanHold;

        public AIResult(AIResultType type)
        {
            Type = type;
            Time = 0;
            Values = null;
            CanHold = false;
        }

        public AIResult(AIResultType type, float time)
        {
            Type = type;
            Time = time;
            Values = null;
            CanHold = true;
        }

        public AIResult(AIResultType type, Value[] values)
        {
            Type = type;
            Time = 0;
            Values = values;
            CanHold = false;
        }

        public AIResult(AIResultType type, Value[] values, float time)
        {
            Type = type;
            Time = 0;
            Values = values;
            CanHold = true;
        }

        public static AIResult Hold(float time = 0) { return new AIResult(AIResultType.Hold, time); }
        public static AIResult Success(Value[] values = null) { return new AIResult(AIResultType.Success, values); }
        public static AIResult Failure(Value[] values = null) { return new AIResult(AIResultType.Failure, values); }
        public static AIResult Finish(Value[] values = null) { return new AIResult(AIResultType.Finish, values); }
        public static AIResult SuccessOrHold(Value[] values = null) { return new AIResult(AIResultType.Success, values, 0); }
        public static AIResult FailureOrHold(Value[] values = null) { return new AIResult(AIResultType.Failure, values, 0); }
    }
}