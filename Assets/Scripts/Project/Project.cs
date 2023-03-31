using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

public class Project
{
    public static event Action<string> onProjectFinished = delegate { };
    public static ConfigProject._Project config_project = null;
    public static int current_progress;
    static int station_progress = 0;
    static bool addonce = false;

    public static void startProject(string new_project)
    {
        addonce = false;
        Debug.Log("starting Project " + new_project);

        GameStart.onReturnToMenu += cleanup;

        if (!Configs.config_project.Project.ContainsKey(new_project))
        {
            throw new System.Exception("Invalid project id " + new_project);
        }
        if (config_project != null) throw new System.Exception("Tried to start a project while we are already in a project.");
        config_project = Configs.config_project.Project[new_project];
        VariantManager.setVariant(config_project.variantTag);
        current_progress = 0;
        station_progress = 0;
        Scenario.onScenarioLoaded += onScenarioLoaded;
        Scenario.Activate(config_project.scenarioId, Scenario.current.objective);
        Scenario.Load(config_project.scenarioId);
    }

    private static void onScenarioLoaded()
    {
        Scenario.onScenarioLoaded -= onScenarioLoaded;
        if (config_project.startPlaylistIds != null)
            foreach (string playlistId in config_project.startPlaylistIds)
                Sound.playBark(playlistId);
        if (config_project.classIntro != null)
        {
            startStation();
        }
    }


    private static void cleanup()
    {
        InteractionManager.all_interactions_destroyed_event -= finishProject;
        current_progress = 0;
        config_project = null;
    }

    public static void addProgress(int progress)
    {       
        if (config_project == null) return;
        if (current_progress != -1)
            current_progress += progress;
        int stars = 0;
        foreach(int[] progress_for_stars in config_project.progressForStars)
        {
            if (current_progress >= progress_for_stars[1])
                stars = progress_for_stars[0];
        }
        if (stars >= config_project.progressForStars.Length - 1)
        {
            current_progress = -1;
        }
        //Honestly it doesn't matter
        //Autotunes fuck everything up anyway
        if (stars >= 1 && addonce == false)
        {
            addonce = true;
            if (config_project.stations == null) //Stations work differently. They trigger the end of the project by themselves.
                InteractionManager.all_interactions_destroyed_event += finishProject;
        }
    }

    public static int getProgressNeeded()
    {
        return config_project.progressForStars[config_project.progressForStars.Length - 1][1];
    }

    public static void finishProject()
    {
        InteractionManager.all_interactions_destroyed_event -= finishProject;

        Debug.Log("Project was finished with id " + config_project.projectId);

        if (config_project.rewardsForStars != null)
        {
            foreach(string[] r in config_project.rewardsForStars)
            {
                Reward.getReward(r[1]);
            }
        }
        if (config_project.skillId != null)
        {
            Reward.getSkill(config_project.skillId);
        }
        VariantManager.removeVariant();

        Scenario.Activate(config_project.outroScenarioId, Scenario.current.objective);
        Scenario.Load(config_project.outroScenarioId);
        if (config_project.passPlaylistIds != null)
            foreach (string playlistId in config_project.passPlaylistIds)
                Sound.playBark(playlistId);
        onProjectFinished.Invoke(config_project.projectId);
        config_project = null;
    }

    /*----- Stations -----*/
    private static void startStation()
    {
        if (station_progress >= config_project.stations.Length)
        {
            finishProject();
            return;
        }

        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        if (project_station.actorAliases != null)
        {
            foreach(var actor_alias in project_station.actorAliases)
            {
                Actor.addAlias(actor_alias.Key, actor_alias.Value);
            }
        }
        if (project_station.speakerAliases != null)
        {
            foreach (var speaker_alias in project_station.speakerAliases)
            {
                Speaker.addAlias(speaker_alias.Key, speaker_alias.Value);
            }
        }
        GameStart.interaction_manager.spawnInteraction(station.title);
        InteractionManager.all_interactions_destroyed_event += onStationTitleComplete;
    }

    private static void onStationTitleComplete()
    {
        InteractionManager.all_interactions_destroyed_event -= onStationTitleComplete;

        if (station_progress == 0)
        {
            GameStart.interaction_manager.spawnInteraction(config_project.classIntro);
            InteractionManager.all_interactions_destroyed_event += spawnStationIntro;
            return;
        }

        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.stationIntro);
        InteractionManager.all_interactions_destroyed_event += spawnStationProgress;
    }

    private static void spawnStationIntro()
    {
        InteractionManager.all_interactions_destroyed_event -= spawnStationIntro;
        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.stationIntro);
        InteractionManager.all_interactions_destroyed_event += spawnStationProgress;
    }

    private static void spawnStationProgress()
    {
        InteractionManager.all_interactions_destroyed_event -= spawnStationProgress;
        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.progress);
        InteractionManager.all_interactions_destroyed_event += spawnStationOnComplete;
    }

    private static void spawnStationOnComplete()
    {
        InteractionManager.all_interactions_destroyed_event -= spawnStationOnComplete;
        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.onComplete);
        InteractionManager.all_interactions_destroyed_event += spawnStationOutro;
    }

    private static void spawnStationOutro()
    {
        InteractionManager.all_interactions_destroyed_event -= spawnStationOutro;
        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.outro);
        InteractionManager.all_interactions_destroyed_event += spawnStationQuiz;
    }

    private static void spawnStationQuiz()
    {
        InteractionManager.all_interactions_destroyed_event -= spawnStationQuiz;
        var project_station = config_project.stations[station_progress];
        var station = Configs.config_station.Station[project_station.id];
        GameStart.interaction_manager.spawnInteraction(station.quiz);
        InteractionManager.all_interactions_destroyed_event += completeStation;
    }
    
    private static void completeStation()
    {
        InteractionManager.all_interactions_destroyed_event -= completeStation;
        var project_station = config_project.stations[station_progress];
        if (project_station.actorAliases != null)
        {
            foreach (var actor_alias in project_station.actorAliases)
            {
                Actor.removeAlias(actor_alias.Key);
            }
        }
        if (project_station.speakerAliases != null)
        {
            foreach (var speaker_alias in project_station.speakerAliases)
            {
                Speaker.removeAlias(speaker_alias.Key);
            }
        }
        station_progress++;
        startStation();
    }


}