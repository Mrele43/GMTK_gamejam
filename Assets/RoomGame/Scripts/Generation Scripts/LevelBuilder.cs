using UnityEngine;
public class LevelBuilder : MonoBehaviour
{
    [SerializeField] AdjacentRoomsLayout adjacentRoomsLayout;
    [SerializeField] AdjacentRoomsGeneration adjacentRoomsGeneration;
    void Start()
    {
        if (adjacentRoomsLayout == null)
            adjacentRoomsLayout = GetComponent<AdjacentRoomsLayout>();
        if (adjacentRoomsGeneration == null)
            adjacentRoomsGeneration = GetComponent<AdjacentRoomsGeneration>();
        Level level = adjacentRoomsLayout.generateLevel();
        adjacentRoomsGeneration.createLevel(level);
        NavMeshBaker baker = GetComponent<NavMeshBaker>();
        if (baker != null)
        {
            baker.BakeAndInstallLinks(level);
        }
        PlayerSpawner spawner = GetComponent<PlayerSpawner>();
        if (spawner != null)
        {
            spawner.SpawnPlayer(level);
        }
    }
}
