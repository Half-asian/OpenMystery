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
    bool force_finish = false;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.groupMembers, "InteractionAutotuneGroup(): interaction.groupMembers can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        return this;
    }


    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        member_counter = 0;
        GameStart.dialogue_manager.in_bubble = true;

        memberInteractionFinished(null);
    }

    //This whole thing is basically a big messy hack.
    //To try and get the "bubbles" to play one at a time.
    //Probably can eventually retire this whole thing and use a normal group at some stage.

    public void memberInteractionFinished(ConfigInteraction.Interaction member_interaction)
    {
        Debug.Log("member Interaction Finished autotune ");
        if (member_interaction != null)
        {
            Debug.Log("adding group progress " + member_interaction.groupProgress);
            group_progress += member_interaction.groupProgress;

            if (member_interaction.groupProgress != 0)
            {
                if (group_progress >= config_interaction.progressRequired)
                {
                    GameStart.dialogue_manager.finishBubbleDialogue();
                    interactionComplete(); //We are done
                    return;
                }
            }
        }

        if (member_counter < config_interaction.groupMembers.Length)
        {
            Interaction new_bubble = null;
            while (new_bubble == null && member_counter < config_interaction.groupMembers.Length)
            {
                GameObject new_bubble_gameobject = GameStart.interaction_manager.activateInteraction(config_interaction.groupMembers[member_counter]);
                if (new_bubble_gameobject != null)
                {
                    new_bubble = new_bubble_gameobject.GetComponent<Interaction>();
                    new_bubble.parent_autotune_group_interaction = this;
                    new_bubble.parent_autotune_group_id = id;
                }
                member_counter++;
            }
            if (new_bubble != null)
            {
                new_bubble.parent_autotune_group_interaction = this;
                new_bubble.parent_autotune_group_id = id;
                new_bubble.interaction_gameobject.transform.parent = interaction_gameobject.transform;
            }
            else
            {
                GameStart.dialogue_manager.finishBubbleDialogue();
                force_finish = true;
                interactionComplete(); //We are done
            }
        }
        else
        {
            GameStart.dialogue_manager.finishBubbleDialogue();
            force_finish = true;
            interactionComplete(); //We are done
        }
    }
    public override void activate() { return; }


    //This is basically a mod of original game logic.
    //Usually the game will repeat the same bubble several times until the star goal has been met
    //We don't care about that, so instead we finish the project early once all bubbles have played once.
    protected override void onFinishedExitEvents()
    {
        if (force_finish)
            Project.addProgress(9999);
        base.onFinishedExitEvents();
    }
}
