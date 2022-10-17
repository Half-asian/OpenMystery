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

        DialogueManager.onDialogueFinishedEventPrimary += onDialogueFinished;
        GameStart.dialogue_manager.activateDialogue(config_opposition.negativeIntroDialogue);

        //EventManager.all_script_events_finished_event += finished;

    }

    private void onDialogueFinished(string dialogue_id)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= onDialogueFinished;

        string mood_id = "";
        switch (current_mood)
        {
            case "negative":
                mood_id = config_opposition.negativeMoods[0][0];
                break;
            case "neutral":
                mood_id = config_opposition.neutralMoods[0][0];
                break;
            case "positive":
                mood_id = config_opposition.positiveMoods[0][0];
                break;
        }
        var config_mood = Configs.config_encounter_mood.EncounterMood[mood_id];

        var answer_pool = new List<string>(config_encounter.playerChoices);

        //Lets do only index 0 for now
        string question_id = config_mood.choices[0][0];

        var config_question = Configs.config_encounter_choice.EncounterChoice[question_id];

        foreach(var choice in config_question.playerChoiceBlacklist){
            answer_pool.Remove(choice);
        }

        var correct_pool = new List<string>() { answer_pool[0] };
        var okay_pool = new List<string>() { answer_pool[1] };
        var incorrect_pool = new List<string>() { answer_pool[2] };


        SocialQuizUI.onSocialQuizGameFinished += onQuizQuestionFinished;
        SocialQuizUI.startSocialQuiz(config_question.choiceId, correct_pool, okay_pool, incorrect_pool);


    }

    private void onQuizQuestionFinished(bool succeeded)
    {
        SocialQuizUI.onSocialQuizGameFinished -= onQuizQuestionFinished;

        switch (current_mood)
        {
            case "negative":
                current_mood = "neutral";
                DialogueManager.onDialogueFinishedEventPrimary += onDialogueFinished;
                GameStart.dialogue_manager.activateDialogue(config_opposition.neutralIntroDialogue);
                break;
            case "neutral":
                current_mood = "positive";
                DialogueManager.onDialogueFinishedEventPrimary += onDialogueFinished;
                GameStart.dialogue_manager.activateDialogue(config_opposition.positiveIntroDialogue);
                break;
            case "positive":
                finishedSuccesfully = true;
                finishedMainEncounter();
                break;
        }
    }




    
}
