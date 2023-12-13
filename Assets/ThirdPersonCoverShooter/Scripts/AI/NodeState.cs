using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public struct NodeState
    {
        public float Duration;
        public bool IsHolding;
        public float Hold;
        public bool HasFinished;

        public ActionState Values;
    }

    public struct ActionState
    {
        public Vector3 Position;
        public float Angle;
        public float Covered;
        public float Time;
        public int Index;
        public int Count;
        public int Count2;
        public bool HasStarted;
        public bool IsRunning;
        public bool IsSearching;
        public bool IsThrowing;
        public bool IsInvalid;
        public bool IsWaiting;
        public Vector3 ApproachPosition;
        public Cover ApproachCover;
        public AISearchState SearchState;
        public Waypoints Waypoints;
    }
}