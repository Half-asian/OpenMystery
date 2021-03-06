using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

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
        public Camera[] cameras;
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
        }
        public PropLocator[] proplocators;
        public Dictionary<string, PropLocator> proplocator_dict;
        [System.Serializable]
        public class WayPointConnection
        {
            public string[] connection;
        }
        public WayPointConnection[] waypointconnections;
        [System.Serializable]
        public class WayPoint
        {
            public float[] scale;
            public float[] position;
            public float[] rotation;
            public string name;
            public string reference;
            public string animationType;
            public string[] lightLayerOverride;
        }
        public WayPoint[] waypoints;
        public Dictionary<string, WayPoint> waypoint_dict;
        [System.Serializable]
        public class HotSpot
        {
            public float[] position;
            public float[] rotation;
            public string name;
        }
        public HotSpot[] hotspots;
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
            }
            public Dictionary<string, Light> lights;
        }
        public _Lighting Lighting;
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
