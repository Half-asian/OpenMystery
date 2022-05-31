using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveSocialEncounterComplete : Objective
{

    //Hagrid asks you to go and improve your social skills by eating sandwiches.

    public ObjectiveSocialEncounterComplete(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        GameStart.current.GetComponent<GameStart>().StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveSocialEncounterComplete objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }

}