using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    private class Settings
    {
        private Vector2 resolution;
        private FullScreenMode fullScreenMode;
        private int vSyncCount;
        private int targetFrameRate;

        public Settings (Vector2 resolution, FullScreenMode fullScreenMode, int vSyncCount, int targetFrameRate)
        {
            this.resolution = resolution;
            this.fullScreenMode = fullScreenMode;
            this.vSyncCount = vSyncCount;
            this.targetFrameRate = targetFrameRate;
        }

        #region Getters
        public Vector2 GetResolution()
        {
            return resolution;
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
        public void SetResolution(Vector2 resolution)
        {
            this.resolution = resolution;
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

    private Dropdown resolutionDropDown = null;
    private Dropdown fullScreenModeDropDown = null;
    private Dropdown vSyncCountDropDown = null;
    private Slider targetFrameRateSlider = null;
    private TMP_InputField targetFrameRateField = null;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
