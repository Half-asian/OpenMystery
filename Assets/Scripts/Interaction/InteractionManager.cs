using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using System;
public class InteractionManager : MonoBehaviour {
    //This class and the other interaction classesneeds to be rock solid
    //as they are a critical part of the engine. 
    //Don't change any code, unless you know what you're doing.

    //Coroutines from the Interaction class run on this object. 

    //public List<ObjectiveInteractionComplete> objective_callbacks = new List<ObjectiveInteractionComplete>();

    public static Interaction interaction_group;

    public static List<Interaction> active_interactions = new List<Interaction>();

    public static event Action all_interactions_finished_event;
    public static event Action<string> interaction_finished_event;

    public static void changeOptionInteractionsVisibility(bool show)
    {
        foreach (Interaction i in active_interactions)
        {
            i.gameObject.SetActive(show);
        }
    }

    public void spawnHubNPCInteraction(ref string dialogue_id, ref Vector3 location)
    {
        GameObject interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
        InteractionHubNPCDialog hub_npc_interaction = interaction_gameobject.AddComponent<InteractionHubNPCDialog>();
        hub_npc_interaction.hubNpcDialogSetup(ref dialogue_id, ref location);
    }

    public GameObject spawnInteraction(string interaction_name) 
    {
        Debug.Log("Spawning Interaction: " + interaction_name);

        if (!Configs.config_interaction.Interactions.ContainsKey(interaction_name)) //Does the interaction exist in configs?
        {
            throw new System.Exception("Spawn Interaction - Invalid Interaction " + interaction_name);
        }

        ConfigInteraction.Interaction new_interaction = Configs.config_interaction.Interactions[interaction_name];

        Assert.IsNotNull(new_interaction);

        if (Configs.config_interaction.Interactions[interaction_name].filterPredicate != null) //Is the interaction filtered by a predicate?
        {
            if (!Predicate.parsePredicate(Configs.config_interaction.Interactions[interaction_name].filterPredicate))
            {
                Debug.Log("Failed the predicate: " + Configs.config_interaction.Interactions[interaction_name].filterPredicate);
                return null;
            }
        }

        //If the interaction is already active, destroy the old one.
        List<Interaction> to_destroy = new List<Interaction>();
        foreach (Interaction active in active_interactions)
        {
            if (active.config_interaction != null && active.config_interaction.id == interaction_name)
                to_destroy.Add(active);
        }

        foreach(Interaction active in to_destroy)
            active.destroy();

        GameObject interaction_gameobject = null;

        switch (new_interaction.type)
        {
            //There can only ever be one group interaction at one time.
            case "Group":
                bool could_be_project = false;
                foreach (string group_member in new_interaction.groupMembers)
                {
                    if (Configs.config_interaction.Interactions[group_member].type == null)
                    { //Aka a bubble
                        could_be_project = true;
                    }
                    else if (could_be_project == true)
                        Debug.LogWarning("Weird mix of null interaction and non null interaction " + new_interaction.id);
                }
                if (could_be_project == false)
                {
                    //Group interactions can be spawned from the new scenario but also from a leads to. Double check that we're not spawning a dupe
                    if (interaction_group != null)
                        interaction_group.destroy();
                    interaction_gameobject = new GameObject();
                    active_interactions.Add(interaction_gameobject.AddComponent<InteractionGroup>().setup(ref new_interaction));
                    interaction_group = interaction_gameobject.GetComponent<InteractionGroup>();

                    break;
                }
                else
                    goto case "AutotuneGroup";
                

            case "AutotuneGroup":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionAutotuneGroup>().setup(ref new_interaction));
                if (interaction_group != null)
                    interaction_group.destroy();
                interaction_group = interaction_gameobject.GetComponent<InteractionAutotuneGroup>();
                break;

            case "Dialog":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_dialog"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionDialog>().setup(ref new_interaction));
                break;

            case "ScenarioTransition":

                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionScenarioTransition>().setup(ref new_interaction));               
                break;

            case "GoalDialog":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionGoalDialog>().setup(ref new_interaction));
                break;

            case "Project":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionProject>().setup(ref new_interaction));
                break;

            case "Match":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionMatch>().setup(ref new_interaction));
                break;

            case "Exit":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_exit"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionExit>().setup(ref new_interaction));
                break;

            case null:
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionBubble>().setup(ref new_interaction));
                break;

            case "Encounter":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionEncounter>().setup(ref new_interaction));
                break;

            case "TitleCard":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionTitleCard>().setup(ref new_interaction));
                break;

            case "ActivityComplete":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionActivityComplete>().setup(ref new_interaction));
                break;

            case "Focus":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionFocus>().setup(ref new_interaction));
                break;

            case "InteractionQTE":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionQTE>().setup(ref new_interaction));
                break;

            case "SwipeSortQTE":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionSwipeSortQTE>().setup(ref new_interaction));
                break;

            case "Quiz":
                interaction_gameobject = new GameObject();
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionQuiz>().setup(ref new_interaction));
                break;

            case "Optional":
                interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                active_interactions.Add(interaction_gameobject.AddComponent<InteractionOptional>().setup(ref new_interaction));
                break;
            case "FirstPersonExploration":
                break;
            default:
                throw new Exception("Unknown interaction type " + new_interaction.type + " in " + new_interaction.id);
        }

        return interaction_gameobject;
    }


    public void finishInteraction(Interaction interaction)
    {
        interaction_finished_event?.Invoke(interaction.config_interaction.id);

        bool all_finished = true;
        foreach(var i in active_interactions)
        {
            if (i.config_interaction.type != "Exit")
                all_finished = false;
        }

        if (all_finished)
            all_interactions_finished_event?.Invoke();
    }

    public void destroyAllInteractions()
    {
        foreach(Interaction i in active_interactions.ToArray())
        {
            i.destroy();
        }
        active_interactions = new List<Interaction>();
    }

    public string[][] serializeInteractions()
    {
        string[][] serialized_interactions = new string[active_interactions.Count][];
        for (int i = 0; i < active_interactions.Count; i++)
        {
            serialized_interactions[i] = active_interactions[i].toStringArray();
        }

        return serialized_interactions;
    }

    public void loadSerializedInteractions(string[][] serializedInteractions)
    {
        destroyAllInteractions();
        Debug.Log("loading interactions");  
        Dictionary<System.Guid, Interaction> guid_to_interaction = new Dictionary<Guid, Interaction>();

        foreach (string[] interaction_s in serializedInteractions) //Spawn
        {
            GameObject gameobject = spawnInteraction(interaction_s[0]);
            Interaction interaction = gameobject.GetComponent<Interaction>();
            interaction.config_interaction = Configs.config_interaction.Interactions[interaction_s[0]];
            interaction.id = System.Guid.Parse(interaction_s[1]);
            if (interaction_s[2] != null) interaction.parent_group_id = System.Guid.Parse(interaction_s[2]);
            if (interaction_s[3] != null) interaction.parent_autotune_group_id = System.Guid.Parse(interaction_s[3]);
            interaction.group_progress = int.Parse(interaction_s[4]);
            interaction.shouldShow = bool.Parse(interaction_s[5]);
            interaction.destroyed = bool.Parse(interaction_s[6]);
            interaction.should_onFinishedEnterEvents_when_respawned = bool.Parse(interaction_s[7]);
            interaction.gameObject.SetActive(true);

            guid_to_interaction[interaction.id] = interaction;
        }
        foreach (Interaction interaction in active_interactions) //Link
        {
            if (interaction.parent_group_id != System.Guid.Empty)
            {
                interaction.parent_group_interaction = (InteractionGroup)guid_to_interaction[interaction.parent_group_id];
                interaction.transform.parent = interaction.parent_group_interaction.transform;
                interaction.parent_group_interaction.onRespawnAddMemberInteraction(interaction);
            }
            if (interaction.parent_autotune_group_id != System.Guid.Empty) 
            {
                interaction.parent_autotune_group_interaction = (InteractionAutotuneGroup)guid_to_interaction[interaction.parent_autotune_group_id];
                interaction.transform.parent = interaction.parent_autotune_group_interaction.transform;
            }
        }
    }
}
