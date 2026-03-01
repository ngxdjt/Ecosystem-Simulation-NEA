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
        currentTickRate = simulationManager.tickRate;
        simulationManager.pendingTickRate = 0;
    }

    public void PopulationButton()
    {
        traitText.text = "POPULATION";
        populationChart.AnimationReset();
    }

    public void SpeedButton()
    {
        traitText.text = "SPEED";
        speedChart.AnimationReset();
    }

    public void FOVButton()
    {
        traitText.text = "FOV";
        fovChart.AnimationReset();
    }

    public void ReproductiveUrgeButton()
    {
        traitText.text = "REPRODUCTIVE URGE";
        reproductiveUrgeChart.AnimationReset();
    }

    public void DesirabilityButton()
    {
        traitText.text = "DESIRABILITY";
        desirabilityChart.AnimationReset();
    }

    public void GestationDurationButton()
    {
        traitText.text = "GESTATION DURATION";
        gestationDurationChart.AnimationReset();
    }

    public void BackButton()
    {
        simulationManager.pendingTickRate = currentTickRate;
    }
}
