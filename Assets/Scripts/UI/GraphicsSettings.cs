using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GraphicsSettings : MonoBehaviour
{
    readonly Dictionary<string, Vector2Int> resolutions = new Dictionary<string, Vector2Int> { ["1280 x 720 16:9"] = new Vector2Int(1280, 720),
        ["1600 x 900 16:9"] = new Vector2Int(1600, 900),
        ["1920 x 1080 16:9"] = new Vector2Int(1920, 1080),
        ["2560 x 1440 16:9"] = new Vector2Int(2560, 1440),
        ["3840 x 2160 16:9"] = new Vector2Int(3840, 2160),
        ["1280 x 800 16:10"] = new Vector2Int(1280, 800),
        ["1440 x 900 16:10"] = new Vector2Int(1440, 900),
        ["1920 × 1200 16:10"] = new Vector2Int(1920, 1200) };


    public Dropdown graphics_dropdown;
    public Dropdown resolution_dropdown;
    private void Awake()
    {
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("quality_level"));
        graphics_dropdown.value = PlayerPrefs.GetInt("quality_level");

        Debug.Log(PlayerPrefs.GetString("resolution"));

        if (resolutions.ContainsKey(PlayerPrefs.GetString("resolution")))
        {
            Screen.SetResolution(resolutions[PlayerPrefs.GetString("resolution")].x, resolutions[PlayerPrefs.GetString("resolution")].y, true);
        }
        

        Resolution current_resolution = Screen.currentResolution;
        foreach (string r in resolutions.Keys)
        {
            if (resolutions[r].x == current_resolution.width && resolutions[r].y == current_resolution.height)
            {
                PlayerPrefs.SetString("resolution", r);
                PlayerPrefs.Save();
            }
        }
        for (int i = 0; i < resolution_dropdown.options.Count; i++)
        {
            if (PlayerPrefs.GetString("resolution") == resolution_dropdown.options[i].text)
            {
                resolution_dropdown.value = i;
            }
        }
    }


    public void setQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        Debug.Log(QualitySettings.GetQualityLevel());
        PlayerPrefs.SetInt("quality_level", index);
        PlayerPrefs.Save();
    }

    public void setResolution(int index)
    {
        string resolution = resolution_dropdown.options[resolution_dropdown.value].text;
        Debug.Log("set resolution " + resolution);
        Screen.SetResolution(resolutions[resolution].x, resolutions[resolution].y, true);
        PlayerPrefs.SetString("resolution", resolution);
        PlayerPrefs.Save();
    }
}
