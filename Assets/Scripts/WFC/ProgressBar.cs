using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public WaveFunction waveFunction;
    public Slider slider;
    public TextMeshProUGUI progressText;
    public GameObject uiElements;

    void Start()
    {
        slider.gameObject.SetActive(true);
    }

    void Update()
    {
        // Gets the number of collapsed cells to calculate progress
        float progress = (float) waveFunction.collapsedCells / (waveFunction.gridComponents.Count-1);
        progress = Mathf.Round(progress * 1000f) / 1000f;
        slider.value = progress;
        progressText.text = progress*100 + "%";
        
        // If map loaded, hide slider
        if (progress == 1)
        {
            StartCoroutine(HideSliderAfterDelay(1f));
        }
    }

    IEnumerator HideSliderAfterDelay(float delay)
    {
        // Wait delay seconds, deactivate slider and activate UI elements
        yield return new WaitForSeconds(delay);
        slider.gameObject.SetActive(false);
        uiElements.SetActive(true);
    }
}
