using UnityEngine;
using TMPro;

public class SpeedSlider : MonoBehaviour
{
    public SimulationManager simulationManager;
    public TextMeshProUGUI speedText;

    public void SetTickRate(float value)
    {
        simulationManager.pendingTickRate = value;
        speedText.text = (value + "x");
    }
}
