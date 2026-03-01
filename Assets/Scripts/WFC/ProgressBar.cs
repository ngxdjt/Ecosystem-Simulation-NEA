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
        float progress = (float) waveFunction.collapsedCells / (waveFunction.gridComponents.Count-1);
        progress = Mathf.Round(progress * 1000f) / 1000f;
        slider.value = progress;
        progressText.text = progress*100 + "%";
        if (progress == 1)
        {
            StartCoroutine(HideSliderAfterDelay(1f));
        }
    }

    IEnumerator HideSliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        slider.gameObject.SetActive(false);
        uiElements.SetActive(true);
    }
}
