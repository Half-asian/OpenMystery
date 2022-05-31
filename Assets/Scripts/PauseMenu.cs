using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;
using System.IO;
public class PauseMenu : MonoBehaviour
{
    public GameObject pause_menu;

    public AudioSource character_volume;
    public Slider character_volume_slider;

    public AudioSource music_volume;
    public Slider music_volume_slider; 
    
    public AudioSource ambient_volume;
    public Slider ambient_volume_slider;

    public GameObject camera_go;
    public HDAdditionalCameraData camera_data;

    public Canvas main_canvas;

    int width;
    int height;
    bool fullscreen;
    int screenshot_num;


    // Update is called once per frame
    void Awake()
    {
        width = Screen.currentResolution.width;
        height = Screen.currentResolution.height;
        fullscreen = Screen.fullScreen;
    }

    IEnumerator takeScreenshot()
    {
        yield return null;
        yield return null; // so it's at least one
        ScreenCapture.CaptureScreenshot(GlobalEngineVariables.player_folder + "\\..\\..\\screenshots\\" + screenshot_num + ".png");
        //camera.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
    }

    void Update()
    {
        if (Input.GetKeyDown("escape")){
            if (pause_menu.activeSelf)
            {
                pause_menu.SetActive(false);
            }
            else
            {
                pause_menu.SetActive(true);
            }
        }

        if (Input.GetKeyDown("r"))
        {
            camera_go.GetComponent<SimpleCameraController>().enabled = false;
            camera_go.transform.position = new Vector3(0, 1, 0);
            camera_go.GetComponent<SimpleCameraController>().enabled = true;

        }



        if (Input.GetKeyDown("f12"))
        {
            for (int i = 1; i <= 9999; i++)
            {
                if (!System.IO.File.Exists(GlobalEngineVariables.player_folder + "\\..\\..\\screenshots\\" + i + ".png"))
                {
                    //camera.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    screenshot_num = i;
                    StartCoroutine(takeScreenshot());
                    break;
                }
            }
        }

        if (Input.GetKeyDown("f4"))
        {
            main_canvas.enabled = !main_canvas.enabled;
        }

    }


    public string output = "";
    public string stack = "";
    public GameObject crash;



    public void quitGame()
    {
        Debug.Log("should quit game");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }

    public void setCharacterVolume()
    {
        character_volume.volume = character_volume_slider.value;
    }
    public void setMusicVolume()
    {
        music_volume.volume = music_volume_slider.value;
    }
    public void setAmbientVolume()
    {
        ambient_volume.volume = ambient_volume_slider.value;
    }
}
