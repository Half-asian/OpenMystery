using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using static ConfigProject._Project;

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

    public static void getAllReferences(string project_id, ref ReferenceTree reference_tree)
    {
        if (!reference_tree.projects.Contains(project_id))
            reference_tree.projects.Add(project_id);
        else
            return;

        var project = Configs.config_project.Project[project_id];


        if (project.failureOutroScenarioId != null)
            ConfigScenario.getAllReferences(project.failureOutroScenarioId, ref reference_tree);
        if (project.introScenarioId != null)
            ConfigScenario.getAllReferences(project.introScenarioId, ref reference_tree);
        if (project.outroScenarioId != null)
            ConfigScenario.getAllReferences(project.outroScenarioId, ref reference_tree);
        if (project.variantTag != null)
        if (project.scenarioId != null)
            ConfigScenario.getAllReferences(project.scenarioId, ref reference_tree);
        if (project.variantTag != null) ;
            VariantManager.setVariant(project.variantTag);
        if (project.classIntro != null)
            ConfigInteraction.getAllReferences(project.classIntro, ref reference_tree);
        foreach (var station in project.stations ?? Enumerable.Empty<Station>())
        {
            ConfigStation.getAllReferences(station.id, ref reference_tree);
        }

        VariantManager.removeVariant();
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
    public static void getAllReferences(string station_id, ref ReferenceTree reference_tree)
    {
        if (!reference_tree.stations.Contains(station_id))
            reference_tree.stations.Add(station_id);
        else
            return;

        var station = Configs.config_station.Station[station_id];

        if (station.onComplete != null)
            ConfigInteraction.getAllReferences(station.onComplete, ref reference_tree);
        if (station.outro != null)
            ConfigInteraction.getAllReferences(station.outro, ref reference_tree);
        if (station.progress != null)
            ConfigInteraction.getAllReferences(station.progress, ref reference_tree);
        if (station.quiz != null)
            ConfigInteraction.getAllReferences(station.quiz, ref reference_tree);
        if (station.stationIntro != null)
            ConfigInteraction.getAllReferences(station.stationIntro, ref reference_tree);
        if (station.title != null)
            ConfigInteraction.getAllReferences(station.title, ref reference_tree);
    }
}