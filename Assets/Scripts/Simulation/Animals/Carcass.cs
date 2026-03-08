using UnityEngine;
using System.Collections;

public class Carcass : MonoBehaviour, IGameIterable
{
    public int tickDied;
    public SimulationManager simulationManager;

    void Start()
    {
        // Add to iterables and store tick created
        SimulationManager.Instance.iterables.Add(this);
        tickDied = simulationManager.currentTick;
    }

    public void TickUpdate(int currentTick)
    {
        // After a day destroy carcass
        if ((currentTick - tickDied) % 30 == 0)
        {
            simulationManager.iterables.Remove(this);
            Destroy(gameObject);
        }
    }

    public IEnumerator Eaten()
    {
        // Destroy carcass
        simulationManager.iterables.Remove(this);
        yield return new WaitForSeconds(1f / simulationManager.tickRate);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Another check just in case
        simulationManager.iterables.Remove(this);
    }
}
