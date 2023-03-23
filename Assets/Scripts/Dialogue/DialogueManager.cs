using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public static event Action<bool> setDialogueUIActive = delegate { };
    public static event Action<string> setDialogueText = delegate { };
    public static event Action<string> setNameText = delegate { };
    public static event Action<string> setChoice1ActiveWithText = delegate { };
    public static event Action<string> setChoice2ActiveWithText = delegate { };
    public static event Action<string> setChoice3ActiveWithText = delegate { };

    public static event Action onDialogueStartedEvent = delegate { };
    public static event Action<string> onDialogueFinishedEventPrimary = delegate { };
    public static event Action<string> onDialogueFinishedEventSecondary = delegate { };

    public static bool in_dialogue = false;

    public string dialogue_id;

    public int next_choice_index = -1;
    public string dialogue_choice_1_next_dialogue;
    public string dialogue_choice_2_next_dialogue;
    public string dialogue_choice_3_next_dialogue;

    public List<string> choices;
    public ConfigHPDialogueLine.HPDialogueLine current_dialogue_line;

    public bool in_bubble;

    public List<string> exit_stack = new List<string>();

    public string text_holder;
    public string name_holder;


    public DialogueStatus dialogue_status = DialogueStatus.Finished;

    public string[] camera_params;

    public void Awake()
    {
        dialogue_status = DialogueStatus.Finished;
        EventManager.all_script_events_finished_event += onScriptEventsFinished;
        GameStart.onReturnToMenu += cleanup;
    }

    void cleanup()
    {
        in_dialogue = false;
        dialogue_status = DialogueStatus.Finished;
        setDialogueUIActive(false);
    }

    public void showBubbleDialogue(string speaker, string dialogue)
    {
        in_dialogue = true;
        speaker = Speaker.mapSpeakerName(speaker);
        setDialogueUIActive.Invoke(true);
        setDialogueText.Invoke(LocalData.getLine(dialogue));
        setNameText.Invoke(speaker); 
    }

    public void finishBubbleDialogue()
    {
        in_dialogue = false;
        in_bubble = false;
        setDialogueUIActive.Invoke(false);
    }

    //When we only know the dialogue id
    public bool activateDialogue(string dialogue)
    {
        if (Configs.dialogue_dict.ContainsKey(VariantManager.getVariantForId(dialogue)))
            dialogue = VariantManager.getVariantForId(dialogue);

        in_dialogue = true;

        dialogue_id = dialogue;

        string initial_dialogue_line = null;

        if (!Configs.dialogue_dict.ContainsKey(dialogue))
        {
            Debug.LogError("DialogueManager:activateNewDialogue - Config empty for dialogue " + dialogue + " in HPDialogueLines");
            return false;
        }

        foreach (ConfigHPDialogueLine.HPDialogueLine i in Configs.dialogue_dict[dialogue])
        {
            if (i.initialTurn == true)
            {
                initial_dialogue_line = i.id;
                break; //There can be multiple. Take the first one.
            }
        }
        if (initial_dialogue_line is null) {
            foreach (ConfigHPDialogueLine.HPDialogueLine i in Configs.dialogue_dict[dialogue])
            {
                if (i.id == dialogue + "1")
                {
                    initial_dialogue_line = i.id;
                    break;
                }

                if (i.id.ToLower().EndsWith("line1"))
                {//This is our second best bet 
                    initial_dialogue_line = i.id;
                    break;
                }
            }
        }
        if (initial_dialogue_line is null)
            initial_dialogue_line = Configs.dialogue_dict[dialogue][0].id; //No initial turn? Thats fine. Take the first match.

        return activateDialogueLine(initial_dialogue_line);
    }

    private bool activateDialogueLine(string dialogue_name)
    {
        in_dialogue = true;

        onDialogueStartedEvent.Invoke();
        in_bubble = false;

        if (String.IsNullOrEmpty(dialogue_name))
        {
            Debug.LogError("activateDialogue name was null");
            return true;
        }
        
        Debug.Log("activated dialogue " + dialogue_name);
        
        dialogue_status = DialogueStatus.WaitingEnterEvents;

        ConfigHPDialogueLine.HPDialogueLine dialogue_line = Configs.config_hp_dialogue_line.HPDialogueLines[dialogue_name];

        if (Configs.dialogue_line_override_dict.ContainsKey(dialogue_line.id))
        {
            foreach(ConfigHPDialogueOverride._HPDialogueOverride override_line in Configs.dialogue_line_override_dict[dialogue_line.id])
            {
                if (override_line.companionId == EncounterDate.companion)
                {
                    Debug.Log("Overriding dialogue " + dialogue_line.id + " with " + override_line.id);
                    override_line.overrideLine(dialogue_line);
                }
            }
        }

        current_dialogue_line = dialogue_line;

        dialogueLineStartActions(dialogue_line);

        string name = Speaker.mapSpeakerName(dialogue_line.speakerId);
        string text = "";



        if (dialogue_line.token != null)
        {
            if (!Configs.config_local_data.LocalData.ContainsKey(dialogue_line.token))
            {
                if (dialogue_line.token.EndsWith("_gendered"))
                {
                    dialogue_line.token = dialogue_line.token.Substring(0, dialogue_line.token.Length - 8);
                    dialogue_line.token += Player.local_avatar_gender;
                    if (!Configs.config_local_data.LocalData.ContainsKey(dialogue_line.token))
                    {
                        Debug.LogError("Couldn't find dialogue " + dialogue_line.token);
                        text = dialogue_line.token;
                    }
                    else
                    {
                        text = LocalData.getLine(dialogue_line.token);
                    }
                }
                else
                {
                    Debug.LogError("Couldn't find dialogue " + dialogue_line.token);
                    text = dialogue_line.token;
                }
            }
            else
            {
                text =
                    (Configs.config_local_data.LocalData.ContainsKey(dialogue_line.token + "+female")
                    && Player.local_avatar_gender == "female") ?
                    LocalData.getLine(dialogue_line.token + "+female") :
                    LocalData.getLine(dialogue_line.token);
            }
        }

        setDialogueUIActive(false);

        if (camera_params != null)
        {
            Debug.Log("Setting camera " + camera_params[0]);
            CameraManager.current.focusCam(ref camera_params);
            camera_params = null;
        }

        if (dialogue_line.enterEvents != null)
        {
            Debug.Log("Adding enter events to stack from " + dialogue_line.id);
            List<string> entry_stack = new List<string>();
            entry_stack.AddRange(dialogue_line.enterEvents);
            GameStart.event_manager.main_event_player.addEvents(entry_stack);
            GameStart.event_manager.main_event_player.runImmediateEvents();
        }

        if (dialogue_line.exitEvents != null)
            exit_stack.AddRange(dialogue_line.exitEvents);

        setDialogueText.Invoke(text);
        setNameText.Invoke(name);


        if (dialogue_line.nextTurnIds == null && dialogue_line.dialogueChoiceIds == null)
        {
            in_dialogue = false;
            Debug.Log("finished dialogue");
            return true; //Finished dialogue
        }
        else
        {
            return false;
        }
    }

    void dialogueLineStartActions(ConfigHPDialogueLine.HPDialogueLine dialogue_line)
    {
        if (dialogue_line.emoteResetEvents != null)
            GameStart.event_manager.main_event_player.addEvents(dialogue_line.emoteResetEvents);

        if (dialogue_line.emoteEvents != null)
            GameStart.event_manager.main_event_player.addEvents(dialogue_line.emoteEvents);




        if (dialogue_line.barkPlaylistIds != null)
        {
            for (int i = 0; i < dialogue_line.barkPlaylistIds.Length; i++)
            {
                if (dialogue_line.barkPredicates != null)
                {
                    if (dialogue_line.barkPredicates.Length > i)
                    {
                        if (Predicate.parsePredicate(dialogue_line.barkPredicates[i]))
                        {
                            Sound.playBark(dialogue_line.barkPlaylistIds[i]);
                            break;
                        }
                    }
                    else
                    {
                        Sound.playBark(dialogue_line.barkPlaylistIds[i]);
                        break;
                    }
                }
                else
                {
                    Sound.playBark(dialogue_line.barkPlaylistIds[i]);
                    break;
                }
            }
        }

        if (dialogue_line.lookAt != null)
        {
            for (int i = 0; i < dialogue_line.lookAt.Length; i++)
            {
                string[] action_params = dialogue_line.lookAt[i].Split(':');

                if (dialogue_line._headOnly != null && dialogue_line._headOnly.Length > i)
                {
                    if (dialogue_line._headOnly[i] == "true")
                        Events.turnHeadAt(action_params);
                    else if (dialogue_line._headOnly[i] == "false")
                        Events.lookAt(action_params);
                    else
                        Debug.LogError("Unknown headOnly parameter in dialogue: " + dialogue_line._headOnly[i]);
                }
                else
                    Events.lookAt(action_params);
            }
        }
        if (dialogue_line.cameraShot != null)
        {
            camera_params = new string[] { dialogue_line.cameraShot, dialogue_line.CameraTransitionTime.ToString() };
        }
        else
        {
            camera_params = null;
        }
    }

    void setDialogueLineChoices(ConfigHPDialogueLine.HPDialogueLine dialogue_line)
    {
        choices = new List<string>();
        if (!Configs.config_dialogue_choices.DialogueChoice.ContainsKey(dialogue_line.dialogueChoiceIds[0]))
        {
            Debug.Log("Could not find choice " + dialogue_line.dialogueChoiceIds[0]);
        }

        if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue_line.dialogueChoiceIds[0]))
        {
            foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue_line.dialogueChoiceIds[0]])
            {
                if (override_choice.companionId == EncounterDate.companion)
                {
                    Debug.Log("Overriding choice " + dialogue_line.dialogueChoiceIds[0] + " with " + override_choice.id);
                    override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]]);
                }
            }
        }

        if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue_line.dialogueChoiceIds[1]))
        {
            foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue_line.dialogueChoiceIds[1]])
            {
                if (override_choice.companionId == EncounterDate.companion)
                {
                    Debug.Log("Overriding choice " + dialogue_line.dialogueChoiceIds[1] + " with " + override_choice.id);
                    override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]]);
                }
            }
        }

        if (dialogue_line.dialogueChoiceIds.Length > 2)
        {
            if (Configs.dialogue_choice_override_dict.ContainsKey(dialogue_line.dialogueChoiceIds[2]))
            {
                foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride override_choice in Configs.dialogue_choice_override_dict[dialogue_line.dialogueChoiceIds[2]])
                {
                    if (override_choice.companionId == EncounterDate.companion)
                    {
                        Debug.Log("Overriding choice " + dialogue_line.dialogueChoiceIds[2] + " with " + override_choice.id);
                        override_choice.overrideChoice(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]]);
                    }
                }
            }
        }

        setChoice1ActiveWithText.Invoke(LocalData.getLine(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]].choiceToken));
        if (dialogue_line.nextTurnIds != null)
            dialogue_choice_1_next_dialogue = dialogue_line.nextTurnIds[0];
        else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]].noQteTurnId != null)
            dialogue_choice_1_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]].noQteTurnId; //dates
        else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]].preQteTurnId != null)
            dialogue_choice_1_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[0]].successTurnId; //dates
        else
            throw new System.Exception("A");
        choices.Add(dialogue_line.dialogueChoiceIds[0]);

        setChoice2ActiveWithText.Invoke(LocalData.getLine(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]].choiceToken));
        if (dialogue_line.nextTurnIds != null)
            dialogue_choice_2_next_dialogue = dialogue_line.nextTurnIds[1];
        else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]].noQteTurnId != null)
            dialogue_choice_2_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]].noQteTurnId; //dates
        else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]].preQteTurnId != null)
            dialogue_choice_2_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[1]].successTurnId; //dates
        else
            throw new System.Exception("A");
        choices.Add(dialogue_line.dialogueChoiceIds[1]);

        if (dialogue_line.dialogueChoiceIds.Length > 2)
        {
            setChoice3ActiveWithText.Invoke(LocalData.getLine(Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]].choiceToken));
            if (dialogue_line.nextTurnIds != null)
                dialogue_choice_3_next_dialogue = dialogue_line.nextTurnIds[2];
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]].noQteTurnId != null)
                dialogue_choice_3_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]].noQteTurnId; //dates
            else if (Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]].preQteTurnId != null)
                dialogue_choice_3_next_dialogue = Configs.config_dialogue_choices.DialogueChoice[dialogue_line.dialogueChoiceIds[2]].successTurnId; //dates
            else
                throw new System.Exception("A");
            choices.Add(dialogue_line.dialogueChoiceIds[2]);
        }
    }




    public void activateDialogueOption1()
    {
        next_choice_index = 1;
        GameStart.event_manager.main_event_player.addEvents(exit_stack);
        exit_stack.Clear();
        setDialogueUIActive(false);
        dialogue_status = DialogueStatus.WaitingExitEvents;
        GameStart.event_manager.main_event_player.runImmediateEvents();
    }
    public void activateDialogueOption2()
    {
        next_choice_index = 2;
        GameStart.event_manager.main_event_player.addEvents(exit_stack);
        exit_stack.Clear();
        setDialogueUIActive(false);
        dialogue_status = DialogueStatus.WaitingExitEvents;
        GameStart.event_manager.main_event_player.runImmediateEvents();
    }
    public void activateDialogueOption3()
    {
        next_choice_index = 3;
        GameStart.event_manager.main_event_player.addEvents(exit_stack);
        exit_stack.Clear();
        setDialogueUIActive(false);
        dialogue_status = DialogueStatus.WaitingExitEvents;
        GameStart.event_manager.main_event_player.runImmediateEvents();
    }
    public void Update()
    {
        if (dialogue_status == DialogueStatus.WaitingPlayerConfirm)
        {
            if (Input.GetKeyDown("space") || (current_dialogue_line.speakerId == null && current_dialogue_line.token == null))
            {
                onPlayerConfirmed();
            }
        }
    }

    public void onScriptEventsFinished()
    {
        switch (dialogue_status)
        {
            case DialogueStatus.WaitingEnterEvents:
                onEntryEventsFinished();
                break;
            case DialogueStatus.WaitingExitEvents:
                onExitEventsFinished();
                break;
        }
    }

    void onEntryEventsFinished()
    {
        dialogue_status = DialogueStatus.WaitingPlayerConfirm;

        if (current_dialogue_line.dialogueChoiceIds is not null)
        {
            setDialogueLineChoices(current_dialogue_line);
            dialogue_status = DialogueStatus.WaitingPlayerOptionSelect;
        }

        setDialogueUIActive.Invoke(true);
    }

    void onPlayerConfirmed()
    {
        if (DialogueUI.finished_showing_text == false)
        {
            DialogueUI.finishShowingText();
            return;
        }

        GameStart.event_manager.main_event_player.addEvents(exit_stack);
        exit_stack.Clear();
        setDialogueUIActive(false);
        dialogue_status = DialogueStatus.WaitingExitEvents;
        GameStart.event_manager.main_event_player.runImmediateEvents();
    }

    void onExitEventsFinished()
    {
        string next_turn_id = null;
        if (current_dialogue_line.dialogueChoiceIds != null)
        {
            switch (next_choice_index)
            {
                case 1:
                    next_turn_id =dialogue_choice_1_next_dialogue;
                    break;
                case 2:
                    next_turn_id = dialogue_choice_2_next_dialogue;
                    break;
                case 3:
                    next_turn_id = dialogue_choice_3_next_dialogue;
                    break;
                default:
                    throw new System.Exception("I fucked up the choice system");
            }
            next_choice_index = -1;
        }

        else {
            if (current_dialogue_line.nextTurnIds != null)
            {
                //Check predicates
                if (current_dialogue_line.nextTurnPredicates != null)
                {
                    for (int p = 0; p < current_dialogue_line.nextTurnPredicates.Length; p++)
                    {
                        if (Predicate.parsePredicate(current_dialogue_line.nextTurnPredicates[p]))
                        {
                            try {
                                next_turn_id = current_dialogue_line.nextTurnIds[p];
                            }
                            catch {
                                Debug.LogError("Tried accessing index " + p + " of array with " + current_dialogue_line.nextTurnIds.Length + " elements.");
                            }
                            break;
                        }
                        else
                        {
                            if (current_dialogue_line.nextTurnIds.Length > p + 1)
                                next_turn_id = current_dialogue_line.nextTurnIds[p + 1];
                            else
                                next_turn_id = current_dialogue_line.nextTurnIds[p];
                        }
                    }
                }
                else
                    next_turn_id = current_dialogue_line.nextTurnIds[0];
            }
        }

        if (next_turn_id != null)
        {
            dialogue_status = DialogueStatus.WaitingEnterEvents;
            activateDialogueLine(next_turn_id);
        }
        else
        {
            dialogue_status = DialogueStatus.Finished;
            onDialogueFinished();
        }
    }
    
    void onDialogueFinished()
    {
        in_dialogue = false;

        Debug.Log("Dialogue finished " + dialogue_id);

        setDialogueUIActive(false);
        current_dialogue_line = null;

        onDialogueFinishedEventPrimary?.Invoke(dialogue_id); //For things that need to finish first, like interactions
        onDialogueFinishedEventSecondary?.Invoke(dialogue_id); //For things that need to finish last, like objectives
    }
}
