using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;


public class ConfigMatch : Config<ConfigMatch>
{
    public class _Match
    {
        public string failureRewardId;
        public string introScenarioId;
        public string matchId;
        public string matchType;
        public string outroScenarioId;
        public string perfectRewardId;
        public string scenarioId;
        public string startPriceId;
        public string successRewardId;
        public string teamId_gOpponent;
        public string teamId_gPlayer;
        public string teamId_hOpponent;
        public string teamId_hPlayer;
        public string teamId_rOpponent;
        public string teamId_rPlayer;
        public string teamId_sOpponent;
        public string teamId_sPlayer;
        public string practisePosition;
        public string[] outroPlays;
        public string[] pivotalPlaySlots;
        public int startDifficulty;
        public int maxDifficulty;
        public int[] baseCostPerYear;
        public int[] redemptionCosts;
        public float winPercentageRequirement;
    }
    public Dictionary<string, _Match> Match;

    public override void combine(List<ConfigMatch> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Match.Keys)
            {
                Match[key] = other_list[i].Match[key];
            }
        }
    }

}

public class ConfigPlayPhase : Config<ConfigPlayPhase>
{
    public class _PlayPhase
    {
        public string dialogueId;
        public string nodeletId;
        public string type;
        public string[] enterEvents;
        public string[] exitEvents;
    }
    public Dictionary<string, _PlayPhase> PlayPhase;

    public override void combine(List<ConfigPlayPhase> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].PlayPhase.Keys)
            {
                PlayPhase[key] = other_list[i].PlayPhase[key];
            }
        }
    }
}

public class ConfigPivotalPlay : Config<ConfigPivotalPlay>
{
    public class _PivotalPlay
    {
        public string QTEPhase;
        public string availablePredicate;
        public string momentId;
        public string[] introPhases;
        public string[] outroPhases;
        public string[] failPhases;
        public string[] successPhases;
        public int maxScore;
    }
    public Dictionary<string, _PivotalPlay> PivotalPlay;
    public override void combine(List<ConfigPivotalPlay> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].PivotalPlay.Keys)
            {
                PivotalPlay[key] = other_list[i].PivotalPlay[key];
            }
        }
    }
}


public class ConfigPivotalPlayBucket : Config<ConfigPivotalPlayBucket>
{
    public class _PivotalPlayBucket
    {
        public string bucketId;
        public Dictionary<string, int> pivotalPlays;
    }
    public Dictionary<string, _PivotalPlayBucket> PivotalPlayBucket;

    public override void combine(List<ConfigPivotalPlayBucket> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].PivotalPlayBucket.Keys)
            {
                PivotalPlayBucket[key] = other_list[i].PivotalPlayBucket[key];
            }
        }
    }
}

public class ConfigQuidditchTeam : Config<ConfigQuidditchTeam>
{
    public class _QuidditchTeam
    {
        public string[] beater1ActorIds;
        public string[] beater1Predicates;
        public string beater1DefaultActorId;
        public string[] beater2ActorIds;
        public string[] beater2Predicates;
        public string beater2DefaultActorId;
        public string[] chaser1ActorIds;
        public string[] chaser1Predicates;
        public string chaser1DefaultActorId;
        public string[] chaser2ActorIds;
        public string[] chaser2Predicates;
        public string chaser2DefaultActorId;
        public string[] chaser3ActorIds;
        public string[] chaser3Predicates;
        public string chaser3DefaultActorId;
        public string[] keeperActorIds;
        public string[] keeperPredicates;
        public string keeperDefaultActorId;
        public string[] seekerActorIds;
        public string[] seekerPredicates;
        public string seekerDefaultActorId;
        public string matchCrestImage;
        public string matchProgressFillImage;
        public string teamId;
    }
    public Dictionary<string, _QuidditchTeam> QuidditchTeam;

    public override void combine(List<ConfigQuidditchTeam> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].QuidditchTeam.Keys)
            {
                QuidditchTeam[key] = other_list[i].QuidditchTeam[key];
            }
        }
    }

}

public class ConfigQuidditchBroomInfo : Config<ConfigQuidditchBroomInfo>
{
    public class _QuidditchBroomInfo
    {
        public string actorId;
        public string defaultBroom;
        public string[] brooms;
        public string[] predicates;
    }

    public Dictionary<string, _QuidditchBroomInfo> QuidditchBroomInfo;

    public override void combine(List<ConfigQuidditchBroomInfo> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].QuidditchBroomInfo.Keys)
            {
                QuidditchBroomInfo[key] = other_list[i].QuidditchBroomInfo[key];
            }
        }
    }
}


class ConfigQuidditchLoader
{
    static readonly string[] match_config_names = {
        "Quidditch_Matches-",
        "QuidditchS2_Matches-",
        "QuidditchS3_Matches-",
    };

    static readonly string[] playphase_config_names = {
        "Quidditch_PlayPhases-",
        "QuidditchS2_PlayPhases-",
        "QuidditchS3_PlayPhases-",
    };

    static readonly string[] pivotal_play_config_names = {
        "Quidditch_PivotalPlays-",
        "QuidditchS2_PivotalPlays-",
        "QuidditchS3_PivotalPlays-",
    };

    public static async Task loadConfigsAsync()
    {
        Configs.config_quidditch_team = await ConfigQuidditchTeam.CreateFromJSONAsync(Common.getConfigPath("Quidditch_Team-"));
        Configs.config_quidditch_broom_info = await ConfigQuidditchBroomInfo.CreateFromJSONAsync(Common.getConfigPath("Quidditch_Broom-"));

        List<ConfigMatch> list_match = await ConfigMatch.getDeserializedConfigsList("Match");
        Configs.config_match = list_match[0];
        Configs.config_match.combine(list_match);

        List<ConfigPivotalPlayBucket> list_pivotal_play_bucket = await ConfigPivotalPlayBucket.getDeserializedConfigsList("PivotalPlayBucket");
        Configs.config_pivotal_play_bucket = list_pivotal_play_bucket[0];
        Configs.config_pivotal_play_bucket.combine(list_pivotal_play_bucket);

        List<ConfigPlayPhase> list_play_phase = await ConfigPlayPhase.getDeserializedConfigsList("PlayPhase");
        Configs.config_play_phase = list_play_phase[0];
        Configs.config_play_phase.combine(list_play_phase);

        List<ConfigPivotalPlay> list_pivotal_play = await ConfigPivotalPlay.getDeserializedConfigsList("PivotalPlay");
        Configs.config_pivotal_play = list_pivotal_play[0];
        Configs.config_pivotal_play.combine(list_pivotal_play);
    }

}

