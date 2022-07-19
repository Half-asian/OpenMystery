using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public abstract class Encounter
{
    public ConfigEncounter._Encounter config_encounter { get; protected set; }
    public abstract void activate();
}

public class EncounterDate : Encounter
{
    public static string companion;
    private static EncounterDate current; 

    public static event Action<bool> toggleCompanionCanvas = delegate { };

    int date_prompt_counter = 0;
    public EncounterDate(ConfigEncounter._Encounter _encounter) {

        Assert.IsNotNull(_encounter, "EncounterDate() encounter cannot be null");
        config_encounter = _encounter;
        current = this;
        showCompanionCanvas();
    }

    private void showCompanionCanvas()
    {
        toggleCompanionCanvas.Invoke(true);
    }

    public static void setCompanion(string companion)
    {
        EncounterDate.companion = companion;

        if (!Configs.config_companion.Companion.ContainsKey(EncounterDate.companion))
        {
            Debug.LogError("Could not find companion " + EncounterDate.companion);
            return;
        }

        toggleCompanionCanvas.Invoke(false);
        current.activate();
    }

    public override void activate()
    {
        ConfigCompanion._Companion companion = Configs.config_companion.Companion[EncounterDate.companion];
        string companion_id = null;
        if (companion.specialActorIds != null)
            companion_id = companion.specialActorIds["date"];
        else
            companion_id = companion.actorId;

        //GameStart.dialogue_manager.activateDialogue((string)encounter.conditionalIntroDialogs[0][1]);

        ConfigDatePrompt._DatePrompt date_prompt = Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[0]];

        ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[date_prompt.avatarSpawn];

        Vector3 position = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (waypoint.position != null)
            position = new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]);
        if (waypoint.rotation != null)
            rotation = new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]);


        Actor.actor_controllers["Avatar"].actor_movement.teleportCharacter(position, rotation);

        Actor.spawnActor(companion_id, date_prompt.dateSpawn, "opponent");

        DialogueManager.onDialogueFinishedEvent += dialogueCallback;

        GameStart.dialogue_manager.activateDialogue(date_prompt.dialogue);
        
    }
    public void dialogueCallback(string dialogue)
    {
        DialogueManager.onDialogueFinishedEvent -= dialogueCallback;

        if (dialogue == Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[date_prompt_counter]].dialogue)
        {
            DialogueManager.onDialogueFinishedEvent -= dialogueCallback;
            Debug.Log("Encounter Dialogue Callback");
            date_prompt_counter++;
            if (date_prompt_counter >= config_encounter.datePromptIds.Length)
                finished();
            else
            {
                ConfigDatePrompt._DatePrompt date_prompt = Configs.config_date_prompt.DatePrompt[config_encounter.datePromptIds[date_prompt_counter]];

                ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[date_prompt.avatarSpawn];

                Vector3 position = Vector3.zero;
                Vector3 rotation = Vector3.zero;

                if (waypoint.position != null)
                    position = new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]);
                if (waypoint.rotation != null)
                    rotation = new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]);

                Actor.actor_controllers["Avatar"].actor_movement.teleportCharacter(position, rotation);

                waypoint = Scene.current.waypoint_dict[date_prompt.dateSpawn];

                position = Vector3.zero;
                rotation = Vector3.zero;

                if (waypoint.position != null)
                    position = new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]);
                if (waypoint.rotation != null)
                    rotation = new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]);

                Actor.actor_controllers["opponent"].actor_movement.teleportCharacter(position, rotation);

                DialogueManager.onDialogueFinishedEvent += dialogueCallback;
                
                GameStart.dialogue_manager.activateDialogue(date_prompt.dialogue);
            }
        }
    }

    void finished()
    {
        EncounterManager.onEncounterComplete(config_encounter.encounterId);
    }
}
