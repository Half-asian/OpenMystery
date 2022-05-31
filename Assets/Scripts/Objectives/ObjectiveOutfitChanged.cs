using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveOutfitChanged : Objective
{
    public ObjectiveOutfitChanged(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        GameStart.current.GetComponent<GameStart>().StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveOutfitChanged objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }

}