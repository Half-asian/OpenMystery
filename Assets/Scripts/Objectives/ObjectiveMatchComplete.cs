using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectiveMatchComplete : Objective
{
    public ObjectiveMatchComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        Assert.IsNotNull(objective_config.keys, "ObjectiveMatchComplete(): objective.keys cannot be null.");
        Assert.IsNotNull(objective_config.objectiveScenario, "ObjectiveMatchComplete(): objective.objectiveScenario cannot be null");
        keys = new List<string>(objective_config.keys);
        GameStart.quidditch_manager.match_finished_event += quidditchMatchFinishedCheck;
    }

    public void quidditchMatchFinishedCheck(string match_id)
    {
        if (keys.Contains(match_id))
        {
            keys_completed++;
        }
        if (keys_completed >= objective_config.required_count)
        {
            GameStart.quidditch_manager.match_finished_event -= quidditchMatchFinishedCheck;
            objectiveCompleted();
        }
    }
}