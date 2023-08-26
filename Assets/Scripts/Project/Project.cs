using UnityEngine;
using System;
using System.Linq;

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

        if (config_project.repeatableOptions != null) //Some projects are just links to other projects of various time limits
        {
            //We want the longest one to get all the content
            config_project = Configs.config_project.Project[config_project.repeatableOptions.Last()];
        }

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
        if (config_project.classIntro != null || config_project.stations != null)
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
    private enum StationSegment
    {
        Title,
        ClassIntro,
        Intro,
        Progress,
        OnComplete,
        Outro,
        Quiz
    }
    private static StationSegment current_station_segment;
    private static ConfigStation._Station config_station;
    private static void startStation()
    {
        if (station_progress >= config_project.stations.Length)
        {
            finishProject();
            return;
        }

        var project_station = config_project.stations[station_progress];
        config_station = Configs.config_station.Station[project_station.id];
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


        current_station_segment = StationSegment.Title;

        startNextCurrentStationSegment();
    }

    private static string getCurrentStationSegmentInteractionId()
    {
        switch(current_station_segment)
        {
            case StationSegment.Title:
                return config_station.title;
            case StationSegment.ClassIntro:
                return station_progress == 0 ? config_project.classIntro : null;
            case StationSegment.Intro:
                return config_station.stationIntro;
            case StationSegment.Progress:
                return config_station.progress;
            case StationSegment.OnComplete:
                return config_station.onComplete;
            case StationSegment.Outro:
                return config_station.outro;
            case StationSegment.Quiz: 
                return config_station.quiz;
            default:
                throw new NotImplementedException("Unknown project station segment: " + current_station_segment.ToString());
        }
    }

    private static void startNextCurrentStationSegment()
    {
        string next_interaction_id = getCurrentStationSegmentInteractionId();
        while (next_interaction_id == null)
        {
            if (current_station_segment == StationSegment.Quiz) //There was no valid segment left
            {
                completeStation();
                return;
            }
            current_station_segment++;
            next_interaction_id = getCurrentStationSegmentInteractionId();
        }
        GameStart.interaction_manager.spawnInteraction(next_interaction_id);
        InteractionManager.all_interactions_destroyed_event += onStationSegmentComplete;
    }

    private static void onStationSegmentComplete()
    {
        InteractionManager.all_interactions_destroyed_event -= onStationSegmentComplete;
        if (current_station_segment == StationSegment.Quiz) //This was the last segment
        {
            completeStation();
            return;
        }
        current_station_segment++;
        startNextCurrentStationSegment();
    }
    
    private static void completeStation()
    {
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
