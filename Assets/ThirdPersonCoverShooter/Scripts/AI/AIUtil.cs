using UnityEngine;
using UnityEngine.AI;

using CoverShooter.AI;

namespace CoverShooter
{
    /// <summary>
    /// A number of utility functions related to the AI.
    /// </summary>
    public static class AIUtil
    {
        /// <summary>
        /// BaseActor array filled by some of the methods.
        /// </summary>
        public static BaseActor[] Actors = new BaseActor[128];

        private static Collider[] _colliders = new Collider[512];
        private static Vector3[] _corners = new Vector3[128];

        public static bool CompareTeams(BaseActor self, BaseActor other, ActorTeam team)
        {
            if (self == null || other == null)
                return false;

            switch (team)
            {
                case ActorTeam.All:
                    return true;

                case ActorTeam.Enemy:
                    return self.Side != other.Side;

                case ActorTeam.Friendly:
                    return self.Side == other.Side;
            }

            return false;
        }

        /// <summary>
        /// Finds all actors near the given position in a given radius. Includes actors that are dead. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActorsIncludingDead(Vector3 position, float radius, BaseActor ignore = null)
        {
            return FindActors(position, radius, false, ignore);
        }

        /// <summary>
        /// Finds all actors near the given position in a given radius. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActors(Vector3 position, float radius, BaseActor ignore = null)
        {
            return FindActors(position, radius, true, ignore);
        }

        /// <summary>
        /// Finds all actors near the given position in a given radius. Returns number of them and fills the Actors array with the results.
        /// </summary>
        public static int FindActors(Vector3 position, float radius, bool ignoreDead, BaseActor ignore = null)
        {
            int count = 0;
            var physicsCount = Physics.OverlapSphereNonAlloc(position, radius, _colliders, Layers.Character);

            for (int i = 0; i < physicsCount; i++)
            {
                if (ignore != null && _colliders[i].gameObject == ignore.gameObject)
                    continue;

                var actor = CoverShooter.Actors.Get(_colliders[i].gameObject);

                if (actor != null && (!ignoreDead || actor.IsAlive))
                {
                    if (count < Actors.Length)
                        Actors[count++] = actor;
                    else
                        return count;
                }
            }

            return count;
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius. Can include dead actors.
        /// </summary>
        public static BaseActor FindClosestActorIncludingDead(Vector3 position, float radius, BaseActor ignore = null)
        {
            return FindClosestActor(position, radius, false, ignore);
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius.
        /// </summary>
        public static BaseActor FindClosestActor(Vector3 position, float radius, BaseActor ignore = null)
        {
            return FindClosestActor(position, radius, true, ignore);
        }

        /// <summary>
        /// Finds a closest actor to the given position in a given radius.
        /// </summary>
        public static BaseActor FindClosestActor(Vector3 position, float radius, bool ignoreDead, BaseActor ignore = null)
        {
            BaseActor result = null;
            float minDist = 0;

            var physicsCount = Physics.OverlapSphereNonAlloc(position, radius, _colliders, Layers.Character);

            for (int i = 0; i < physicsCount; i++)
            {
                if (ignore != null && _colliders[i].gameObject == ignore.gameObject)
                    continue;

                var actor = _colliders[i].GetComponent<BaseActor>();

                if (actor != null && (!ignoreDead || actor.IsAlive))
                {
                    var dist = Vector3.Distance(actor.transform.position, position);

                    if (result == null || dist < minDist)
                    {
                        result = actor;
                        minDist = dist;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if a ray cannot be traced on a navmesh without hiting anything.
        /// </summary>
        public static bool IsNavigationBlocked(Vector3 origin, Vector3 target)
        {
            NavMeshHit hit;
            return NavMesh.Raycast(origin, target, out hit, NavMesh.AllAreas);
        }

        /// <summary>
        /// Modifies the position to the closed on a nav mesh. Returns true if any were found.
        /// </summary>
        public static bool GetClosestStandablePosition(ref Vector3 position)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(position, out hit, 3, NavMesh.AllAreas))
            {
                position = hit.position;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns true if the given position is on a nav mesh.
        /// </summary>
        public static bool IsPositionOnNavMesh(Vector3 position)
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(position, out hit, 0.2f, NavMesh.AllAreas);
        }

        /// <summary>
        /// Returns closest edge to the point on the nav mesh.
        /// </summary>
        public static Vector3 ClosestNavMeshEdge(Vector3 position)
        {
            NavMeshHit hit;

            if (NavMesh.FindClosestEdge(position, out hit, NavMesh.AllAreas))
                return hit.position;
            else
                return position;
        }

        /// <summary>
        /// Calculates a path from the source to target.
        /// </summary>
        public static void Path(ref NavMeshPath path, Vector3 source, Vector3 target)
        {
            if (path == null)
                path = new NavMeshPath();

            GetClosestStandablePosition(ref source);
            GetClosestStandablePosition(ref target);

            NavMesh.CalculatePath(source, target, NavMesh.AllAreas, path);
        }

        public static float DistanceOf(NavMeshPath path)
        {
            int count = path.GetCornersNonAlloc(_corners);

            float distance = 0;

            for (int i = 1; i < count; i++)
                distance += Vector3.Distance(_corners[i - 1], _corners[i]);

            return distance;
        }

        /// <summary>
        /// Returns true if a given position is in sight.
        /// </summary>
        public static bool IsInSight(Actor actor, Vector3 target, float maxDistance, float fieldOfView, float obstacleObstructionDistance = 1)
        {
            var motorTop = actor.StandingTopPosition;
            var vector = target - motorTop;

            if (vector.magnitude > maxDistance)
                return false;

            vector.y = 0;

            var vectorXZ = new Vector3(vector.x, 0, vector.z).normalized;
            var headXZ = new Vector3(actor.HeadDirection.x, 0, actor.HeadDirection.z).normalized;

            var angle = Mathf.Abs(Mathf.DeltaAngle(0, Mathf.Acos(Vector3.Dot(vectorXZ, headXZ)) * Mathf.Rad2Deg));
            if (angle > fieldOfView * 0.5f)
                return false;

            return vector.magnitude < obstacleObstructionDistance || !IsObstructed(motorTop, target);
        }

        /// <summary>
        /// Returns true if there is no unobstructed line between the given origin and the target.
        /// </summary>        
        public static bool IsObstructed(Vector3 origin, Vector3 target, float threshold = 0.4f)
        {
            var vector = target - origin;
            var distanceToTarget = vector.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(origin, vector / distanceToTarget, out hit, distanceToTarget + 0.5f, Layers.Geometry, QueryTriggerInteraction.Ignore))
            {
                var distanceToHit = Vector3.Distance(origin, hit.point);

                if (distanceToHit > distanceToTarget)
                    return false;
                else if (distanceToTarget - distanceToHit < threshold)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
    }
}
