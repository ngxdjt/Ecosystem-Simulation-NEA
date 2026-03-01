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

        for (int attempts = 0; attempts < 100; attempts++)
        {
            spawnPos = UnityEngine.Random.Range(0, map.gridTile.Length);
            if (map.gridTile[spawnPos].isWalkable && map.gridTile[spawnPos].occupant == null)
            {
                Vector3 vector3SpawnPos = new Vector3(spawnPos % map.dimensions - offset, spawnPos / map.dimensions - offset, 0f);
                GameObject sheepObject = Instantiate(sheepPrefab, vector3SpawnPos, Quaternion.identity, sheepContainer);

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
