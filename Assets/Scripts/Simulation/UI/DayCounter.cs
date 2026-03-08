using UnityEngine;
using TMPro;

public class DayCounter : MonoBehaviour
{
    public SimulationManager simulationManager;
    public TextMeshProUGUI dayText;

    void Update()
    {
        // Update text to current day
        dayText.text = "Day\n" + simulationManager.currentTick / 30;
    }
}
