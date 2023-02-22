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

    private int question_index = 0; //a question index that's reset on mood change.

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
        base.activate();

        config_companion = Configs.config_companion.Companion[config_encounter.companionId];
        if (!Actor.actor_controllers.ContainsKey(config_encounter.companionId))
            Actor.spawnActor(config_companion.actorId, config_encounter.opponentWaypoint, config_encounter.companionId);
        else
            Actor.actor_controllers[config_encounter.companionId].teleportCharacter(config_encounter.opponentWaypoint);
        config_opposition = Configs.config_encounter_opposition.EncounterOpposition[config_encounter.oppositionId];
        Actor.actor_controllers["opponent"] = Actor.actor_controllers[config_encounter.companionId];
        Actor.actor_controllers.Remove(config_encounter.companionId); //The original actor becomes opponent.

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
        startQuestion();
    }

    private void startQuestion()
    {
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

        string question_id = config_mood.choices[question_index][0];

        var config_question = Configs.config_encounter_choice.EncounterChoice[question_id];

        if (config_question.playerChoiceWhitelist != null)
        {
            answer_pool.Clear();
            foreach(var choice in config_question.playerChoiceWhitelist)
            {
                answer_pool.Add(choice);
            }
        }
        if (config_question.playerChoiceBlacklist != null)
        {
            foreach (var choice in config_question.playerChoiceBlacklist)
            {
                answer_pool.Remove(choice);
            }
        }
        foreach(var choice in answer_pool)
        {
            Debug.Log("Adding choice: " + choice + " to answer pool");
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
        if (!succeeded)
        {
            finishedSuccesfully = false;
            finish();
            return;
        }

        question_index++;

        switch (current_mood)
        {
            case "negative":
                if (question_index > config_opposition.negativeMoods.Length)
                {
                    question_index = 0;
                    current_mood = "neutral";
                    DialogueManager.onDialogueFinishedEventPrimary += onDialogueFinished;
                    GameStart.dialogue_manager.activateDialogue(config_opposition.neutralIntroDialogue);
                }
                else
                {
                    startQuestion();
                }
                break;
            case "neutral":
                if (question_index > config_opposition.negativeMoods.Length)
                {
                    question_index = 0;
                    current_mood = "positive";
                    DialogueManager.onDialogueFinishedEventPrimary += onDialogueFinished;
                    GameStart.dialogue_manager.activateDialogue(config_opposition.positiveIntroDialogue);
                }
                else
                {
                    startQuestion();
                }
                break;
            case "positive":
                if (question_index > config_opposition.positiveMoods.Length) {
                    finishedSuccesfully = true;
                    finish();
                }
                else
                {
                    startQuestion();
                }
                break;
        }
    }


    private void finish()
    {
        SocialQuizUI.onSocialQuizGameFinished -= onQuizQuestionFinished;

        finishedMainEncounter();
    }

    public override void cleanup()
    {
        Actor.actor_controllers[config_encounter.companionId] = Actor.actor_controllers["opponent"]; //We put it back how it was originally.
        Actor.actor_controllers.Remove("opponent");

        SocialQuizUI.onSocialQuizGameFinished -= onQuizQuestionFinished;

        base.cleanup();
    }



}
