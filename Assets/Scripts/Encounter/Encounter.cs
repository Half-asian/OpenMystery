using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public abstract class Encounter
{
    public ConfigEncounter._Encounter encounter { get; protected set; }
    public abstract void activate();
}

public class EncounterDate : Encounter, DialogueCallback
{
    int date_prompt_counter = 0;
    public EncounterDate(ConfigEncounter._Encounter _encounter) {

        Assert.IsNotNull(_encounter, "EncounterDate() encounter cannot be null");
        encounter = _encounter;
        activate();
    }
    public override void activate()
    {
        if (!Configs.config_companion.Companion.ContainsKey(Player.companionId))
        {
            throw new System.Exception("Could not find companion " + Player.companionId);
        }

        ConfigCompanion._Companion companion = Configs.config_companion.Companion[Player.companionId];
        string companion_id = null;
        if (companion.specialActorIds != null)
            companion_id = companion.specialActorIds["date"];
        else
            companion_id = companion.actorId;

        //companion_id = "c_Skye_skin";
        //GameStart.dialogue_manager.activateDialogue((string)encounter.conditionalIntroDialogs[0][1]);

        Debug.Log(encounter.datePromptIds.Length);
        Debug.Log(Configs.config_date_prompt.DatePrompt != null);

        ConfigDatePrompt._DatePrompt date_prompt = Configs.config_date_prompt.DatePrompt[encounter.datePromptIds[0]];

        ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[date_prompt.avatarSpawn];

        Actor.actor_controllers["Avatar"].actor_movement.teleportCharacter(new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]), new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]));

        Actor.spawnActor(companion_id, date_prompt.dateSpawn, "opponent");

        DialogueManager.onDialogueFinishedEvent += dialogueCallback;

        GameStart.dialogue_manager.activateDialogueLine(date_prompt.dialogue);
        
    }
    public void dialogueCallback(string dialogue)
    {
        if (dialogue == Configs.config_date_prompt.DatePrompt[encounter.datePromptIds[date_prompt_counter]].dialogue)
        {
            DialogueManager.onDialogueFinishedEvent -= dialogueCallback;
            Debug.Log("SUCEESSEFUL DIALGOEUY CALLBASKC");
            date_prompt_counter++;
            if (date_prompt_counter > encounter.datePromptIds.Length)
                finished();
            else
            {
                ConfigDatePrompt._DatePrompt date_prompt = Configs.config_date_prompt.DatePrompt[encounter.datePromptIds[date_prompt_counter]];

                ConfigScene._Scene.WayPoint waypoint = Scene.current.waypoint_dict[date_prompt.avatarSpawn];

                Actor.actor_controllers["Avatar"].actor_movement.teleportCharacter(new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]), new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]));

                waypoint = Scene.current.waypoint_dict[date_prompt.dateSpawn];

                Actor.actor_controllers["opponent"].actor_movement.teleportCharacter(new Vector3(waypoint.position[0], waypoint.position[1], waypoint.position[2]), new Vector3(waypoint.rotation[0], waypoint.rotation[1], waypoint.rotation[2]));

                DialogueManager.onDialogueFinishedEvent += dialogueCallback;
                
                GameStart.dialogue_manager.activateDialogueLine(date_prompt.dialogue);
            }
        }
    }

    void finished()
    {
        DialogueManager.onDialogueFinishedEvent += dialogueCallback;
    }
}
