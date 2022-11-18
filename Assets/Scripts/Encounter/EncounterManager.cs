using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public class EncounterManager : MonoBehaviour
{
    public static event Action<string, bool> onEncounterFinished = delegate { };
    public List<Encounter> active_encounters = new List<Encounter>();

    public void cleanup()
    {
        onEncounterFinished = delegate { };
        foreach(var encounter in active_encounters)
        {
            encounter.cleanup();
        }
        active_encounters.Clear();
    }

    public void activateEncounter(string encounter_name)
    {
        Debug.Log("Activating Encounter: " + encounter_name);

        if (!Configs.config_encounter.Encounter.ContainsKey(encounter_name))
        {
            throw new System.Exception("Activate encounter - Invalid encounter " + encounter_name);
        }

        ConfigEncounter._Encounter new_encounter = Configs.config_encounter.Encounter[encounter_name];

        //Autopass this for now
        /*if (Configs.config_interaction.Interactions[encounter_name].filterPredicate != null)
        {
            if (!Predicate.parsePredicate(Configs.config_interaction.Interactions[encounter_name].filterPredicate))
            {
                Debug.Log("Failed the predicate: " + Configs.config_interaction.Interactions[encounter_name].filterPredicate);
                return null;
            }
        }*/

        Encounter result_encounter = null;

        Assert.IsNotNull(new_encounter);

        switch (new_encounter.type)
        {
            case "Date":
                result_encounter = new EncounterDate(new_encounter);
                break;
            case "Social":
                result_encounter = new EncounterSocial(new_encounter);
                break;
            default:
                Debug.LogWarning("Unknown interaction type " + new_encounter.type);
                return;
        }

        active_encounters.Add(result_encounter);
    }

    public static void onEncounterComplete(string encounter_id, bool succeeded)
    {
        onEncounterFinished.Invoke(encounter_id, succeeded);
    }
}

public abstract class Encounter
{
    public ConfigEncounter._Encounter config_encounter { get; protected set; }
    protected bool finishedSuccesfully = false;
    public virtual void activate()
    { 
        Debug.Log("Activating encounter " + config_encounter.encounterId);

        if (config_encounter.enterEvents != null)
        {
            GameStart.event_manager.main_event_player.addEvents(config_encounter.enterEvents);
        }
        EventManager.all_script_events_finished_event += onFinishedEnterEvents;
    }
    public virtual void onFinishedEnterEvents()
    {
        EventManager.all_script_events_finished_event -= onFinishedEnterEvents;
    }

    protected virtual void finishedMainEncounter()
    {
        if (config_encounter.successDialogue != null && finishedSuccesfully)
        {
            DialogueManager.onDialogueFinishedEventSecondary += finishedDialogueCallback;
            GameStart.dialogue_manager.activateDialogue(config_encounter.successDialogue);
        }
        else if (config_encounter.failDialogue != null && !finishedSuccesfully)
        {
            DialogueManager.onDialogueFinishedEventSecondary += finishedDialogueCallback;
            GameStart.dialogue_manager.activateDialogue(config_encounter.failDialogue);
        }
        else
        {
            finishedDialogueCallback(null);
        }
    }

    protected virtual void finishedDialogueCallback(string dialogue_id)
    {            

        DialogueManager.onDialogueFinishedEventSecondary -= finishedDialogueCallback;
        EventManager.all_script_events_finished_event += encounterComplete;
        if (config_encounter.exitEvents != null)
        {
            GameStart.event_manager.main_event_player.addEvents(config_encounter.exitEvents);
        }
        if (config_encounter.successEvents != null && finishedSuccesfully)
        {
            GameStart.event_manager.main_event_player.addEvents(config_encounter.successEvents);
        }
        if (config_encounter.failEvents != null && !finishedSuccesfully)
        {
            GameStart.event_manager.main_event_player.addEvents(config_encounter.failEvents);
        }
    }

    public virtual void encounterComplete()
    {
        cleanup();
        EncounterManager.onEncounterComplete(config_encounter.encounterId, finishedSuccesfully);
    }

    public virtual void cleanup()
    {
        EventManager.all_script_events_finished_event -= encounterComplete;
        DialogueManager.onDialogueFinishedEventSecondary -= finishedDialogueCallback;
    }
}
