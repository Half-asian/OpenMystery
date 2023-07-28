using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using ModelLoading;
using System.IO;

public class SerializedScenario
{
    public string[][] interactions;
    public string[][] actors;
    public List<string> appliedClothes = null;
}

public class Scenario
{
    public static Scenario current;
    public static event Action onScenarioLoading = delegate { };
    public static event Action onScenarioCallClear = delegate { };
    public static event Action onScenarioLoaded = delegate { };

    public static bool block_screenfades = false;

    public static Dictionary<string, SerializedScenario> scenarios_serialized = new Dictionary<string, SerializedScenario>();

    public ConfigScenario._Scenario scenario_config;
    public Objective objective;
    public Dictionary<string, List<string>> scenario_interactions_completed = new Dictionary<string, List<string>>();

    public List<string> appliedClothes = null; //This is stored in the scenario, because it probably resets on scenario change

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
        if (current == null) return;
        if (!current.scenario_interactions_completed.ContainsKey(current.scenario_config.scenarioId))
            current.scenario_interactions_completed[current.scenario_config.scenarioId] = new List<string>();

        current.scenario_interactions_completed[current.scenario_config.scenarioId].Add(interaction);
    }

    public static void setContentVar(string[] parameters)
    {
        string keya = parameters[0];
        string keyb = parameters[1];

        string content_vars_txt = Path.Combine(GlobalEngineVariables.player_folder, "content_vars.txt");
        string text = File.ReadAllText(content_vars_txt);
        int keya_index = text.IndexOf(keya + ":");
        if (keya_index != -1)
        {
            int next_line = text.IndexOf("\n", keya_index);
            Debug.Log("Duplicate contentVar! removing " + keya_index + " " + (next_line - keya_index));
            text = text.Remove(keya_index, next_line - keya_index + 1);
        }
        text += keya + ":" + keyb + "\n";
        Debug.Log("setContentVar " + keya + ":" + keyb + "\n");
        File.WriteAllText(content_vars_txt, text);
    }

    //keya not used
    public static int getContentVar(string keya, string keyb)
    {
        string content_vars_txt = Path.Combine(GlobalEngineVariables.player_folder, "content_vars.txt");
        string text = File.ReadAllText(content_vars_txt);
        int keya_index = text.IndexOf(keyb + ":");
        if (keya_index != -1)
        {
            int next_line = text.IndexOf("\n", keya_index);
            string value = text.Substring(keya_index + keyb.Length + 1, next_line - keyb.Length - keya_index - 1);
            Debug.Log("Reading content var: " + keyb + " " + value);
            return int.Parse(value);
        }
        throw new Exception("Unknown content var " + keyb);
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
            //GameStart.interaction_manager.reloadGroups();

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

        float start_time = Time.realtimeSinceStartup;

        Debug.Log("Loading Scenario " + scenario_id);
        if (!Configs.config_scenario.Scenario.ContainsKey(scenario_id))
            throw new System.Exception("Load Scenario - invalid scenario name: " + scenario_id);

        block_screenfades = true;
        onScenarioLoading.Invoke();

        Scenario preactivated_scenario = Location.getScenarioById(scenario_id);

        if (current != null)
        {
            if (current.scenario_config.scenarioId == scenario_id)
            {
                Debug.Log("Didn't load the scenario, as it is the one we're in.");

                onScenarioLoaded.Invoke();

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


        //Set the scene
        string chosen_scene = current.scenario_config.scene;
        if (current.scenario_config.sceneOverrides != null)
        {
            if (current.scenario_config.sceneOverrides.TryGetValue(Player.local_avatar_house, out chosen_scene) == false)
                chosen_scene = current.scenario_config.scene;
        }
        Scene.setCurrentScene(chosen_scene);

        onScenarioCallClear.Invoke();

        Prop.spawnScenarioProps();

        if (scenarios_serialized.ContainsKey(current.scenario_config.scenarioId))
        {
            var serialized = scenarios_serialized[current.scenario_config.scenarioId];
            Actor.spawnSerializedActors(serialized.actors);
            current.appliedClothes = serialized.appliedClothes;
        }
        else
        {
            Actor.spawnScenarioActors();
            current.appliedClothes = null;
        }
        Tappie.spawnTappies();
        //Events

        ScreenFade.fadeFrom(1, Color.black, true);

        if (current.scenario_config.enterEvents != null)
            GameStart.event_manager.main_event_player.addEvents(current.scenario_config.enterEvents);
        if (current.scenario_config.resumeEvents != null)
            GameStart.event_manager.main_event_player.addEvents(current.scenario_config.resumeEvents); //Not sure how these work. Maybe if a player leaves and comes back to a scenario?

        //Interactions
        onScenarioLoaded.Invoke();

        EventManager.all_script_events_finished_event += onScenarioLoadScriptEventsFinished;

        Debug.Log("Time to load scenario: " + (Time.realtimeSinceStartup - start_time));
    }

    private static void onScenarioLoadScriptEventsFinished()
    {
        block_screenfades = false;

        EventManager.all_script_events_finished_event -= onScenarioLoadScriptEventsFinished;

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

    //This is basically local to the scenario
    public static void changeClothes(string clothing_type, string secondary_clothing_option)
    {
        switch (clothing_type)
        {
            case "Scripted":
                if (!Configs.config_scripted_clothing_set.ScriptedClothingSet.ContainsKey(secondary_clothing_option))
                {
                    Debug.LogError("Unknown scripted clothing set " + secondary_clothing_option);
                    return;
                }
                if (Player.local_avatar_gender == "male")
                    current.appliedClothes = new List<string>(Configs.config_scripted_clothing_set.ScriptedClothingSet[secondary_clothing_option].maleComponents);
                else
                    current.appliedClothes = new List<string>(Configs.config_scripted_clothing_set.ScriptedClothingSet[secondary_clothing_option].femaleComponents);
                break;
            case "Base":
                current.appliedClothes = null;
                break;
            case "ClassRobes":
                if (Player.local_avatar_gender == "male")
                    current.appliedClothes = new List<string>() { "robeMale" };
                else
                    current.appliedClothes = new List<string>() { "robeFemale" };
                break;
            case "QuidditchRobesWalk":
                switch (secondary_clothing_option)
                {
                    case "houseCup":
                        current.appliedClothes = new List<string>() { "o_quidditchHouseCupRobesWalk" };
                        break;
                    case "friendly":
                        current.appliedClothes = new List<string>() { "o_quidditchPracticeRobesWalk" };
                        break;
                    case "preTryout":
                        current.appliedClothes = new List<string>() { "o_quidditchPreTryoutRobesWalk" };
                        break;
                }
                break;

            case "QuidditchRobesFly":
                switch (secondary_clothing_option)
                {
                    case "houseCup":
                        current.appliedClothes = new List<string>() { "o_quidditchHouseCupRobesFly" };
                        break;
                    case "friendly":
                        current.appliedClothes = new List<string>() { "o_quidditchPracticeRobesFly" };
                        break;
                    case "preTryout":
                        current.appliedClothes = new List<string>() { "o_quidditchPreTryoutRobesFly" };
                        break;
                }
                break;
            default:
                throw new Exception("Unknown changeClothes type: " + clothing_type);
        }

        if (Actor.getActor(Player.local_avatar_onscreen_name) != null)
        {
            if (current.appliedClothes == null)
            {
                Actor.getActor(Player.local_avatar_onscreen_name).avatar_components.resetFromPlayerFile();
                Actor.getActor(Player.local_avatar_onscreen_name).avatar_components.spawnComponents();
            }
            else
            {
                foreach (var component in current.appliedClothes)
                {
                    Actor.getActor(Player.local_avatar_onscreen_name).avatar_components.equipAvatarComponent(component);
                }
            }
        }
        else
        {
            Debug.Log("AVATAR WAS NOT SPAWNED FOR REPLACED CLOTHES/ROBES");
        }
    }

    public static void setQuidditchHelmet(string predicate)
    {
        if (Actor.getActor(Player.local_avatar_onscreen_name) != null)
        {
            if (predicate.ToLower() == "true")
            {
                Actor.getActor(Player.local_avatar_onscreen_name).avatar_components.equipAvatarComponent("o_quidditchHouseCupHelmet"); //These helmets all seem to be the same across components
            }
            else
            {
                string avatar_hair_id = PlayerManager.current.customization_categories["hair"].component_id;
                Actor.getActor(Player.local_avatar_onscreen_name).avatar_components.equipAvatarComponent(avatar_hair_id);
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
        serialized_scenario.appliedClothes = current.appliedClothes;
        scenarios_serialized[current.scenario_config.scenarioId] = serialized_scenario;
    }
}
