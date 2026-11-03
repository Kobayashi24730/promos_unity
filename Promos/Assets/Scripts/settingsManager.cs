using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.Rendering;

public class settingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public TMP_Dropdown qualityDropdown;
    public void Start()
    {
        loadSettigs();
        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(setVolume);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(setBrightness);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(setQuality);

    }

    public void setVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void setBrightness(float Brightness)
    {
        PlayerPrefs.SetFloat("MasterBrightness", Brightness);
        PlayerPrefs.Save();
    }

    public void setQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("MasterQuality", qualityIndex);
        PlayerPrefs.Save();
    }

    public void loadSettigs()
    {
        if (volumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = savedVolume;
            AudioListener.volume = savedVolume;
        }
        if (brightnessSlider != null)
        {
            brightnessSlider.value = PlayerPrefs.GetFloat("MasterBrightness", 1f);
        }
        if (qualityDropdown != null)
        {
            int savedQuality = PlayerPrefs.GetInt("MasterQuality", QualitySettings.GetQualityLevel());
            qualityDropdown.value = savedQuality;
            QualitySettings.SetQualityLevel(savedQuality);
        }
    }

}
