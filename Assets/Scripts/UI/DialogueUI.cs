using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class DialogueUI : MonoBehaviour
{
    [SerializeField]
    GameObject dialogue_ui;
    [SerializeField]
    TMP_Text name_text;
    [SerializeField]
    TextMeshProUGUI dialogue_text;
    [SerializeField]
    GameObject divider;
    [SerializeField]
    GameObject dialoguebg;
    [SerializeField]
    GameObject choice_1_gameobject;
    [SerializeField]
    TMP_Text choice_1_text;
    [SerializeField]
    GameObject choice_2_gameobject;
    [SerializeField]
    TMP_Text choice_2_text;
    [SerializeField]
    GameObject choice_3_gameobject;
    [SerializeField]
    TMP_Text choice_3_text;

    const float letter_seperator = 0.02f;
    float start_dialogue_time;

    public static bool finished_showing_text;

    private void Awake()
    {
        DialogueManager.setDialogueUIActive += setDialogueUIActive;
        DialogueManager.setDialogueText += setDialogueText;
        DialogueManager.setNameText += setNameText;
        DialogueManager.setChoice1ActiveWithText += setChoice1Text;
        DialogueManager.setChoice2ActiveWithText += setChoice2Text;
        DialogueManager.setChoice3ActiveWithText += setChoice3Text;
    }

    void setDialogueUIActive(bool active)
    {
        dialogue_ui.SetActive(active);
        start_dialogue_time = Time.realtimeSinceStartup;

    }

    void setDialogueText(string text)
    {
        finished_showing_text = false;
        dialogue_text.maxVisibleCharacters = 0;
        dialogue_text.text = text;
        dialogue_text.ForceMeshUpdate();
        if (dialogue_text.isTextOverflowing)
            moveUIUp();
        else
            moveUIDefault();
    }

    void moveUIUp()
    {
        dialogue_text.gameObject.transform.localPosition = new Vector3(0, -160.5f, 0);
        name_text.gameObject.transform.localPosition = new Vector3(0, -124.5f, 0);
        divider.transform.localPosition = new Vector3(0, -139.1f, 0);
        dialoguebg.transform.localPosition = new Vector3(0, -170.3f, 0);
    }

    void moveUIDefault()
    {
        dialogue_text.gameObject.transform.localPosition = new Vector3(0, -185.5f, 0);
        name_text.gameObject.transform.localPosition = new Vector3(0, -144.5f, 0);
        divider.transform.localPosition = new Vector3(0, -164.1f, 0);
        dialoguebg.transform.localPosition = new Vector3(0, -195.3f, 0);

    }


    void setNameText(string text)
    {
        name_text.text = text;
        name_text.ForceMeshUpdate();
    }

    void setChoice1Text(string text)
    {
        choice_1_gameobject.SetActive(true);
        choice_1_text.text = text;
    }

    void setChoice2Text(string text)
    {
        choice_2_gameobject.SetActive(true);
        choice_2_text.text = text;
    }

    void setChoice3Text(string text)
    {
        choice_3_gameobject.SetActive(true);
        choice_3_text.text = text;
    }

    public void dialogueChoice1Callback()
    {
        Debug.Log("dialogueChoice1Callback");
        choice_1_gameobject.SetActive(false);
        choice_2_gameobject.SetActive(false);
        choice_3_gameobject.SetActive(false);

        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + GameStart.dialogue_manager.choices[0] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + GameStart.dialogue_manager.choices[0] + "\")");
            writer.Close();
        }

        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[1] + "\")", ""));
        if (GameStart.dialogue_manager.choices.Count > 2)
            File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[2] + "\")", ""));
        GameStart.dialogue_manager.activateDialogueOption1();
    }

    public void dialogueChoice2Callback()
    {
        Debug.Log("dialogueChoice2Callback");
        choice_1_gameobject.SetActive(false);
        choice_2_gameobject.SetActive(false);
        choice_3_gameobject.SetActive(false);
        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + GameStart.dialogue_manager.choices[1] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + GameStart.dialogue_manager.choices[1] + "\")");
            writer.Close();
        }
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[0] + "\")", ""));
        if (GameStart.dialogue_manager.choices.Count > 2)
            File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[2] + "\")", ""));
        GameStart.dialogue_manager.activateDialogueOption2();
    }

    public void dialogueChoice3Callback()
    {
        Debug.Log("dialogueChoice3Callback");
        choice_1_gameobject.SetActive(false);
        choice_2_gameobject.SetActive(false);
        choice_3_gameobject.SetActive(false);
        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + GameStart.dialogue_manager.choices[2] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + GameStart.dialogue_manager.choices[2] + "\")");
            writer.Close();
        }
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[0] + "\")", ""));
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + GameStart.dialogue_manager.choices[1] + "\")", ""));
        GameStart.dialogue_manager.activateDialogueOption3();
    }

    public static void finishShowingText()
    {
        finished_showing_text = true;
    }


    private void Update()
    {
        if (dialogue_text.isTextOverflowing)
            moveUIUp();
        else
            moveUIDefault();

        if (!dialogue_ui.activeSelf) return;

        if (finished_showing_text == true)
        {
            dialogue_text.maxVisibleCharacters = dialogue_text.textInfo.characterCount;
            return;
        }

        int text_holder_index = Mathf.Min((int)((Time.realtimeSinceStartup - start_dialogue_time) / letter_seperator), dialogue_text.textInfo.characterCount + 1);

        dialogue_text.maxVisibleCharacters = text_holder_index;

        if (dialogue_text.textInfo.characterCount < text_holder_index)
            finished_showing_text = true;
    }
}
