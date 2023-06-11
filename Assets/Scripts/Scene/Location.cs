using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location
{

    public static string current;

    public static Dictionary<string, List<Scenario>> activeScenarios 
        = new Dictionary<string, List<Scenario>>();

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        activeScenarios
                = new Dictionary<string, List<Scenario>>();
    }

    public static string currentLocation()
    {
        if (current != null)
            return current;
        return "";
    }

    public static void addScenarioToLocation(string location_id, Scenario scenario)
    {
        if (!activeScenarios.ContainsKey(location_id))
            activeScenarios[location_id] = new List<Scenario>();

        if (!activeScenarios[location_id].Contains(scenario))
            activeScenarios[location_id].Add(scenario);
    }

    public static void removeScenarioFromLocation(string location_id, Scenario scenario)
    {
        activeScenarios[location_id].Remove(scenario);

        if (activeScenarios[location_id].Count == 0)
            activeScenarios.Remove(location_id);
    }

    public static Scenario getScenarioById(string scenario_id)
    {
        ConfigScenario._Scenario scenario_cfg = Configs.config_scenario.Scenario[scenario_id];
        if (scenario_cfg.mapLocationId == null)
            return null;

        if (!activeScenarios.ContainsKey(scenario_cfg.mapLocationId))
        {
            return null;
        }

        foreach (Scenario active_scenario in activeScenarios[scenario_cfg.mapLocationId])
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
        if (Scenario.current.scenario_config == null || Scenario.current.scenario_config.mapLocationId == null)
        {
            current = null;
            return;
        }

        string mapId = Scenario.current.scenario_config.mapLocationId;
        if (Scenario.current.scenario_config.mapLocationOverrides != null)
            if (Scenario.current.scenario_config.mapLocationOverrides.TryGetValue(Player.local_avatar_house, out mapId) == false)
                mapId = Scenario.current.scenario_config.mapLocationId;

        current = mapId;
        var config_location = Configs.config_location.Location.ContainsKey(mapId) ? Configs.config_location.Location[mapId] : null; 

        if (config_location != null && config_location.musicPlaylistId != null)
        {
            Sound.playBGMusic(config_location.musicPlaylistId);
        }
        else
        {
            Sound.playBGMusic("BGM");
        }
        if (config_location != null && config_location.ambientId != null && config_location.ambientId != "noSoundPL")
        {
            Sound.playAmbient(config_location.ambientId);
        }
        else
        {
            Sound.playAmbient("none");
        }

        LocationHub.exitLocationHub();
    }

}
