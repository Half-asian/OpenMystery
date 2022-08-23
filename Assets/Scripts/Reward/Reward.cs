using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Reward
{
    public static void getReward(string reward_id)
    {
        if (!Configs.config_reward.Reward.ContainsKey(reward_id)){
            Debug.LogError("Failed to claim reward with invalid id " + reward_id);
            return;
        }
        ConfigReward._Reward reward = Configs.config_reward.Reward[reward_id];
        if (reward.wizardSkills != null)
        {
            string skills_unlocked_txt = Path.Combine(GlobalEngineVariables.player_folder, "skills_unlocked.txt");
            foreach(string skill_id in reward.wizardSkills)
            {
                if (!File.ReadAllText(skills_unlocked_txt).Contains("skillUnlocked(\"" + skill_id + "\")"))
                {
                    StreamWriter writer = new StreamWriter(skills_unlocked_txt, true);
                    writer.WriteLine("skillUnlocked(\"" + skill_id + "\")");
                    writer.Close();
                    Debug.Log("Unlocked new skill " + skill_id);
                }
            }
        }


    }

    public static void getSkill(string skill_id)
    {
        string skills_unlocked_txt = Path.Combine(GlobalEngineVariables.player_folder, "skills_unlocked.txt");
        if (!File.ReadAllText(skills_unlocked_txt).Contains("skillUnlocked(\"" + skill_id + "\")"))
        {
            StreamWriter writer = new StreamWriter(skills_unlocked_txt, true);
            writer.WriteLine("skillUnlocked(\"" + skill_id + "\")");
            writer.Close();
            Debug.Log("Unlocked new skill " + skill_id);
        }
    }
}
