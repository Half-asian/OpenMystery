﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

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
        }
        public CharSpawn[] charSpawns;
        public List<string[]> randomSpawns;
        public string[] enterEvents;
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

    public static void getConfig()
    {
        Configs.config_scenario = getJObjectsConfigsListST("3DModelConfig");
    }
    public static async Task getConfigAsyncv2()
    {
        Configs.config_scenario = await getJObjectsConfigsListAsync("3DModelConfig");
    }
    public static async Task loadJ()
    {
        Configs.config_scenario = await loadConfigType();
    }
}


