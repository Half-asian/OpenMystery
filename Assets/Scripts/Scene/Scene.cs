using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Scene
{
    public static Model scene_model;

    public static ConfigScene._Scene current;

    public static event Action onSceneChanged = delegate { };

    public static GameObject scene_postprocessing_and_lighting;

    public static void Initialize(){
        GameStart.onReturnToMenu += cleanup;
    }

    static void cleanup()
    {
        current = null;
        if (scene_model != null)
            GameObject.Destroy(scene_model.game_object);
        scene_model = null;
    }

    public static void setCurrentScene(string scene_id)
    {
        destroyScenePrefab();
        if (!Configs.config_scene.Scene.ContainsKey(scene_id))
            throw new System.Exception("Set Scene - invalid scene name.");

        Debug.Log("Loading scene: " + scene_id);

        bool scene_has_changed = false;
        if (current != null)
            scene_has_changed = current.layoutId != Configs.config_scene.Scene[scene_id].layoutId;
        else
            scene_has_changed = true;

        current = Configs.config_scene.Scene[scene_id];

        addMasterSceneItems(current);

        if (scene_has_changed)
        {
            Scene.checkAddScenePrefab(current.layoutId);
            Scene.checkAddScenePrefab(current.masterSceneId);

            if (scene_model != null)
                GameObject.Destroy(scene_model.game_object);
            scene_model = ModelManager.loadModel(current.envId);
            onSceneChanged.Invoke();

            spawnLights();

        }
    }

    private static void spawnLights()
    {
        if (current.Lighting == null || current.Lighting.lights == null)
            return;

        foreach(ConfigScene._Scene._Lighting.Light light in current.Lighting.lights.Values)
        {
            Debug.Log("Spawning light type " + light.type + " with colour " + light.color[0]);
            GameObject light_go = new GameObject();

            float average_r = 0;
            float average_g = 0;
            float average_b = 0;
            int directional_light_counter = 0;



            switch (light.type)
            {
                case "directionalLight":
                    Light light_component = light_go.AddComponent<Light>();
                    light_component.type = LightType.Directional;
                    average_r += float.Parse(light.color[0]) / 255;
                    average_g += float.Parse(light.color[1]) / 255;
                    average_b += float.Parse(light.color[2]) / 255;
                    directional_light_counter++;
                    Debug.Log(light.intensity);
                    break;
                case "ambientLight":
                    Color color = new Color(float.Parse(light.color[0]) / 255, float.Parse(light.color[1]) / 255, float.Parse(light.color[2]) / 255);
                    GameStart.post_process_manager.changeFilter(color);
                    //GameStart.post_process_manager.PostProcessDefaultLight.GetComponent<Light>().color = color;
                    //GameStart.post_process_manager.PostProcessDefaultLight.GetComponent<Light>().intensity = 0.1f;
                    break;
            }
            GameStart.post_process_manager.PostProcessDefaultLight.GetComponent<Light>().color = 
                new Color(average_r / directional_light_counter, average_g / directional_light_counter, average_b / directional_light_counter);
            //GameStart.post_process_manager.PostProcessDefaultLight.GetComponent<Light>().intensity = 0.1f;


        }
    }

    public static void setMainCamera()
    {
        if (current.camera_dict == null) return;
        ConfigScene._Scene.Camera c = null;


        foreach (ConfigScene._Scene.Camera cam in current.camera_dict.Values)
        {
            if (cam.main == 1)
            {
                string[] paramaters = new string[] { cam.name, "0" };
                CameraManager.current.focusCam(ref paramaters);
                return;
            }
            c = cam;
        }

        //If we didn't find a main cam just set it to any.
        string[] a = new string[] { c.name, "0" };
        CameraManager.current.focusCam(ref a);
    }


    public static void checkAddScenePrefab(string scene_id)
    {
        string path = scene_id;// + ".prefab";
        GameObject resource_obj = Resources.Load<GameObject>(path);

        if (resource_obj != null)
        {
            GameObject resource = GameObject.Instantiate(resource_obj);
            scene_postprocessing_and_lighting = resource;
            GameStart.post_process_manager.PostProcessDefaultLight.SetActive(false);
        }
    }

    public static void destroyScenePrefab()
    {
        GameObject.Destroy(scene_postprocessing_and_lighting);
        GameStart.post_process_manager.PostProcessDefaultLight.SetActive(true);
        scene_postprocessing_and_lighting = null;
    }

    public static void addMasterSceneItems(ConfigScene._Scene current_scene)
    {
        if (current_scene.masterSceneId == null)
            return;
        if (!Configs.config_scene.Scene.ContainsKey(current_scene.masterSceneId))
            return;

        if (Configs.config_scene.Scene[current_scene.masterSceneId].masterSceneId != null)
        {
            addMasterSceneItems(Configs.config_scene.Scene[current_scene.masterSceneId]);
        }

        if (Configs.config_scene.Scene[current_scene.masterSceneId].camera_dict != null)
        {
            if (current_scene.camera_dict == null)
            {
                current_scene.camera_dict = new Dictionary<string, ConfigScene._Scene.Camera>();
            }
            foreach (string master_camera_name in Configs.config_scene.Scene[current_scene.masterSceneId].camera_dict.Keys)
            {
                if (!current_scene.camera_dict.ContainsKey(master_camera_name))
                {
                    current_scene.camera_dict[master_camera_name] = Configs.config_scene.Scene[current_scene.masterSceneId].camera_dict[master_camera_name];
                }
            }
        }

        if (Configs.config_scene.Scene[current_scene.masterSceneId].waypoint_dict != null)
        {
            if (current_scene.waypoint_dict == null)
            {
                current_scene.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
            }
            foreach (string master_waypoint_name in Configs.config_scene.Scene[current_scene.masterSceneId].waypoint_dict.Keys)
            {
                if (!current_scene.waypoint_dict.ContainsKey(master_waypoint_name))
                {
                    current_scene.waypoint_dict[master_waypoint_name] = Configs.config_scene.Scene[current_scene.masterSceneId].waypoint_dict[master_waypoint_name];
                }
            }
        }
        if (Configs.config_scene.Scene[current_scene.masterSceneId].proplocator_dict != null)
        {
            if (current_scene.proplocator_dict == null)
            {
                current_scene.proplocator_dict = new Dictionary<string, ConfigScene._Scene.PropLocator>();
            }

            foreach (string master_proplocator_name in Configs.config_scene.Scene[current_scene.masterSceneId].proplocator_dict.Keys)
            {
                if (!current_scene.proplocator_dict.ContainsKey(master_proplocator_name))
                {
                    current_scene.proplocator_dict[master_proplocator_name] = Configs.config_scene.Scene[current_scene.masterSceneId].proplocator_dict[master_proplocator_name];
                }
            }
        }
        if (Configs.config_scene.Scene[current_scene.masterSceneId].hotspot_dict != null)
        {
            if (current_scene.hotspot_dict == null)
            {
                current_scene.hotspot_dict = new Dictionary<string, ConfigScene._Scene.HotSpot>();
            }

            foreach (string master_hotspot_name in Configs.config_scene.Scene[current_scene.masterSceneId].hotspot_dict.Keys)
            {
                if (!current_scene.hotspot_dict.ContainsKey(master_hotspot_name))
                {
                    current_scene.hotspot_dict[master_hotspot_name] = Configs.config_scene.Scene[current_scene.masterSceneId].hotspot_dict[master_hotspot_name];
                }
            }
        }

        if (Configs.config_scene.Scene[current_scene.masterSceneId].Lighting != null)
        {
            if (current_scene.Lighting == null)
                current_scene.Lighting = new ConfigScene._Scene._Lighting();
            if (current_scene.Lighting.lights == null)
                current_scene.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();


            foreach (string master_light_name in Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.lights.Keys)
            {
                if (!current_scene.Lighting.lights.ContainsKey(master_light_name))
                {
                    current_scene.Lighting.lights[master_light_name] = Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.lights[master_light_name];
                }
            }
        }

    }

    public static void setGameObjectToHotspot(GameObject go, string hotspot_id)
    {
        if (current.hotspot_dict != null)
        {
            if (current.hotspot_dict.ContainsKey(hotspot_id))
            {
                ConfigScene._Scene.HotSpot hotspot = current.hotspot_dict[hotspot_id];
                go.transform.position = new Vector3(hotspot.position[0] * -0.01f, hotspot.position[1] * 0.01f + 0.2f, hotspot.position[2] * 0.01f);

                //We ignore rotation because interactions always face camera
            }
            else
            {
                Debug.LogError("Scene " + Scene.current.envId + " does not define hotspot " + hotspot_id);
            }
        }
        else
        {
            Debug.LogError("Scene " + current.envId + " does not have any hotspots. Tried to find " + hotspot_id);
        }
    }

    public static void setGameObjectToWaypoint(GameObject actor_gameobject, string waypoint_id)
    {
        if (current.waypoint_dict != null)
        {
            if (current.waypoint_dict.ContainsKey(waypoint_id))
            {
                ConfigScene._Scene.WayPoint w = Scene.current.waypoint_dict[waypoint_id];

                if (actor_gameobject != null)
                {
                    Vector3 temp = new Vector3(w.position[0] * -0.01f, w.position[1] * 0.01f, w.position[2] * 0.01f);
                    actor_gameobject.transform.position = temp;
                    if (w.rotation != null)
                    {
                        actor_gameobject.transform.rotation = Quaternion.identity;
                        actor_gameobject.transform.Rotate(new Vector3(0, 0, -w.rotation[2]));
                        actor_gameobject.transform.Rotate(new Vector3(0, -w.rotation[1], 0));
                        actor_gameobject.transform.Rotate(new Vector3(w.rotation[0], 0, 0));
                    }
                    else
                    {
                        actor_gameobject.transform.rotation = Quaternion.Euler(Vector3.zero);
                    }
                    if (w.scale != null)
                    {
                        //The whole scene is actually set to 0.01 * scale
                        //We can only set localscale, so need to adjust
                        actor_gameobject.transform.localScale = new Vector3(w.scale[0] / 100.0f, w.scale[1] / 100.0f, w.scale[2] / 100.0f); 
                    }
                }
            }
            else
            {
                Debug.LogError("Scene " + current.envId + " does not define waypoint " + waypoint_id);
            }
        }
        else
        {
            Debug.LogError("Scene " + current.envId + " does not have any waypoints. Tried to find " + waypoint_id);
        }
    }
}