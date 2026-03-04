using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown resolutionDropDown;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private int currentRefreshRate;
    private int currentResolutionIndex;
    float targetAspect = 16f / 9f;

    public AudioMixer audioMixer;

    void Awake()
    {
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();

        resolutionDropDown.ClearOptions();
        currentRefreshRate = Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value);

        for (int i = 0; i < resolutions.Length; i++)
        {
            float aspectRatio = (float) resolutions[i].width / resolutions[i].height;
            bool matchRefreshRate = Mathf.RoundToInt((float) resolutions[i].refreshRateRatio.value) == currentRefreshRate;
            bool matchAspectRatio = aspectRatio == targetAspect;

            if (matchRefreshRate && matchAspectRatio) filteredResolutions.Add(resolutions[i]);
        }

        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height; 
            options.Add(resolutionOption);
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        Debug.Log("Fullscreen: " + isFullScreen);
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Volume", volume);
    }
}

