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

    public override void combine(List<ConfigGoalChain> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].GoalChain.Keys)
            {
                GoalChain[key] = other_list[i].GoalChain[key];
            }
        }
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

    public override void combine(List<ConfigGoal> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Goals.Keys)
            {
                Goals[key] = other_list[i].Goals[key];
            }
        }
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

    public override void combine(List<ConfigObjective> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Objectives.Keys)
            {
                Objectives[key] = other_list[i].Objectives[key];
            }
        }
    }
}

class ConfigGoalsLoader
{
    public static async Task loadConfigsAsync()
    {
        List<string> string_configs = await ConfigGoal.getDecryptedConfigsList("Objectives");
        await Task.Run(
        () =>
        {
            List<ConfigGoalChain> list_goal_chain = new List<ConfigGoalChain>();
            List<ConfigGoal> list_goal = new List<ConfigGoal>();
            List<ConfigObjective> list_objective = new List<ConfigObjective>();
            foreach (string content in string_configs)
            {
                ConfigGoalChain a = JsonConvert.DeserializeObject<ConfigGoalChain>(content);
                if (a.GoalChain != null)
                    list_goal_chain.Add(a);

                ConfigGoal b = JsonConvert.DeserializeObject<ConfigGoal>(content);
                if (b.Goals != null)
                    list_goal.Add(b);

                ConfigObjective c = JsonConvert.DeserializeObject<ConfigObjective>(content);
                if (c.Objectives != null)
                    list_objective.Add(c);

            }
            Configs.config_goal_chain = list_goal_chain[0];
            Configs.config_goal_chain.combine(list_goal_chain);






            Configs.config_goal = list_goal[0];
            Configs.config_goal.combine(list_goal);

            Configs.config_objective = list_objective[0];
            Configs.config_objective.combine(list_objective);

            //This goal is for the avatar to change clothes. Not worth programming in the logic for this.
            //Configs.config_goal_chain.GoalChain["C3_v2"].goalIds.RemoveAt(3);
            //Configs.config_goal.Goals["Y1_C3_P6_v2"].dependencies = null;


            #region Patches

            Configs.config_goal_chain.GoalChain["C2_v2"].classGoalIds.Insert(1, "Y1_C2_P4_hub"); //Tutorial triggers this class in between goals

            //Insert broom flying class within the rest of the goals. It needs to be done in order.
            Configs.config_goal_chain.GoalChain["C3_v2"].goalIds.Insert(5, new List<string> { "Y1_C3_SummonBroom_v2" });
            Configs.config_goal_chain.GoalChain["C3_v2"].classGoalIds = null;

            Configs.config_goal.Goals["QuidditchS1C1_P1"].predicate = "true"; //Remove check to see if player has completed part of Y2

            //Configs.config_objective.Objectives["Y1_C9_P2aObj1"].objectiveScenario = "MQ4C5P2a";
            #endregion

        }
    );
    }

}
