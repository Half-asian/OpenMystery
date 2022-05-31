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

    public override void combine(List<ConfigScene> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Scene.Keys)
            {
                Scene[key] = other_list[i].Scene[key];
            }
        }
    }
}


class ConfigSceneLoader
{
    public static async Task loadConfigsAsync()
    {
        await Task.Run(
        () =>
        {
            var settings = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };


            List<ConfigScene> list_scene = new List<ConfigScene>();

            foreach (string config_name in Configs.config_contents.Contents["Scene"])
            {
                byte[] byte_array = File.ReadAllBytes(Common.getConfigPath(config_name));

                if ((char)byte_array[0] != '{')
                {
                    ConfigDecrypt.decrypt(byte_array, Common.getConfigPath(config_name));
                }

                string content = Encoding.UTF8.GetString(byte_array);

                list_scene.Add(JsonConvert.DeserializeObject<ConfigScene>(content, settings));
            }

            Configs.config_scene = list_scene[0];
            
            Configs.config_scene.combine(list_scene);

            foreach (ConfigScene._Scene scene in Configs.config_scene.Scene.Values)
            {
                if (scene.waypoints != null)
                {
                    scene.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
                    foreach (ConfigScene._Scene.WayPoint waypoint in scene.waypoints)
                    {
                        scene.waypoint_dict[waypoint.name] = waypoint;
                    }
                }
                if (scene.proplocators != null)
                {
                    scene.proplocator_dict = new Dictionary<string, ConfigScene._Scene.PropLocator>();
                    foreach (ConfigScene._Scene.PropLocator prop_locator in scene.proplocators)
                    {
                        scene.proplocator_dict[prop_locator.name] = prop_locator;
                    }
                }
                if (scene.cameras != null)
                {
                    scene.camera_dict = new Dictionary<string, ConfigScene._Scene.Camera>();
                    foreach (ConfigScene._Scene.Camera camera in scene.cameras)
                    {
                        scene.camera_dict[camera.name] = camera;
                    }
                }
                if (scene.hotspots != null)
                {
                    scene.hotspot_dict = new Dictionary<string, ConfigScene._Scene.HotSpot>();
                    foreach (ConfigScene._Scene.HotSpot hotspot in scene.hotspots)
                    {
                        scene.hotspot_dict[hotspot.name] = hotspot;
                    }
                }
            }

            Configs.config_scene.Scene["s_MQ5C5P1_rig"].hotspot_dict["hot_project"].position = new float[] { 507.067952f, 98.075569f, 428.769708f };
        }
        );
    }

}

