using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveFriendTutComplete : Objective
{
    public ObjectiveFriendTutComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        InteractionManager.interaction_finished_event += interactionFinishedCheck;
    }

    public void interactionFinishedCheck(string interaction_id)
    {
        if (interaction_id == "Y1C3P7_v2_OutroGroup_MainDialogue")
        {
            InteractionManager.interaction_finished_event -= interactionFinishedCheck;
            objectiveCompleted();
        }
    }
}