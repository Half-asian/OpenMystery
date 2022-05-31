using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
public class ConfigEncounter : Config<ConfigEncounter>
{
    [System.Serializable]
    public class _Encounter
    {
        public string className;
        public dynamic conditionalIntroDialogs; //string or string[]
        public string encounterDesc;
        public string encounterId;
        public string encounterName;
        public string[] exitEvents;
        public string[] failEvents;
        public string[] successEvents;
        public string icon;
        public int maxCompanionYear;
        public int maxRomXpBar;
        public int numPrompts;
        public string opponentWaypoint;
        public string participationRewardId;
        public string playlistId;
        public string specialActorId;
        public string startPrice;
        public string tagSet;
        public int turnLimit;
        public string type;
        public string unlockPredicate;

        /*Dating exclusive*/
        public string[] datePartnerIds;
        public string[] datePromptIds;
        public string[][] outroDateScriptEvents;
        public int romLevelReqd;

    }

    public Dictionary<string, _Encounter> Encounter;
    public override void combine(List<ConfigEncounter> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Encounter.Keys)
            {
                Encounter[key] = other_list[i].Encounter[key];
            }
        }
    }
}

public class ConfigCompanion : Config<ConfigCompanion>
{
    [System.Serializable]
    public class _Companion
    {
        public string actorId;
        public string[] busyIntervals;
        public string companionId;
        public string description;
        public string fallbackUnlockPredicate;
        public string giftDesc;
        public string giftName;
        public string hideFromEncounters;
        public string[] levelRewards;
        public string maxLevelDescription;
        public string maxLevelRewardIconOverride;
        public string maxLevelRewardLabelOverride;
        public string repeatableCostCurve;
        public Dictionary<string, int>[] repeatableRecommendedAttributes;
        public int sortOrder;
        public string speakerId;
        public string visiblePredicate;
        public string xpLevelCurve;
        public Dictionary<string, string> specialActorIds;
    }

    public Dictionary<string, _Companion> Companion;

    public override void combine(List<ConfigCompanion> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Companion.Keys)
            {
                Companion[key] = other_list[i].Companion[key];
            }
        }
    }
}

public class ConfigDatePrompt : Config<ConfigDatePrompt>
{
    [System.Serializable]
    public class _DatePrompt
    {
        public string animationPose;
        public string avatarSpawn;
        public string dateSpawn;
        public string dialogue;
        public string id;
    }

    public Dictionary<string, _DatePrompt> DatePrompt;

    public override void combine(List<ConfigDatePrompt> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DatePrompt.Keys)
            {
                DatePrompt[key] = other_list[i].DatePrompt[key];
            }
        }
    }
}



class ConfigEncounterLoader
{
    public static async Task loadConfigsAsync()
    {
        Configs.config_companion = await ConfigCompanion.CreateFromJSONAsync(Common.getConfigPath("Companion-"));
        Configs.config_date_prompt = await ConfigDatePrompt.CreateFromJSONAsync(Common.getConfigPath("Romance_DatePrompt-"));

        List<ConfigEncounter> list_encounter = await ConfigEncounter.getDeserializedConfigsList("Encounter");
        Configs.config_encounter = list_encounter[0];
        Configs.config_encounter.combine(list_encounter);
    }
}