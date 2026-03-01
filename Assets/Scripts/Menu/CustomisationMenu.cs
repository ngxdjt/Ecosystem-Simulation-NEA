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
        Tile selectedTile = tileSelector.selectedTile;
        SceneManager.LoadScene("Simulation");
    }

    public void DimensionSelection(float value)
    {
        int dimensions = Mathf.RoundToInt(value);
        dimensionText.text = "Dimensions: " + dimensions;
        TransportData.transportDimensions = dimensions;
    }

    public void MutationSelection(float value)
    {
        int mutationRate = Mathf.RoundToInt(value);
        mutationText.text = "Mutation Rate: " + mutationRate + "%";
        TransportData.transportMutationRate = mutationRate;
    }
}
