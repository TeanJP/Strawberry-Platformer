using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    private class Settings
    {
        [SerializeField]
        private Vector2Int resolution;
        [SerializeField]
        private int refreshRate;
        [SerializeField]
        private FullScreenMode fullScreenMode;
        [SerializeField]
        private int vSyncCount;
        [SerializeField]
        private int targetFrameRate;

        public Settings (Vector2Int resolution, int refreshRate, FullScreenMode fullScreenMode, int vSyncCount, int targetFrameRate)
        {
            this.resolution = resolution;
            this.refreshRate = refreshRate;
            this.fullScreenMode = fullScreenMode;
            this.vSyncCount = vSyncCount;
            this.targetFrameRate = targetFrameRate;
        }

        #region Getters
        public Vector2Int GetResolution()
        {
            return resolution;
        }

        public int GetRefreshRate()
        {
            return refreshRate;
        }

        public FullScreenMode GetFullScreenMode()
        {
            return fullScreenMode;
        }

        public int GetVSyncCount()
        {
            return vSyncCount;
        }

        public int GetTargetFrameRate()
        {
            return targetFrameRate;
        }
        #endregion

        #region Setters
        public void SetResolution(Vector2Int resolution)
        {
            this.resolution = resolution;
        }

        public void SetRefreshRate(int refreshRate)
        {
            this.refreshRate = refreshRate;
        }

        public void SetFullScreenMode(FullScreenMode fullScreenMode)
        {
            this.fullScreenMode = fullScreenMode;
        }

        public void SetVSyncCount(int vSyncCount)
        {
            this.vSyncCount = vSyncCount;
        }

        public void SetTargetFrameRate(int targetFrameRate)
        {
            this.targetFrameRate = targetFrameRate;
        }
        #endregion
    }

    private Settings settings;

    private Resolution[] resolutions;

    [SerializeField]
    private GameObject settingsScreen = null;

    [SerializeField]
    private TMP_Dropdown resolutionDropDown = null;
    [SerializeField]
    private TMP_Dropdown fullScreenModeDropDown = null;
    [SerializeField]
    private TMP_Dropdown vSyncCountDropDown = null;

    [SerializeField]
    private GameObject targetFrameRateSettings = null;
    private Toggle targetFrameRateToggle = null;
    private Slider targetFrameRateSlider = null;
    private TMP_InputField targetFrameRateField = null;

    void Start()
    {
        resolutions = Screen.resolutions;
        resolutionDropDown.AddOptions(GetResolutionStrings(resolutions));

        if (PlayerPrefs.HasKey("Settings"))
        {
            //If the player's settings have been saved before load them.
            settings = JsonUtility.FromJson<Settings>(PlayerPrefs.GetString("Settings"));

            int resolutionIndex = GetResolutionIndex(settings.GetResolution(), settings.GetRefreshRate());

            if (resolutionIndex == -1)
            {
                //If the saved resolution isn't available load the default resolution.
                int lastResolution = resolutions.Length - 1;
                Vector2Int resolution = new Vector2Int(resolutions[lastResolution].width, resolutions[lastResolution].height);
                int refreshRate = resolutions[lastResolution].refreshRate;
                settings.SetResolution(resolution);
                settings.SetRefreshRate(refreshRate);
                resolutionDropDown.value = resolutions.Length - 1;
            }
            else
            {
                resolutionDropDown.value = resolutionIndex;
            }
        }
        else
        {
            //If the player's settings can't be found save the default settings.
            int lastResolution = resolutions.Length - 1;
            Vector2Int resolution = new Vector2Int(resolutions[lastResolution].width, resolutions[lastResolution].height);
            int refreshRate = resolutions[lastResolution].refreshRate;
            settings = new Settings(resolution, refreshRate, FullScreenMode.FullScreenWindow, 0, 60);
            resolutionDropDown.value = resolutions.Length - 1;

            SaveSettings();
        }

        //Apply the loaded settings to the game.
        ApplySettings();

        //Update the full screen mode UI.
        fullScreenModeDropDown.AddOptions(GetFullScreenModeStrings());
        fullScreenModeDropDown.value = FullScreenModeToValue();

        vSyncCountDropDown.value = settings.GetVSyncCount();

        //Get the UI elements that are used for the target frame rate setting.
        targetFrameRateToggle = targetFrameRateSettings.transform.GetChild(0).GetComponent<Toggle>();
        targetFrameRateSlider = targetFrameRateSettings.transform.GetChild(1).GetComponent<Slider>();
        targetFrameRateField = targetFrameRateSettings.transform.GetChild(2).GetComponent<TMP_InputField>();

        int targetFrameRate = settings.GetTargetFrameRate();
        
        //Update the target frame rate UI.
        targetFrameRateToggle.isOn = targetFrameRate > 0;
        targetFrameRateSlider.value = targetFrameRate;
        targetFrameRateField.text = targetFrameRateSlider.value.ToString();
    }

    private List<string> GetResolutionStrings(Resolution[] resolutions)
    {
        List<string> resolutionStrings = new List<string>();

        //Convert all the resolutions provided into strings in the correct format for the drop down list.
        foreach (Resolution resolution in resolutions)
        {
            resolutionStrings.Add(resolution.width + "x" + resolution.height + " " + resolution.refreshRate + "Hz");
        }

        return resolutionStrings;
    }

    private List<string> GetFullScreenModeStrings()
    {
        List<string> fullScreenModes = new List<string>();

        fullScreenModes.Add("Windowed");
        fullScreenModes.Add("Borderless Full Screen");

        //Add the exclusive full screen option if the platform is Windows.
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            fullScreenModes.Add("Exclusive Full Screen");
        }

        return fullScreenModes;
    }

    public void OnFrameRateLimitToggleChange()
    {
        //Adjust whether the player can change the target frame rate if the toggle for it is changed.
        bool active = targetFrameRateToggle.isOn;

        targetFrameRateSlider.interactable = active;
        targetFrameRateField.interactable = active;
    }

    public void OnFrameRateLimitFieldEdit()
    {
        int value = int.Parse(targetFrameRateField.text);
        //Prevent the player from setting the frame rate above or below the limits.
        value = Mathf.Clamp(value, (int)targetFrameRateSlider.minValue, (int)targetFrameRateSlider.maxValue);
        targetFrameRateField.text = value.ToString();

        if (targetFrameRateSlider.value != value)
        {
            //Make the target frame rate slider reflect the target frame rate field.
            targetFrameRateSlider.value = value;
        }
    }

    public void OnFrameRateLimitSliderChange()
    {
        string value = targetFrameRateSlider.value.ToString();

        if (targetFrameRateField.text != value)
        {
            //Make the target frame rate field reflect the target frame rate slider.
            targetFrameRateField.text = value;
        }
    }

    public void ApplyChanges()
    {
        //Update the settings object.
        Vector2Int resolution = new Vector2Int(resolutions[resolutionDropDown.value].width, resolutions[resolutionDropDown.value].height);
        settings.SetResolution(resolution);
        settings.SetRefreshRate(resolutions[resolutionDropDown.value].refreshRate);
        settings.SetFullScreenMode(GetFullScreenMode());
        settings.SetVSyncCount(vSyncCountDropDown.value);
        if (targetFrameRateToggle.isOn)
        {
            settings.SetTargetFrameRate((int)targetFrameRateSlider.value);
        }
        else
        {
            settings.SetTargetFrameRate(-1);
        }

        //Set the game's settings based on the player's choices.
        ApplySettings();
        //Save the player's settings.
        SaveSettings();
    }

    private void ApplySettings()
    {
        //Set the game's settings using the values in the settings object.
        Vector2Int resolution = settings.GetResolution();
        int refreshRate = settings.GetRefreshRate();
        Screen.SetResolution(resolution.x, resolution.y, settings.GetFullScreenMode(), refreshRate);
        int vSyncCount = settings.GetVSyncCount();
        QualitySettings.vSyncCount = vSyncCount;
        if (vSyncCount == 0)
        {
            //If VSync is not applied use the target frame rate that the player set.
            Application.targetFrameRate = settings.GetTargetFrameRate();
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetString("Settings", JsonUtility.ToJson(settings));
    }

    private int GetResolutionIndex(Vector2 searchResolution, int searchRefreshRate)
    {
        //Search the list of the resolutions that the monitor supports and compare them against the resolution to find.
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == searchResolution.x && resolutions[i].height == searchResolution.y && resolutions[i].refreshRate == searchRefreshRate)
            {
                return i;
            }
        }

        return - 1;
    }

    private FullScreenMode GetFullScreenMode()
    {
        //Convert the string value of the full screen mode drop down to a full screen mode.
        string value = fullScreenModeDropDown.options[fullScreenModeDropDown.value].text;

        if (value == "Exclusive Full Screen")
        {
            return FullScreenMode.ExclusiveFullScreen;
        }
        else if (value == "Borderless Full Screen")
        {
            return FullScreenMode.FullScreenWindow;
        }
        else
        {
            return FullScreenMode.Windowed;
        }
    }

    private int FullScreenModeToValue()
    {
        //Convert the full screen mode to the value of the drop down list.
        FullScreenMode fullScreenMode = settings.GetFullScreenMode();

        if (fullScreenMode == FullScreenMode.ExclusiveFullScreen)
        {
            return 2;
        }
        else if (fullScreenMode == FullScreenMode.FullScreenWindow)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void Back(GameObject previousScreen)
    {
        //If the player exits the settings screen update the settings UI to reflect the currently saved settings.
        int resolutionIndex = GetResolutionIndex(settings.GetResolution(), settings.GetRefreshRate());
        
        if (resolutionIndex == -1)
        {
            resolutionDropDown.value = resolutions.Length - 1;
        }
        else
        {
            resolutionDropDown.value = resolutionIndex;
        }

        fullScreenModeDropDown.value = FullScreenModeToValue();
        
        vSyncCountDropDown.value = settings.GetVSyncCount();

        int targetFrameRate = settings.GetTargetFrameRate();

        targetFrameRateToggle.isOn = targetFrameRate > 0;
        targetFrameRateSlider.value = targetFrameRate;
        targetFrameRateField.text = targetFrameRateSlider.value.ToString();

        previousScreen.SetActive(true);
        settingsScreen.SetActive(false);
    }
}
