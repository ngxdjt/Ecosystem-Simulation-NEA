using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class PlantGrowth : MonoBehaviour, IGameIterable
{
    private Tile tile;
    public Sprite[] plants = new Sprite[4];
    public int growthLevel;
    public GameObject plantPrefab;
    private SpriteRenderer plant;

    void Awake()
    {
        // Instantiate plant on tile
        tile = GetComponent<Tile>();
        plant = Instantiate(plantPrefab, transform).GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SimulationManager.Instance.iterables.Add(this);
    }

    void OnDestroy()
    {
        SimulationManager.Instance.iterables.Remove(this);
    }

    public void TickUpdate(int currentTick)
    {
        // Update plant sprite
        plant.sprite = plants[growthLevel];
        if (currentTick % 5 == 0)
        {
            // Increment growth level at a given chance
            int chance = UnityEngine.Random.Range(1, 101);
            if (chance <= (int)(tile.nutrients * 100) && growthLevel < 3) growthLevel++;
        }
    }

    public void Eaten()
    {
        growthLevel = 0;
    }
}
