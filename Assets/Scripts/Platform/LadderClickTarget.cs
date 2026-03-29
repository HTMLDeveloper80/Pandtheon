using UnityEngine;

public class LadderClickTarget : MonoBehaviour
{
    public PlatformNode bottomPlatform;
    public PlatformNode topPlatform;

    public bool TryGetTargetFrom(PlatformNode currentPlatform, out PlatformNode targetPlatform)
    {
        if (currentPlatform == bottomPlatform)
        {
            targetPlatform = topPlatform;
            return targetPlatform != null;
        }

        if (currentPlatform == topPlatform)
        {
            targetPlatform = bottomPlatform;
            return targetPlatform != null;
        }

        targetPlatform = null;
        return false;
    }
}