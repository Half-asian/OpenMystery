using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
            processCommand(inputfield.text);
            inputfield.text = "";
        }

    }

    void processCommand(string command)
    {
        Debug.Log("processCommand " + command);
        StopAllCoroutines();

        string[] parameters = command.Split();

        if (parameters.Length == 0)
            return;

        switch (parameters[0])
        {
            case "loadscenario":
                Scenario.Load(parameters[1]);
                break;
            case "loadscene":
                Scene.setCurrentScene(parameters[1]);
                Scenario.cleanup();
                break;
            case "replaceCharacterIdle":
                Events.doEventAction("replaceCharacterIdle", new string[] { parameters[1], parameters[2] });
                break;
            default:
                StartCoroutine(sendErrorMessage("Unknown command " + parameters[0]));
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
