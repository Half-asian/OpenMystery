using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location
{

    public static ConfigLocation._Location current;

    public static Dictionary<ConfigLocation._Location, List<Scenario>> activeScenarios 
        = new Dictionary<ConfigLocation._Location, List<Scenario>>();

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        activeScenarios
                = new Dictionary<ConfigLocation._Location, List<Scenario>>();
    }

    public static string currentLocation()
    {
        if (current != null)
            return current.locationId;
        return "";
    }

    public static void addScenarioToLocation(string location_id, Scenario scenario)
    {
        if (!activeScenarios.ContainsKey(Configs.config_location.Location[location_id]))
            activeScenarios[Configs.config_location.Location[location_id]] = new List<Scenario>();

        if (!activeScenarios[Configs.config_location.Location[location_id]].Contains(scenario))
            activeScenarios[Configs.config_location.Location[location_id]].Add(scenario);
    }

    public static void removeScenarioFromLocation(string location_id, Scenario scenario)
    {
        activeScenarios[Configs.config_location.Location[location_id]].Remove(scenario);

        if (activeScenarios[Configs.config_location.Location[location_id]].Count == 0)
            activeScenarios.Remove(Configs.config_location.Location[location_id]);
    }

    public static Scenario getScenarioById(string scenario_id)
    {
        ConfigScenario._Scenario scenario_cfg = Configs.config_scenario.Scenario[scenario_id];
        if (scenario_cfg.mapLocationId == null)
            return null;
        ConfigLocation._Location location_cfg = Configs.config_location.Location[scenario_cfg.mapLocationId];

        if (!activeScenarios.ContainsKey(location_cfg))
        {
            return null;
        }

        foreach (Scenario active_scenario in activeScenarios[location_cfg])
        {
            if (active_scenario.scenario_config.scenarioId == scenario_id)
            {
                return active_scenario;
            }
        }
        return null;
    }

    public static void setLocation()
    {
        if (Scenario.current.scenario_config.mapLocationId == null)
        {
            current = null;
            return;
        }

        string mapId = Scenario.current.scenario_config.mapLocationId;
        if (Scenario.current.scenario_config.mapLocationOverrides != null)
            if (Scenario.current.scenario_config.mapLocationOverrides.TryGetValue(Player.local_avatar_house, out mapId) == false)
                mapId = Scenario.current.scenario_config.mapLocationId;

        current = Configs.config_location.Location[mapId];
        LocationHub.exitLocationHub();
    }

}
