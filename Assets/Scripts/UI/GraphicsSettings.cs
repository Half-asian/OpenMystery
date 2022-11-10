using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using System;

public class GraphicsSettings : MonoBehaviour
{
    private List<string> full_screen_mode_settings = new List<string> { "Windowed", "FullScreen Window", "Exclusive FullScreen" };
    private List<string> anti_aliasing_settings = new List<string> { "Off", "FXAA", "TAA", "SMAA" };
    private List<string> bloom_settings = new List<string> { "Off", "On" };
    private List<string> ambient_occlusion_settings = new List<string> { "Off", "On" };
    private List<string> global_illumination_settings = new List<string> { "Off", "On" };
    private List<string> tonemapping_settings = new List<string> { "Default", "ACES" };
    private List<string> chromatic_aberration_settings = new List<string> { "Off", "On" };
    private List<string> vignette_settings = new List<string> { "Off", "On" };
    private List<string> depth_of_field_settings = new List<string> { "Off", "On" };

    const string bloom_playerprefs_id = "bloom";
    const string antialiasing_playerprefs_id = "antialiasing";
    const string ambientocclusion_playerprefs_id = "ambientocclusion";
    const string ssglobalillumination_playerprefs_id = "ssglobalillumination";
    const string fullscreenmode_playerprefs_id = "fullscreenmode";
    const string renderscale_playerprefs_id = "renderscale";
    const string resolution_playerprefs_id = "resolution";
    const string tonemapping_playerprefs_id = "tonemapping";
    const string chromaticaberration_playerprefs_id = "chromaticaberration";
    const string vignette_playerprefs_id = "vignette";
    const string exposure_playerprefs_id = "exposure";
    const string dof_playerprefs_id = "dof";

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
    Volume volumeProfile;
    [SerializeField]
    Dropdown fullscreen_mode_dropdown;
    [SerializeField]
    Dropdown resolution_dropdown;
    [SerializeField]
    Slider render_scale_slider;
    [SerializeField]
    TMP_Text render_scale_text;
    [SerializeField]
    Dropdown anti_aliasing_dropdown;
    [SerializeField]
    Dropdown bloom_dropdown;
    [SerializeField]
    Dropdown ambient_occlusion_dropdown;
    [SerializeField]
    Dropdown global_illumination_dropdown;
    [SerializeField]
    Dropdown tonemapping_dropdown;
    [SerializeField]
    Dropdown chromatic_aberration_dropdown;
    [SerializeField]
    Dropdown vignette_dropdown;
    [SerializeField]
    Slider exposure_slider;
    [SerializeField]
    Dropdown depth_of_field_dropdown;

    /*----------- Graphics overrides ----------*/
    Bloom bloom_override;
    AmbientOcclusion ambient_occlusion_override;
    GlobalIllumination global_illumination_override;
    Tonemapping tonemapping_override;
    ColorAdjustments color_adjustments_override;
    LiftGammaGain lift_gamma_gain_override;
    Vignette vignette_override;
    ChromaticAberration chromatic_aberration_override;
    DepthOfField depth_of_field_override;

    FullScreenMode current_fullScreenMode;
    int current_width;
    int current_height;

    private bool opened_on_pause = false;

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
        initOverrides();
        initGeneric(    antialiasing_playerprefs_id,            anti_aliasing_settings,         "SMAA",     anti_aliasing_dropdown,         setAntiAliasing);
        initGeneric(    bloom_playerprefs_id,                   bloom_settings,                 "Off",      bloom_dropdown,                 setBloom);
        initGeneric(    ambientocclusion_playerprefs_id,        ambient_occlusion_settings,     "Off",      ambient_occlusion_dropdown,     setAmbientOcclusion);
        initGeneric(    ssglobalillumination_playerprefs_id,    global_illumination_settings,   "Off",      global_illumination_dropdown,   setGlobalIllumination);
        initGeneric(    tonemapping_playerprefs_id,             tonemapping_settings,           "Default",  tonemapping_dropdown,           setToneMapping);
        initGeneric(    chromaticaberration_playerprefs_id,     chromatic_aberration_settings,  "Off",      chromatic_aberration_dropdown,  setChromaticAberration);
        initGeneric(    vignette_playerprefs_id,                vignette_settings,              "Off",      vignette_dropdown,              setVignette);
        initGeneric(    dof_playerprefs_id,                     depth_of_field_settings,        "Off",      depth_of_field_dropdown,        setDof);
        initBrightness();
        Scenario.onScenarioLoading += () => menu_go.SetActive(false);

    }

    private void Update()
    {
        if (menu_go.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            opened_on_pause = true;
            menu_go.SetActive(false);
        }
        else if (!menu_go.activeSelf && Input.GetKeyDown(KeyCode.Escape) && opened_on_pause)
        {
            opened_on_pause = false;
            menu_go.SetActive(true);
        }


        detectResolutionChanged();
        detectFullScreenModeChanged();
    }

    private void initOverrides()
    {
        volumeProfile.profile.TryGet(out bloom_override);
        volumeProfile.profile.TryGet(out ambient_occlusion_override);
        volumeProfile.profile.TryGet(out global_illumination_override);
        volumeProfile.profile.TryGet(out tonemapping_override);
        volumeProfile.profile.TryGet(out color_adjustments_override);
        volumeProfile.profile.TryGet(out lift_gamma_gain_override);
        volumeProfile.profile.TryGet(out chromatic_aberration_override);
        volumeProfile.profile.TryGet(out vignette_override);
        volumeProfile.profile.TryGet(out depth_of_field_override);

    }

    private void initGeneric(string option_id, List<string> options, string default_option, Dropdown dropdown, UnityEngine.Events.UnityAction<int> call)
    {
        string current_option = default_option;
        if (PlayerPrefs.HasKey(option_id))
        {
            current_option = PlayerPrefs.GetString(option_id);
        }
        else
        {
            PlayerPrefs.SetString(option_id, current_option);
            PlayerPrefs.Save();
        }
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(call);
        dropdown.SetValueWithoutNotify(options.IndexOf(current_option));
        call.Invoke(options.IndexOf(current_option));
    }


    private void detectFullScreenModeChanged() {
        if (Screen.fullScreenMode != current_fullScreenMode)
        {
            int index = -1;
            switch (Screen.fullScreenMode)
            {
                case FullScreenMode.Windowed:
                    index = full_screen_mode_settings.IndexOf("Windowed");
                    PlayerPrefs.SetString(fullscreenmode_playerprefs_id, "Windowed");
                    PlayerPrefs.Save();
                    break;
                case FullScreenMode.MaximizedWindow:
                case FullScreenMode.FullScreenWindow:
                    index = full_screen_mode_settings.IndexOf("FullScreen Window");
                    PlayerPrefs.SetString(fullscreenmode_playerprefs_id, "FullScreen Window");
                    PlayerPrefs.Save();
                    break;
                case FullScreenMode.ExclusiveFullScreen:
                    index = full_screen_mode_settings.IndexOf("Exclusive FullScreen");
                    PlayerPrefs.SetString(fullscreenmode_playerprefs_id, "Exclusive FullScreen");
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
        opened_on_pause = false;
        menu_go.SetActive(!menu_go.activeSelf);
    }
    public void closeMenu()
    {
        opened_on_pause = false;
        menu_go.SetActive(false);
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
        if (PlayerPrefs.HasKey(fullscreenmode_playerprefs_id))
        {
            fullscreen_mode_dropdown.value = full_screen_mode_settings.IndexOf(PlayerPrefs.GetString("fullscreenmode"));
        }
        else
        {
            PlayerPrefs.SetString(fullscreenmode_playerprefs_id, "FullScreen Window");
            fullscreen_mode_dropdown.value = full_screen_mode_settings.IndexOf("FullScreen Window");
        }
    }

    private void setFullScreenMode(int index)
    {
        switch (full_screen_mode_settings[index])
        {
            case "Windowed":
                current_fullScreenMode = FullScreenMode.Windowed;
                break;
            case "FullScreen Window":
                current_fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case "Exclusive FullScreen":
                current_fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            default:
                throw new System.Exception("setFullScreenMode: Unknown full screen type " + full_screen_mode_settings[index]);
        }
        PlayerPrefs.SetString(fullscreenmode_playerprefs_id, full_screen_mode_settings[index]);
        PlayerPrefs.Save();
        Screen.fullScreenMode = current_fullScreenMode;
    }

    private void initRenderScale()
    {
        if (PlayerPrefs.HasKey(renderscale_playerprefs_id))
        {
            current_render_scale = PlayerPrefs.GetFloat(renderscale_playerprefs_id);
        }
        else
        {
            current_render_scale = 100.0f;
            PlayerPrefs.SetFloat(renderscale_playerprefs_id, 100.0f);
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
        PlayerPrefs.SetFloat(renderscale_playerprefs_id, current_render_scale);
        render_scale_text.text = "Render Resolution Scale: " + current_render_scale + "%";
    }

    private void initDisplayResolution()
    {
        resolution_dropdown.AddOptions(new List<string>(resolutions.Keys)); //set options

        if (PlayerPrefs.HasKey(resolution_playerprefs_id) && resolutions.ContainsKey(PlayerPrefs.GetString(resolution_playerprefs_id))) //We have a player pref set
        {
            Screen.SetResolution(resolutions[PlayerPrefs.GetString(resolution_playerprefs_id)].x, resolutions[PlayerPrefs.GetString(resolution_playerprefs_id)].y, true);
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
            PlayerPrefs.SetString(resolution_playerprefs_id, found_resolution);
            PlayerPrefs.Save();
        }

        for (int i = 0; i < resolution_dropdown.options.Count; i++) //Set the dropdown value
        {
            if (PlayerPrefs.GetString(resolution_playerprefs_id) == resolution_dropdown.options[i].text)
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
        PlayerPrefs.SetString(resolution_playerprefs_id, resolution);
        PlayerPrefs.Save();
    }

    private void setAntiAliasing(int index)
    {
        Debug.Log("Setting anti aliasing: " + anti_aliasing_settings[index]);
        var hdaddiationalcameradata = CameraManager.current.camera_component.GetComponent<HDAdditionalCameraData>();

        switch (anti_aliasing_settings[index])
        {
            case "Off":
                hdaddiationalcameradata.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
                break;
            case "TAA":
                hdaddiationalcameradata.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                break;
            case "SMAA":
                hdaddiationalcameradata.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
            case "FXAA":
                hdaddiationalcameradata.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                break;
        }
        PlayerPrefs.SetString(antialiasing_playerprefs_id, anti_aliasing_settings[index]);
        PlayerPrefs.Save();
    }

    private void setBloom(int index)
    {
        if (bloom_override is null)
            throw new System.Exception("Couldn't find bloom volume override");
        Debug.Log("Setting bloom: " + bloom_settings[index]);
        switch (bloom_settings[index])
        {
            case "Off":
                bloom_override.intensity.value = 0.0f;
                break;
            case "On":
                bloom_override.intensity.value = 0.26f;
                break;
        }
        PlayerPrefs.SetString(bloom_playerprefs_id, bloom_settings[index]);
        PlayerPrefs.Save();
    }

    private void setAmbientOcclusion(int index) 
    {
        if (ambient_occlusion_override is null)
            throw new System.Exception("Couldn't find ambient occlusion volume override");
        Debug.Log("Setting AmbientOcclusion: " + ambient_occlusion_settings[index]);
        switch (ambient_occlusion_settings[index])
        {
            case "Off":
                ambient_occlusion_override.active = false;
                break;
            case "On":
                ambient_occlusion_override.active = true;
                break;
        }
        PlayerPrefs.SetString(ambientocclusion_playerprefs_id, ambient_occlusion_settings[index]);
        PlayerPrefs.Save();
    }

    private void setGlobalIllumination(int index)
    {
        if (global_illumination_override is null)
            throw new System.Exception("Couldn't find global illumination volume override");
        Debug.Log("Setting GlobalIllumination: " + global_illumination_settings[index]);
        switch (global_illumination_settings[index])
        {
            case "Off":
                global_illumination_override.active = false;
                break;
            case "On":
                global_illumination_override.active = true;
                break;
        }
        PlayerPrefs.SetString(ssglobalillumination_playerprefs_id, global_illumination_settings[index]);
        PlayerPrefs.Save();
    }

    private void setToneMapping(int index)
    {
        if (tonemapping_override is null)
            throw new System.Exception("Couldn't find tonemapping volume override");
        if (lift_gamma_gain_override is null)
            throw new System.Exception("Couldn't find lift gamma gain volume override");
        Debug.Log("Setting tonemapping: " + tonemapping_settings[index]);
        switch (tonemapping_settings[index])
        {
            case "Default":
                tonemapping_override.active = false;
                lift_gamma_gain_override.active = false;
                break;
            case "ACES":
                tonemapping_override.active = true;
                lift_gamma_gain_override.active = true;
                break;
        }
        PlayerPrefs.SetString(tonemapping_playerprefs_id, tonemapping_settings[index]);
        PlayerPrefs.Save();
    }

    private void setChromaticAberration(int index)
    {
        if (chromatic_aberration_override is null)
            throw new System.Exception("Couldn't find chromatic aberration volume override");
        Debug.Log("Setting chromatic aberration: " + chromatic_aberration_settings[index]);
        switch (chromatic_aberration_settings[index])
        {
            case "Off":
                chromatic_aberration_override.active = false;
                break;
            case "On":
                chromatic_aberration_override.active = true;
                break;
        }
        PlayerPrefs.SetString(chromaticaberration_playerprefs_id, chromatic_aberration_settings[index]);
        PlayerPrefs.Save();
    }

    private void setVignette(int index)
    {
        if (vignette_override is null)
            throw new System.Exception("Couldn't find vignette volume override");
        Debug.Log("Setting vignette: " + vignette_settings[index]);
        switch (vignette_settings[index])
        {
            case "Off":
                vignette_override.active = false;
                break;
            case "On":
                vignette_override.active = true;
                break;
        }
        PlayerPrefs.SetString(vignette_playerprefs_id, vignette_settings[index]);
        PlayerPrefs.Save();
    }


    private void initBrightness()
    {
        if (!PlayerPrefs.HasKey(exposure_playerprefs_id)) { 

            PlayerPrefs.Save();
        }

        exposure_slider.onValueChanged.AddListener(setExposure);
        exposure_slider.value = PlayerPrefs.GetFloat(exposure_playerprefs_id);
        setExposure(exposure_slider.value);
    }

    private void setExposure(float value)
    {
        if (color_adjustments_override is null)
            throw new System.Exception("Couldn't find color adjustments volume override");
        color_adjustments_override.postExposure.value = value;
        PlayerPrefs.SetFloat(exposure_playerprefs_id, value);
        PlayerPrefs.Save();
    }

    private void setDof(int index)
    {
        if (depth_of_field_override is null)
            throw new System.Exception("Couldn't find depth of field volume override");
        Debug.Log("Setting depth of field: " + depth_of_field_settings[index]);
        switch (depth_of_field_settings[index])
        {
            case "Off":
                depth_of_field_override.active = false;
                break;
            case "On":
                depth_of_field_override.active = true;
                break;
        }
        PlayerPrefs.SetString(dof_playerprefs_id, depth_of_field_settings[index]);
        PlayerPrefs.Save();
    }

}
