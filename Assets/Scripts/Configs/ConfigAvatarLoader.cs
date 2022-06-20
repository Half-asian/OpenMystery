using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ConfigAvatarComponents : Config<ConfigAvatarComponents>
{
    [System.Serializable]
    public class AvatarComponent
    {
        public string animationModel;
        public string category;
        public string componentId;
        public string outfitId;
        public string patchModelId;
        public string priceId;
        public string showPredicate;
        public string[] sliderId;
        public string[] sliderValue;
        public int sortOrder;
    }
    public Dictionary<string, AvatarComponent> AvatarComponents;

    public override ConfigAvatarComponents combine(List<ConfigAvatarComponents> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarComponents.Keys)
            {
                AvatarComponents[key] = other_list[i].AvatarComponents[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_avatar_components = getJObjectsConfigsListST("AvatarComponents");
    }
}

public class ConfigAvatarOutfitData : Config<ConfigAvatarOutfitData>
{
    [System.Serializable]
    public class _AvatarOutfitData
    {
        public string headpatch;
        public string armpatch;
        public string legpatch;
        public string category;
        public string gender;
        public string modelid;
        public string name;

        public string[] patchDefines;


        public class Material
        {
            public string[] vec3Ids;
            public float[][] vec3Values;
            public string[] stringIds;
            public string[] stringValueKeys;
        }
        public Material materials;
        public Material patchMaterials;

    }
    public Dictionary<string, _AvatarOutfitData> AvatarOutfitData;

    public override ConfigAvatarOutfitData combine(List<ConfigAvatarOutfitData> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarOutfitData.Keys)
            {
                AvatarOutfitData[key] = other_list[i].AvatarOutfitData[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_avatar_outfit_data = getJObjectsConfigsListST("AvatarOutfitData");
    }
}
public class ConfigAvatarAttributeColors : Config<ConfigAvatarAttributeColors>
{
    [System.Serializable]
    public class AvatarAttributeColor
    {
        public string category;
        public string colorSliderId;
        public string price;
        public string salePriceId;
        public Dictionary<string, Dictionary<string, int>> shaderData;
        [System.Serializable]
        public class colorConfig
        {
            public int[] codes;
            public List<int[]> sets;
        }
        public List<colorConfig> colorConfigs;
    }

    public Dictionary<string, AvatarAttributeColor> AvatarAttributeColors;
    public override ConfigAvatarAttributeColors combine(List<ConfigAvatarAttributeColors> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarAttributeColors.Keys)
            {
                AvatarAttributeColors[key] = other_list[i].AvatarAttributeColors[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_avatar_attribute_colors = getJObjectsConfigsListST("AvatarAttributeColors");
    }
}

public class ConfigAvatarPatchConfig : Config<ConfigAvatarPatchConfig>
{
    [System.Serializable]
    public class _AvatarPatchConfig
    {
        public string contentPack;
        public string femaleModelId;
        public string maleModelId;
        public string patchName;
    }

    public Dictionary<string, _AvatarPatchConfig> AvatarPatchConfig;
    public override ConfigAvatarPatchConfig combine(List<ConfigAvatarPatchConfig> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarPatchConfig.Keys)
            {
                AvatarPatchConfig[key] = other_list[i].AvatarPatchConfig[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_avatar_patch_config = getJObjectsConfigsListST("AvatarPatchConfig");
    }
}
