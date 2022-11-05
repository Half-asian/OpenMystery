using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System.Globalization;
public class Scene
{
    public static Model scene_model;

    public static ConfigScene._Scene current;

    public static GameObject scene_postprocessing_and_lighting;

    public static Dictionary<string, SceneLight> scene_lights;

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
        if (!Configs.config_scene.Scene.ContainsKey(scene_id))
            throw new System.Exception("Set Scene - invalid scene name.");

        Debug.Log("Loading scene: " + scene_id);

        bool scene_has_changed = false;
        if (current != null)
            scene_has_changed = current.layoutId != Configs.config_scene.Scene[scene_id].layoutId;
        else
            scene_has_changed = true;

        current = Configs.config_scene.Scene[scene_id];

        List<string> related_scenes = addMasterSceneItems(current);

        if (scene_has_changed)
        {
            Location.setLocation();

            destroyScenePrefab();

            foreach(string related_scene_id in related_scenes)
            {
                Debug.Log("CHECKING SCENE ID PREFAB " + related_scene_id);
                Scene.checkAddScenePrefab(related_scene_id);
            }

            if (scene_model != null)
                GameObject.Destroy(scene_model.game_object);
            spawnLights();
            ModelMaterials.lighting_phase = "ENV";
            scene_model = ModelManager.loadModel(current.envId);
            applySceneMaterials();
            setMainCamera();
        }
    }

    public static void spawnLights()
    {
        if (current.Lighting == null || current.Lighting.lights == null)
            return;
        
        scene_lights = new Dictionary<string, SceneLight>();

        if (current.Lighting.layers == null)
        {
            Debug.LogError("NO LAYERS");
            return;
        }

       
        foreach(var light in current.Lighting.lights.Values)
        {
            Debug.Log("Spawning light type " + light.type + " with colour " + light.color[0]);

            switch (light.type)
            {
                case "directionalLight":
                    {
                        GameObject light_go = new GameObject("sceneLight");
                        Light light_component = light_go.AddComponent<Light>();
                        light_component.type = LightType.Directional;
                        light_component.color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture), float.Parse(light.color[1], CultureInfo.InvariantCulture), float.Parse(light.color[2], CultureInfo.InvariantCulture));

                        light_component.transform.rotation = Quaternion.identity;
                        light_component.transform.rotation = Quaternion.identity;
                        light_component.transform.Rotate(new Vector3(0, 0, -float.Parse(light.rotation[2], CultureInfo.InvariantCulture)));
                        light_component.transform.Rotate(new Vector3(0, -float.Parse(light.rotation[1], CultureInfo.InvariantCulture), 0));
                        light_component.transform.Rotate(new Vector3(float.Parse(light.rotation[0], CultureInfo.InvariantCulture), 0, 0));


                        Matrix4x4 translation = Matrix4x4.Translate(Vector3.zero);

                        Matrix4x4 rotation = Matrix4x4.Rotate(light_component.transform.rotation);

                        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(1, 1, 1));

                        Matrix4x4 m = translation * rotation * scale;

                        DirLight d = new DirLight();
                        d.name = light.name;
                        d.preCameraMatrix = m;

                        d.color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[1], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[2], CultureInfo.InvariantCulture) / 255 * light.intensity);

                        Vector3 newdirection = new Vector3(-m[8], -m[9], -m[10]);
                        newdirection.Normalize();
                        d.direction = newdirection;

                        scene_lights[light.name] = d;

                        GameObject.Destroy(light_go);
                        break;
                    }
                case "ambientLight": //Take the highest value
                    {
                        Color color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[1], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[2], CultureInfo.InvariantCulture) / 255 * light.intensity);
                        AmbLight ambLight = new AmbLight();
                        ambLight.color = color;
                        ambLight.name = light.name;
                        scene_lights[light.name] = ambLight;
                        break;
                        /*case "spotLight":


                            break;*/
                    }
                case "spotLight":
                    {
                        GameObject light_go = new GameObject("sceneLight");
                        Light light_component = light_go.AddComponent<Light>();

                        light_component.transform.rotation = Quaternion.identity;
                        light_component.transform.Rotate(new Vector3(0, 0, -float.Parse(light.rotation[2], CultureInfo.InvariantCulture)));
                        light_component.transform.Rotate(new Vector3(0, -float.Parse(light.rotation[1], CultureInfo.InvariantCulture), 0));
                        light_component.transform.Rotate(new Vector3(float.Parse(light.rotation[0], CultureInfo.InvariantCulture), 0, 0));


                        SpotLight spotLight = new SpotLight();
                        spotLight.color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[1], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[2], CultureInfo.InvariantCulture) / 255 * light.intensity);
                        float x = float.Parse(light.position[0], CultureInfo.InvariantCulture);
                        float y = float.Parse(light.position[1], CultureInfo.InvariantCulture);
                        float z = float.Parse(light.position[2], CultureInfo.InvariantCulture);
                        spotLight.position = new Vector3(-x * 0.01f, y * 0.01f, z * 0.01f);

                        Matrix4x4 translation = Matrix4x4.Translate(Vector3.zero);

                        Matrix4x4 rotation = Matrix4x4.Rotate(light_component.transform.rotation);

                        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(1, 1, 1));

                        Matrix4x4 m = translation * rotation * scale;

                        Vector3 newdirection = new Vector3(-m[8], -m[9], -m[10]);
                        newdirection.Normalize();
                        spotLight.direction = newdirection;
                        spotLight.dropoff = light.dropoff;
                        spotLight.penumbraAngle = light.penumbraAngle;
                        spotLight.coneAngle = light.coneAngle;


                        scene_lights[light.name] = spotLight;



                        //GameObject spotlight = new GameObject(light.name);
                        //spotlight.transform.position = new Vector3(float.Parse(light.position[0], CultureInfo.InvariantCulture) * -0.01f, float.Parse(light.position[1], CultureInfo.InvariantCulture) * 0.01f, float.Parse(light.position[2], CultureInfo.InvariantCulture) * 0.01f);
                        //spotlight.transform.rotation = Quaternion.Euler(new Vector3(-float.Parse(light.rotation[0], CultureInfo.InvariantCulture), float.Parse(light.rotation[1], CultureInfo.InvariantCulture) + 180.0f, float.Parse(light.rotation[2], CultureInfo.InvariantCulture)));



                        //Light spotlight_component = spotlight.AddComponent<Light>();
                        //spotlight_component.type = LightType.Spot;
                        //spotlight_component.color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[1], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[2], CultureInfo.InvariantCulture) / 255 * light.intensity);

                        GameObject.DestroyImmediate(light_go);
                        break;
                    }
                case "pointLight":
                    {
                        GameObject light_go = new GameObject("pointLight");
                        Debug.LogError("NEW POINTLIGHT!");
                        PointLight pointLight = new PointLight();
                        pointLight.color = new Color(float.Parse(light.color[0], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[1], CultureInfo.InvariantCulture) / 255 * light.intensity, float.Parse(light.color[2], CultureInfo.InvariantCulture) / 255 * light.intensity);
                        float x = float.Parse(light.position[0], CultureInfo.InvariantCulture);
                        float y = float.Parse(light.position[1], CultureInfo.InvariantCulture);
                        float z = float.Parse(light.position[2], CultureInfo.InvariantCulture);

                        pointLight.position = new Vector3(-x * 0.01f, y * 0.01f, z * 0.01f);
                        light_go.transform.position = new Vector3(-x * 0.01f, y * 0.01f, z * 0.01f);

                        scene_lights[light.name] = pointLight;
                        break;
                    }
            }
        }

    }

    public static void setMainCamera()
    {
        Debug.Log("setMainCamera");
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


    private static void applySceneMaterials()
    {
        if (current.material_dict == null)
            return;

        for(int c = 0; c < scene_model.game_object.transform.childCount; c++)
        {
            Transform child = scene_model.game_object.transform.GetChild(c);
            if (!current.material_dict.ContainsKey(child.name))
                continue;
            Material mat = child.GetComponent<MeshRenderer>().material;
            var material = current.material_dict[child.name];

            if (material.stringValueKeys != null)
            {
                for (int i = 0; i < material.stringValueKeys.Length; i++) {
                    mat.SetTexture(material.stringIds[i], TextureManager.loadTextureDDS(material.stringValueKeys[i]));
                }
            }
            if (material.floatIds != null)
            {
                for (int i = 0; i < material.floatIds.Length; i++)
                {
                    mat.SetFloat(material.floatIds[i], material.floatValues[i]);
                }
            }
            if (material.vec3Ids != null)
            {
                for (int i = 0; i < material.vec3Ids.Length; i++)
                {
                    mat.SetColor(material.vec3Ids[i], new Color(material.vec3Values[i][0], material.vec3Values[i][1], material.vec3Values[i][2]).gamma);
                }
            }
            if (material.vec4Ids != null)
            {

                for (int i = 0; i < material.vec4Ids.Length; i++)
                {
                    mat.SetColor(material.vec4Ids[i], new Color(material.vec4Values[i][0], material.vec4Values[i][1], material.vec4Values[i][2], material.vec4Values[i][3]).gamma);
                }
            }
            if (material.intSettingIds != null)
            {
                for (int i = 0; i < material.intSettingIds.Length; i++)
                {
                    mat.SetFloat(material.intSettingIds[i], material.intSettingValues[i]);

                }
            }

        }
    }

    public static void checkAddScenePrefab(string scene_id)
    {
        return;
        /*string path = scene_id;// + ".prefab";
        GameObject resource_obj = Resources.Load<GameObject>(path);

        if (resource_obj != null)
        {
            GameObject resource = GameObject.Instantiate(resource_obj);
            scene_postprocessing_and_lighting = resource;
            GameStart.post_process_manager.PostProcessDefaultLight.SetActive(false);
        }*/
    }

    public static void destroyScenePrefab()
    {
        GameObject.Destroy(scene_postprocessing_and_lighting);
        GameStart.post_process_manager.PostProcessDefaultLight.SetActive(true);
        scene_postprocessing_and_lighting = null;
    }

    public static List<string> addMasterSceneItems(ConfigScene._Scene current_scene, List<string> master_scenes = null)
    {

        if (master_scenes == null)
            master_scenes = new List<string>();
        if (!master_scenes.Contains(current_scene.layoutId))
            master_scenes.Add(current_scene.layoutId);
        if (current_scene.masterSceneId != null && !master_scenes.Contains(current_scene.masterSceneId))
            master_scenes.Add(current_scene.masterSceneId);

        if (current_scene.masterSceneId == null)
        {
            return master_scenes;
        }
        if (!Configs.config_scene.Scene.ContainsKey(current_scene.masterSceneId))
        {
            return master_scenes;
        }
        if (Configs.config_scene.Scene[current_scene.masterSceneId].masterSceneId != null)
        {
            master_scenes = addMasterSceneItems(Configs.config_scene.Scene[current_scene.masterSceneId], master_scenes);
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
            if (current_scene.Lighting.layers == null)
                current_scene.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();

            foreach (string master_light_name in Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.lights.Keys)
            {
                if (!current_scene.Lighting.lights.ContainsKey(master_light_name))
                {
                    current_scene.Lighting.lights[master_light_name] = Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.lights[master_light_name];
                }
            }
            foreach (string master_layer_name in Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.layers.Keys)
            {
                if (!current_scene.Lighting.layers.ContainsKey(master_layer_name))
                {
                    current_scene.Lighting.layers[master_layer_name] = Configs.config_scene.Scene[current_scene.masterSceneId].Lighting.layers[master_layer_name];
                }
            }
        }

        if (Configs.config_scene.Scene[current_scene.masterSceneId].material_dict != null)
        {
            if (current_scene.material_dict == null)
                current_scene.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();

            foreach (string material_name in Configs.config_scene.Scene[current_scene.masterSceneId].material_dict.Keys)
            {
                if (!current_scene.material_dict.ContainsKey(material_name))
                {
                    current_scene.material_dict[material_name] = Configs.config_scene.Scene[current_scene.masterSceneId].material_dict[material_name];
                }
            }
        }

        return master_scenes;

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
                Debug.LogError("Scene " + current.layoutId + " does not define hotspot " + hotspot_id);
                go.transform.position = Vector3.zero;
                foreach (string key in current.hotspot_dict.Keys)
                {
                    if (key.Contains(hotspot_id)){
                        ConfigScene._Scene.HotSpot hotspot = current.hotspot_dict[key];
                        go.transform.position = new Vector3(hotspot.position[0] * -0.01f, hotspot.position[1] * 0.01f + 0.2f, hotspot.position[2] * 0.01f);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Scene " + current.layoutId + " does not have any hotspots. Tried to find " + hotspot_id);
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
                        actor_gameobject.transform.localScale = new Vector3(w.scale[0], w.scale[1], w.scale[2]); 
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