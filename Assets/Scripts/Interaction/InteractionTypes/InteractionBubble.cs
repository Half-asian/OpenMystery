using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionBubble : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction new_interaction)
    {
        base.setup(ref new_interaction);
        GameStart.dialogue_manager.in_bubble = true;

        activate();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();


        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        if (config_interaction.hudDialogSpeaker != null)
            GameStart.dialogue_manager.showBubbleDialogue(config_interaction.hudDialogSpeaker, config_interaction.endHudDialog);

    }

}
