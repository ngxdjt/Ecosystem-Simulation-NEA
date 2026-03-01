using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TileSelector : MonoBehaviour
{
    public Slider tileSelector;
    public Tile[] tileOptions;
    public RectTransform CustomisationMenu;
    public Image displayTile;
    public TextMeshProUGUI displayText;

    public Tile selectedTile;

    void Start()
    {
        // Slider Settings
        tileSelector.minValue = 0;
        tileSelector.maxValue = tileOptions.Length - 1;
        tileSelector.wholeNumbers = true;
        tileSelector.onValueChanged.AddListener(OnSliderChanged);

        // Initialize display
        OnSliderChanged(tileSelector.value);
    }

    void OnSliderChanged(float value)
    {
        int index = Mathf.RoundToInt(value);
        Tile selectedTile = tileOptions[index];

        // Preview Tile
        Sprite tileSprite = selectedTile.GetComponent<SpriteRenderer>().sprite;
        displayTile.sprite = tileSprite;

        displayText.text = selectedTile.name;

        // Transport Tile to Next Scene
        TransportData.transportTile = selectedTile;

        Debug.Log("Selected: " + selectedTile.name);
    }
}
