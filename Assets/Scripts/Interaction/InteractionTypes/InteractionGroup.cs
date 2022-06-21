using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionGroup : Interaction
{
    Dictionary<string, Interaction> member_interactions;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        should_onFinishedEnterEvents_when_respawned = false;
        Assert.IsNotNull(_interaction.groupMembers, "InteractionGroup(): interaction.groupMembers can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        return this;
    }

    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        group_progress = 0;
        member_interactions = new Dictionary<string, Interaction>();


        spawnMemberInteractions();

        if (member_interactions.Count == 0)
        {
            GameObject.DestroyImmediate(interaction_gameobject);
        }
    }

    public void onRespawnAddMemberInteraction(Interaction interaction)
    {
        if (member_interactions == null)
            member_interactions = new Dictionary<string, Interaction>();
        member_interactions[interaction.config_interaction.id] = interaction;
    }

    public void addMemberInteraction(Interaction interaction)
    {
        member_interactions[interaction.config_interaction.id] = interaction;
        interaction.parent_group_interaction = this;
        interaction.parent_group_id = id;
    }

    public void spawnMemberInteractions()
    {
        foreach (string member_interaction in config_interaction.groupMembers)
        {
            if (!member_interactions.ContainsKey(member_interaction))
            {
                //Debug.Log("Activating member interaction " + member_interaction + " Interaction 169 of group " + interaction.id);


                GameObject result = GameStart.interaction_manager.activateInteraction(member_interaction);
                Interaction new_interaction = null;
                if (result != null)
                    new_interaction = result.GetComponent<Interaction>();
                if (new_interaction != null)
                {
                    new_interaction.parent_group_interaction = this;
                    new_interaction.parent_group_id = id;
                    if (interaction_gameobject != null)
                        new_interaction.interaction_gameobject.transform.parent = interaction_gameobject.transform;
                    member_interactions[member_interaction] = new_interaction;
                }
            }
        }
    }

    public void memberInteractionFinished(Interaction member_interaction)
    {

        Debug.Log("member interaction finished " + member_interaction.config_interaction.id);
        group_progress += member_interaction.config_interaction.GroupProgress;

        List<string> keys_to_remove = new List<string>();
        foreach (string key in member_interactions.Keys)
        {
            if (member_interactions[key] == member_interaction)
                keys_to_remove.Add(key);
        }
        foreach (string key in keys_to_remove)
        {

            member_interactions.Remove(key);
        }

        if (member_interaction.config_interaction.GroupProgress != 0 || (config_interaction.GroupProgress == 0 && member_interactions.Count == 0))
        {
            if (group_progress >= config_interaction.ProgressRequired)
            {

                foreach (Interaction i in member_interactions.Values) //This seems sus?
                {
                    GameStart.interaction_manager.finishInteraction(i);
                }


                Debug.Log("Group complete." + config_interaction.id);
                interactionComplete(); //No more member interactions.

            }
        }


       // else //when was this useful?
            //spawnMemberInteractions(); //PREDICATES SHOULD BE CULLING THIS OFF. NOT A DANGER IF PREDICATES ARE FUNCTIONING - OBJECTION!!! PREDICATES ARENT ALWAYS DEFINED!
    }

    public override void destroy()
    {
        if (member_interactions != null && member_interactions.Keys != null)
        {
            foreach (string i in member_interactions.Keys)
            {
                member_interactions[i].destroy();
            }
        }
        base.destroy();
    }

    public override void activate() { return; }

}

