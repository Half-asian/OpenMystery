using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
public enum GoalChainType
{
    Main,
    Class,
    Assignment,
}

public class GoalChain
{
    public static event Action<string> onGoalChainCompleted = delegate { };
    public static List<GoalChain> active_goalchains = new List<GoalChain>();

    public ConfigGoalChain._GoalChain goal_chain_config;
    public GoalChainType goal_chain_type;
    public int active_goal_index;
    public int active_goal_sub_index;

    private static bool just_started = true;
    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        active_goalchains = new List<GoalChain>();
        just_started = true;
    }

    public static void startGoalChain(string goal_chain_id, GoalChainType goal_chain_type, int starting_index = 0)
    {
        if (!Configs.config_goal_chain.GoalChain.ContainsKey(goal_chain_id))
            throw new Exception("Tried to start invalid goal chain with id " + goal_chain_id);

        ConfigGoalChain._GoalChain goal_chain_config = Configs.config_goal_chain.GoalChain[goal_chain_id];
        startGoalChain(goal_chain_config, goal_chain_type, starting_index);
    }

    public static void startGoalChain(ConfigGoalChain._GoalChain _goal_chain_config, GoalChainType _goal_chain_type, int starting_index)
    {
        Debug.Log("Starting goalchain: " + _goal_chain_config.id);
        GoalChain goal_chain = new GoalChain();
        Goal.onGoalCompleted += goal_chain.GoalCompleted;
        goal_chain.goal_chain_config = _goal_chain_config;
        goal_chain.goal_chain_type = _goal_chain_type;
        goal_chain.active_goal_index = starting_index;
        active_goalchains.Add(goal_chain);
        goal_chain.startGoalFromActiveIndex();
    }


    public void startGoalFromActiveIndex()
    {
        //We must start at least one goal.
        //If the index is the same as the length, we have finished the goal chain.
        switch (goal_chain_type)
        {
            case GoalChainType.Main:
                if (goal_chain_config.goalIds.Count <= active_goal_index) {
                    GoalChainCompleted();
                    return;
                }

                int result = -1;
                List<string> possible = goal_chain_config.goalIds[active_goal_index];
                for (int i = 0; i < possible.Count; i++)
                {
                    if (Goal.checkGoalRequirements(possible[i]) && active_goal_sub_index <= i)
                    {
                        result = i;
                        break;
                    }
                }
                if (result == -1) {
                    if (just_started)
                    {
                        result = 0;
                    }
                    else
                    {
                        active_goal_index++;
                        active_goal_sub_index = 0;
                        startGoalFromActiveIndex();
                        return;
                    }
                }
                just_started = false;
                active_goal_sub_index = result;
                Goal.startGoal(possible[result]);
                break;

            case GoalChainType.Class:
                if (goal_chain_config.classGoalIds.Count <= active_goal_index)
                {
                    GoalChainCompleted();
                    return;
                }
                Goal.startGoal(goal_chain_config.classGoalIds[active_goal_index]);
                break;

            case GoalChainType.Assignment:
                if (goal_chain_config.assignments.Count <= active_goal_index)
                {
                    GoalChainCompleted();
                    return;
                }
                Goal.startGoal(goal_chain_config.classGoalIds[active_goal_index]);
                break;
        }
    }

    public void GoalCompleted(string goal_id)
    {
        switch (goal_chain_type)
        {
            case GoalChainType.Main:
                if (goal_chain_config.goalIds[active_goal_index].Contains(goal_id))
                {
                    active_goal_sub_index++;
                    startGoalFromActiveIndex();
                }
                break;
            case GoalChainType.Class:
                if (goal_chain_config.classGoalIds[active_goal_index] == goal_id)
                {
                    active_goal_index++;
                    startGoalFromActiveIndex();
                }
                break;
            case GoalChainType.Assignment:
                if (goal_chain_config.assignments[active_goal_index] == goal_id)
                {
                    active_goal_index++;
                    startGoalFromActiveIndex();
                }
                break;
        }
    }

    public void GoalChainCompleted()
    {
        Debug.Log("Goal Chain Completed: " + goal_chain_config.id);
        active_goalchains.Remove(this);
        Goal.onGoalCompleted -= GoalCompleted;


        string goalchainss_complete_txt = Path.Combine(GlobalEngineVariables.player_folder, "goalchains_complete.txt");
        if (!File.ReadAllText(goalchainss_complete_txt).Contains("goalChainComplete(\"" + goal_chain_config.id + "\")"))
        {
            StreamWriter writer = new StreamWriter(goalchainss_complete_txt, true);
            writer.WriteLine("goalChainComplete(\"" + goal_chain_config.id + "\")");
            writer.Close();
        }

        if (goal_chain_config.rewardId != null)
        {
            Reward.getReward(goal_chain_config.rewardId);
        }

        onGoalChainCompleted.Invoke(goal_chain_config.id);
        GameStart.ui_manager.showExitMenuButton();
    }
}
