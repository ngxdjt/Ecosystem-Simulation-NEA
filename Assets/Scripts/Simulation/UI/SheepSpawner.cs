using UnityEngine;
using System.Linq;

public class SheepSpawner : MonoBehaviour
{
    public GameObject sheepPrefab;
    public WaveFunction map;
    public SimulationManager simulationManager;
    public Transform sheepContainer;

    public void SpawnSheep()
    {
        int spawnPos;
        float offset = (map.dimensions - 1) / 2f;

        // 100 attempts to find random spawn position
        for (int attempts = 0; attempts < 100; attempts++)
        {
            spawnPos = UnityEngine.Random.Range(0, map.gridTile.Length); // Get random tile in map
            if (map.gridTile[spawnPos].isWalkable && map.gridTile[spawnPos].occupant == null) // Validity checks
            {
                // Instantiate the sheep
                Vector3 vector3SpawnPos = new Vector3(spawnPos % map.dimensions - offset, spawnPos / map.dimensions - offset, 0f);
                GameObject sheepObject = Instantiate(sheepPrefab, vector3SpawnPos, Quaternion.identity, sheepContainer);

                // Set up the sheep
                Sheep sheep = sheepObject.GetComponent<Sheep>();
                Vector2Int vector2SpawnPos = Geometry.IndexToCoordinate(map.dimensions, spawnPos);
                int tickBorn = simulationManager.currentTick;
                Sex sex = (Sex) UnityEngine.Random.Range(0, 2);
                sheep.SetupAnimal(vector2SpawnPos, map, tickBorn, sex, simulationManager, sheepPrefab);
                sheep.animalContainer = sheepContainer;
                Debug.Log("Spawned Sheep");
                break;
            }
        }
    }
}
