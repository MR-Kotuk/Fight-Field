using UnityEngine;

namespace CoverShooter
{
    /// <summary>
    /// Displays a button inside the inspector that triggers a manual regroup.
    /// </summary>
    [RequireComponent(typeof(BaseActor))]
    [RequireComponent(typeof(AIMovement))]
    public class AIRegrouperButton : AIBaseRegrouper
    {
    }
}
