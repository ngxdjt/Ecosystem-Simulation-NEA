using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] 
    private TMP_Dropdown resolutionDropDown;

    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private int currentRefreshRate;
    private int currentResolutionIndex;
    float targetAspect = 16f / 9f;

    public AudioMixer audioMixer;

    void Awake()
    {
        resolutions = Screen.resolutions; // Get user's available resolutions
        filteredResolutions = new List<Resolution>(); // Creates new list for filtered resolutions

        resolutionDropDown.ClearOptions(); // Clear dropdown
        currentRefreshRate = Mathf.RoundToInt((float) Screen.currentResolution.refreshRateRatio.value); // Gets user's current refresh rate

        // Iterate through every available resolution
        for (int i = 0; i < resolutions.Length; i++)
        {
            // Add resolution if matches conditions
            float aspectRatio = (float) resolutions[i].width / resolutions[i].height;
            bool matchRefreshRate = Mathf.RoundToInt((float) resolutions[i].refreshRateRatio.value) == currentRefreshRate;
            bool matchAspectRatio = aspectRatio == targetAspect;

            if (matchRefreshRate && matchAspectRatio) filteredResolutions.Add(resolutions[i]);
        }
        
        // Format each filtered resolution
        List<string> options = new List<string>();
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string resolutionOption = filteredResolutions[i].width + "x" + filteredResolutions[i].height; 
            options.Add(resolutionOption);
            // Sets current selection in drop down to user's current resolution
            if (filteredResolutions[i].width == Screen.width && filteredResolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        // Add resolutions to drop down
        resolutionDropDown.AddOptions(options);
        resolutionDropDown.value = currentResolutionIndex;
        resolutionDropDown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        // Sets resolution given index of filteredResolutions
        Resolution resolution = filteredResolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        // Sets application to full screen
        Screen.fullScreen = isFullScreen;
        Debug.Log("Fullscreen: " + isFullScreen);
    }

    public void SetVolume(float volume)
    {
        // Changes audio mixer "Volume" to have the value volume
        audioMixer.SetFloat("Volume", volume);
    }
}

