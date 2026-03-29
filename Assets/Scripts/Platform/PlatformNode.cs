using UnityEngine;

public class PlatformNode : MonoBehaviour
{
    [Header("Platform Id")]
    public int platformId;

    [Header("Links from this platform")]
    public JumpLink[] jumpLinks;

    [System.Serializable]
    public class JumpLink
    {
        public PlatformNode toPlatform;

        [Header("Common points")]
        public Transform jumpStart;
        public Transform jumpLand;

        [Header("Drop")]
        public bool dropOnly;
        public Transform dropExitPoint;

        [Header("Ladder")]
        public bool useLadder;
    }

    public bool TryGetLinkTo(PlatformNode target, out JumpLink link)
    {
        if (jumpLinks != null)
        {
            foreach (var l in jumpLinks)
            {
                if (l != null && l.toPlatform == target)
                {
                    link = l;
                    return true;
                }
            }
        }

        link = null;
        return false;
    }
}