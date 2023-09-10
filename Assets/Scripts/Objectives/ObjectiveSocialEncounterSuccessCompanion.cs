using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectiveSocialEncounterSuccessCompanion : Objective
{
    //Game asks you to do a regular social encounter with a select person
    //nothing interesting at all
    public ObjectiveSocialEncounterSuccessCompanion(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;
        GameStart.current.GetComponent<GameStart>().StartCoroutine(ImmediateComplete(this));
    }

    IEnumerator ImmediateComplete(ObjectiveSocialEncounterSuccessCompanion objective)
    {
        yield return null;
        objective.finish();
    }

    public void finish()
    {
        objectiveCompleted();
    }


}