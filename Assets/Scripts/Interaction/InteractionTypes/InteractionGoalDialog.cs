using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionGoalDialog : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.dialogId, "InteractionGoalDialog(): interaction.dialogId can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        setHotspot();
        interaction_gameobject.SetActive(false);
        return this;
    }

    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        if (interaction_gameobject != null)
            interaction_gameobject.SetActive(true);

    }

    public override void activate()
    {
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
