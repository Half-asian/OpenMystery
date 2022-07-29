using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class SerializedScenario
{
    public string[][] interactions;
    public string[][] actors;
    public Dictionary<string, Dictionary<string, int>> contentVars;
}

public class Scenario
{
    public static Scenario current;
    public static event Action onScenarioLoaded = delegate { };
    public static Dictionary<string, SerializedScenario> scenarios_serialized = new Dictionary<string, SerializedScenario>();

    public ConfigScenario._Scenario scenario_config;
    public Objective objective;
    public Dictionary<string, List<string>> scenario_interactions_completed = new Dictionary<string, List<string>>();
    public Dictionary<string, Dictionary<string, int>> contentVars = new Dictionary<string, Dictionary<string, int>>();


    #region non-statics

    //Remove the scenario from the map
    //Can be called by the objective that spawned the scenario and the scenarios children
    public void removeScenarioFromMap()
    {
        Objective.onObjectiveCompleted -= onObjectiveCompleted;
        if (Location.current != null)
            Location.activeScenarios[Location.current].Remove(current);
    }
    public void onObjectiveCompleted(string objective_id)
    {
        if (objective.objective_config.objective_id == objective_id)
            removeScenarioFromMap();
    }


    #endregion

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        current = null;
    }

    public static ConfigScenario._Scenario getCurrentScenarioConfig()
    {
        if (current != null)
            return current.scenario_config;
        else
            return new ConfigScenario._Scenario();
    }

    public static List<string> getInteractionsCompleted()
    {
        if (current == null) return new List<string>();
        if (!current.scenario_interactions_completed.ContainsKey(current.scenario_config.scenarioId))
            current.scenario_interactions_completed[current.scenario_config.scenarioId] = new List<string>();
        return current.scenario_interactions_completed[current.scenario_config.scenarioId];
    }

    public static void completeInteraction(string interaction)
    {
        if (!current.scenario_interactions_completed.ContainsKey(current.scenario_config.scenarioId))
            current.scenario_interactions_completed[current.scenario_config.scenarioId] = new List<string>();

        current.scenario_interactions_completed[current.scenario_config.scenarioId].Add(interaction);
    }

    public static void setContentVar(string[] parameters)
    {
        string keya = parameters[0];
        string keyb = parameters[1];
        int value = int.Parse(parameters[2]);

        if (!current.contentVars.ContainsKey(keya))
            current.contentVars[keya] = new Dictionary<string, int>();
        current.contentVars[keya][keyb] = value;
    }

    public static int getContentVar(string keya, string keyb)
    {
        if (!current.contentVars.ContainsKey(keya))
            return 0;
        if (!current.contentVars[keya].ContainsKey(keyb))
            return 0;
        return current.contentVars[keya][keyb];
    }


    //Activating a scenario adds the scenario to the map, but doesn't load it. If we travel to that location, we should be able to access the scenario.
    //If we activate a scenario in the same location we're already in, we mark our current scenario as being completed, removing it from the map.
    public static void Activate(string scenario_id, Objective objective = null)
    {
        Debug.Log("Activating Scenario " + scenario_id);

        if (!Configs.config_scenario.Scenario.ContainsKey(scenario_id))
            throw new System.Exception("Activate Scenario - invalid scenario name: " + scenario_id);

        ConfigScenario._Scenario activated_scenario = Configs.config_scenario.Scenario[scenario_id];

        if (current != null && activated_scenario.scenarioId == current.scenario_config.scenarioId) //We are trying to activate a scenario that we're already in.
        {
            Debug.Log("Didn't activate the scenario, as it is the one we're in.");
            return;
        }

        if (activated_scenario.mapLocationId == null) //If we are in a location, we do not need to save the scenario ever.
            return;

        string mapId = activated_scenario.mapLocationId;
        if (activated_scenario.mapLocationOverrides != null)
            if (activated_scenario.mapLocationOverrides.TryGetValue(Player.local_avatar_house, out mapId) == false)
                mapId = activated_scenario.mapLocationId;

        Scenario new_scenario = new Scenario();
        new_scenario.scenario_config = Configs.config_scenario.Scenario[scenario_id];

        if (current != null && current.scenario_config.mapLocationId == mapId) 
        {
            current.removeScenarioFromMap();            //We're replacing a scenario
        }

        if (objective != null)
        {
            new_scenario.objective = objective;
            Objective.onObjectiveCompleted += new_scenario.onObjectiveCompleted;
        }

        new_scenario.scenario_interactions_completed[scenario_id] = new List<string>();
        Location.addScenarioToLocation(mapId, new_scenario);
    }


    //We load into the scenario, setting the scene, spawning the actors etc.
    //If we are in a default scenario, we do not need to pre-activate the scenario. Progress will not be saved.
    public static void Load(string scenario_id)
    {
        Debug.Log("Loading Scenario " + scenario_id);
        if (!Configs.config_scenario.Scenario.ContainsKey(scenario_id))
            throw new System.Exception("Load Scenario - invalid scenario name: " + scenario_id);

        Scenario preactivated_scenario = Location.getScenarioById(scenario_id);

        if (current != null)
        {
            if (current.scenario_config.scenarioId == scenario_id)
            {
                Debug.Log("Didn't load the scenario, as it is the one we're in.");

                return; //We are already in the right scenario
            }
        }

        //We are in a location. Get the pre-activated location.
        if (preactivated_scenario != null)
        {
            current = preactivated_scenario;
        }
        //We are not in a location or we are in a default scenario. Make a new scenario.
        else
        {
            current = new Scenario();
            current.scenario_config = Configs.config_scenario.Scenario[scenario_id];
        }


        GameStart.interaction_manager.destroyAllInteractions();
        GameStart.ui_manager.closePopup();
        GameStart.ui_manager.next_area_button.SetActive(false);
        GameStart.ui_manager.exit_to_menu_button.SetActive(false);


        //Set the scene
        string chosen_scene = current.scenario_config.scene;
        if (current.scenario_config.sceneOverrides != null)
        {
            if (current.scenario_config.sceneOverrides.TryGetValue(Player.local_avatar_house, out chosen_scene) == false)
                chosen_scene = current.scenario_config.scene;
        }
        Scene.setCurrentScene(chosen_scene);

        onScenarioLoaded.Invoke();

        if (scenarios_serialized.ContainsKey(current.scenario_config.scenarioId))
        {
            Actor.spawnSerializedActors(scenarios_serialized[current.scenario_config.scenarioId].actors);
            current.contentVars = scenarios_serialized[current.scenario_config.scenarioId].contentVars;
        }
        else
        {
            Actor.spawnScenarioActors();
            current.contentVars = new Dictionary<string, Dictionary<string, int>>();
        }
        Prop.spawnScenarioProps();
        Tappie.spawnTappies();
        //Events

        if (current.scenario_config.enterEvents != null)
        {
            foreach (string event_string in current.scenario_config.enterEvents)
            {
                GameStart.event_manager.main_event_player.addEvent(event_string);
            }
        }

        /*if (current.scenario_config.scenarioId == "NUX_TrainScene")
        {
            Debug.Log("choo choo");
            string[] a = new string[] { "cam_shotA", "0" };
            CameraManager.focusCam(ref a);
        }*/

        //Interactions

        if (scenarios_serialized.ContainsKey(current.scenario_config.scenarioId)) //Reload saved interactions
        {
            GameStart.interaction_manager.loadSerializedInteractions(scenarios_serialized[current.scenario_config.scenarioId].interactions);
        }
        else
        {
            if (current.scenario_config.firstAction != null)
            {
                GameStart.interaction_manager.spawnInteraction(current.scenario_config.firstAction);
            }
        }
    }

    public static void restartScenario()
    {
        if (current == null || current.scenario_config == null) return;
        string scenario_id = current.scenario_config.scenarioId;
        current = null;
        Activate(scenario_id);
        Load(scenario_id);
    }
    public static void ExitSave() {
        SerializedScenario serialized_scenario = new SerializedScenario();
        serialized_scenario.interactions = GameStart.interaction_manager.serializeInteractions();
        serialized_scenario.actors = Actor.serializeActors();
        serialized_scenario.contentVars = current.contentVars;
        scenarios_serialized[current.scenario_config.scenarioId] = serialized_scenario;
    }
}
