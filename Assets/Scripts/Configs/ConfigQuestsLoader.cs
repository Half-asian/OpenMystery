using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigTimeLimitedSideQuest : Config<ConfigTimeLimitedSideQuest>
{
    [System.Serializable]
    public class _TimeLimitedSideQuest
    {
        public string failDescription;
        public string id;
        public string introDescription;
        public string timedPromoId;
        public string title;
        public string awardRomanceXp;
        public string bonusRomanceTitle;
        public string duration;
        public string startPredicate;
        public int priority;
        public int weight;
        public int bonusRomanceXp;
        public Dictionary<string, string> choiceToCompanion;
        public string[] goalChainIds;
        public string[] icon;
        public string[] iconPredicate;
        public Dictionary<string, string>[] iconStart;
        public Dictionary<string, string>[] iconQuest;
        public Dictionary<string, string>[] iconComplete;

    }
    public Dictionary<string, _TimeLimitedSideQuest> TimeLimitedSideQuest;

    public override void combine(List<ConfigTimeLimitedSideQuest> other_list)
    {
        throw new NotImplementedException();
    }
}

public class ConfigYears : Config<ConfigYears>
{
    [System.Serializable]
    public class Year
    {
        public dynamic chapterIds;
    }
    public Dictionary<string, Year> Years;

    public override void combine(List<ConfigYears> other_list)
    {
        throw new NotImplementedException();
    }
}

class ConfigQuestsLoader
{
    public static async Task loadConfigsAsync()
    {
        Configs.config_years = await ConfigYears.CreateFromJSONAsync(Common.getConfigPath("Levels-"));
        Configs.config_time_limited_side_quest = await ConfigTimeLimitedSideQuest.CreateFromJSONAsync(Common.getConfigPath("TLSQ-"));
    }
}

