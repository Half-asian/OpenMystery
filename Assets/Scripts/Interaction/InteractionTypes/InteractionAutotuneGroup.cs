﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionAutotuneGroup : Interaction
{
    int member_counter;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        Assert.IsNotNull(_interaction.groupMembers, "InteractionAutotuneGroup(): interaction.groupMembers can't be null");
        member_counter = 0;
        GameStart.dialogue_manager.in_bubble = true;
        _interaction.autoSelect = true;
        return this;
    }

    public override void onFinishedEnterEvents() //Not really accurate. What would normally happen is all the children spawn on parent spawn, but are not visible because of active event actions.
    {
        base.onFinishedEnterEvents();
        memberInteractionComplete(null);
        return; 
    }

    //This whole thing is basically a big messy hack.
    //To try and get the "bubbles" to play one at a time.
    //Probably can eventually retire this whole thing and use a normal group at some stage.

    public void memberInteractionComplete(Interaction member_interaction)
    {
        if (member_interaction != null)
            member_interaction.destroy();

        Debug.Log("member Interaction Finished autotune ");

        //This would be the correct way to handle it
        /*if (member_interaction != null)
        {
            Debug.Log("adding group progress " + group_progress);
            group_progress += member_interaction.config_interaction.GroupProgress;

            if (member_interaction.config_interaction.GroupProgress != 0)
            {
                if (group_progress >= config_interaction.ProgressRequired)
                {
                    GameStart.dialogue_manager.finishBubbleDialogue();
                    interactionComplete(); //We are done
                    return;
                }
            }
        }*/

        if (member_counter < config_interaction.groupMembers.Length)
        {
            Interaction new_bubble = null;
            while (new_bubble == null && member_counter < config_interaction.groupMembers.Length)
            {
                GameObject new_bubble_gameobject = GameStart.interaction_manager.spawnInteraction(config_interaction.groupMembers[member_counter]);
                if (new_bubble_gameobject != null)
                {
                    new_bubble = new_bubble_gameobject.GetComponent<Interaction>();
                    new_bubble.parent_autotune_group_interaction = this;
                    new_bubble.parent_autotune_group_guid = guid;
                    new_bubble.can_add_project_progress = false;
                }
                member_counter++;
            }
            if (new_bubble != null)
            {
                new_bubble.parent_autotune_group_interaction = this;
                new_bubble.parent_autotune_group_guid = guid;
                new_bubble.interaction_gameobject.transform.parent = interaction_gameobject.transform;
                new_bubble.can_add_project_progress = false;
                new_bubble.is_active = true;
            }
            else
            {
                GameStart.dialogue_manager.finishBubbleDialogue();
                interactionComplete(); //We are done
            }
        }
        else
        {
            GameStart.dialogue_manager.finishBubbleDialogue();
            interactionComplete(); //We are done
        }
    }

    //This is basically a mod of original game logic.
    //Usually the game will repeat the same bubble several times until the star goal has been met
    //We don't care about that, so instead we finish the project early once all bubbles have played once.
    //Unless we are in a class, where there can be multiple autotunegroups.
    protected override void onFinishedExitEvents()
    {
        Project.addProgress(config_interaction.MaxProgress);

        base.onFinishedExitEvents();
    }
}
