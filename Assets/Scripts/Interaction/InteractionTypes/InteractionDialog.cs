using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionDialog : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        Assert.IsNotNull(_interaction.dialogId, "InteractionDialog(): interaction.dialogId can't be null");
        interaction_gameobject.SetActive(true);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        if (config_interaction.spot != null) setHotspot();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        DialogueManager.onDialogueFinishedEvent += dialogueFinishedListener;
        GameStart.dialogue_manager.activateDialogue(config_interaction.dialogId);
    }

    public void dialogueFinishedListener(string dialogue)
    {
        //if (dialogue == interaction.dialogId)

        //We probably don't need to care about what dialogue actually finishes. Just that dialogue did end. Dialogues can lead to new dialogues, which wouldn't match.
        DialogueManager.onDialogueFinishedEvent -= dialogueFinishedListener;
        interactionComplete();
    }
}
