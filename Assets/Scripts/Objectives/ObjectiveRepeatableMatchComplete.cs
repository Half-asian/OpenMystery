using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectiveRepeatableMatchComplete : Objective
{
    public ObjectiveRepeatableMatchComplete(ConfigObjective.Objective _objective)
    {
        Assert.IsNotNull(_objective, "ObjectiveMatchComplete(): objective cannot be null.");
        objective_config = _objective;
        Assert.IsTrue(objective_config.message == "repeatableMatchComplete");
        GameStart.current.StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveRepeatableMatchComplete objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }

}

