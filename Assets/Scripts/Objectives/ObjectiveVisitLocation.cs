using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveVisitLocation : Objective
{
    public ObjectiveVisitLocation(ConfigObjective.Objective _objective)
    {
        objective_config = _objective;

        if (objective_config.keys != null)
        {
            keys = new List<string>(objective_config.keys);
        }

        Scenario.onScenarioCallClear += checkLocation;
    }

    void checkLocation()
    {
        if (keys != null)
        {
            if (keys.Contains(Location.current.locationId))
            {
                keys_completed++;
            }
            if (keys_completed >= objective_config.required_count)
            {
                Scenario.onScenarioCallClear -= checkLocation;
                objectiveCompleted();
            }
        }
    }
}