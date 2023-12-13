using System;
using UnityEngine;

namespace CoverShooter.AI
{
    public enum AIEvent
    {
        Trigger,
        GetHit, // Position, Normal, Attacker
        HearEnemySound, // Position, Object
        NewTarget,
        NoTarget,
        NewTargetPosition,
        Investigate, // Position
        Search, // Position
        TargetVisible,
        TargetInvisible,
        TargetTooClose,
        HearFriendTarget, // Target, Position, Visible, Sender
        Dead,
        SelfDead,
        TargetDead,
        FriendDead,
        EnemyDead,
        Resurrected,
        SelfResurrected,
        FriendResurrected,
        EnemyResurrected,
        Spawned,
        SelfSpawned,
        FriendSpawned,
        EnemySpawned
    }

    [Serializable]
    public struct EventDesc
    {
        /// <summary>
        /// Type of the event.
        /// </summary>
        public AIEvent Type;

        /// <summary>
        /// ID of the trigger (if Type is 'trigger').
        /// </summary>
        public int ID;

        /// <summary>
        /// Name of the custom trigger.
        /// </summary>
        public string Name;

        public Value Value0;
        public Value Value1;
        public Value Value2;
        public Value Value3;
    }
}