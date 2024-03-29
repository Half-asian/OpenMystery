using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class ObjectiveDialogueComplete : Objective
{
    public ObjectiveDialogueComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        Assert.IsNotNull(objective_config.keys, "ObjectiveDialogueComplete(): objective.keys cannot be null.");
        keys = new List<string>(objective_config.keys);
        DialogueManager.onDialogueFinishedEventSecondary += dialogueCallback;
    }

    public void dialogueCallback(string dialogue_id)
    {
        if (keys.Contains(dialogue_id))
        {
            keys_completed++;
        }
        if (keys_completed >= objective_config.required_count)
        {
            DialogueManager.onDialogueFinishedEventSecondary -= dialogueCallback;
            objectiveCompleted();
        }
    }
}