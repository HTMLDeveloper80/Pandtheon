using UnityEngine;

public class LadderLink : MonoBehaviour
{
    [Header("Platforms")]
    public PlatformNode fromPlatform;
    public PlatformNode toPlatform;

    [Header("Points")]
    public Transform bottomPoint;
    public Transform topPoint;

    public bool Connects(PlatformNode from, PlatformNode to)
    {
        return fromPlatform == from && toPlatform == to;
    }
}
