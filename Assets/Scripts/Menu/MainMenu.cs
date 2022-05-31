﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public Image loading_image;
    public GameObject canvas;
    public GameObject menu_object;
    public GameObject crash;

    //Loading Screen stuff
    public GameObject loading_screen;
    public Image loading_spinner;



    public enum State
    {
        stateLoadingScreenLoading,
        stateLoadingScreenAwait,
        stateMenu,
    }

    public State state;


    IEnumerator countdownLaunch(int countdown)
    {
        while (countdown > 1)
        {
            countdown--;
            yield return null;
        }
        canvas.SetActive(true);
        Sound.playBGMusic("BGM");

        if (Objective.active_objectives.Count == 0)
        {
            Debug.LogError("Objectives count was 0!");
        }

        Objective.active_objectives[0].LoadScenarioIfValid();

        loading_image.enabled = false;
        menu_object.SetActive(false);
        GameStart.menu_background.destroy();
    }


    public void enterGoalID(string starting_goal_chain, int starting_goal, GoalChainType goal_chain_type)
    {
        GoalChain.startGoalChain(starting_goal_chain, goal_chain_type, starting_goal);
        loading_image.enabled = true;
        StartCoroutine(countdownLaunch(10));
        GetComponent<PauseMenu>().enabled = true;
    }



    public string output = "";
    public string stack = "";


    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;

        if (type == LogType.Exception)
        {
            crash.SetActive(true);
            crash.transform.Find("Error").GetComponent<Text>().text = logString;
            crash.transform.Find("Trace").GetComponent<Text>().text = stackTrace;

            System.IO.FileStream oFileStream = null;

            if (!File.Exists("error_log.txt"))
            {
                oFileStream = new System.IO.FileStream("error_log.txt", System.IO.FileMode.Create);
                oFileStream.Close();
            }

            StreamWriter writer = new StreamWriter("error_log.txt", true);
            writer.WriteLine(logString);
            writer.WriteLine(stackTrace);
            writer.Close();
        }
    }

    public void fakeCrash(string logString, string stackTrace)
    {
        crash.SetActive(true);
        crash.transform.Find("Error").GetComponent<Text>().text = logString;
        crash.transform.Find("Trace").GetComponent<Text>().text = stackTrace;

        System.IO.FileStream oFileStream = null;

        if (!File.Exists("error_log.txt"))
        {
            oFileStream = new System.IO.FileStream("error_log.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        StreamWriter writer = new StreamWriter("error_log.txt", true);
        writer.WriteLine(logString);
        writer.WriteLine(stackTrace);
        writer.Close();
    }

    public void quitGame()
    {
        Debug.Log("should quit game");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void chapterSelect()
    {


        StartCoroutine(fromMenuToChapter());
    }

    IEnumerator fromMenuToChapter()
    {
        float start_time = Time.realtimeSinceStartup;

        GameStart.event_manager.screen_fade.GetComponent<Animator>().SetTrigger("fade_to_black");

        while (Time.realtimeSinceStartup < start_time + 1)
        {
            yield return null;
        }
        menu_object.SetActive(false);

        CameraManager.current.main_camera_holder.GetComponent<Animator>().SetTrigger("changing room");
        while (Time.realtimeSinceStartup < start_time + 2f)
        {
            yield return null;
        }
        GameStart.event_manager.screen_fade.GetComponent<Animator>().SetTrigger("fade_from_black");
    }



    IEnumerator fromLoadingToMenu()
    {
        state = State.stateMenu;
        float start_time = Time.realtimeSinceStartup;
        GameStart.event_manager.screen_fade.GetComponent<Animator>().SetTrigger("fade_to_black");
        while (Time.realtimeSinceStartup < start_time + 1)
        {
            yield return null;
        }
        loading_screen.SetActive(false);
        while (Time.realtimeSinceStartup < start_time + 2f)
        {
            yield return null;
        }
        GameStart.event_manager.screen_fade.GetComponent<Animator>().SetTrigger("fade_from_black");

    }

    public void Update()
    {
        if (state == State.stateLoadingScreenLoading)
        {
            loading_spinner.transform.Rotate(new Vector3(0, 0, -150 * Time.deltaTime));
        }
        if (state == State.stateLoadingScreenAwait)
        {
            GameStart.ui_manager.press_space_text.color = new Color(Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * 2)) + 0.1f, Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * 2)) + 0.1f, Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * 2)) + 0.1f);
            if (Input.GetKeyDown("space"))
            {
                StartCoroutine(fromLoadingToMenu());
            }
        }
    }
}