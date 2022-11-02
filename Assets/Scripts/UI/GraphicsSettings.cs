using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
public class GraphicsSettings : MonoBehaviour
{
    private List<string> full_screen_mode_settings = new List<string> { "Windowed", "FullScreen Window", "Exclusive FullScreen" };

    readonly Dictionary<string, Vector2Int> resolutions = new Dictionary<string, Vector2Int>
    {
        ["3840 x 2160 16:9"] = new Vector2Int(3840, 2160),
        ["2560 x 1440 16:9"] = new Vector2Int(2560, 1440),
        ["2048 x 1152 16:9"] = new Vector2Int(2048, 1152),
        ["1920 x 1080 16:9"] = new Vector2Int(1920, 1080),
        ["1600 x 900 16:9"] = new Vector2Int(1600, 900),
        ["1366 x 768 16:9"] = new Vector2Int(1366, 768),
        ["1280 x 720 16:9"] = new Vector2Int(1280, 720),
        ["640 x 360 16:9"] = new Vector2Int(640, 360),

        ["3440 x 1440 21:9"] = new Vector2Int(3440, 1440),
        ["2560 x 1080 21:9"] = new Vector2Int(2560, 1080),

        ["2560 × 1600 16:10"] = new Vector2Int(2560, 1600),
        ["1920 × 1200 16:10"] = new Vector2Int(1920, 1200),
        ["1680 x 1050 16:10"] = new Vector2Int(1680, 1050),
        ["1440 x 900 16:10"] = new Vector2Int(1440, 900),
        ["1280 x 800 16:10"] = new Vector2Int(1280, 800),

        ["2048 x 1536 4:3"] = new Vector2Int(2048, 1536),
        ["1024 x 768 4:3"] = new Vector2Int(1024, 768),
        ["800 x 600 4:3"] = new Vector2Int(800, 600),
    };


    [SerializeField]
    GameObject menu_go;

    [SerializeField]
    Dropdown fullscreen_mode_dropdown;
    [SerializeField]
    Dropdown resolution_dropdown;
    [SerializeField]
    Dropdown graphics_dropdown;

    [SerializeField]
    Slider render_scale_slider;
    [SerializeField]
    TMP_Text render_scale_text;

    FullScreenMode current_fullScreenMode;
    int current_width;
    int current_height;

    float current_render_scale = 100.0f;
    
    private float setDynamicResolutionScale()
    {
        return current_render_scale;
    }

    private void Awake()
    {
        QualitySettings.SetQualityLevel(0);
        //graphics_dropdown.value = PlayerPrefs.GetInt("quality_level");


        initFullScreenMode();
        initDisplayResolution();
        initRenderScale();



        Scenario.onScenarioLoading += () => menu_go.SetActive(false);

    }

    private void Update()
    {
        detectResolutionChanged();
        detectFullScreenModeChanged();
    }

    private void detectFullScreenModeChanged() {
        if (Screen.fullScreenMode != current_fullScreenMode)
        {
            int index = -1;
            switch (Screen.fullScreenMode)
            {
                case FullScreenMode.Windowed:
                    index = full_screen_mode_settings.IndexOf("Windowed");
                    PlayerPrefs.SetString("fullscreenmode", "Windowed");
                    PlayerPrefs.Save();
                    break;
                case FullScreenMode.MaximizedWindow:
                case FullScreenMode.FullScreenWindow:
                    index = full_screen_mode_settings.IndexOf("FullScreen Window");
                    PlayerPrefs.SetString("fullscreenmode", "FullScreen Window");
                    PlayerPrefs.Save();
                    break;
                case FullScreenMode.ExclusiveFullScreen:
                    index = full_screen_mode_settings.IndexOf("Exclusive FullScreen");
                    PlayerPrefs.SetString("fullscreenmode", "Exclusive FullScreen");
                    PlayerPrefs.Save();
                    break;
            }
            if (index == -1)
                throw new System.Exception("detectFullScreenModeChanged: Unknown full screen type " + Screen.fullScreenMode.ToString());
            fullscreen_mode_dropdown.SetValueWithoutNotify(index);
            current_fullScreenMode = Screen.fullScreenMode;
        }
    }

    private void detectResolutionChanged()
    {
        //Ignore changes and overwrite with previous values
        if (Screen.width != current_width || Screen.height != current_height)
        {
            Screen.SetResolution(current_width, current_height, Screen.fullScreenMode);
        }
    }

    public void toggleMenu()
    {
        menu_go.SetActive(!menu_go.activeSelf);
    }
    public void closeMenu()
    {
        menu_go.SetActive(false);
    }

    public void setQuality(int index)
    {
        //QualitySettings.SetQualityLevel(index);
        //Debug.Log(QualitySettings.GetQualityLevel());
        //PlayerPrefs.SetInt("quality_level", index);
        //PlayerPrefs.Save();
    }

    private void initFullScreenMode()
    {
        current_fullScreenMode = Screen.fullScreenMode;

        List<string> to_remove = new List<string>();
        foreach (var key in resolutions.Keys)
        {
            if (resolutions[key].x > Screen.mainWindowDisplayInfo.width || resolutions[key].y > Screen.mainWindowDisplayInfo.height)
            {
                to_remove.Add(key);
            }  
        }
        foreach(var remove in to_remove)
        {
            resolutions.Remove(remove);
        }

        fullscreen_mode_dropdown.AddOptions(full_screen_mode_settings);
        fullscreen_mode_dropdown.onValueChanged.AddListener(setFullScreenMode);
        if (PlayerPrefs.HasKey("fullscreenmode"))
        {
            fullscreen_mode_dropdown.value = full_screen_mode_settings.IndexOf(PlayerPrefs.GetString("fullscreenmode"));
        }
        else
        {
            PlayerPrefs.SetString("fullscreenmode", "FullScreen Window");
            fullscreen_mode_dropdown.value = full_screen_mode_settings.IndexOf("FullScreen Window");
        }
    }

    private void setFullScreenMode(int index)
    {
        switch (full_screen_mode_settings[index])
        {
            case "Windowed":
                current_fullScreenMode = FullScreenMode.Windowed;
                PlayerPrefs.SetString("fullscreenmode", "Windowed");
                PlayerPrefs.Save();
                break;
            case "FullScreen Window":
                current_fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerPrefs.SetString("fullscreenmode", "FullScreen Window");
                PlayerPrefs.Save();
                break;
            case "Exclusive FullScreen":
                current_fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                PlayerPrefs.SetString("fullscreenmode", "Exclusive FullScreen");
                PlayerPrefs.Save();
                break;
            default:
                throw new System.Exception("setFullScreenMode: Unknown full screen type " + full_screen_mode_settings[index]);
        }
        Screen.fullScreenMode = current_fullScreenMode;
    }

    private void initRenderScale()
    {
        if (PlayerPrefs.HasKey("renderscale"))
        {
            current_render_scale = PlayerPrefs.GetFloat("renderscale");
        }
        else
        {
            current_render_scale = 100.0f;
            PlayerPrefs.SetFloat("renderscale", 100.0f);
            PlayerPrefs.Save();
        }

        render_scale_slider.onValueChanged.AddListener(setRenderScale);
        render_scale_slider.value = current_render_scale;
        render_scale_text.text = "Render Resolution Scale: " + current_render_scale + "%";

        DynamicResolutionHandler.SetDynamicResScaler(setDynamicResolutionScale, DynamicResScalePolicyType.ReturnsPercentage);
    }

    private void setRenderScale(float scale)
    {
        current_render_scale = scale;
        PlayerPrefs.SetFloat("renderscale", current_render_scale);
        render_scale_text.text = "Render Resolution Scale: " + current_render_scale + "%";
    }

    private void initDisplayResolution()
    {
        resolution_dropdown.AddOptions(new List<string>(resolutions.Keys)); //set options

        if (PlayerPrefs.HasKey("resolution") && resolutions.ContainsKey(PlayerPrefs.GetString("resolution"))) //We have a player pref set
        {
            Screen.SetResolution(resolutions[PlayerPrefs.GetString("resolution")].x, resolutions[PlayerPrefs.GetString("resolution")].y, true);
        }
        else //Set a brand new player pref
        {
            string found_resolution = null;
            foreach (string r in resolutions.Keys)
            {
                if (resolutions[r].x == Screen.currentResolution.width && resolutions[r].y == Screen.currentResolution.height)
                {
                    found_resolution = r;
                }
            }
            if (found_resolution == null)
                found_resolution = "1920 x 1080 16:9";
            PlayerPrefs.SetString("resolution", found_resolution);
            PlayerPrefs.Save();
        }

        for (int i = 0; i < resolution_dropdown.options.Count; i++) //Set the dropdown value
        {
            if (PlayerPrefs.GetString("resolution") == resolution_dropdown.options[i].text)
            {
                resolution_dropdown.value = i;
            }
        }
        current_width = Screen.width;
        current_height = Screen.height;
        resolution_dropdown.onValueChanged.AddListener(setResolution);
    }

    private void setResolution(int index)
    {
        string resolution = resolution_dropdown.options[index].text;
        Debug.Log("set resolution " + resolution);
        Screen.SetResolution(resolutions[resolution].x, resolutions[resolution].y, Screen.fullScreenMode);
        current_width = resolutions[resolution].x;
        current_height = resolutions[resolution].y;
        PlayerPrefs.SetString("resolution", resolution);
        PlayerPrefs.Save();
    }
}
