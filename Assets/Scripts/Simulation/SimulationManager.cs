using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    public float tickRate;
    public float pendingTickRate;
    public static SimulationManager Instance;
    public int currentTick;
    private Coroutine tickCoroutine;
    public WaveFunction map;
    public GraphManager graphManager;

    public List<IGameIterable> iterables = new List<IGameIterable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void StartTick()
    {
        if (tickCoroutine == null) tickCoroutine = StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        // Wait for progress bar to unactivate
        yield return new WaitForSeconds(1f);

        while (true)
        {
            // Update tick rate
            tickRate = pendingTickRate;
            if (tickRate != 0)
            {
                // Increment current tick
                currentTick++;
                Debug.Log("Current Tick: " + currentTick);

                // Create copy of iterables and iterate through
                List<IGameIterable> iterableCopy = new List<IGameIterable>(iterables);
                foreach (var iter in iterableCopy)
                {
                    iter.TickUpdate(currentTick);
                }

                // Add data to graphs
                if (currentTick % 30 == 0)
                {
                    int day = currentTick / 30;
                    graphManager.AddPopulationData(day);
                    graphManager.AddSpeedData(day);
                    graphManager.AddFovData(day);
                    graphManager.AddReproductiveUrgeData(day);
                    graphManager.AddDesirabilityData(day);
                    graphManager.AddGestationDurationData(day);
                }

                // Tick delay
                yield return new WaitForSeconds(1f / tickRate);
            }
            else yield return null;
        }
    }

} 

