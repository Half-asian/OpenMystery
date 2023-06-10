using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public static MainMenu current;

    public GameObject canvas;
    public GameObject menu_object;
    public GameObject crash;

    //Loading Screen stuff
    public GameObject loading_screen;
    public Image loading_spinner;

    void Start()
    {
        current = this;
    }


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

        if (Assignment.active_assignments.Count == 0)
        {
            if (Objective.active_objectives.Count == 0)
            {
                Debug.LogError("Objectives count was 0!");
            }
            Objective.active_objectives[0].LoadScenarioIfValid();
        }
        else
        {
            Assignment.active_assignments[0].startIntroScenario();
        }
        menu_object.SetActive(false);
        GameStart.menu_background.destroy();
    }


    public void enterGoalID(string starting_goal_chain, int starting_goal, GoalChainType goal_chain_type)
    {
        GoalChain.startGoalChain(starting_goal_chain, goal_chain_type, starting_goal);
        LoadingScreenCanvas.current.showImage();
        StartCoroutine(countdownLaunch(10));
        GetComponent<PauseMenu>().enabled = true;
    }



    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            if (crash) {
                crash.SetActive(true);
                crash.transform.Find("Error").GetComponent<Text>().text = logString;
                crash.transform.Find("Trace").GetComponent<Text>().text = stackTrace;
                return;
            }
            Debug.Log("Couldn't show crash to user.");
            if (logString != null && logString.Length > 1) Debug.Log(logString);
            if (stackTrace != null && stackTrace.Length > 1) Debug.Log(stackTrace);
        }
    }

    public void fakeCrash(string logString, string stackTrace)
    {
        if (crash) {
            crash.SetActive(true);
            crash.transform.Find("Error").GetComponent<Text>().text = logString;
            crash.transform.Find("Trace").GetComponent<Text>().text = stackTrace;
        }
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

        ScreenFade.fadeTo(1.0f, Color.black, true);

        while (Time.realtimeSinceStartup < start_time + 1)
        {
            yield return null;
        }
        menu_object.SetActive(false);

        while (Time.realtimeSinceStartup < start_time + 2f)
        {
            yield return null;
        }
        ScreenFade.fadeFrom(1.0f, Color.black, true);

    }



    IEnumerator fromLoadingToMenu()
    {
        state = State.stateMenu;
        float start_time = Time.realtimeSinceStartup;
        ScreenFade.fadeTo(1.0f, Color.black, true);
        while (Time.realtimeSinceStartup < start_time + 1)
        {
            yield return null;
        }
        loading_screen.SetActive(false);
        while (Time.realtimeSinceStartup < start_time + 2f)
        {
            yield return null;
        }
        ScreenFade.fadeFrom(1.0f, Color.black, true);

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
