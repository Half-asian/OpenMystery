using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System.Globalization;
using static SceneEnvOverrides;

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
        overrideScene(ref current);
        Debug.Log(current != null);
        Debug.Log(current.Lighting != null);
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
            spawnSceneModel();
            applySceneMaterials();
            setMainCamera();
        }
    }

    private static void spawnSceneModel()
    {
        ModelMaterials.lighting_layers = new List<string>() {};
        foreach(var layer in current.Lighting.layers.Values)
        {
            if (layer.objects.Contains("ENVIRONMENT"))
            {
                ModelMaterials.lighting_layers.Add(layer.name);
            }
        }
        Debug.Log("Spawning scene model");
        scene_model = ModelManager.loadModel(getEnvOverride(current.envId));
    }

    private static string getEnvOverride(string envId)
    {
        foreach(var envOverride in Configs.config_scene_env_override.EnvOverride)
        {
            if (envOverride.envId == envId) {
                if (TimedPromo.isTimedPromoActive(envOverride.timedPromoId))
                {
                    if (envOverride.exceptions == null ||
                        !envOverride.exceptions.Contains(Scenario.current.scenario_config.scenarioId))
                    {
                        if (envOverride.predicate == null ||
                            Predicate.parsePredicate(envOverride.predicate)){
                            Debug.Log("Overrideing " + envId + " with " + envOverride.overrideEnvId);

                            return envOverride.overrideEnvId;
                        }
                    }
                }
            }
        }
        return envId;
    }

    private static void overrideScene(ref ConfigScene._Scene current)
    {
        string scene_override_id = getSceneOverride(current.layoutId);
        if (scene_override_id == current.layoutId)
            return;
        Debug.Log("OVERRIDING");
        var scene_override = Configs.config_scene.Scene[scene_override_id];
        if (scene_override.cameras != null)
        {
            if (current.cameras == null)
                current.cameras = new List<ConfigScene._Scene.Camera>();
            if (current.camera_dict == null)
                current.camera_dict = new Dictionary<string, ConfigScene._Scene.Camera>();
            current.cameras.AddRange(scene_override.cameras);
            foreach(var camera in scene_override.cameras)
            {
                current.camera_dict[camera.name] = camera;
            }
        }
        if (scene_override.proplocators != null)
        {
            if (current.proplocators == null)
                current.proplocators = new List<ConfigScene._Scene.PropLocator>();
            if (current.proplocator_dict == null)
                current.proplocator_dict = new Dictionary<string, ConfigScene._Scene.PropLocator>();
            current.proplocators.AddRange(scene_override.proplocators);
            foreach (var proplocator in scene_override.proplocators)
            {
                current.proplocator_dict[proplocator.name] = proplocator;
            }
        }
        if (scene_override.waypoints != null)
        {
            if (current.waypoints == null)
                current.waypoints = new List<ConfigScene._Scene.WayPoint>();
            if (current.waypoint_dict == null)
                current.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
            current.waypoints.AddRange(scene_override.waypoints);
            foreach (var waypoint in scene_override.waypoints)
            {
                if (!current.waypoint_dict.ContainsKey(waypoint.name)) //Don't overwrite, seems worse
                    current.waypoint_dict[waypoint.name] = waypoint;
            }
        }
        if (scene_override.waypointconnections != null)
        {
            if (current.waypointconnections == null)
                current.waypointconnections = new List<ConfigScene._Scene.WayPointConnection>();
            current.waypointconnections.AddRange(scene_override.waypointconnections);
        }
        if (scene_override.hotspots != null)
        {
            if (current.hotspots == null)
                current.hotspots = new List<ConfigScene._Scene.HotSpot>();
            if (current.hotspot_dict == null)
                current.hotspot_dict = new Dictionary<string, ConfigScene._Scene.HotSpot>();
            current.hotspots.AddRange(scene_override.hotspots);
            foreach (var hotspot in scene_override.hotspots)
            {
                current.hotspot_dict[hotspot.name] = hotspot;
            }
        }
        if (scene_override.Lighting != null)
            current.Lighting = scene_override.Lighting;
        if (scene_override.fogSettings != null)
            current.fogSettings = scene_override.fogSettings;
        if (scene_override.envmaterials != null) {
            if (current.material_dict == null)
                current.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();
            current.envmaterials = scene_override.envmaterials;
            foreach(var envMaterial in scene_override.envmaterials.materials)
            {
                current.material_dict[envMaterial.nodeName] = envMaterial;
            }

        }
        if (scene_override.layoutId != null)
            current.layoutId = scene_override.layoutId;
        if (scene_override.envId != null)
            current.envId = scene_override.envId;
    }

    private static string getSceneOverride(string sceneId)
    {
        Debug.Log("Checking Scene override " + sceneId);
        foreach (var sceneOverride in Configs.config_scene_env_override.SceneOverride)
        {
            if (sceneOverride.masterSceneId == sceneId)
            {
                if (TimedPromo.isTimedPromoActive(sceneOverride.timedPromoId))
                {
                    if (sceneOverride.exceptions == null ||
                        !sceneOverride.exceptions.Contains(Scenario.current.scenario_config.scenarioId))
                    {
                        if (sceneOverride.predicate == null ||
                            Predicate.parsePredicate(sceneOverride.predicate))
                        {
                            Debug.Log("Found Scene override " + sceneOverride.overrideSceneId);
                            return sceneOverride.overrideSceneId;
                        }
                    }
                }
            }
        }
        return sceneId;
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
                        spotLight.coneAngle = light.coneAngle * 0.5f * 0.017453f; //degrees to radian
                        //if coneAngle was not set, penumbraAngle = coneAngle
                        spotLight.penumbraAngle = spotLight.coneAngle + (light.penumbraAngle * 0.017453f);


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
            var meshrenderer = child.GetComponent<MeshRenderer>();
            if (meshrenderer == null)
                continue;
            Material mat = meshrenderer.material;
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

    public static ConfigScene._Scene.WayPoint getWayPoint(string waypoint_id)
    {
        if (current == null || waypoint_id == null)
            return null;
        if (current.waypoint_dict == null)
            return null;
        if (!current.waypoint_dict.ContainsKey(waypoint_id))
        {
            Debug.LogError("Scene.getWayPoint(string waypoint_id): Current scene does not contain waypoint " + waypoint_id);
            return null;
        }
        return current.waypoint_dict[waypoint_id];
    }


    public static List<string> addMasterSceneItems(ConfigScene._Scene current_scene, List<string> master_scenes = null)
    {

        if (master_scenes == null)
            master_scenes = new List<string>();
        if (!master_scenes.Contains(current_scene.layoutId))
            master_scenes.Add(current_scene.layoutId);

        string master_scene_id = getSceneOverride(current_scene.masterSceneId);
        

        if (master_scene_id != null && !master_scenes.Contains(master_scene_id))
            master_scenes.Add(master_scene_id);

        if (master_scene_id == null)
        {
            return master_scenes;
        }
        if (!Configs.config_scene.Scene.ContainsKey(master_scene_id))
        {
            return master_scenes;
        }
        if (Configs.config_scene.Scene[master_scene_id].masterSceneId != null)
        {
            master_scenes = addMasterSceneItems(Configs.config_scene.Scene[master_scene_id], master_scenes);
        }

        if (Configs.config_scene.Scene[master_scene_id].camera_dict != null)
        {
            if (current_scene.camera_dict == null)
            {
                current_scene.camera_dict = new Dictionary<string, ConfigScene._Scene.Camera>();
            }
            foreach (string master_camera_name in Configs.config_scene.Scene[master_scene_id].camera_dict.Keys)
            {
                if (!current_scene.camera_dict.ContainsKey(master_camera_name))
                {
                    current_scene.camera_dict[master_camera_name] = Configs.config_scene.Scene[master_scene_id].camera_dict[master_camera_name];
                }
            }
        }

        if (Configs.config_scene.Scene[master_scene_id].waypoint_dict != null)
        {
            if (current_scene.waypoint_dict == null)
            {
                current_scene.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
            }
            foreach (string master_waypoint_name in Configs.config_scene.Scene[master_scene_id].waypoint_dict.Keys)
            {
                if (!current_scene.waypoint_dict.ContainsKey(master_waypoint_name))
                {
                    current_scene.waypoint_dict[master_waypoint_name] = Configs.config_scene.Scene[master_scene_id].waypoint_dict[master_waypoint_name];
                }
            }
        }
        if (Configs.config_scene.Scene[master_scene_id].proplocator_dict != null)
        {
            if (current_scene.proplocator_dict == null)
            {
                current_scene.proplocator_dict = new Dictionary<string, ConfigScene._Scene.PropLocator>();
            }

            foreach (string master_proplocator_name in Configs.config_scene.Scene[master_scene_id].proplocator_dict.Keys)
            {
                if (!current_scene.proplocator_dict.ContainsKey(master_proplocator_name))
                {
                    current_scene.proplocator_dict[master_proplocator_name] = Configs.config_scene.Scene[master_scene_id].proplocator_dict[master_proplocator_name];
                }
                else
                {
                    var master_pl = Configs.config_scene.Scene[master_scene_id].proplocator_dict[master_proplocator_name];
                    var child_pl = current_scene.proplocator_dict[master_proplocator_name];
                    if (master_pl.material_dict != null)
                    {
                        if (child_pl.material_dict == null)
                            child_pl.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();
                        foreach (var mat_name in master_pl.material_dict.Keys)
                        {
                            if (!child_pl.material_dict.ContainsKey(mat_name))
                            {
                                child_pl.material_dict.Add(mat_name, master_pl.material_dict[mat_name]);
                            }
                        }
                    }
                }
            }
        }
        if (Configs.config_scene.Scene[master_scene_id].hotspot_dict != null)
        {
            if (current_scene.hotspot_dict == null)
            {
                current_scene.hotspot_dict = new Dictionary<string, ConfigScene._Scene.HotSpot>();
            }

            foreach (string master_hotspot_name in Configs.config_scene.Scene[master_scene_id].hotspot_dict.Keys)
            {
                if (!current_scene.hotspot_dict.ContainsKey(master_hotspot_name))
                {
                    current_scene.hotspot_dict[master_hotspot_name] = Configs.config_scene.Scene[master_scene_id].hotspot_dict[master_hotspot_name];
                }
            }
        }

        if (Configs.config_scene.Scene[master_scene_id].Lighting != null)
        {
            if (current_scene.Lighting == null)
                current_scene.Lighting = new ConfigScene._Scene._Lighting();
            if (current_scene.Lighting.lights == null)
                current_scene.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();
            if (current_scene.Lighting.layers == null)
                current_scene.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();

            foreach (string master_light_name in Configs.config_scene.Scene[master_scene_id].Lighting.lights.Keys)
            {
                if (!current_scene.Lighting.lights.ContainsKey(master_light_name))
                {
                    current_scene.Lighting.lights[master_light_name] = Configs.config_scene.Scene[master_scene_id].Lighting.lights[master_light_name];
                }
            }
            foreach (string master_layer_name in Configs.config_scene.Scene[master_scene_id].Lighting.layers.Keys)
            {
                if (!current_scene.Lighting.layers.ContainsKey(master_layer_name))
                {
                    current_scene.Lighting.layers[master_layer_name] = Configs.config_scene.Scene[master_scene_id].Lighting.layers[master_layer_name];
                }
            }
        }

        if (Configs.config_scene.Scene[master_scene_id].material_dict != null)
        {
            if (current_scene.material_dict == null)
                current_scene.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();

            foreach (string material_name in Configs.config_scene.Scene[master_scene_id].material_dict.Keys)
            {
                if (!current_scene.material_dict.ContainsKey(material_name))
                {
                    current_scene.material_dict[material_name] = Configs.config_scene.Scene[master_scene_id].material_dict[material_name];
                }
            }
        }

        if (Configs.config_scene.Scene[master_scene_id].fogSettings != null)
        {
            if (current_scene.fogSettings == null)
                current_scene.fogSettings = Configs.config_scene.Scene[master_scene_id].fogSettings;
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