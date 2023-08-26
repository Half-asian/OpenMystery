using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigHPActorInfo : Config<ConfigHPActorInfo>
{
    [System.Serializable]
    public class _HPActorInfo
    {
        public string actorId;
        public string animId_idle;
        public string animId_renownboard;
        public string animId_run;
        public string animId_sitting_idle;
        public string animId_success;
        public string animId_walk;
        public string houseId;
        public string includeInPointsBoard;
        public string[] loctags;
        public string modelId;
        public string[] modelPatches;
        public string[] portraitModelOffset;
        public string[] linkedComponentColorOverrides;
        public string nameFirst;
        public string nameLast;
        public float? animationScale = null;
        public float? headAnimationScale = null;
        public float tooltipHeight;
        public float portraitModelScale = 1.0f;
        public int startingYear;
        [System.Serializable]
        public class _quidditchMaterialOptions
        {
            public string houseId;
            [System.Serializable]
            public class _mapping
            {
                public bool useColors;
                public bool useEmblem;
                public string[] types;
            }
            public Dictionary<string, _mapping> mapping;
        }
        public _quidditchMaterialOptions quidditchMaterialOptions;
    }

    public Dictionary<string, _HPActorInfo> HPActorInfo;

    public override ConfigHPActorInfo combine(List<ConfigHPActorInfo> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].HPActorInfo.Keys)
            {
                HPActorInfo[key] = other_list[i].HPActorInfo[key];
            }
        }
        return this;
    }
}

public class ConfigActorMapping : Config<ConfigActorMapping>
{
    [System.Serializable]
    public class ActorMap
    {
        public string femaleActorId;
        public string maleActorId;
        public string mapId;
        public string predicate;
        public int priority;
    }
    public ActorMap[] ActorMapping;

    public static string getActorMapping(string actor, string gender)
    {
        string actor_map_result = actor;

        for (int priority = 3; priority >= 1; priority--)
        {
            foreach (ActorMap actor_map in Configs.config_actor_mapping.ActorMapping)
            {
                if (actor_map.priority == priority && actor_map.mapId == actor)
                {
                    if (Predicate.parsePredicate(actor_map.predicate))
                    {
                        if (gender == "male")
                        {
                            actor_map_result = actor_map.maleActorId;
                        }
                        else
                        {
                            actor_map_result = actor_map.femaleActorId;
                        }
                    }
                }
            }
        }

        if (actor_map_result == "::prefectIdAlias::")
        {
            actor_map_result = Configs.config_house.House[Player.local_avatar_house].snippets_prefectIdAlias;
        }

        return actor_map_result;
    }

    public override ConfigActorMapping combine(List<ConfigActorMapping> other_list)
    {
        throw new NotImplementedException();
    }


}

public class ConfigHouse : Config<ConfigHouse>
{
    [System.Serializable]
    public class _House
    {
        public string bannerIcon;
        public string snippets_eventXpIcon;
        public string snippets_PrefectCaps;
        public string crestIcon;
        public string snippets_HouseGreeting;
        public string snippets_prefectIdAlias;
        public string snippets_eventXpThumb;
        public string snippets_quidditchRivalHouse;
        public string trophyIcon;
        public string id;
        public string bannerIconSmall;
        public string snippets_House;
        public string grayscaleCrest;
        public string snippets_quidditchRivalHouseId;
        public string snippets_HouseHubLocation;
        public string fullBanner;
        public string dialogueBg;
        public string snippets_PrefectSpeakerName;
        public string pointsIcon;
        public string description;
        public string characterSheetIcon;
        public string snippet_Student;
        public string companionLevelBanner;
        public string companionGraduatedBanner;
        public string snippets_HouseFight;
        public string name;
        public string commonRoomLocation;
        public string fullCrest;
        public string[] variantTags;
    }

    public Dictionary<string, _House> House;

    public override ConfigHouse combine(List<ConfigHouse> other_list)
    {
        throw new NotImplementedException();
    }
    public static void getConfig()
    {
        Configs.config_house = getJObjectsConfigsListST("House");
    }
}


//Genderered Actor Mapping Config - Only for Rowan and FemRowan, basically not needed

