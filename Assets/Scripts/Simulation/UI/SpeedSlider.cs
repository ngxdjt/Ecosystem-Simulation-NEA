using UnityEngine;
using TMPro;

public class SpeedSlider : MonoBehaviour
{
    public SimulationManager simulationManager;
    public TextMeshProUGUI speedText;

    public void SetTickRate(float value)
    {
        // Change tick rate of simulation manager
        simulationManager.pendingTickRate = value;
        speedText.text = (value + "x");
    }
}
