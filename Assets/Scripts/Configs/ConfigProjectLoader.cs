using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigProject : Config<ConfigProject>
{
    [System.Serializable]
    public class _Project
    {
        public string failureOutroScenarioId;
        public string introScenarioId;
        public string outroScenarioId;
        public string projectDuration;
        public string projectId;
        public string progressType;
        public string projectNameToken;
        public string scenarioId;
        public string skillId;
        public string classIntro;
        public string variantTag;
        public int starsToPass;
        public int[][] progressForStars;
        public string[] passPlaylistIds;
        public string[] startPlaylistIds;
        public string[][] rewardsForStars;

        [System.Serializable]
        public class Station
        {
            public string id;
            public Dictionary<string, string> actorAliases;
            public Dictionary<string, string> speakerAliases;
        }

        public Station[] stations;
    }
    public Dictionary<string, _Project> Project;

    public override ConfigProject combine(List<ConfigProject> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Project.Keys)
            {
                Project[key] = other_list[i].Project[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_project = getJObjectsConfigsListST("Project");
    }
}

public class ConfigStation : Config<ConfigStation>
{
    [System.Serializable]
    public class _Station
    {
        public string id;
        public string onComplete;
        public string outro;
        public string progress;
        public string quiz;
        public string stationIntro;
        public string title;
    }
    public Dictionary<string, _Station> Station;

    public override ConfigStation combine(List<ConfigStation> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Station.Keys)
            {
                Station[key] = other_list[i].Station[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_station = getJObjectsConfigsListST("Station");
    }
}