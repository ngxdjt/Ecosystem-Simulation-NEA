using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CustomisationMenu : MonoBehaviour
{
    public TileSelector tileSelector;
    public Slider dimensionSelector;
    public TextMeshProUGUI dimensionText;
    public TextMeshProUGUI mutationText;

    public void ToSimulation()
    {
        // Loads simulation scene
        SceneManager.LoadScene("Simulation");
    }

    public void DimensionSelection(float value)
    {
        int dimensions = Mathf.RoundToInt(value); // Sets value to int
        dimensionText.text = "Dimensions: " + dimensions; // Display dimensions
        TransportData.transportDimensions = dimensions; // Transport dimensions to next scene
    }

    public void MutationSelection(float value)
    {
        int mutationRate = Mathf.RoundToInt(value); // Sets value to int
        mutationText.text = "Mutation Rate: " + mutationRate + "%"; // Display mutation rate
        TransportData.transportMutationRate = mutationRate; // Transport mutation rate to next scene
    }
}
