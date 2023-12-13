using UnityEngine;

namespace CoverShooter.AI
{
    [Folder("Complex")]
    [Success("Done")]
    [AllowCrouch]
    public class Search : BaseAction
    {
        [ValueType(ValueType.Vector3)]
        [ValueType(ValueType.GameObject)]
        public Value InitialPosition;

        /// <summary>
        /// At which height the AI confirms the point as investigated.
        /// </summary>
        [ValueType(ValueType.Float)]
        public Value VerifyHeight = new Value(0.7f);

        /// <summary>
        /// Field of sight to register the search position.
        /// </summary>
        [ValueType(ValueType.Float)]
        public Value FieldOfView = new Value(90f);

        /// <summary>
        /// Distance at which AI turns from running to walking to safely investigate the position.
        /// </summary>
        [ValueType(ValueType.Float)]
        public Value WalkDistance = new Value(8f);

        public override void Enter(State state, int layer, ref ActionState values)
        {
            values.SearchState = state.Actor.GetComponent<AISearchState>();

            if (values.SearchState == null)
                values.SearchState = state.Actor.gameObject.AddComponent<AISearchState>();

            var searchPoint = new SearchPoint(state.GetPosition(ref InitialPosition), false);

            values.SearchState.ClearSearchHistory();
            values.SearchState.startSearch();

            var distance = Vector3.Distance(state.Actor.transform.position, searchPoint.Position);

            values.SearchState.MarkInvestigatedInRegion(state.Actor.transform.position, distance * 0.6f);

            values.SearchState.setPoint(values.SearchState.addPoint(searchPoint), state.Dereference(ref WalkDistance).Float);
            values.SearchState._hasPreviousBlockPosition = false;
            values.SearchState._hasSeenThePoint = false;
        }

        public override AIResult Update(State state, int layer, ref ActionState values)
        {
            var actor = state.Actor;
            var search = values.SearchState;
            var verifyHeight = state.Dereference(ref VerifyHeight).Float;
            var fieldOfView = state.Dereference(ref FieldOfView).Float;
            var walkDistance = state.Dereference(ref WalkDistance).Float;

            if (search == null)
                Debug.Assert(search != null);

            var DebugPoints = true;
            var DebugTarget = true;

            if (search._blocks.Count == 0 && !search._hasPoint)
                return AIResult.Finish();

            if (DebugPoints)
            {
                foreach (var block in search._blocks)
                    search.debugBlock(block);

                foreach (var block in search._investigatedBlocks)
                    search.debugBlock(block);
            }

            if (search._block.Empty && !search._hasPoint && search._blocks.Count > 0)
            {
                var pickedIndex = -1;
                var previousValue = 0f;
                var hasFound = false;
                var changePreviousBlockPosition = false;

                if (search._hasPreviousBlockPosition)
                    for (int i = 0; i < search._blocks.Count; i++)
                    {
                        var distance = Vector3.Distance(search._previousBlockPosition, search._blocks[i].Center);

                        if (distance < 64)
                        {
                            var value = Vector3.Distance(search._searchPosition, search._blocks[i].Center);

                            if (pickedIndex < 0 || value < previousValue)
                            {
                                pickedIndex = i;
                                previousValue = value;
                                hasFound = true;
                            }
                        }
                    }

                if (!hasFound)
                {
                    var origin = search._searchPosition;

                    for (int i = 0; i < search._blocks.Count; i++)
                    {
                        var value = Vector3.Distance(origin, search._blocks[i].Center);

                        if (pickedIndex < 0 || value < previousValue)
                        {
                            pickedIndex = i;
                            previousValue = value;
                            changePreviousBlockPosition = true;
                        }
                    }
                }

                search._block = search._blocks[pickedIndex];
                search._blocks.RemoveAt(pickedIndex);
                search._investigatedBlocks.Add(search._block);

                if (changePreviousBlockPosition)
                {
                    search._hasPreviousBlockPosition = true;
                    search._previousBlockPosition = search._block.Center;
                }
            }

            if (!search._hasPoint)
            {
                int index;
                float value;

                if (search.findBestPoint(actor, search._block, out index, out value))
                {
                    search._hasSeenThePoint = false;
                    search.setPoint(search._block.Indices[index], walkDistance);
                    search._block.Investigate(index);
                }
            }

            if (!search._hasPoint)
                return AIResult.Hold();

            if (!search._hasApproached && !search.shouldApproach(search._point))
                search._hasApproached = true;

            search._shouldAim.Set(search.shouldAim(actor, AISearchState.VerifyDistance), Time.deltaTime, 0.5f);

            var shouldMoveRegardless = false;

            if (search._shouldAim.Value)
            {
                if (!search.aim(actor, verifyHeight))
                    shouldMoveRegardless = true;
            }
            else
            {
                shouldMoveRegardless = true;
                search.lookForward(actor, verifyHeight);
            }

            search._shouldMove.Set(shouldMoveRegardless || search.shouldMove(actor, search.investigationDistance(actor, search._point) - 1, verifyHeight), Time.deltaTime, 0.5f);
            search._shouldRun.Set(search.shouldRun(actor, walkDistance), Time.deltaTime, 0.5f);

            if (search._shouldMove.Value)
            {
                if (search._shouldRun.Value)
                    search.run(actor);
                else
                    search.walk(actor);
            }

            search._checkWait -= Time.deltaTime;

            if (search._checkWait <= float.Epsilon)
            {
                search.glimpse(actor, search._block, AISearchState.VerifyDistance, verifyHeight, 180);

                for (int b = search._blocks.Count - 1; b >= 0; b--)
                {
                    search.glimpse(actor, search._blocks[b], AISearchState.VerifyDistance, verifyHeight, 180);

                    if (search._blocks[b].Empty)
                    {
                        search._investigatedBlocks.Add(search._blocks[b]);
                        search._blocks.RemoveAt(b);
                    }
                }

                search._checkWait = 0.25f;
            }

            if (DebugTarget)
                Debug.DrawLine(state.Actor.transform.position, search._point.Position, Color.yellow);

            if (search.canBeInvestigated(actor, search._point, AISearchState.VerifyDistance, verifyHeight, fieldOfView))
            {
                if (search._shouldAim.Value)
                {
                    if (search._hasSeenThePoint)
                    {
                        if (search._lookAtPointTimer > 0.3f)
                        {
                            search.finishInvestigatingThePoint();
                            search._hasSeenThePoint = false;
                        }

                        search._lookAtPointTimer += Time.deltaTime;
                    }
                    else
                    {
                        search._lookAtPointTimer = 0;
                        search._hasSeenThePoint = true;
                    }
                }
                else
                {
                    search.finishInvestigatingThePoint();
                    search._hasSeenThePoint = false;
                }
            }
            else
                search._hasSeenThePoint = false;

            return AIResult.Hold();
        }
    }
}