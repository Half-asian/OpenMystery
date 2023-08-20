using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveEarnedAttributeXp : Objective
{
    public ObjectiveEarnedAttributeXp(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        GameStart.current.GetComponent<GameStart>().StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveEarnedAttributeXp objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }

}