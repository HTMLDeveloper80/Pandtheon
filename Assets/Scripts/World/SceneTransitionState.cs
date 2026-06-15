public static class SceneTransitionState
{
    public static string NextSpawnId { get; private set; }

    public static void SetNextSpawn(string spawnId)
    {
        NextSpawnId = string.IsNullOrWhiteSpace(spawnId)
            ? null
            : spawnId.Trim();
    }

    public static void ClearNextSpawn()
    {
        NextSpawnId = null;
    }
}
