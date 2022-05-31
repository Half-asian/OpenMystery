using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionBubble : Interaction
{
    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        GameStart.dialogue_manager.in_bubble = true;

        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;

        if (config_interaction.hudDialogSpeaker != null)
            GameStart.dialogue_manager.showBubbleDialogue(config_interaction.hudDialogSpeaker, config_interaction.endHudDialog);
    }

    public override void activate()
    {
        interactionComplete();
    }
}
