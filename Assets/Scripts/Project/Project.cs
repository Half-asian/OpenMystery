using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

public class Project
{
    public static event Action<string> onProjectFinished = delegate { };
    public static ConfigProject._Project project_config = null;
    public static int current_progress;
    
    public static void startProject(string new_project)
    {
        Debug.Log("starting Project " + new_project);

        GameStart.onReturnToMenu += cleanup;

        InteractionManager.interaction_finished_event += onInteractionFinished; //callback for group progress
        if (!Configs.config_project.Project.ContainsKey(new_project))
        {
            throw new System.Exception("Invalid project id " + new_project);
        }
        if (project_config != null) throw new System.Exception("Tried to start a project while we are already in a project.");
        project_config = Configs.config_project.Project[new_project];
        current_progress = 0;

        Scenario.Activate(project_config.scenarioId, Scenario.current.objective);
        Scenario.Load(project_config.scenarioId);
        if (project_config.startPlaylistIds != null)
            foreach(string playlistId in project_config.startPlaylistIds)
                Sound.playBark(playlistId);
        InteractionManager.all_interactions_finished_event += finishProject;

    }

    private static void cleanup()
    {
        InteractionManager.all_interactions_finished_event -= finishProject;
        current_progress = 0;
        project_config = null;
    }

    private static void onInteractionFinished(string interaction_id)
    {
        Debug.Log("onInteractionFinished Called");
        addProgress(Configs.config_interaction.Interactions[interaction_id].projectProgress);
    }

    public static void addProgress(int progress)
    {
        if (project_config == null) return;
        if (current_progress != -1)
            current_progress += progress;
        int stars = 0;
        foreach(int[] progress_for_stars in project_config.progressForStars)
        {
            if (current_progress >= progress_for_stars[1])
                stars = progress_for_stars[0];
        }
        if (stars >= project_config.progressForStars.Length - 1)
        {
            current_progress = -1;
        }
    }

    public static int getProgressNeeded()
    {
        return project_config.progressForStars[project_config.progressForStars.Length - 1][1];
    }

    public static void finishProject()
    {
        InteractionManager.all_interactions_finished_event -= finishProject;

        Debug.Log("Project was finished with id " + project_config.projectId);
        InteractionManager.interaction_finished_event -= onInteractionFinished;

        if (project_config.rewardsForStars != null)
        {
            foreach(string[] r in project_config.rewardsForStars)
            {
                Reward.getReward(r[1]);
            }
        }
        if (project_config.skillId != null)
        {
            Reward.getSkill(project_config.skillId);
        }

        Scenario.Activate(project_config.outroScenarioId, Scenario.current.objective);
        Scenario.Load(project_config.outroScenarioId);
        if (project_config.passPlaylistIds != null)
            foreach (string playlistId in project_config.passPlaylistIds)
                Sound.playBark(playlistId);
        onProjectFinished.Invoke(project_config.projectId);
        project_config = null;
    }
}