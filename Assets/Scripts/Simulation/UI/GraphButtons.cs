using UnityEngine;
using TMPro;
using XCharts.Runtime;

public class GraphButtons : MonoBehaviour
{
    public SimulationManager simulationManager;
    public TextMeshProUGUI traitText;
    public float currentTickRate;

    public LineChart populationChart;
    public LineChart speedChart;
    public LineChart fovChart;
    public LineChart reproductiveUrgeChart;
    public LineChart desirabilityChart;
    public LineChart gestationDurationChart;

    public void GraphButton()
    {
        // Store current tick rate and pause simulation
        currentTickRate = simulationManager.tickRate;
        simulationManager.pendingTickRate = 0;
    }

    public void PopulationButton()
    {
        // Changes title text and loads animation
        traitText.text = "POPULATION";
        populationChart.AnimationFadeIn();
    }

    public void SpeedButton()
    {
        // Changes title text and loads animation
        traitText.text = "SPEED";
        speedChart.AnimationFadeIn();
    }

    public void FOVButton()
    {
        // Changes title text and loads animation
        traitText.text = "FOV";
        fovChart.AnimationFadeIn();
    }

    public void ReproductiveUrgeButton()
    {
        // Changes title text and loads animation
        traitText.text = "REPRODUCTIVE URGE";
        reproductiveUrgeChart.AnimationFadeIn();
    }

    public void DesirabilityButton()
    {
        // Changes title text and loads animation
        traitText.text = "DESIRABILITY";
        desirabilityChart.AnimationFadeIn();
    }

    public void GestationDurationButton()
    {
        // Changes title text and loads animation
        traitText.text = "GESTATION DURATION";
        gestationDurationChart.AnimationFadeIn();
    }

    public void BackButton()
    {
        // Set tick rate back to what it was before
        simulationManager.pendingTickRate = currentTickRate;
    }
}
