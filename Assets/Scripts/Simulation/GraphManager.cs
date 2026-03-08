using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using XCharts.Runtime;

public class GraphManager : MonoBehaviour
{
    public static List<Animal> animals = new List<Animal>();

    public SimulationManager simulationManager;

    public LineChart populationChart;
    public LineChart speedChart;
    public LineChart fovChart;
    public LineChart reproductiveUrgeChart;
    public LineChart desirabilityChart;
    public LineChart gestationDurationChart;

    private List<Sheep> GetSheep()
    {
        List<Sheep> sheep = new List<Sheep>();

        // Get a list of all sheep from the animal list
        foreach (Animal animal in animals)
        {
            if (animal is Sheep) sheep.Add((Sheep) animal);
        }

        return sheep;
    }

    private List<Wolf> GetWolves()
    {
        List<Wolf> wolves = new List<Wolf>();

        // Get a list of all wolves from the animal list
        foreach (Animal animal in animals)
        {
            if (animal is Wolf) wolves.Add((Wolf) animal);
        }

        return wolves;
    }

    public void AddPopulationData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();

        // Add data to graph
        populationChart.AddXAxisData("Day " + day);
        populationChart.AddData("Sheep", sheep.Count);
        populationChart.AddData("Wolves", wolves.Count);
    }

    public void AddSpeedData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();
        // Calculate average speeds
        double sheepAverageSpeed = sheep.Count > 0 ? sheep.Average(s => s.traits.speed) : 0;
        double wolfAverageSpeed = wolves.Count > 0 ? wolves.Average(w => w.traits.speed) : 0;

        // Add data to graph
        speedChart.AddXAxisData("Day " + day);
        speedChart.AddData("Sheep", sheepAverageSpeed);
        speedChart.AddData("Wolves", wolfAverageSpeed);
    }

    public void AddFovData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();
        // Calculate average fovs
        double sheepAverageFov = sheep.Count > 0 ? sheep.Average(s => s.traits.fov) : 0;
        double wolfAverageFov = wolves.Count > 0 ? wolves.Average(w => w.traits.fov) : 0;

        // Add data to graph
        fovChart.AddXAxisData("Day " + day);
        fovChart.AddData("Sheep", sheepAverageFov);
        fovChart.AddData("Wolves", wolfAverageFov);
    }

    public void AddReproductiveUrgeData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();
        // Calculate average reproductive urges
        double sheepAverageReproductiveUrge = sheep.Count > 0 ? sheep.Average(s => s.traits.reproductiveUrge) : 0;
        double wolfAverageReproductiveUrge = wolves.Count > 0 ? wolves.Average(w => w.traits.reproductiveUrge) : 0;

        // Add data to graph
        reproductiveUrgeChart.AddXAxisData("Day " + day);
        reproductiveUrgeChart.AddData("Sheep", sheepAverageReproductiveUrge);
        reproductiveUrgeChart.AddData("Wolves", wolfAverageReproductiveUrge);
    }

    public void AddDesirabilityData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();
        // Calculate average desirabilities
        double sheepAverageDesirability = sheep.Count > 0 ? sheep.Average(s => s.traits.desirability) : 0;
        double wolfAverageDesirability = wolves.Count > 0 ? wolves.Average(w => w.traits.desirability) : 0;

        // Add data to graph
        desirabilityChart.AddXAxisData("Day " + day);
        desirabilityChart.AddData("Sheep", sheepAverageDesirability);
        desirabilityChart.AddData("Wolves", wolfAverageDesirability);
    }
  
    public void AddGestationDurationData(int day)
    {
        // Get species
        List<Sheep> sheep = GetSheep();
        List<Wolf> wolves = GetWolves();
        // Calculate average gestation durations
        double sheepAverageGestationDuration = sheep.Count > 0 ? sheep.Average(s => s.traits.gestationDuration) : 0;
        double wolfAverageGestationDuration = wolves.Count > 0 ? wolves.Average(w => w.traits.gestationDuration) : 0;

        // Add data to graph
        gestationDurationChart.AddXAxisData("Day " + day);
        gestationDurationChart.AddData("Sheep", sheepAverageGestationDuration);
        gestationDurationChart.AddData("Wolves", wolfAverageGestationDuration);
    }
}

