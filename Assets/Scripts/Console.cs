using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using System;

public class Console : MonoBehaviour
{
    [SerializeField]
    private TMPro.TMP_InputField inputfield;
    [SerializeField]
    private TMPro.TMP_Text error_text;


    public bool input_active = false;

    void Start()
    {
        input_active = false;
        inputfield.gameObject.SetActive(input_active);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            input_active = !input_active;
            inputfield.gameObject.SetActive(input_active);
            if (input_active)
                inputfield.Select();
        }

        if (input_active && Input.GetKeyDown(KeyCode.Return))
        {
            var inputs = inputfield.text.Split('\n');
            foreach (var i in inputs) {
                processCommand(i);
            }
            inputfield.text = "";
        }

    }

    void processCommand(string command)
    {
        Debug.Log("processCommand " + command);
        StopAllCoroutines();

        string[] parameters = command.Split();

        string[] actionParams = new string[parameters.Length - 1];
        Array.Copy(parameters, 1, actionParams, 0, actionParams.Length);

        if (parameters.Length == 0)
            return;



        switch (parameters[0])
        {
            case "loadscenario":
                Scenario.Load(parameters[1]);
                break;
            case "loadmatch":
                GameStart.quidditch_manager.startMatch(parameters[1]);
                break;
            case "loadscene":
                ConfigScenario._Scenario test_scenario = new ConfigScenario._Scenario();
                test_scenario.scene = parameters[1];
                test_scenario.scenarioId = "test_scenario";
                Configs.config_scenario.Scenario["test_scenario"] = test_scenario;
                Scenario.Activate("test_scenario");
                Scenario.Load("test_scenario");
                break;
            case "completeobjective":
                Objective.active_objectives[0].objectiveCompleted();
                break;
            case "markinteractioncomplete":
                Scenario.completeInteraction(parameters[1]);
                break;
            case "fadeToBlack":
                ScreenFade.fadeTo(float.Parse(parameters[1], CultureInfo.InvariantCulture), Color.black);
                break;
            case "fadeFromBlack":
                ScreenFade.fadeFrom(float.Parse(parameters[1], CultureInfo.InvariantCulture), Color.black);
                break;
            case "spawnProp":
                Prop.spawnPropFromEvent(parameters[1], null, parameters[1], "");
                break;
            case "spawnActor":
                Actor.spawnActor(parameters[1], null, parameters[1]);
                break;
            case "animateCharacter":
                Events.doEventAction("animateCharacter", new string[] { parameters[1], parameters[2] }, null);
                break;
            default:
                Events.doEventAction(parameters[0], actionParams, null);

                break;
        }






    }

    public void reactivate()
    {
        inputfield.ActivateInputField();

    }

    IEnumerator sendErrorMessage(string message)
    {
        error_text.text = message;
        yield return new WaitForSeconds(3.0f);
        error_text.text = "";
    }

}
