﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Numerics;

public class ConfigScene : Config<ConfigScene>
{
    [System.Serializable]
    public class _Scene
    {
        [System.Serializable]
        public class Camera
        {
            public string name;
            public string type;
            public int main;
            public string fullRoomCam;
            public float verticalAOV;
            public float[] position;
            public float[] rotation;
            public float[] fullRoomCam_position;
            public float[] fullRoomCam_rotation;
            public float startTrack;
            public float panSpeedModifier;
            public string animation;
        }
        public List<Camera> cameras;
        public Dictionary<string, Camera> camera_dict;

        public string envId;
        //public string[] labels;
        public string layoutId;
        public string masterSceneId;
        [System.Serializable]
        public class PropLocator
        {
            public string name;
            public int UseShadowAttrs;
            public int visibleOnLoad;
            public string reference;
            public float[] position;
            public float[] scale;
            public float[] rotation;
            public string animation;
            public List<Material> materials;
            public string neededTextureKeys;
            public Dictionary<string, Material> material_dict;
        }
        public List<PropLocator> proplocators;
        public Dictionary<string, PropLocator> proplocator_dict;
        [System.Serializable]
        public class WayPointConnection
        {
            public string[] connection;
        }
        public List<WayPointConnection> waypointconnections;
        [System.Serializable]
        public class WayPoint
        {
            public float[] scale;
            public float[] position;
            public float[] rotation;
            public float flatness;
            public string name;
            public string reference;
            public string animationType;
            public string[] lightLayerOverride;

            public UnityEngine.Vector3 getRotation()
            {
                if (rotation != null)
                {
                    return new UnityEngine.Vector3(0, rotation[1] * -1, 0);
                }
                return UnityEngine.Vector3.zero;
            }

            public UnityEngine.Vector3 getWorldPosition()
            {
                if (position != null)
                    return new UnityEngine.Vector3(position[0] * -0.01f, position[1] * 0.01f, position[2] * 0.01f);
                return UnityEngine.Vector3.zero;
            }

        }
        public List<WayPoint> waypoints;
        public Dictionary<string, WayPoint> waypoint_dict;
        [System.Serializable]
        public class HotSpot
        {
            public float[] position;
            public float[] rotation;
            public string name;
        }
        public List<HotSpot> hotspots;
        public Dictionary<string, HotSpot> hotspot_dict;

        [System.Serializable]
        public class _Lighting
        {
            [System.Serializable]
            public class Light
            {
                public string name;
                public string type;
                public string[] color;
                public float intensity;
                public string[] position;
                public string[] rotation;
                public float dropoff;
                public float coneAngle;
                public float penumbraAngle;
            }
            public Dictionary<string, Light> lights;
            [System.Serializable]
            public class Layer
            {
                public bool fx;
                public string[] lights;
                public List<string> objects;
                public string name;
            }
            public Dictionary<string, Layer> layers;
        }
        public _Lighting Lighting;

        [System.Serializable]
        public class _FogSettings
        {
            public float maxDistance;
            public float minDistance;
            public float maxFog;
            public float heightFogDensity;
            public float heightFogStart;
            public float heightFogEnd;
            public string[] heightFogColor;
            public string[] fogColor;
            public bool flipFogOrder;
        }

        public _FogSettings fogSettings;

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

        [System.Serializable]
        public class EnvironmentMaterial
        {
            public string[] neededTextureStrings;
            public List<Material> materials;
        }
        public EnvironmentMaterial envmaterials;
        public Dictionary<string, Material> material_dict;

    }


    public Dictionary<string, _Scene> Scene;

    public override ConfigScene combine(List<ConfigScene> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Scene.Keys)
            {
                Scene[key] = other_list[i].Scene[key];
            }
        }
        return this;
    }
    public static async Task getConfigAsyncv1()
    {
        await Task.Run(() => {
            //Configs.config_scene = await getJObjectsConfigsListAsyncV2("Scene");
            Configs.config_scene = getJObjectsConfigsListST("Scene");
        }
        );
    }

    public static void getConfig()
    {
        Configs.config_scene = getJObjectsConfigsListST("Scene");
    }
}

public class SceneEnvOverrides : Config<SceneEnvOverrides>
{
    [System.Serializable]

    public class _SceneOverride
    {
        public string masterSceneId;
        public string overrideSceneId;
        public string timedPromoId;
        public string predicate;
        public List<string> exceptions;
    }

    public _SceneOverride[] SceneOverride;

    public class _EnvOverride
    {
        public string envId;
        public string overrideEnvId;
        public string timedPromoId;
        public string predicate;
        public List<string> exceptions;
    }

    public _EnvOverride[] EnvOverride;

    public override SceneEnvOverrides combine(List<SceneEnvOverrides> other_list)
    {
        return this;
    }
}
