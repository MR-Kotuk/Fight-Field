using System.Collections.Generic;
using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Denotes an open area for the AI to check out when searching. By default AI only searches covers, using this component an additional areas can be added.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class SearchZone : Zone<SearchZone>
    {
        /// <summary>
        /// Returns a list of all checkable points inside the zone.
        /// </summary>
        public IEnumerable<Vector3> Points(float threshold)
        {
            var countx = (int)(Width / threshold);
            var countz = (int)(Depth / threshold);

            if (countx < 3)
                countx = 3;
            else
                countx += 1 - countx % 2;

            if (countz < 3)
                countz = 3;
            else
                countz += 1 - countz % 2;

            var boxSize = boxCollider.size;
            var xstep = boxSize.x / (countx - 1);
            var zstep = boxSize.z / (countz - 1);
            var xorigin = -boxSize.x * 0.5f;
            var zorigin = -boxSize.z * 0.5f;

            Vector3 position;
            position.y = -boxSize.y * 0.5f;

            for (int x = 0; x < countx; x++)
            {
                position.x = xorigin + xstep * x;

                for (int z = 0; z < countz; z++)
                {
                    position.z = zorigin + zstep * z;

                    var worldPosition = transform.TransformPoint(position);
                    if (AIUtil.GetClosestStandablePosition(ref worldPosition))
                        yield return worldPosition;
                }
            }
        }
    }

    /// <summary>
    /// Maintains a list of search zones inside an area (denoted by a position and radius).
    /// </summary>
    public class SearchZoneCache
    {
        public List<SearchZone> Items = new List<SearchZone>();

        /// <summary>
        /// Creates a list of search zones that are in the area surounding the observer.
        /// </summary>
        public void Reset(Vector3 observer, float maxDistance)
        {
            Items.Clear();
            var count = Physics.OverlapSphereNonAlloc(observer, maxDistance, Util.Colliders, Layers.Zones, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var collider = Util.Colliders[i];
            
                if (!collider.isTrigger)
                    continue;

                var block = SearchZone.Get(collider.gameObject);

                if (block != null)
                    Items.Add(block);
            }
        }
    }
}
