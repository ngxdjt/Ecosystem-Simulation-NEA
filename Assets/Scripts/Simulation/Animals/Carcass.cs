using UnityEngine;
using System.Collections;

public class Carcass : MonoBehaviour, IGameIterable
{
    public int tickDied;
    public SimulationManager simulationManager;

    void Start()
    {
        SimulationManager.Instance.iterables.Add(this);
        tickDied = simulationManager.currentTick;
    }

    public void TickUpdate(int currentTick)
    {
        if ((currentTick - tickDied) % 30 == 0)
        {
            simulationManager.iterables.Remove(this);
            Destroy(gameObject);
        }
    }

    public IEnumerator Eaten()
    {
        simulationManager.iterables.Remove(this);
        yield return new WaitForSeconds(1f / simulationManager.tickRate);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        simulationManager.iterables.Remove(this);
    }
}
