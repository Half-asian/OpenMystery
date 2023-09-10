using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveDuellingComplete : Objective
{
    public ObjectiveDuellingComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        GameStart.current.GetComponent<GameStart>().StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveDuellingComplete objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }

}