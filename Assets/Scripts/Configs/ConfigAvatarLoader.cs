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

    public override void combine(List<ConfigAvatarComponents> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarComponents.Keys)
            {
                AvatarComponents[key] = other_list[i].AvatarComponents[key];
            }
        }
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

    public override void combine(List<ConfigAvatarOutfitData> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarOutfitData.Keys)
            {
                AvatarOutfitData[key] = other_list[i].AvatarOutfitData[key];
            }
        }
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
    public override void combine(List<ConfigAvatarAttributeColors> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarAttributeColors.Keys)
            {
                AvatarAttributeColors[key] = other_list[i].AvatarAttributeColors[key];
            }
        }
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
    public override void combine(List<ConfigAvatarPatchConfig> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].AvatarPatchConfig.Keys)
            {
                AvatarPatchConfig[key] = other_list[i].AvatarPatchConfig[key];
            }
        }
    }
}

public class ConfigsAvatarLoader
{
    public static async Task loadConfigsAsync()
    {
        List<ConfigAvatarComponents> list_avatar_components = await ConfigAvatarComponents.getDeserializedConfigsList("AvatarComponents");
        Configs.config_avatar_components = list_avatar_components[0];
        Configs.config_avatar_components.combine(list_avatar_components);

        List<ConfigAvatarAttributeColors> list_avatar_attribute_colors = await ConfigAvatarAttributeColors.getDeserializedConfigsList("AvatarAttributeColors");
        Configs.config_avatar_attribute_colors = list_avatar_attribute_colors[0];
        Configs.config_avatar_attribute_colors.combine(list_avatar_attribute_colors);

        List<ConfigAvatarOutfitData> list_avatar_outfit_datas = await ConfigAvatarOutfitData.getDeserializedConfigsList("AvatarOutfitData");
        Configs.config_avatar_outfit_data = list_avatar_outfit_datas[0];
        Configs.config_avatar_outfit_data.combine(list_avatar_outfit_datas);

        List<ConfigAvatarPatchConfig> list_avatar_patch_config = await ConfigAvatarPatchConfig.getDeserializedConfigsList("AvatarPatchConfig");
        Configs.config_avatar_patch_config = list_avatar_patch_config[0];
        Configs.config_avatar_patch_config.combine(list_avatar_patch_config);

        AvatarComponents.avatar_components_hair = new List<string>();
        foreach (string key in Configs.config_avatar_components.AvatarComponents.Keys)
        {
            if (Configs.config_avatar_components.AvatarComponents[key].category == "hair")
                AvatarComponents.avatar_components_hair.Add(key);
        }

        AvatarComponents.avatar_components_tops = new List<string>();
        foreach (string key in Configs.config_avatar_components.AvatarComponents.Keys)
        {
            if (Configs.config_avatar_components.AvatarComponents[key].category == "tops")
                AvatarComponents.avatar_components_tops.Add(key);
        }

        AvatarComponents.avatar_components_one_piece = new List<string>();
        foreach (string key in Configs.config_avatar_components.AvatarComponents.Keys)
        {
            if (Configs.config_avatar_components.AvatarComponents[key].category == "one-piece")
                AvatarComponents.avatar_components_one_piece.Add(key);
        }

        AvatarComponents.avatar_components_bottoms = new List<string>();
        foreach (string key in Configs.config_avatar_components.AvatarComponents.Keys)
        {
            if (Configs.config_avatar_components.AvatarComponents[key].category == "bottoms")
                AvatarComponents.avatar_components_bottoms.Add(key);
        }
    }
}
