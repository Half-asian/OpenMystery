using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;



public class EncounterSocial : Encounter
{
    private static EncounterSocial current;

    private ConfigEncounterOpposition._EncounterOpposition config_opposition;

    private ConfigCompanion._Companion config_companion;

    private string current_mood = "negative";

    public EncounterSocial(ConfigEncounter._Encounter _encounter) {

        Debug.Log("Starting Encounter Social");
        Assert.IsNotNull(_encounter, "EncounterDate() encounter cannot be null");
        config_encounter = _encounter;
        current = this;

        if (!Configs.config_companion.Companion.ContainsKey(config_encounter.companionId))
        {
            Debug.LogError("Could not find companion " + config_encounter.companionId);
            return;
        }
        if (!Configs.config_encounter_opposition.EncounterOpposition.ContainsKey(config_encounter.oppositionId)){
            Debug.LogError("Could not find encounter opposition " + config_encounter.oppositionId);
            return;
        }

        activate();
    }

    public override void activate()
    {
        config_companion = Configs.config_companion.Companion[config_encounter.companionId];
        if (!Actor.actor_controllers.ContainsKey(config_encounter.companionId))
            Actor.spawnActor(config_companion.actorId, config_encounter.opponentWaypoint, config_encounter.companionId);
        config_opposition = Configs.config_encounter_opposition.EncounterOpposition[config_encounter.oppositionId];

        base.activate();
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

        DialogueManager.onDialogueFinishedEventSecondary += onDialogueMoodFinished;
        GameStart.dialogue_manager.activateDialogue(config_opposition.negativeIntroDialogue);

        //EventManager.all_script_events_finished_event += finished;

    }

    private void onDialogueMoodFinished(string dialogue_id)
    {
        switch (current_mood)
        {
            case "negative":
                current_mood = "neutral";
                GameStart.dialogue_manager.activateDialogue(config_opposition.neutralIntroDialogue);
                break;
            case "neutral":
                current_mood = "positive";
                GameStart.dialogue_manager.activateDialogue(config_opposition.positiveIntroDialogue);
                break;
            case "positive":
                DialogueManager.onDialogueFinishedEventSecondary -= onDialogueMoodFinished;
                finishedSuccesfully = false;
                finishedMainEncounter();
                break;
        }
    }




    
}
