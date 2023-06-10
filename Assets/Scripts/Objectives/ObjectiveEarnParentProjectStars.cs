using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectiveEarnParentProjectStars : Objective
{
    public ObjectiveEarnParentProjectStars(ConfigObjective.Objective _objective) //Just treat this like a project complete objective for now.
    {
        objective_config = _objective;
        Assert.IsNotNull(objective_config.keys, "ObjectiveProjectSuccess(): objective.keys cannot be null.");
        keys = new List<string>(objective_config.keys);
        Project.onProjectFinished += projectCallback;
    }

    public void projectCallback(string project_id)
    {
        Debug.Log("ProjectCallback " + project_id);
        var parent_project_id = Configs.config_project.Project[project_id].parentProjectId;
        if (parent_project_id == null)
        {
            Debug.Log("parent_project_id was null :(");
            return;
        }

        if (keys.Contains(parent_project_id))
        {
            Debug.Log("parent_project_id success!");
            keys_completed++;
        }
        if (true)//keys_completed >= objective_config.required_count)
        {
            Project.onProjectFinished -= projectCallback;
            objectiveCompleted();
        }
    }
}
