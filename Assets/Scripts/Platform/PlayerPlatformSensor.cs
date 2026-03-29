using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformSensor : MonoBehaviour
{
    public PlatformNode CurrentPlatform { get; private set; }
    public bool IsGrounded => CurrentPlatform != null;

    private readonly HashSet<PlatformNode> overlappingPlatforms = new HashSet<PlatformNode>();

    public bool suspendDetection = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlatformNode platform = GetPlatform(other);
        if (platform == null)
            return;

        overlappingPlatforms.Add(platform);

        if (!suspendDetection)
            RefreshCurrentPlatform();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlatformNode platform = GetPlatform(other);
        if (platform == null)
            return;

        Debug.Log($"Feet stay on: {platform.name}");

        overlappingPlatforms.Add(platform);

        if (!suspendDetection)
            RefreshCurrentPlatform();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlatformNode platform = GetPlatform(other);
        if (platform == null)
            return;

        overlappingPlatforms.Remove(platform);

        if (!suspendDetection && platform == CurrentPlatform)
            RefreshCurrentPlatform();
    }

    private PlatformNode GetPlatform(Collider2D other)
    {
        return other.GetComponentInParent<PlatformNode>() ?? other.GetComponent<PlatformNode>();
    }

    private void RefreshCurrentPlatform()
    {
        if (overlappingPlatforms.Count == 0)
        {
            // NIE resetuj od razu — zostaw ostatni¹ platformê
            return;
        }

        PlatformNode bestPlatform = null;
        float bestTopY = float.NegativeInfinity;

        foreach (PlatformNode platform in overlappingPlatforms)
        {
            if (platform == null)
                continue;

            Collider2D col = platform.GetComponent<Collider2D>();
            if (col == null)
                col = platform.GetComponentInChildren<Collider2D>();

            if (col == null)
                continue;

            float topY = col.bounds.max.y;

            // wybieramy najwy¿sz¹ platformê, której dotykaj¹ stopy 
            if (topY > bestTopY)
            {
                bestTopY = topY;
                bestPlatform = platform;
            }
        }

        CurrentPlatform = bestPlatform;
    }

    public void ForcePlatform(PlatformNode platform)
    {
        CurrentPlatform = platform;
    }

    public void RefreshNow()
    {
        RefreshCurrentPlatform();
    }
}