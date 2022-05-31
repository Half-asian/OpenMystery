using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveY1C1P6Complete : Objective
{
    public ObjectiveY1C1P6Complete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
    }

    public override void activateScenarioIfValid()
    {
        Scenario.Activate("NUX_TrainScene", this);
    }

    public override void LoadScenarioIfValid()
    {
        GameStart.current.StartCoroutine(waitTrainFinished(this));
        Scenario.Load("NUX_TrainScene");
    }

    IEnumerator waitTrainFinished(ObjectiveY1C1P6Complete objective) //TODO: cancel the coroutine if exited to menu
    {
        float countdown = 12.0f;
        float start_time = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < start_time + countdown)
        {
            yield return null;
        }
        objective.finish();
    }
    public void finish()
    {
        objectiveCompleted();
    }
}
