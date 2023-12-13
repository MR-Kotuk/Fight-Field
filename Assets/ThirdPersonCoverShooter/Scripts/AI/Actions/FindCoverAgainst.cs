using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter.AI
{
    [Folder("Find")]
    [Success("Found")]
    [SuccessParameter("Object", ValueType.GameObject, false)]
    [SuccessParameter("Position", ValueType.GameObject)]
    [Failure("Failed")]
    [Immediate]
    public class FindCoverAgainst : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value EnemyPosition;

        [ValueType(ValueType.Float)]
        public Value MinDistance = new Value(0f);

        [ValueType(ValueType.Float)]
        public Value MaxDistance = new Value(30);

        [ValueType(ValueType.Float)]
        public Value MinEnemyDistance = new Value(5);

        [ValueType(ValueType.Float)]
        public Value MaxEnemyDistance = new Value(30);

        [ValueType(ValueType.Float)]
        public Value AvoidDistance = new Value(5);

        [ValueType(ValueType.Float)]
        public Value TakenThreshold = new Value(2);

        private NavMeshPath _path;
        private Vector3[] _corners;

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;

            var self = state.Actor;
            var selfPosition = actor.transform.position;
            var minDistance = state.Dereference(ref MinDistance).Float;
            var maxDistance = state.Dereference(ref MaxDistance).Float;
            var enemyPosition = state.GetPosition(ref EnemyPosition);
            var minEnemyDistance = state.Dereference(ref MinEnemyDistance).Float;
            var maxEnemyDistance = state.Dereference(ref MaxEnemyDistance).Float;
            var avoidDistance = state.Dereference(ref AvoidDistance).Float;
            var takenThreshold = state.Dereference(ref TakenThreshold).Float;

            var currentDistanceToEnemy = Vector3.Distance(selfPosition, enemyPosition);

            var foundCount = Physics.OverlapSphereNonAlloc(selfPosition, maxDistance, Util.Colliders, Layers.Cover, QueryTriggerInteraction.Collide);

            GameObject closest = null;
            var closestPosition = Vector3.zero;
            var closestDistance = 0f;

            for (int i = 0; i < foundCount; i++)
            {
                var coverObject = Util.Colliders[i].gameObject;
                var cover = CoverSearch.GetCover(coverObject);

                if (cover == null)
                    continue;

                var position = Vector3.zero;

                if (AICoverUtil.Consider(cover, ref position, selfPosition, maxDistance, takenThreshold, state.Actor))
                {
                    var distanceToCover = Vector3.Distance(selfPosition, position);

                    if (distanceToCover >= minDistance && distanceToCover <= maxDistance)
                    {
                        var distanceToEnemy = Vector3.Distance(enemyPosition, position);

                        if (distanceToEnemy >= minEnemyDistance && distanceToEnemy <= maxEnemyDistance)
                            if (AICoverUtil.IsValidCoverAgainst(cover, position, enemyPosition))
                            {
                                bool hasPath = false;

                                if (_path == null)
                                    _path = new NavMeshPath();

                                if (NavMesh.CalculatePath(selfPosition, position, 1, _path) && _path.status == NavMeshPathStatus.PathComplete)
                                {
                                    hasPath = true;

                                    if (currentDistanceToEnemy + 0.5f > avoidDistance)
                                    {
                                        if (_corners == null)
                                            _corners = new Vector3[32];

                                        var count = _path.GetCornersNonAlloc(_corners);

                                        for (int j = 0; j < count; j++)
                                        {
                                            var a = j == 0 ? selfPosition : _corners[j - 1];
                                            var b = _corners[j];

                                            var closestPointToEnemy = Util.FindClosestToPath(a, b, enemyPosition);

                                            if (Vector3.Distance(closestPointToEnemy, enemyPosition) < avoidDistance)
                                            {
                                                hasPath = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (hasPath)
                                    if (closest == null || closestDistance > distanceToCover)
                                    {
                                        closestPosition = position;
                                        closest = coverObject;
                                        closestDistance = distanceToCover;
                                    }
                            }
                    }
                }
            }

            if (closest != null)
                return AIResult.Success(new Value[] { new Value(closest), new Value(closestPosition) });
            else
                return AIResult.Failure();
        }
    }
}