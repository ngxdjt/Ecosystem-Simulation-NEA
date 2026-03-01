using UnityEngine;
using TMPro;

public class DayCounter : MonoBehaviour
{
    public SimulationManager simulationManager;
    public TextMeshProUGUI dayText;

    // Update is called once per frame
    void Update()
    {
        dayText.text = "Day\n" + simulationManager.currentTick / 30;
    }
}
