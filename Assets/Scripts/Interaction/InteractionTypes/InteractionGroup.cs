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

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        should_onFinishedEnterEvents_when_respawned = false;
        Assert.IsNotNull(_interaction.groupMembers, "InteractionGroup(): interaction.groupMembers can't be null");
        group_progress = 0;
        member_interactions = new Dictionary<string, Interaction>();
        spawnMemberInteractions();
        _interaction.autoSelect = true;
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

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
        interaction.parent_group_guid = guid;
    }

    public void spawnMemberInteractions()
    {
        foreach (string member_interaction in config_interaction.groupMembers)
        {
            if (!member_interactions.ContainsKey(member_interaction))
            {
                //Debug.Log("Activating member interaction " + member_interaction + " Interaction 169 of group " + interaction.id);


                GameObject result = GameStart.interaction_manager.spawnInteraction(member_interaction);
                Interaction new_interaction = null;
                if (result != null)
                    new_interaction = result.GetComponent<Interaction>();
                if (new_interaction != null)
                {
                    new_interaction.parent_group_interaction = this;
                    new_interaction.parent_group_guid = guid;
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

        if (member_interaction.config_interaction.GroupProgress != 0 || (config_interaction.GroupProgress == 0 && member_interactions.Count == 0))
        {
            if (group_progress >= config_interaction.ProgressRequired)
            {
                Debug.Log("Group complete." + config_interaction.id);
                interactionComplete(); //No more member interactions.
            }
        }
        else
        {
            foreach (Interaction i in member_interactions.Values)
            {
                if (i != member_interaction)
                {
                    if (i.config_interaction.filterPredicate != null)
                    {
                        i.is_active = Predicate.parsePredicate(i.config_interaction.filterPredicate);
                    }
                    else
                    {
                        i.is_active = true;
                    }
                }
            }
        }
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
}

