using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CoverShooter
{
    public class AISearchState : MonoBehaviour
    {
        /// <summary>
        /// Offset from cover the AI keeps when approaching it from a side.
        /// </summary>
        public static float CoverOffset = 2;

        /// <summary>
        /// Search points belong in the same search block if they are closer to each other than this distance.
        /// </summary>
        public static float BlockThreshold = 3;

        /// <summary>
        /// A search point is considered to belong in a block if it is closer than this value to it's center.
        /// </summary>
        public static float BlockCenterThreshold = 6;

        /// <summary>
        /// Distance to target location the AI has to reach for it to be marked as investigated.
        /// </summary>
        public static float VerifyDistance = 16;

        /// <summary>
        /// Maximum distance of a location for AI to search.
        /// </summary>
        public static float MaxDistance = 10000;

        internal SustainedBool _shouldAim;
        internal SustainedBool _shouldMove;
        internal SustainedBool _shouldRun;

        internal bool _hasSearchDirection;
        internal bool _hasPreviousBlockPosition;
        internal Vector3 _searchDirection;
        internal Vector3 _previousBlockPosition;
        internal Vector3 _searchPosition;

        internal bool _hasPoint;
        internal int _pointIndex;
        internal SearchPoint _point;

        internal float _lookAtPointTimer;
        internal bool _hasSeenThePoint;

        internal bool _hasPreviousPoint;
        internal int _previousPointIndex;

        internal bool _isSearching;
        internal bool _hasApproached;

        internal SearchPointData _points = new SearchPointData();
        internal SearchBlock _block;
        internal SearchBlockCache _blockCache;
        internal List<SearchBlock> _investigatedBlocks = new List<SearchBlock>();
        internal List<SearchBlock> _blocks = new List<SearchBlock>();

        internal List<InvestigatedPoint> _investigated = new List<InvestigatedPoint>();

        internal float _timeOfReset;

        internal float _checkWait;

        private void Awake()
        {
            _block = new SearchBlock(_points);
            _blockCache = new SearchBlockCache(_points);
        }

        /// <summary>
        /// Told by the brains to forget all search history.
        /// </summary>
        public void ClearSearchHistory()
        {
            _timeOfReset = Time.timeSinceLevelLoad;
            _investigated.Clear();
            _blocks.Clear();
            _block.Clear();
        }

        public void MarkInvestigatedInRegion(Vector3 position, float radius)
        {
            for (int i = 0; i < _blocks.Count; i++)
            {
                for (int b = _blocks[i].Count - 1; b >= 0; b--)
                {
                    var p = _blocks[i].Get(b);

                    if (Vector3.Distance(position, p.Position) < radius)
                        _blocks[i].Investigate(b);
                }
            }
        }

        internal void finishInvestigatingThePoint()
        {
            var point = new InvestigatedPoint(_point.Position);
            _hasPoint = false;

            markInvestigated(point);
        }

        internal int addPoint(SearchPoint point)
        {
            point.CalcVisibility(VerifyDistance, false);
            var index = _points.Add(point);

            if (!_block.Empty)
                if (_block.IsClose(point, BlockThreshold, BlockCenterThreshold))
                {
                    _block.Add(index);
                    return index;
                }

            for (int i = 0; i < _blocks.Count; i++)
                if (_blocks[i].IsClose(point, BlockThreshold, BlockCenterThreshold))
                {
                    _blocks[i].Add(index);
                    return index;
                }

            var new_ = _blockCache.Take();
            new_.Add(index);
            _blocks.Add(new_);

            return index;
        }

        internal void debugBlock(SearchBlock block)
        {
            var color = Color.white;

            switch (block.Index % 5)
            {
                case 0: color = Color.red; break;
                case 1: color = Color.green; break;
                case 2: color = Color.blue; break;
                case 3: color = Color.yellow; break;
                case 4: color = Color.cyan; break;
            }

            for (int i = 0; i < block.Count; i++)
                debugPoint(block.Get(i), false, color);

            foreach (var index in block.InvestigatedIndices)
                debugPoint(_points.Points[index], !_hasPoint || index != _pointIndex, color);
        }

        internal void debugPoint(SearchPoint point, bool wasInvestigated, Color color)
        {
            Debug.DrawLine(point.Position, point.Position + Vector3.up * (wasInvestigated ? 0.2f : 0.75f), color);

            //if (point.Left >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Left].Position - point.Position) * 0.25f, Color.white);
            //if (point.Right >= 0) Debug.DrawLine(point.Position, point.Position + (_points.Points[point.Right].Position - point.Position) * 0.25f, Color.magenta);
        }

        internal bool findBestPoint(Actor actor, SearchBlock block, out int pointIndex, out float pointValue)
        {
            var pickedIndex = -1;
            var previousValue = 0f;

            var previousLeft = -1;
            var previousRight = -1;

            if (_hasPreviousPoint)
            {
                var previousPoint = _points.Points[_previousPointIndex];
                previousLeft = previousPoint.Left;
                previousRight = previousPoint.Right;
            }

            for (int i = 0; i < block.Count; i++)
            {
                var index = block.Indices[i];
                var point = block.Get(i);

                var vector = _searchPosition - point.Position;
                var distance = vector.magnitude;
                var direction = vector / distance;

                var value = distance;

                if (_hasPreviousPoint && (index == previousLeft || index == previousRight))
                    value *= -1;
                else
                {
                    if (_hasSearchDirection)
                        value *= -Vector3.Dot(direction, _searchDirection) * 0.5f + 1.5f;
                    else
                        value *= -Vector3.Dot(direction, actor.HeadDirection) * 0.5f + 1.5f;
                }

                if (pickedIndex < 0 || (value > 0 && value < previousValue) || (value < 0 && previousValue < 0 && value > previousValue) || (value < 0 && previousValue > 0))
                {
                    pickedIndex = i;
                    previousValue = value;
                }
            }

            pointIndex = pickedIndex;
            pointValue = previousValue;

            return pointIndex >= 0;
        }

        internal float investigationDistance(BaseActor actor, SearchPoint point)
        {
            var vector = transform.position - point.Position;
            var distanceToPoint = vector.magnitude;

            var checkDistance = VerifyDistance;

            if (point.Visibility < checkDistance)
                checkDistance = point.Visibility;

            float senseDistance = 0.5f;

            if (!point.HasNormal || Vector3.Dot(vector, point.Normal) > 0)
                senseDistance = 1.5f;

            if (checkDistance < senseDistance)
                checkDistance = senseDistance;

            return checkDistance;
        }

        internal bool canBeInvestigated(Actor actor, SearchPoint point, float verifyDistance, float verifyHeight, float fieldOfView)
        {
            var vector = transform.position - point.Position;
            var distanceToPoint = vector.magnitude;

            var checkDistance = verifyDistance;

            if (point.Visibility < checkDistance)
                checkDistance = point.Visibility;

            float senseDistance = 0.5f;

            if (!point.HasNormal || Vector3.Dot(vector, point.Normal) > 0)
                senseDistance = 1.5f;

            if (checkDistance < senseDistance)
                checkDistance = senseDistance;

            if (distanceToPoint < senseDistance)
                return !point.RequiresReaching || distanceToPoint < 0.5f;
            else if (AIUtil.IsInSight(actor, point.Position + Vector3.up * verifyHeight, checkDistance, fieldOfView))
                return !point.RequiresReaching || distanceToPoint < 0.5f;

            return false;
        }

        internal void glimpse(Actor actor, SearchBlock block, float verifyDistance, float verifyHeight, float fieldOfView)
        {
            for (int i = block.Count - 1; i >= 0; i--)
            {
                var p = block.Get(i);

                if (Vector3.Distance(p.Position, _point.Position) < 0.5f)
                    continue;

                if (canBeInvestigated(actor, p, verifyDistance, verifyHeight, fieldOfView))
                {
                    var point = new InvestigatedPoint(p.Position);
                    markInvestigated(point);

                    block.Investigate(i);
                }
            }
        }

        internal bool considerPoint(InvestigatedPoint point)
        {
            if (point.Time < _timeOfReset)
                return false;

            if (_hasPoint && areCloseEnough(point, _point))
            {
                _hasPoint = false;
                markInvestigated(point);
                return true;
            }

            if (considerPoint(_block, point))
                return true;

            for (int i = 0; i < _blocks.Count; i++)
                if (considerPoint(_blocks[i], point))
                {
                    if (_blocks[i].Empty)
                    {
                        _investigatedBlocks.Add(_blocks[i]);
                        _blocks.RemoveAt(i);
                    }

                    return true;
                }

            return false;
        }

        internal bool considerPoint(SearchBlock block, InvestigatedPoint point)
        {
            for (int i = 0; i < block.Count; i++)
                if (areCloseEnough(point, block.Get(i)))
                {
                    block.Investigate(i);
                    markInvestigated(point);
                    return true;
                }

            return false;
        }

        internal void markInvestigated(InvestigatedPoint point)
        {
            _investigated.Add(point);
        }

        internal bool areCloseEnough(InvestigatedPoint a, SearchPoint b)
        {
            if (Vector3.Distance(a.Position, b.Position) < 0.5f)
                return true;

            return false;
        }

        internal bool shouldAim(BaseActor actor, float aimDistance)
        {
            var distance = Vector3.Distance(transform.position, _point.Position);

            if (distance < aimDistance)
                return true;
            else
                return false;
        }

        internal bool shouldMove(BaseActor actor, float distance, float verifyHeight)
        {
            return Vector3.Distance(actor.transform.position, _point.Position) > distance ||
                   Mathf.Abs(actor.transform.position.y - _point.Position.y) > 1.5f ||
                   AIUtil.IsObstructed(actor.StandingTopPosition, _point.Position + Vector3.up * verifyHeight);
        }

        internal bool shouldRun(Actor actor, float walkDistance)
        {
            if (actor.MovePathLength > walkDistance)
                return true;
            else
                return false;
        }

        internal void setPoint(int index, float walkDistance)
        {
            _pointIndex = index;
            _point = _points.Points[index];
            _searchPosition = _point.Position;
            _hasPoint = true;

            _hasPreviousPoint = true;
            _previousPointIndex = index;

            _hasSearchDirection = true;
            _searchDirection = (_point.Position - transform.position).normalized;

            _hasApproached = !shouldApproach(_point);
        }

        internal bool shouldApproach(SearchPoint point)
        {
            return Vector3.Dot(point.Normal, point.Position - transform.position) > 0 && Vector3.Distance(point.ApproachPosition, transform.position) > 0.3f;
        }

        internal bool aim(Actor actor, float verifyHeight)
        {
            if (Vector3.Distance(actor.transform.position, _point.Position) < 1)
            {
                actor.InputAim(_point.Position + Vector3.up * verifyHeight);
                return true;
            }
            else if (AIUtil.IsObstructed(actor.StandingTopPosition, _point.Position + Vector3.up * verifyHeight, 0.2f))
            {
                var target = actor.transform.position + Vector3.Distance(actor.transform.position, _point.Position) * actor.MoveDirection;
                target.y = _point.Position.y;
                actor.InputAim(target + Vector3.up * verifyHeight);
                return false;
            }
            else
            {
                actor.InputAim(_point.Position + Vector3.up * verifyHeight);
                return true;
            }
        }

        internal void lookForward(Actor actor, float verifyHeight)
        {
            actor.InputLook(actor.transform.position + actor.MoveDirection * 100);
        }

        internal void walk(Actor actor)
        {
            if (!_hasApproached)
                actor.InputMoveTo(_point.ApproachPosition, 0.5f);
            else
                actor.InputMoveTo(_point.Position, 0.5f);
        }

        internal void run(Actor actor)
        {
            if (!_hasApproached)
                actor.InputMoveTo(_point.ApproachPosition, 1);
            else
                actor.InputMoveTo(_point.Position, 1);
        }

        internal void startSearch()
        {
            _isSearching = true;
            _hasPoint = false;

            _blocks.Clear();

            ClearSearchHistory();

            for (int i = 0; i < _investigatedBlocks.Count; i++)
                _blockCache.Give(_investigatedBlocks[i]);

            _investigatedBlocks.Clear();

            _hasPreviousPoint = false;
            _points.Clear();

            GlobalSearchCache.GeneratedPoints.WriteTo(_points);

            for (int i = 0; i < GlobalSearchCache.GeneratedBlocks.Count; i++)
            {
                var block = _blockCache.Take();
                GlobalSearchCache.GeneratedBlocks[i].WriteTo(ref block);

                _blocks.Add(block);
            }
        }
    }

    /// <summary>
    /// Stores covers and search blocks inside the level.
    /// </summary>
    public struct GlobalSearchCache
    {
        public static List<SearchBlock> GeneratedBlocks = new List<SearchBlock>();
        public static SearchPointData GeneratedPoints = new SearchPointData();

        private static List<SearchBlock> _blocks = new List<SearchBlock>();
        private static SearchPointData _points = new SearchPointData();
        private static CoverCache _coverCache = new CoverCache();
        private static SearchZoneCache _zoneCache = new SearchZoneCache();
        private static SearchBlockCache _blockCache = new SearchBlockCache(_points);
        private static HashSet<Cover> _usedCovers = new HashSet<Cover>();

        private static bool _isGenerating;
        private static int _currentMergedGeneratedBlockId;
        private static float _currentTime = -1000;
        private static float _timeAtGenerationStart = -1000;
        private static bool _hasJustRestarted;

        /// <summary>
        /// Rebuilds the search point database. Ignores other calls to rebuild in this frame.
        /// </summary>
        public static void Restart()
        {
            if (_hasJustRestarted)
                return;

            _hasJustRestarted = true;
            Rebuild();
        }

        /// <summary>
        /// Checks if the database has to be rebuilt.
        /// </summary>
        public static void Update()
        {
            if (Time.timeSinceLevelLoad - _currentTime < 1f / 60f)
                return;

            _currentTime = Time.timeSinceLevelLoad;
            _hasJustRestarted = false;

            if (_isGenerating)
                mergeBlocks();
            else if (_currentTime > _timeAtGenerationStart + 30 * 10)
                Rebuild();
        }

        /// <summary>
        /// Rebuilds the search point database.
        /// </summary>
        public static void Rebuild()
        {
            const float searchRadius = 1000;

            _isGenerating = true;
            _currentMergedGeneratedBlockId = 0;
            _timeAtGenerationStart = Time.timeSinceLevelLoad;

            for (int i = 0; i < _blocks.Count; i++)
                _blockCache.Give(_blocks[i]);

            _blocks.Clear();
            _usedCovers.Clear();
            _points.Clear();

            _coverCache.Items.Clear();
            _coverCache.Reset(Vector3.zero, searchRadius, false);

            for (int i = 0; i < _coverCache.Items.Count; i++)
            {
                var item = _coverCache.Items[i];

                if (_usedCovers.Contains(item.Cover))
                    continue;

                var cover = item.Cover;
                var starting = item.Cover;

                while (cover.LeftAdjacent != null && !_usedCovers.Contains(cover.LeftAdjacent))
                {
                    if (cover.LeftAdjacent == starting)
                        break;

                    cover = cover.LeftAdjacent;
                }

                var index = -1;
                var previousCover = cover;

                while (cover != null)
                {
                    _usedCovers.Add(cover);

                    var leftCorner = cover.LeftCorner(cover.Bottom);
                    AIUtil.GetClosestStandablePosition(ref leftCorner);

                    var rightCorner = cover.RightCorner(cover.Bottom);
                    AIUtil.GetClosestStandablePosition(ref rightCorner);

                    if (previousCover == null || cover.LeftAdjacent != previousCover)
                    {
                        if (AIUtil.GetClosestStandablePosition(ref leftCorner))
                        {
                            var approachPosition = leftCorner;

                            if (cover.LeftAdjacent == null)
                            {
                                var position = leftCorner + cover.Left * AISearch.CoverOffset;

                                if (AIUtil.GetClosestStandablePosition(ref position) && Mathf.Abs(position.y - leftCorner.y) < 0.5f)
                                    if (!AIUtil.IsObstructed(position + Vector3.up * 1.5f, leftCorner, 0.1f))
                                        approachPosition = position;
                            }

                            possiblyAddRightPoint(ref index, new SearchPoint(leftCorner, approachPosition, -cover.Forward, false));
                        }
                    }

                    if (AIUtil.GetClosestStandablePosition(ref rightCorner))
                    {
                        var approachPosition = rightCorner;

                        if (cover.RightAdjacent == null)
                        {
                            var position = rightCorner + cover.Right * AISearch.CoverOffset;

                            if (AIUtil.GetClosestStandablePosition(ref position) && Mathf.Abs(position.y - rightCorner.y) < 0.5f)
                                if (!AIUtil.IsObstructed(position + Vector3.up * 1.5f, rightCorner, 0.1f))
                                    approachPosition = position;
                        }

                        possiblyAddRightPoint(ref index, new SearchPoint(rightCorner, approachPosition, -cover.Forward, false));
                    }

                    previousCover = cover;

                    if (cover.RightAdjacent != null && !_usedCovers.Contains(cover.RightAdjacent))
                        cover = cover.RightAdjacent;
                    else
                        cover = null;
                }
            }

            _zoneCache.Reset(Vector3.zero, searchRadius);

            for (int i = 0; i < _zoneCache.Items.Count; i++)
            {
                var block = _zoneCache.Items[i];

                foreach (var position in block.Points(AISearch.BlockThreshold - 0.1f))
                    addPoint(new SearchPoint(position, false));
            }
        }

        private static void possiblyAddRightPoint(ref int index, SearchPoint point)
        {
            if (!AIUtil.GetClosestStandablePosition(ref point.Position))
                return;

            var new_ = addPoint(point);

            if (index >= 0)
                _points.LinkRight(index, new_);

            index = new_;
        }

        private static int addPoint(SearchPoint point)
        {
            point.CalcVisibility(AISearch.VerifyDistance, false);
            var index = _points.Add(point);

            for (int i = 0; i < _blocks.Count; i++)
                if (_blocks[i].IsClose(point, AISearch.BlockThreshold, AISearch.BlockCenterThreshold))
                {
                    _blocks[i].Add(index);
                    return index;
                }

            var new_ = _blockCache.Take();
            new_.Add(index);
            _blocks.Add(new_);

            return index;
        }

        private static void mergeBlocks()
        {
            int processed = 0;

            for (int a = _currentMergedGeneratedBlockId; a < _blocks.Count - 1; a++)
            {
                RESTART:

                if (processed > 0)
                {
                    _currentMergedGeneratedBlockId = a;
                    break;
                }

                processed++;

                for (int b = _blocks.Count - 1; b > a; b--)
                {
                    for (int p = 0; p < _blocks[a].Indices.Count; p++)
                    {
                        var ap = _blocks[a].Indices[p];

                        if (_blocks[b].IsClose(_points.Points[ap], AISearch.BlockThreshold, AISearch.BlockCenterThreshold))
                            goto SUCCESS;
                    }

                    continue;

                    SUCCESS:

                    for (int p = 0; p < _blocks[b].Indices.Count; p++)
                        _blocks[a].Add(_blocks[b].Indices[p]);

                    _blockCache.Give(_blocks[b]);
                    _blocks.RemoveAt(b);

                    goto RESTART;
                }
            }

            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blocks[i];
                block.Index = i;
                _blocks[i] = block;
            }

            GeneratedPoints.Clear();
            _points.WriteTo(GeneratedPoints);

            for (int i = 0; i < _blocks.Count; i++)
            {
                var block = _blockCache.Take();
                _blocks[i].WriteTo(ref block);

                GeneratedBlocks.Add(block);
            }

            _isGenerating = false;
        }
    }

    /// <summary>
    /// Information about a searchable position.
    /// </summary>
    public struct SearchPoint
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 ApproachPosition;
        public bool HasNormal;
        public float Visibility;
        public int Left;
        public int Right;
        public bool RequiresReaching;

        public SearchPoint(Vector3 position, Vector3 approachPosition, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = approachPosition;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, Vector3 normal, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = normal;
            HasNormal = true;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public SearchPoint(Vector3 position, bool requiresReaching)
        {
            Position = position;
            ApproachPosition = position;
            Normal = Vector3.zero;
            HasNormal = false;
            Visibility = 9999999;
            Left = -1;
            Right = -1;
            RequiresReaching = requiresReaching;
        }

        public void CalcVisibility(float maxDistance, bool isAlerted)
        {
            Visibility = Util.GetViewDistance(Position, maxDistance, isAlerted);
        }
    }

    public class SearchPointData
    {
        public List<SearchPoint> Points;

        public SearchPointData()
        {
            Points = new List<SearchPoint>();
        }

        public void WriteTo(SearchPointData other)
        {
            other.Points.Clear();

            for (int i = 0; i < Points.Count; i++)
                other.Points.Add(Points[i]);
        }

        public void LinkLeft(int left, int middle)
        {
            var point = Points[left];
            point.Right = middle;
            Points[left] = point;

            point = Points[middle];
            point.Left = left;
            Points[middle] = point;
        }

        public void LinkRight(int middle, int right)
        {
            var point = Points[middle];
            point.Right = right;
            Points[middle] = point;

            point = Points[right];
            point.Left = middle;
            Points[right] = point;
        }


        public int Add(SearchPoint point)
        {
            Points.Add(point);
            return Points.Count - 1;
        }

        public void Clear()
        {
            Points.Clear();
        }
    }

    public struct SearchBlock
    {
        public bool Empty
        {
            get { return Indices.Count == 0; }
        }

        public int Count
        {
            get { return Indices.Count; }
        }

        public SearchPointData Data;
        public List<int> Indices;
        public List<int> InvestigatedIndices;
        public Vector3 Center;
        public Vector3 Sum;
        public int Index;

        public SearchBlock(SearchPointData data)
        {
            Data = data;
            Indices = new List<int>();
            InvestigatedIndices = new List<int>();
            Center = Vector3.zero;
            Sum = Vector3.zero;
            Index = 0;
        }

        public void WriteTo(ref SearchBlock other)
        {
            other.Index = Index;
            other.Sum = Sum;
            other.Center = Center;

            other.Indices.Clear();
            other.InvestigatedIndices.Clear();

            for (int i = 0; i < Indices.Count; i++)
                other.Indices.Add(Indices[i]);

            for (int i = 0; i < InvestigatedIndices.Count; i++)
                other.InvestigatedIndices.Add(InvestigatedIndices[i]);
        }

        public void Investigate(int index)
        {
            InvestigatedIndices.Add(Indices[index]);
            Indices.RemoveAt(index);
        }

        public SearchPoint Get(int index)
        {
            return Data.Points[Indices[index]];
        }

        public void Add(int index)
        {
            Indices.Add(index);

            Sum += Data.Points[index].Position;
            Center = Sum / Indices.Count;
        }

        public bool IsClose(SearchPoint point, float threshold, float middleThreshold)
        {
            if (Vector3.Distance(Center, point.Position) < threshold)
                return true;

            foreach (var i in Indices)
                if (Vector3.Distance(Data.Points[i].Position, point.Position) < threshold &&
                    !AIUtil.IsObstructed(Data.Points[i].Position + Vector3.up,
                                         point.Position + Vector3.up,
                                         0.05f))
                    return true;

            return false;
        }

        public void Clear()
        {
            Indices.Clear();
            InvestigatedIndices.Clear();
            Center = Vector3.zero;
            Sum = Vector3.zero;
        }
    }

    public class SearchBlockCache
    {
        private List<SearchBlock> _cache = new List<SearchBlock>();
        private SearchPointData _points;

        public SearchBlockCache(SearchPointData points)
        {
            _points = points;
        }

        public void Give(SearchBlock block)
        {
            _cache.Add(block);
        }

        public SearchBlock Take()
        {
            if (_cache.Count == 0)
                return new SearchBlock(_points);
            else
            {
                var block = _cache[_cache.Count - 1];
                _cache.RemoveAt(_cache.Count - 1);

                block.Clear();

                return block;
            }
        }
    }

    /// <summary>
    /// Information about an already investigated position.
    /// </summary>
    public struct InvestigatedPoint
    {
        public Vector3 Position;
        public float Time;

        public InvestigatedPoint(Vector3 position)
        {
            Position = position;
            Time = UnityEngine.Time.timeSinceLevelLoad;
        }
    }
}
