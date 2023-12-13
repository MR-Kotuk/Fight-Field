using System;
using UnityEngine;

namespace CoverShooter
{
    public static class AICoverUtil
    {
        /// <summary>
        /// Returns true if the given position on the cover protects the character from the enemy.
        /// </summary>
        public static bool IsGoodAngle(float maxTallAngle, float maxLowAngle, Cover cover, Vector3 a, Vector3 b, bool isTall)
        {
            var dot = Vector3.Dot((b - a).normalized, cover.Forward);

            if (isTall)
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxTallAngle)
                    return false;
            }
            else
            {
                if (Mathf.DeltaAngle(0, Mathf.Acos(dot) * Mathf.Rad2Deg) > maxLowAngle)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given position is already taken by a friend that's close enough to communicate.
        /// </summary>
        public static bool IsCoverPositionFree(Cover cover, Vector3 position, float threshold, BaseActor newcomer)
        {
            if (!IsJustThisCoverPositionFree(cover, position, threshold, newcomer))
                return false;

            if (cover.LeftAdjacent != null && !IsJustThisCoverPositionFree(cover.LeftAdjacent, position, threshold, newcomer))
                return false;

            if (cover.RightAdjacent != null && !IsJustThisCoverPositionFree(cover.RightAdjacent, position, threshold, newcomer))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the given position is free for taking.
        /// </summary>
        public static bool IsJustThisCoverPositionFree(Cover cover, Vector3 position, float threshold, BaseActor newcomer)
        {
            foreach (var user in cover.Users)
                if (user.Actor != newcomer && Vector3.Distance(user.Position, position) <= threshold)
                    return false;

            return true;
        }

        public static bool IsValidCoverAgainst(Cover cover, Vector3 ground, Vector3 enemyGround)
        {
            var vector = enemyGround - ground;
            var distance = vector.magnitude;
            var direction = vector / distance;
            var tallCoverDirection = 0;

            if (cover.IsTall)
            {
                var leftCorner = cover.LeftCorner(0);
                var rightCorner = cover.RightCorner(0);
                var angle = Util.HorizontalAngle(enemyGround - ground);

                if (Vector3.Distance(ground, leftCorner) < 0.4f)
                {
                    if (!cover.IsLeft(angle))
                        return false;

                    tallCoverDirection = -1;
                }
                else if (Vector3.Distance(ground, rightCorner) < 0.4f)
                {
                    if (!cover.IsRight(angle))
                        return false;

                    tallCoverDirection = 1;
                }
                else
                    return false;
            }

            if (!AICoverUtil.IsGoodAngle(60,
                                         60,
                                         cover,
                                         ground,
                                         enemyGround,
                                         cover.IsTall))
                return false;

            if (distance > 2)
            {
                var obstructionOrigin = ground + Vector3.up * 1.8f;

                if (cover.IsTall)
                    obstructionOrigin += cover.Right * tallCoverDirection * 0.5f;

                if (AIUtil.IsObstructed(obstructionOrigin,
                                        enemyGround + Vector3.up * 2))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidCoverFrom(Cover cover, Vector3 ground, Vector3 enemyGround)
        {
            var vector = enemyGround - ground;
            var distance = vector.magnitude;
            var direction = vector / distance;

            if (!AICoverUtil.IsGoodAngle(60,
                                         60,
                                         cover,
                                         ground,
                                         enemyGround,
                                         cover.IsTall))
                return false;

            return true;
        }

        public static bool Consider(Cover cover, ref Vector3 position, Vector3 observer, float maxDistance, float takenThreshold, BaseActor newcomer = null)
        {
            if (cover.IsTall)
            {
                var leftPosition = Vector3.zero;
                var rightPosition = Vector3.zero;
                var hasLeft = false;
                var hasRight = false;

                if (cover.OpenLeft)
                {
                    leftPosition = cover.LeftCorner(cover.Bottom, -0.3f);
                    hasLeft = consider(cover, ref leftPosition, -1, observer, maxDistance, takenThreshold, newcomer);
                }

                if (cover.OpenRight)
                {
                    rightPosition = cover.RightCorner(cover.Bottom, -0.3f);
                    hasRight = consider(cover, ref rightPosition, 1, observer, maxDistance, takenThreshold, newcomer);
                }

                if (hasLeft && hasRight)
                {
                    if (Vector3.Distance(leftPosition, observer) < Vector3.Distance(rightPosition, observer))
                    {
                        position = leftPosition;
                        return true;
                    }
                    else
                    {
                        position = rightPosition;
                        return true;
                    }
                }
                else if (hasLeft)
                {
                    position = leftPosition;
                    return true;
                }
                else if (hasRight)
                {
                    position = rightPosition;
                    return true;
                }

                return false;
            }
            else
            {
                position = cover.ClosestPointTo(observer, 0.3f, 0);
                return consider(cover, ref position, 0, observer, maxDistance, takenThreshold, newcomer);
            }
        }

        private static bool consider(Cover cover, ref Vector3 position, int direction, Vector3 observer, float maxDistance, float takenThreshold, BaseActor newcomer = null)
        {
            if (float.IsNaN(position.x) || float.IsNaN(position.z))
                return false;

            var distanceToObserver = Vector3.Distance(observer, position);

            if (distanceToObserver > maxDistance)
                return false;

            var areThereOthers = false;

            if (cover.IsTall)
            {
                if (!AICoverUtil.IsCoverPositionFree(cover, position, takenThreshold, newcomer))
                    areThereOthers = true;
            }
            else
            {
                var hasChangedPosition = false;

                Vector3 side;

                if (Vector3.Dot((position - observer).normalized, cover.Right) > 0)
                    side = cover.Right;
                else
                    side = cover.Left;

                do
                {
                    hasChangedPosition = false;

                    if (!AICoverUtil.IsCoverPositionFree(cover, position, takenThreshold, newcomer))
                    {
                        var next = position + side * 0.5f;

                        if (cover.IsInFront(next, false))
                        {
                            position = next;
                            hasChangedPosition = true;
                        }
                        else
                            areThereOthers = true;
                    }
                }
                while (hasChangedPosition);
            }

            if (areThereOthers)
                return false;

            return true;
        }
    }
}