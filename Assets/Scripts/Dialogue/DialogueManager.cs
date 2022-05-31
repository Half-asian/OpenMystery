using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public interface DialogueCallback
{
    public abstract void dialogueCallback(string dialogue);
}

public enum DialogueStatus
{
    WaitingEnterEvents,
    WaitingPlayerConfirm,
    WaitingPlayerOptionSelect,
    WaitingExitEvents,
    Finished
}

public class DialogueManager : MonoBehaviour
{
    GameStart game_start;
    EventManager event_manager;

    public static string companionId = "chiara";

    //public List<DialogueCallback> callbacks = new List<DialogueCallback>();
    public event Action<string> onDialogueFinishedEvent;

    public GameObject ui_dialogue;
    public Text ui_dialogue_name;
    public TextMeshProUGUI ui_dialogue_text;
    public GameObject ui_dialogue_choice_1;
    public string dialogue_choice_1_next_dialogue;
    public GameObject ui_dialogue_choice_2;
    public string dialogue_choice_2_next_dialogue;
    public GameObject ui_dialogue_choice_3;
    public string dialogue_choice_3_next_dialogue;
    public List<string> choices;
    public ConfigHPDialogueLine.HPDialogueLine current_dialogue;
    public string next_dialogue;
    public bool waiting_for_dialogue = false;
    public static string local_avatar_first_name;
    public static string local_avatar_last_name;
    public static string local_avatar_full_name;
    public static string local_avatar_house;
    public static string local_avatar_gender;
    public static string local_avatar_quidditch_position = "chaser";
    public static string local_avatar_clothing_type;
    public static string local_avatar_secondary_clothing_option;
    public static string local_avatar_opponent_house;
    public static string local_avatar_onscreen_name;
    public static int local_avatar_year => GlobalEngineVariables.player_year;
    public bool in_dialogue;
    public bool in_bubble;

    public List<string> madeChoice;

    public List<string> entry_stack = new List<string>();

    public List<string> exit_stack = new List<string>();

    public string text_holder;
    public string name_holder;
    public string next_text_holder;
    public string next_name_holder;

    const float letter_seperator = 0.02f;
    public float start_dialogue_time;

    public DialogueStatus dialogue_status = DialogueStatus.Finished;

    public string[] camera_params;


    public void Start()
    {
        game_start = GetComponent<GameStart>();
        event_manager = GetComponent<EventManager>();
    }

    public void resetCamera()
    {
        CameraManager.current.resetCamera();
    }
    

    public void showBubbleDialogue(string speaker, string dialogue)
    {
        if (speaker == "Avatar")
        {
            speaker = "You";
        }
        else
        {
            speaker = mapName(speaker);
        }

        ui_dialogue.SetActive(true);
        name_holder = speaker;

        text_holder = LocalData.getLine(dialogue);

        ui_dialogue_name.text = "";
        ui_dialogue_text.text = "";
        start_dialogue_time = Time.realtimeSinceStartup;
    }

    public void finishBubbleDialogue()
    {
        in_bubble = false;
        ui_dialogue.SetActive(false);
    }


    public bool activateNewDialogue(string dialogue)
    {
        InteractionManager.changeOptionInteractionsVisibility(false);

        Debug.Log("activateNewDialogue " + dialogue);

        string dialogue_line = null;

        if (Configs.dialogue_dict.ContainsKey(dialogue))
        {
            foreach (ConfigHPDialogueLine.HPDialogueLine i in Configs.dialogue_dict[dialogue])
            {
                if (i.initialTurn == true)
                {
                    dialogue_line = i.id;
                    break; //There can be multiple. Take the first one.
                }
            }
            if (dialogue_line == null)
            {
                dialogue_line = Configs.dialogue_dict[dialogue][0].id; //No initial turn? Thats fine. Take the first match.
            }
        }
        else
        {
            Debug.LogError("DialogueManager:activateNewDialogue - Config empty for dialogue " + dialogue + " in HPDialogueLines");
            return false;
        }

        if (dialogue_line == null)
        {
            throw new System.Exception("DialogueManager:activateNewDialogue - Config empty for dialogue " + dialogue + " in HPDialogueLines");
            // return false;
            //dialogue_line = dialogue + "1";

        }


        return activateDialogue(dialogue_line);
    }
    public bool activateDialogue(string dialogue_name)
    {
        in_bubble = false;

        if (dialogue_name == null)
        {
            Debug.LogError("activateDialogue name was null");
            return true;
        }
        
        Debug.Log("activated dialogue " + dialogue_name);
        Log.writeFull("activated dialogue " + dialogue_name);
        
        dialogue_status = DialogueStatus.WaitingEnterEvents;

        in_dialogue = true;
        ConfigHPDialogueLine.HPDialogueLine dialogue = null;
        if (!Configs.config_hp_dialogue_line.HPDialogueLines.ContainsKey(dialogue_name))
        {

            foreach (string dialogue_key in Configs.config_hp_dialogue_line.HPDialogueLines.Keys)
            {
                if (Configs.config_hp_dialogue_line.HPDialogueLines[dialogue_key].dialogue == dialogue_name)
                {
                    dialogue = Configs.config_hp_dialogue_line.HPDialogueLines[dialogue_key];
                    break;
                }
            }
        }
        else
        {
            dialogue = Configs.config_hp_dialogue_line.HPDialogueLines[dialogue_name];
        }

        if (dialogue == null)
        {
            Debug.Log(dialogue_name + " was not in the dialogue dictionary");
            return false;
        }

        if (Configs.dialogue_line_override_dict.ContainsKey(dialogue.id))
        {
            foreach(ConfigHPDialogueOverride._HPDialogueOverride override_line in Configs.dialogue_line_override_dict[dialogue.id])
            {
                if (override_line.companionId == companionId)
                {
                    Debug.Log("Overriding dialogue " + dialogue.id + " with " + override_line.id);
                    Log.writeFull("Overriding dialogue " + dialogue.id + " with " + override_line.id);
                    override_line.overrideLine(dialogue);
                }
            }
        }




        current_dialogue = dialogue;

        if (dialogue.emoteResetEvents != null)
        {
            foreach (string emote_reset_event in dialogue.emoteResetEvents) {
                event_manager.main_event_player.event_stack.Add(emote_reset_event);
            }
        }

        if (dialogue.emoteEvents != null)
        {
            foreach (string emote_event in dialogue.emoteEvents)
            {
                event_manager.main_event_player.event_stack.Add(emote_event);
            }
        }

        if (dialogue.barkPlaylistIds != null)
        {
            if (dialogue.barkPredicates != null)
            {
                bool played = false;
                for (int i = 0; i < dialogue.barkPlaylistIds.Length; i++)
                {
                    if (i < dialogue.barkPredicates.Length)
                    {

                        if (Predicate.parsePredicate(dialogue.barkPredicates[i]))
                        {
                            if (Configs.playlist_dict.ContainsKey(dialogue.barkPlaylistIds[i]))
                            {
                                played = true;
                                Sound.playBark(dialogue.barkPlaylistIds[i]);
                            }
                        }
                    }
                    else
                    {
                        if (played == false)
                        {
                            if (Configs.playlist_dict.ContainsKey(dialogue.barkPlaylistIds[i]))
                            {
                                played = true;
                                Sound.playBark(dialogue.barkPlaylistIds[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Configs.playlist_dict.ContainsKey(dialogue.barkPlaylistIds[0]))
                {
                    Sound.playBark(dialogue.barkPlaylistIds[0]);
                }
            }
        }

        if (dialogue.lookAt != null)
        {
            for (int i = 0; i < dialogue.lookAt.Length; i++)
            {
                string[] action_params = dialogue.lookAt[i].Split(':');

                if (dialogue._headOnly != null && dialogue._headOnly.Length > i)
                {
                    if (dialogue._headOnly[i] == "true") {
                        Events.turnHeadAt(action_params);
                    }
                    else if (dialogue._headOnly[i] == "false")
                    {
                        Events.lookAt(action_params);
                    }
                    else
                    {
                        Debug.LogError("Unknown headOnly parameter in dialogue: " + dialogue._headOnly[i]);
                    }
                }
                else {
                    Events.lookAt(action_params);
                }
            }
        }

        if (dialogue.speakerId == "Avatar")
        {
            next_name_holder = "You";
        }
        else if (dialogue.speakerId != null)
        {
            next_name_holder = mapName(dialogue.speakerId);
        }
        if (dialogue.token != null)
        {
            if (!Configs.config_local_data.LocalData.ContainsKey(dialogue.token))
            {
                Debug.Log("Couldn't find dialogue " + dialogue.token);
                next_text_holder = "error";
            }
            else
            {
                string text;
                if (Configs.config_local_data.LocalData.ContainsKey(dialogue.token + "+female") && local_avatar_gender == "female")
                {
                    text = LocalData.getLine(dialogue.token + "+female");
                }
                else
                {
                    text = LocalData.getLine(dialogue.token);
                }
                next_text_holder = text;
            }
        }
        else
        {
            next_text_holder = null;
        }

        if (dialogue.dialogueChoiceIds == null)
        {
            if (dialogue.nextTurnIds != null)
            {
                //Check predicates
                if (dialogue.nextTurnPredicates != null)
                {
                    for (int p = 0; p < dialogue.nextTurnPredicates.Length; p++)
                    {
                        if (Predicate.parsePredicate(dialogue.nextTurnPredicates[p]))
                        {
                            next_dialogue = dialogue.nextTurnIds[p];
                            break;
                        }
                        else
                        {
                            if (dialogue.nextTurnIds.Length > p + 1)
                                next_dialogue = dialogue.nextTurnIds[p + 1];
                            else
                                next_dialogue = dialogue.nextTurnIds[p];
                        }
                    }
                }
                else
                {
                    next_dialogue = dialogue.nextTurnIds[0];
                }

            }
            else
            {
                next_dialogue = null;
            }

            waiting_for_dialogue = true;
        }
        else
        {
            choices = new List<string>();
            ui_dialogue_choice_1.SetActive(true);
            //if (!local_data_config.LocalData.ContainsKey(dialogue.dialogueChoiceIds[0]))
            if (!Configs.config_dialogue_choices.DialogueChoice.ContainsKey(dialogue.dialogueChoiceIds[0]))
            {
                Debug.Log("Could not find choice " + dialogue.dialogueChoiceIds[0]);
            }

            if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue.dialogueChoiceIds[0]))
            {
                foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue.dialogueChoiceIds[0]])
                {
                    if (override_choice.companionId == companionId)
                    {
                        Debug.Log("Overriding choice " + dialogue.dialogueChoiceIds[0] + " with " + override_choice.id);
                        Log.writeFull("Overriding choice " + dialogue.dialogueChoiceIds[0] + " with " + override_choice.id);
                        override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]]);
                    }
                }
            }

            if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue.dialogueChoiceIds[1]))
            {
                foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue.dialogueChoiceIds[1]])
                {
                    if (override_choice.companionId == companionId)
                    {
                        Debug.Log("Overriding choice " + dialogue.dialogueChoiceIds[1] + " with " + override_choice.id);
                        Log.writeFull("Overriding choice " + dialogue.dialogueChoiceIds[1] + " with " + override_choice.id);
                        override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]]);
                    }
                }
            }

            if (dialogue.dialogueChoiceIds.Length > 2)
            {
                if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue.dialogueChoiceIds[2]))
                {
                    foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue.dialogueChoiceIds[2]])
                    {
                        if (override_choice.companionId == companionId)
                        {
                            Debug.Log("Overriding choice " + dialogue.dialogueChoiceIds[2] + " with " + override_choice.id);
                            Log.writeFull("Overriding choice " + dialogue.dialogueChoiceIds[2] + " with " + override_choice.id);
                            override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]]);
                        }
                    }
                }
            }

            ui_dialogue_choice_1.GetComponentInChildren<Text>().text = LocalData.getLine(  Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]].choiceToken);
            if (dialogue.nextTurnIds != null)
                dialogue_choice_1_next_dialogue = dialogue.nextTurnIds[0];
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]].noQteTurnId != null)
                dialogue_choice_1_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]].noQteTurnId; //dates
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]].preQteTurnId != null)
                dialogue_choice_1_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[0]].successTurnId; //dates
            else
                throw new System.Exception("A");
            choices.Add(dialogue.dialogueChoiceIds[0]);

            ui_dialogue_choice_2.SetActive(true);
            ui_dialogue_choice_2.GetComponentInChildren<Text>().text = LocalData.getLine(Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]].choiceToken);
            if (dialogue.nextTurnIds != null)
                dialogue_choice_2_next_dialogue = dialogue.nextTurnIds[1];
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]].noQteTurnId != null)
                dialogue_choice_2_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]].noQteTurnId; //dates
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]].preQteTurnId != null)
                dialogue_choice_2_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[1]].successTurnId; //dates
            else
                throw new System.Exception("A");
            choices.Add(dialogue.dialogueChoiceIds[1]);

            if (dialogue.dialogueChoiceIds.Length > 2)
            {
                ui_dialogue_choice_3.SetActive(true);
                ui_dialogue_choice_3.GetComponentInChildren<Text>().text = LocalData.getLine(Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]].choiceToken);
                if (dialogue.nextTurnIds != null)
                    dialogue_choice_3_next_dialogue = dialogue.nextTurnIds[2];
                else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]].noQteTurnId != null)
                    dialogue_choice_3_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]].noQteTurnId; //dates
                else if (Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]].preQteTurnId != null)
                    dialogue_choice_3_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue.dialogueChoiceIds[2]].successTurnId; //dates
                else
                    throw new System.Exception("A");
                choices.Add(dialogue.dialogueChoiceIds[2]);
            }
            waiting_for_dialogue = false;
        }

        if (dialogue.enterEvents != null)
        {
            foreach (string enter_event in dialogue.enterEvents)
            {
                entry_stack.Add(enter_event);
            }
        }

        if (entry_stack.Count != 0)
        {
            event_manager.main_event_player.event_stack.AddRange(entry_stack);
            entry_stack.Clear();
            event_manager.main_event_player.runImmediateEvents();
        }


        if (dialogue.cameraShot != null)
        {
            camera_params = new string[] { dialogue.cameraShot, dialogue.cameraTransitionTime.ToString() };
        }
        else
        {
            camera_params = null;
        }

        if (event_manager.areEventsActive() == false)
        {
            dialogue_status = DialogueStatus.WaitingPlayerConfirm;
            if (camera_params != null)
            {
                CameraManager.current.focusCam(ref camera_params);
                camera_params = null;
            }
            start_dialogue_time = Time.realtimeSinceStartup;
            text_holder = next_text_holder;
            name_holder = next_name_holder;
            

        }

        if (dialogue.exitEvents != null)
        {
            foreach (string exit_event in dialogue.exitEvents)
            {
                exit_stack.Add(exit_event);
            }
        }
        ui_dialogue_name.text = "";
        ui_dialogue_text.text = "";


        
        if (dialogue.nextTurnIds == null && dialogue.dialogueChoiceIds == null)
        {
            Debug.Log("finished dialogue");
            return true; //Finished dialogue
        }
        else
        {
            return false;
        }
    }

    public string mapName(string speaker)
    {
        speaker = speaker.Replace("::Date::", Configs.config_companion.Companion[companionId].speakerId);
        foreach(ConfigDialogueSpeakerMapping._DialogueSpeakerMapping mapping in Configs.config_dialogue_speaker_mapping.DialogueSpeakerMapping)
        {
            if (mapping.mapId == speaker)
            {
                if (Predicate.parsePredicate(mapping.predicate))
                    speaker = mapping.speakerId;
            }
        }
        if (!Configs.config_dialogue_speakers.DialogueSpeaker.ContainsKey(speaker))
        {
            Debug.LogError("Could not find speaker from id " + speaker);
            return speaker;
        }

        string name = Configs.config_dialogue_speakers.DialogueSpeaker[speaker].name;
        return LocalData.getLine(name); 
    }


    public void Update()
    {
        switch (dialogue_status)
        {
            case DialogueStatus.WaitingEnterEvents:


                if (event_manager.areEventsActive() == false)
                {
                    dialogue_status = DialogueStatus.WaitingPlayerConfirm;
                    if (camera_params != null)
                    {
                        CameraManager.current.focusCam(ref camera_params);
                        camera_params = null;
                    }
                    start_dialogue_time = Time.realtimeSinceStartup;
                    if (next_text_holder != null)
                    {
                        ui_dialogue.SetActive(true);
                    }
                    else
                    {
                        ui_dialogue.SetActive(false);
                    }
                    text_holder = next_text_holder;
                    name_holder = next_name_holder;
                }
                break;
            case DialogueStatus.WaitingPlayerConfirm:
                if (ui_dialogue_choice_1.activeSelf)
                {
                    if (next_text_holder != null)
                    {
                        ui_dialogue.SetActive(true);
                    }
                    else
                    {
                        ui_dialogue.SetActive(false);
                    }
                    dialogue_status = DialogueStatus.WaitingPlayerOptionSelect;
                    break;
                }
                if (Input.GetKeyDown("space") || (current_dialogue.speakerId == null && current_dialogue.token == null))
                {
                    dialogue_status = DialogueStatus.WaitingExitEvents;
                    //ui_dialogue.SetActive(false);
                    foreach(string s in exit_stack)
                    {
                        event_manager.main_event_player.event_stack.Add(s);
                    }
                    exit_stack.Clear();
                    event_manager.main_event_player.runImmediateEvents();
                    if (event_manager.areEventsActive() == false)
                    {
                        if (next_dialogue != null)
                        {
                            activateDialogue(next_dialogue);
                            dialogue_status = DialogueStatus.WaitingEnterEvents;
                        }
                        else
                        {
                            InteractionManager.changeOptionInteractionsVisibility(true);
                            dialogue_status = DialogueStatus.Finished;
                            Debug.Log("Dialogue finished " + current_dialogue.dialogue);
                            CameraManager.current.freeCamera();
                            foreach (string character in Actor.actor_controllers.Keys)
                            {
                                Actor.actor_controllers[character].actor_head.clearLookat();
                                Actor.actor_controllers[character].actor_head.clearTurnHeadAt();
                            }
                        }
                    }
                }
                else
                {
                    ui_dialogue.SetActive(true);
                }
                break;
            case DialogueStatus.WaitingExitEvents:
                if (event_manager.areEventsActive() == false)
                {
                    if (next_dialogue != null)
                    {
                        activateDialogue(next_dialogue);
                        dialogue_status = DialogueStatus.WaitingEnterEvents;
                    }
                    else
                    {
                        dialogue_status = DialogueStatus.Finished;
                    }
                }
                break;
            case DialogueStatus.Finished:
                if (!in_bubble)
                {
                    ui_dialogue.SetActive(false);
                }
                if (current_dialogue != null)
                {
                    InteractionManager.changeOptionInteractionsVisibility(true);
                    onDialogueFinishedEvent?.Invoke(current_dialogue.dialogue);
                    CameraManager.current.freeCamera();

                    foreach (string character in Actor.actor_controllers.Keys)
                    {
                        Actor.actor_controllers[character].actor_head.clearLookat();
                        Actor.actor_controllers[character].actor_head.clearTurnHeadAt();
                    }
                    current_dialogue = null;
                }
                break;
        }

        if (ui_dialogue.activeSelf == true && text_holder != null)
        {
            ui_dialogue_name.text = name_holder;
            ui_dialogue_text.text = text_holder;

            int text_holder_index = Mathf.Min((int)((Time.realtimeSinceStartup - start_dialogue_time) / letter_seperator), ui_dialogue_text.textInfo.characterCount);

            ui_dialogue_text.maxVisibleCharacters = text_holder_index;

        }
        else
        {
            ui_dialogue.SetActive(false);
            ui_dialogue_name.text = "";
            ui_dialogue_text.text = "";
        }

    }

    public void Awake()
    {
        dialogue_status = DialogueStatus.Finished;
        madeChoice = new List<string>();
    }
}