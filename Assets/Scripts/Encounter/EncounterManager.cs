using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public class EncounterManager : MonoBehaviour
{
    public static event Action<string> onEncounterFinished = delegate { };
    public Encounter activateEncounter(string encounter_name)
    {
        Debug.Log("Activating Encounter: " + encounter_name);

        if (!Configs.config_encounter.Encounter.ContainsKey(encounter_name))
        {
            throw new System.Exception("Activate encounter - Invalid encounter " + encounter_name);
        }

        ConfigEncounter._Encounter new_encounter = Configs.config_encounter.Encounter[encounter_name];

        //Autopass this for now
        /*if (Configs.config_interaction.Interactions[encounter_name].filterPredicate != null)
        {
            if (!Predicate.parsePredicate(Configs.config_interaction.Interactions[encounter_name].filterPredicate))
            {
                Debug.Log("Failed the predicate: " + Configs.config_interaction.Interactions[encounter_name].filterPredicate);
                return null;
            }
        }*/

        Encounter result_encounter = null;

        Assert.IsNotNull(new_encounter);

        switch (new_encounter.type)
        {
            case "Date":
                result_encounter = new EncounterDate(new_encounter);
                break;
            default:
                Debug.LogWarning("Unknown interaction type " + new_encounter.type);
                break;
        }

        return result_encounter;
    }

    public static void onEncounterComplete(string encounter_id)
    {
        onEncounterFinished.Invoke(encounter_id);
    }
}
