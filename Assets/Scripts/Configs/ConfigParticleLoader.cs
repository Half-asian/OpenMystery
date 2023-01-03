using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
public class ConfigParticleConfig : Config<ConfigParticleConfig>
{
    [System.Serializable]
    public class _ParticleConfig
    {
        public string id;
        public string[] materialFilenames;
        public string systemFilename;
        public string[] textureFilenames;
        public string contentPack;
    }

    public Dictionary<string, _ParticleConfig> ParticleConfig;

    public override ConfigParticleConfig combine(List<ConfigParticleConfig> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].ParticleConfig.Keys)
            {
                ParticleConfig[key] = other_list[i].ParticleConfig[key];
            }
        }
        return this;
    }

    public static ConfigParticleConfig getConfig()
    {
        string type = "ParticleConfig";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigParticleConfig> configs = getConfigList(type);
        configs[0].combine(configs);
        return configs[0];
    }
}

public class ConfigParticleInstance : Config<ConfigParticleInstance>
{
    [System.Serializable]
    public class _ParticleInstance
    {
        public string instanceId;
        public float[] scaleOverride;
        public string systemId;
        public Dictionary<string, float> emitterDuratoinOverrides;
        public Dictionary<string, float> emitterTimeToLiveOverrides;
        public Dictionary<string, float[][]> emitterColorOverrides;
    }

    public Dictionary<string, _ParticleInstance> ParticleInstance;

    public override ConfigParticleInstance combine(List<ConfigParticleInstance> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].ParticleInstance.Keys)
            {
                ParticleInstance[key] = other_list[i].ParticleInstance[key];
            }
        }
        return this;
    }

    public static ConfigParticleInstance getConfig()
    {
        string type = "ParticleInstance";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigParticleInstance> configs = getConfigList(type);
        configs[0].combine(configs);
        return configs[0];
    }

}


