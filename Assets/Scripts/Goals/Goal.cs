using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.IO;
public class Goal
{
    public static List<Goal> active_goals = new List<Goal>();
    public static event Action<string> onGoalCompleted = delegate { };
    public ConfigGoal.Goal goal_config;
    public int active_objective_index;
    public Objective active_objective;

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    private static void cleanup()
    {
        active_goals = new List<Goal>();
    }

    public static void startGoal(string goal_id, int starting_index = 0)
    {
        if (!Configs.config_goal.Goals.ContainsKey(goal_id))
            throw new Exception("Tried to start invalid goal with id " + goal_id);

        ConfigGoal.Goal goal_config = Configs.config_goal.Goals[goal_id];
        startGoal(goal_config, starting_index);
    }

    public static void startGoal(ConfigGoal.Goal _goal_config, int starting_index)
    {
        Debug.Log("Starting goal: " + _goal_config.goal_id);
        Goal goal = new Goal();
        Objective.onObjectiveCompleted += goal.objectiveCompleted;
        goal.goal_config = _goal_config;
        goal.active_objective_index = starting_index;
        active_goals.Add(goal);
        goal.startObjectiveFromActiveIndex();
        showGoalPopup(_goal_config);
    }

    private static void showGoalPopup(ConfigGoal.Goal goal_config)
    {
        GameStart.ui_manager.showPopup(goal_config);
        GameStart.ui_manager.showNextButton();
    }

    public static Goal getGoalById(string id)
    {
        foreach(Goal g in active_goals)
        {
            if (g.goal_config.goal_id == id)
                return g;
        }
        return null;
    }


    public void startObjectiveFromActiveIndex()
    {
        //File.WriteAllText(GlobalEngineVariables.player_folder + "\\goals_complete.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\goals_complete.txt").Replace("isGoalComplete(\"" + goal_config.goal_id + "\")", ""));
        if (active_objective_index == goal_config.required_steps.Length)
        {
            goalCompleted();
            return;
        }
        active_objective = Objective.startObjective(goal_config.required_steps[active_objective_index]);
    }

    public void loadObjectiveScenario()
    {
        active_objective.LoadScenarioIfValid();
    }


    public void goalCompleted()
    {
        Debug.Log("Goal Completed: " + goal_config.goal_id); 
        active_goals.Remove(this);
        Objective.onObjectiveCompleted -= objectiveCompleted;

        string goals_complete_txt = Path.Combine(GlobalEngineVariables.player_folder, "goals_complete.txt");
        if (!File.ReadAllText(goals_complete_txt).Contains("isGoalComplete(\"" + goal_config.goal_id + "\")"))
        {
            StreamWriter writer = new StreamWriter(goals_complete_txt, true);
            writer.WriteLine("isGoalComplete(\"" + goal_config.goal_id + "\")");
            writer.Close();
        }

        onGoalCompleted.Invoke(goal_config.goal_id);
    }


    public void objectiveCompleted(string objective_id)
    {
        if (goal_config.required_steps[active_objective_index] == objective_id)
        {
            active_objective_index++;
            startObjectiveFromActiveIndex();
        }
    }


    public static bool checkGoalRequirements(string goal_id)
    {
        ConfigGoal.Goal goal = Configs.config_goal.Goals[goal_id];
        bool pass_dependencies = true;
        bool pass_predicate = true;
        if (goal.predicate != null)
        {
            pass_predicate = Predicate.parsePredicate(goal.predicate);
        }
        if (goal.dependencies != null)
        {
            foreach (string dependency in goal.dependencies)
            {
                if (Predicate.parsePredicate("isGoalComplete(\"" + dependency + "\")") == false)
                    pass_dependencies = false;
            }
        }

        if (pass_dependencies && pass_predicate)
            return true;
        return false;
    }

    public static bool isGoalInProgress(string goal_id)
    {
        bool result = false;
        foreach(Goal goal in active_goals)
        {
            if (goal.goal_config.goal_id == goal_id)
                result = true;
        }
        return result;
    }

}
