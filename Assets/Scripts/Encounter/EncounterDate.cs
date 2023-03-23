using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;



public class EncounterDate : Encounter
{
    public static string companion;
    private static EncounterDate current;
    private ConfigDatePrompt._DatePrompt date_prompt;
    public static event Action<bool> toggleCompanionCanvas = delegate { };

    int date_prompt_counter = 0;
    public EncounterDate(ConfigEncounter._Encounter _encounter) {

        Assert.IsNotNull(_encounter, "EncounterDate() encounter cannot be null");
        config_encounter = _encounter;
        current = this;
        finishedSuccesfully = true;
        showCompanionCanvas();
    }

    private void showCompanionCanvas()
    {
        toggleCompanionCanvas.Invoke(true);
    }

    public static void setCompanion(string _companion)
    {
        companion = _companion;

        if (!Configs.config_companion.Companion.ContainsKey(companion))
        {
            Debug.LogError("Could not find companion " + companion);
            return;
        }

        toggleCompanionCanvas.Invoke(false);
        current.activate();
    }

    public override void activate()
    {
        base.activate();

        ConfigCompanion._Companion _companion = Configs.config_companion.Companion[companion];
        string companion_id;
        if (_companion.specialActorIds != null)
            companion_id = _companion.specialActorIds["date"];
        else
            companion_id = _companion.actorId;


        date_prompt = Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[0]];

        Actor.getActor(Player.local_avatar_onscreen_name)?.teleportCharacter(date_prompt.avatarSpawn);

        Actor.spawnActor(companion_id, date_prompt.dateSpawn, "opponent");

    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

        DialogueManager.onDialogueFinishedEventPrimary += dialogueCallback;

        GameStart.dialogue_manager.activateDialogue(date_prompt.dialogue);
    }

    public void dialogueCallback(string dialogue)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= dialogueCallback;

        if (dialogue == Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[date_prompt_counter]].dialogue)
        {
            DialogueManager.onDialogueFinishedEventPrimary -= dialogueCallback;
            Debug.Log("Encounter Dialogue Callback");
            date_prompt_counter++;
            if (date_prompt_counter >= config_encounter.datePromptIds.Length)
                finishedMainEncounter();
            else
            {
                ConfigDatePrompt._DatePrompt date_prompt = Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[date_prompt_counter]];


                Actor.getActor(Player.local_avatar_onscreen_name)?.teleportCharacter(date_prompt.avatarSpawn);

                Actor.getActor("opponent")?.teleportCharacter(date_prompt.dateSpawn);

                DialogueManager.onDialogueFinishedEventPrimary += dialogueCallback;
                
                GameStart.dialogue_manager.activateDialogue(date_prompt.dialogue);
            }
        }
    }

}
