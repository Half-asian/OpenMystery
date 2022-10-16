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
        public string companionId;
        public string className;
        public string encounterDesc;
        public string encounterId;
        public string encounterName;
        public string icon;
        public string opponentWaypoint;
        public string oppositionId;
        public string participationRewardId;
        public string playlistId;
        public string specialActorId;
        public string startPrice;
        public string tagSet;
        public string type;
        public string unlockPredicate;

        public string[] companionTagsRequired;
        public string[] companionTagsRestricted;
        public string[] enterEvents;
        public string[] exitEvents;
        public string[] failEvents;
        public string[] successEvents;
        public string[] idleScripts; //Seems to be anim sequences
        public string[] playerChoices;
        public string[] thresholdRewards;

        public int maxCompanionYear;
        public int maxRomXpBar;
        public int numPrompts;
        public int turnLimit;
        public int minThresholdsToPass;

        public bool repeatable;

        public dynamic conditionalIntroDialogs; //string or string[]

        /*Dating exclusive*/
        public string[] datePartnerIds;
        public string[] datePromptIds;
        public string[][] outroDateScriptEvents;
        public int romLevelReqd;

        /*Social exclusive*/
        public string failDialogue;
        public string successDialogue;
        public float stanceEndMargin;
        public string[] stanceOrder;
        public string[] stanceThresholds;
    }

    public Dictionary<string, _Encounter> Encounter;
    public override ConfigEncounter combine(List<ConfigEncounter> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Encounter.Keys)
            {
                Encounter[key] = other_list[i].Encounter[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_encounter = getJObjectsConfigsListST("Encounter");
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

    public override ConfigCompanion combine(List<ConfigCompanion> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Companion.Keys)
            {
                Companion[key] = other_list[i].Companion[key];
            }
        }
        return this;
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

    public override ConfigDatePrompt combine(List<ConfigDatePrompt> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DatePrompt.Keys)
            {
                DatePrompt[key] = other_list[i].DatePrompt[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_date_prompt = getJObjectsConfigsListST("DatePrompt");
    }
}

public class ConfigEncounterOpposition : Config<ConfigEncounterOpposition>
{
    [System.Serializable]
    public class _EncounterOpposition
    {
        public string oppositionId;
        public string startModalAnim;
        public string successModalAnim;
        public string failModalAnim;
        public string negativeIntroDialogue;
        public string neutralIntroDialogue;
        public string positiveIntroDialogue;
        public float tooltipHeight;
        public string[][] negativeMoods;
        public string[][] neutralMoods;
        public string[][] positiveMoods;
    }

    public Dictionary<string, _EncounterOpposition> EncounterOpposition;
    public override ConfigEncounterOpposition combine(List<ConfigEncounterOpposition> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].EncounterOpposition.Keys)
            {
                EncounterOpposition[key] = other_list[i].EncounterOpposition[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_encounter_opposition = getJObjectsConfigsListST("EncounterOpposition");
    }
}