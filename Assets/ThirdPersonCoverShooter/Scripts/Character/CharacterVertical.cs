using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterVertical : MonoBehaviour
    {
        public float Offset = 0.4f;
        public float Threshold = 0.4f;

        [HideInInspector]
        public RaycastHit[] Hits = new RaycastHit[16];

        [HideInInspector]
        public int Count;

        public Vector3 GetStart()
        {
            return transform.position + Vector3.up * Offset;
        }

        private void Update()
        {
            var start = GetStart();
            Count = Physics.RaycastNonAlloc(GetStart(), Vector3.down, Hits, Threshold + Offset, Layers.Geometry);
        }
    }
}