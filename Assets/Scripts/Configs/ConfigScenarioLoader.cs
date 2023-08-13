using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using static ConfigInteraction;

public class ConfigScenario : Config<ConfigScenario>
{
    [System.Serializable]
    public class _Scenario
    {
        [System.Serializable]
        public class CharSpawn
        {
            public string instanceId;
            public string charId;
            public string waypointId;
            public string[] lookupTags;
        }
        public CharSpawn[] charSpawns;
        public List<string[]> randomSpawns;
        public List<string> enterEvents;
        public List<string> resumeEvents; //What is this?
        public List<string> tappies;
        public string firstAction;
        public string mapLocationId;
        [JsonProperty(PropertyName = "ignoreme")]
        public Dictionary<string, string> mapLocationOverrides; //  Some mapLocationOverrides are set to integers, probably erroneously
        [JsonProperty(PropertyName = "mapLocationOverrides")]
        public dynamic _mapLocationOverrides; 

        public string nameToken;
        public string scenarioId;
        public string scene;
        public Dictionary<string, string> sceneOverrides;

        public string bgSoundPlaylistId;
        public string musicPlaylistId;
        public string activeDefaultPredicate;
        public bool useRobeOverride;
    }

    public Dictionary<string, _Scenario> Scenario;

    public override ConfigScenario combine(List<ConfigScenario> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Scenario.Keys)
            {
                Scenario[key] = other_list[i].Scenario[key];
            }
        }
        return this;
    }
    public static void getAllReferences(string scenario_id, ref ReferenceTree reference_tree)
    {
        if (!reference_tree.scenarios.Contains(scenario_id))
            reference_tree.scenarios.Add(scenario_id);
        else
            return;

        var scenario = Configs.config_scenario.Scenario[scenario_id];
        foreach (var script_event in scenario.enterEvents ?? Enumerable.Empty<string>())
        {
            ConfigScriptEvents.getAllReferences(script_event, ref reference_tree);
        }
        foreach (var script_event in scenario.resumeEvents ?? Enumerable.Empty<string>())
        {
            ConfigScriptEvents.getAllReferences(script_event, ref reference_tree);
        }
        if (scenario.firstAction != null)
        {
            ConfigInteraction.getAllReferences(scenario.firstAction, ref reference_tree);
        }
    }

}

public class ConfigTappie : Config<ConfigTappie>
{
    [System.Serializable]
    public class _Tappie
    {
        public string activeAnimation;
        public string activeSequence;
        public string activeModel;
        public string activeWaypoint;
        public string soundEvent;
        public string tapAnimation;
        public string tappieId;
        public string showPredicate;
    }

    public Dictionary<string, _Tappie> Tappie;

    public override ConfigTappie combine(List<ConfigTappie> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Tappie.Keys)
            {
                Tappie[key] = other_list[i].Tappie[key];
            }
        }
        return this;
    }

}
