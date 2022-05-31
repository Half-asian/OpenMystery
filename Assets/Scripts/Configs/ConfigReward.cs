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

    public override void combine(List<ConfigReward> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Reward.Keys)
            {
                Reward[key] = other_list[i].Reward[key];
            }
        }
    }
}
public class ConfigRewardsLoader
{
    public static async Task loadConfigsAsync()
    {
        List<string> string_configs = await ConfigGoal.getDecryptedConfigsList("Reward");
        await Task.Run(
        () =>
        {
            List<ConfigReward> list_reward = new List<ConfigReward>();
            foreach (string content in string_configs)
            {
                ConfigReward a = JsonConvert.DeserializeObject<ConfigReward>(content);
                if (a.Reward != null)
                    list_reward.Add(a);

            }
            Configs.config_reward = list_reward[0];
            Configs.config_reward.combine(list_reward);
        }
    );
    }

}