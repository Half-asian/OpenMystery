using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class ObjectiveInteractionComplete : Objective
{
    public ObjectiveInteractionComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        keys = new List<string>(objective_config.keys);
        InteractionManager.interaction_finished_event += interactionFinishedCheck;
    }

    public void interactionFinishedCheck(string interaction_id)
    {
        if (keys.Contains(interaction_id))
        {
            keys.Remove(interaction_id);
            keys_completed++;
        }
        if (keys_completed >= objective_config.required_count)
        {
            InteractionManager.interaction_finished_event -= interactionFinishedCheck;
            objectiveCompleted();
        }
    }
}