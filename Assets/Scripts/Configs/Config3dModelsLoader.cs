using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ConfigTexture : Config<ConfigTexture>
{
    [System.Serializable]
    public class Type
    {
        public string _unused;
    }
    public Type[] type;
    [System.Serializable]
    public class _TextureConfig
    {
        public string filename;
        public string id;
    }
    public Dictionary<string, _TextureConfig> TextureConfig;

    public override void combine(List<ConfigTexture> other_list)
    {
        throw new NotImplementedException();
    }
}
public class Config3DModel : Config<Config3DModel>
{
    [System.Serializable]
    public class _Config3DModel
    {
        public string name;

        [System.Serializable]
        public class JsonData
        {
            public string[] neededTextureKeys;
            public string mesh;
            [System.Serializable]
            public class Material
            {
                public string nodeName;
                public List<float[]> vec3Values;
                public List<float[]> vec4Values;
                public string[] vec3Ids;
                public string[] vec4Ids;
                public string[] stringValueKeys;
                public string shaderName;
                public string[] stringIds;
                public int CastShadow;
                public int VertexLighting;
                public float[] floatValues;
                public string[] floatIds;
                public int[] intSettingValues;
                public string[] intSettingIds;
                public int transparent;
                public int RecieveShadow;
            }
            public Material[] materials;
            public Dictionary<string, Material> material_dict;
            public float[] boundingBox;
            public int rigVersion;
        }
        public JsonData[] jsonData;
    }
    [JsonProperty(PropertyName = "3DModelConfig")]
    public Dictionary<string, _Config3DModel> ModelConfig;

    public void createMaterialDict()
    {
        foreach (_Config3DModel mc in ModelConfig.Values)
        {
            mc.jsonData[0].material_dict = new Dictionary<string, _Config3DModel.JsonData.Material>();
            foreach (_Config3DModel.JsonData.Material m in mc.jsonData[0].materials)
            {
                mc.jsonData[0].material_dict[m.nodeName] = m;
            }
        }
    }

    public override void combine(List<Config3DModel> other_list)
    {
        throw new NotImplementedException();
    }
}

class Config3dModelsLoader
{
    public static async Task loadConfigsAsync()
    {
        Configs.config_texture = await ConfigTexture.CreateFromJSONAsync(Common.getConfigPath("TextureConfig-"));
        Configs.config_character_model = await Config3DModel.CreateFromJSONAsync(Common.getConfigPath("3DModelConfig_Characters-"));
        Configs.config_character_model.createMaterialDict();
        Configs.config_environment_model = await Config3DModel.CreateFromJSONAsync(Common.getConfigPath("3DModelConfig_Environment-"));
        Configs.config_environment_model.createMaterialDict();
        Configs.config_fx = await Config3DModel.CreateFromJSONAsync(Common.getConfigPath("3DModelConfig_FX-"));
        Configs.config_fx.createMaterialDict();
        Configs.config_outfit_model = await Config3DModel.CreateFromJSONAsync(Common.getConfigPath("3DModelConfig_Outfit-"));
        Configs.config_outfit_model.createMaterialDict();
        Configs.config_prop_model = await Config3DModel.CreateFromJSONAsync(Common.getConfigPath("3DModelConfig_Props-"));
        Configs.config_prop_model.createMaterialDict();
    }
}
