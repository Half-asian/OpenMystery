using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
public class ConfigReward : Config<ConfigReward> 
{
    [System.Serializable]
    public class _Reward
    {
        public string rewardId;
        public string[] wizardSkills;
    }
    public Dictionary<string, _Reward> Reward;

    public override ConfigReward combine(List<ConfigReward> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Reward.Keys)
            {
                Reward[key] = other_list[i].Reward[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_reward = getJObjectsConfigsListST("Reward");
    }
}
