using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigGoalChain : Config<ConfigGoalChain>
{
    [System.Serializable]
    public class _GoalChain
    {
        public string contentPack;
        //public string[] contentPackGoals; can be string[] or string[][]. Better to ignore it
        public string description;
        public List<List<string>> goalIds;
        public List<string> classGoalIds;
        public List<string> assignments;
        public string icon;
        public string id;
        public bool isMainQuest;
        public string name;
        public int order;
        public string rewardId;
    }
    public Dictionary<string, _GoalChain> GoalChain;

    public override ConfigGoalChain combine(List<ConfigGoalChain> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].GoalChain.Keys)
            {
                GoalChain[key] = other_list[i].GoalChain[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_goal_chain = getJObjectsConfigsListST("GoalChain");
    }
}

public class ConfigGoal : Config<ConfigGoal>
{
    [System.Serializable]
    public class Goal
    {
        [JsonProperty(PropertyName = "goal-icon")]
        public string goal_icon;
        [JsonProperty(PropertyName = "goal-id")]
        public string goal_id;
        [JsonProperty(PropertyName = "goal-name")]
        public string goal_name;
        public string goalChainId;
        public bool hideFromHud;
        public string predicate;
        public int priority;
        [JsonProperty(PropertyName = "required-steps")]
        public string[] required_steps;
        public string[] dependencies;
        [JsonProperty(PropertyName = "ready-text")]
        public string ready_text;
        public string characterId;
    }
    public Dictionary<string, Goal> Goals;

    public override ConfigGoal combine(List<ConfigGoal> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Goals.Keys)
            {
                Goals[key] = other_list[i].Goals[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_goal = getJObjectsConfigsListST("Goals");
    }
}

public class ConfigAssignment : Config<ConfigAssignment>
{
    [System.Serializable]
    public class _Assignment
    {
        public string completedDesc;
        public string finalizeDialogTrigger;
        public string icon;
        public string id;
        public string introScenario;
        public string name;
        public string outroScenario;
        public string skill;
        public string startDialogTrigger;
        public string[] objectives;
    }
    public Dictionary<string, _Assignment> Assignment;

    public override ConfigAssignment combine(List<ConfigAssignment> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Assignment.Keys)
            {
                Assignment[key] = other_list[i].Assignment[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_assignment = getJObjectsConfigsListST("Assignment");
    }
}

public class ConfigObjective : Config<ConfigObjective>
{
    [System.Serializable]
    public class Objective
    {
        [JsonProperty(PropertyName = "goal-id")]
        public string goal_id;
        public string[] keys;
        public string message;
        public string notificationSpeaker;
        [JsonProperty(PropertyName = "objective-id")]
        public string objective_id;
        public string objectiveScenario;
        [JsonProperty(PropertyName = "required-count")]
        public int required_count;
        public bool updateHousePointsOnComplete;
        public bool restartScenarioOnComplete;
        public string[] objectiveHubNpcs;
        public string action;
        public string actionParameter;
        [JsonProperty(PropertyName = "total-function")]
        public string total_function;
    }
    public Dictionary<string, Objective> Objectives;

    public override ConfigObjective combine(List<ConfigObjective> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Objectives.Keys)
            {
                Objectives[key] = other_list[i].Objectives[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_objective = getJObjectsConfigsListST("Objectives");
    }
}
