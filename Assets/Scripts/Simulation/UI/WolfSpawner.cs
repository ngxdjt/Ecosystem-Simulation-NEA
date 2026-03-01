using UnityEngine;
using System.Linq;

public class WolfSpawner : MonoBehaviour
{
    public GameObject wolfPrefab;
    public WaveFunction map;
    public SimulationManager simulationManager;
    public Transform wolfContainer;

    public void SpawnWolf()
    {
        int spawnPos;
        float offset = (map.dimensions - 1) / 2f;

        for (int attempts = 0; attempts < 100; attempts++)
        {
            spawnPos = UnityEngine.Random.Range(0, map.gridTile.Length);
            if (map.gridTile[spawnPos].isWalkable && map.gridTile[spawnPos].occupant == null)
            {
                Vector3 vector3SpawnPos = new Vector3(spawnPos % map.dimensions - offset, spawnPos / map.dimensions - offset, 0f);
                GameObject wolfObject = Instantiate(wolfPrefab, vector3SpawnPos, Quaternion.identity, wolfContainer);

                Wolf wolf = wolfObject.GetComponent<Wolf>();
                Vector2Int vector2SpawnPos = Geometry.IndexToCoordinate(map.dimensions, spawnPos);
                int tickBorn = simulationManager.currentTick;
                Sex sex = (Sex)UnityEngine.Random.Range(0, 2);
                wolf.SetupAnimal(vector2SpawnPos, map, tickBorn, sex, simulationManager, wolfPrefab);
                wolf.animalContainer = wolfContainer;
                Debug.Log("Spawned Wolf");
                break;
            }
        }
    }
}
