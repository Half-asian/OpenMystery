using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class ObjectiveProjectSuccess : Objective
{
    public ObjectiveProjectSuccess(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        Assert.IsNotNull(objective_config.keys, "ObjectiveProjectSuccess(): objective.keys cannot be null.");
        keys = new List<string>(objective_config.keys);
        Project.onProjectFinished += projectCallback;
    }

    public void projectCallback(string project_id)
    {
        Debug.Log("project callback " + project_id);
        if (keys.Contains(project_id))
        {
            keys.Remove(project_id);
            keys_completed++;
        }
        if (keys_completed >= objective_config.required_count)
        {
            Project.onProjectFinished -= projectCallback;
            objectiveCompleted();
        }
    }
}