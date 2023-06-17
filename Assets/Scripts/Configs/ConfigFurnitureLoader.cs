using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigFurnitureType : Config<ConfigFurnitureType>
{
    [System.Serializable]
    public class _FurnitureType
    {
        public string category;
        public string defaultComponent;
        [JsonProperty(PropertyName = "local:typeName")]
        public string localtypeName;
        public string waypoint;
        public string[] locTags;
        public string[] occupiedTooltipOffset;
        public string[] vacantTooltipOffset;
    }
    public Dictionary<string, _FurnitureType> FurnitureType;

    public override ConfigFurnitureType combine(List<ConfigFurnitureType> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].FurnitureType.Keys)
            {
                FurnitureType[key] = other_list[i].FurnitureType[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        //Configs.config_furniture_type = getJObjectsConfigsListST("FurnitureType");
    }

}

public class ConfigFurniture : Config<ConfigFurniture>
{
    [System.Serializable]
    public class _Furniture
    {
        public string category;
        public string componentId;
        public string modelId_default;
        public string name;
        public string priceId;
        public string waypoint;
        public string showPredicate;
        public bool isDefault;
        public bool requiresOwnership;
        public string[] locTags;
        public int sortOrder;

    }
    public Dictionary<string, _Furniture> Furniture;

    public override ConfigFurniture combine(List<ConfigFurniture> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Furniture.Keys)
            {
                Furniture[key] = other_list[i].Furniture[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        //Configs.config_furniture = getJObjectsConfigsListST("Furniture");
    }

}

public class ConfigPet : Config<ConfigPet>
{
    [System.Serializable]
    public class _Pet
    {
        public string animSequence;
        public string animSequenceAttached;
        public string hubSurfaceAnimationId;
        public string id;
        public string modelId;
        public string petTypeName;
        public string priceId;
        public string rewardId;
        public string shopAnimationId;
        public string surfacePredicate;
        public string unlockDescription;
        public string unlockPredicate;
        public string[] attachedPreviewModelOffset;
        public string[] characterSheetModelOffset;
        public string[] loctags;
        public string[] occupiedTooltipOffset;
        public string[] portraitOffset;
        public string[] vacantTooltipOffset;
        public string[] zOffset;
        public Dictionary<string, string> detachedPetWaypointMap;
        public Dictionary<string, string> surfaceWaypointMap;
        public float cooldownTime;
        public float despawnTime;
        public float portraitRotation;
        public float portraitScale;
        public float surfacePriority;
        public bool enabled;

    }
    public Dictionary<string, _Pet> Pet;

    public override ConfigPet combine(List<ConfigPet> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Pet.Keys)
            {
                Pet[key] = other_list[i].Pet[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        //Configs.config_pet = getJObjectsConfigsListST("Pet");
    }

}