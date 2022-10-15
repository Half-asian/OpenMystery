using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModelLoading;
public class ModelInspector : MonoBehaviour
{
    Model model;
    ActorController actor_controller;
    [SerializeField]
    InputField animation_input;

    [SerializeField]
    InputField model_input;
    [SerializeField]
    InputField actor_input;
    Prop prop;

    [SerializeField]
    GameObject directional_light;
    [SerializeField]
    GameObject canvas;
    void Start()
    {
        Scene.current = new ConfigScene._Scene();
        Scene.current.Lighting = new ConfigScene._Scene._Lighting();
        Scene.current.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();
        Scene.current.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();

        ConfigScene._Scene._Lighting.Layer env_layer = new ConfigScene._Scene._Lighting.Layer();
        env_layer.name = "CHARACTER";
        env_layer.lights = new string[] { "amb" };
        Scene.current.Lighting.layers["CHARACTER"] = env_layer;

        ConfigScene._Scene._Lighting.Light amb_light = new ConfigScene._Scene._Lighting.Light();
        amb_light.color = new string[] { "255.0", "255.0", "255.0" };
        amb_light.intensity = 1.0f;
        amb_light.name = "amb";
        amb_light.type = "ambientLight";

        Scene.current.Lighting.lights["amb"] = amb_light;
        Scene.spawnLights();


        ConfigScene._Scene.WayPoint waypoint1 = new ConfigScene._Scene.WayPoint();
        waypoint1.position = new float[] { 20f, 0, 20f };

        ConfigScene._Scene.WayPoint waypoint2 = new ConfigScene._Scene.WayPoint();
        waypoint2.position = new float[] { 0, 0, 50 };
        waypoint2.rotation = new float[] { 0, 162.99f, 0 };

        ConfigScene._Scene.WayPoint waypoint3 = new ConfigScene._Scene.WayPoint();
        waypoint3.position = new float[] { 0, 0, 100 };
        waypoint3.rotation = new float[] { 0, 177.28f, 0 };

        Scene.current.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
        Scene.current.waypoint_dict["waypoint1"] = waypoint1;
        Scene.current.waypoint_dict["waypoint2"] = waypoint2;
        Scene.current.waypoint_dict["waypoint3"] = waypoint3;

        Debug.LogError(LocalData.getLine("TEST_QuidditchRivalHouse"));

    }

    private void Update()
    {
        if (Input.GetKeyDown("f4"))
        {
            canvas.SetActive(!canvas.activeSelf);
        }
    }

    public void playAnimation()
    {
        if (prop is not null) //Prop
        {
            if (Configs.config_animation.Animation3D.ContainsKey(animation_input.text))
            {
                HPAnimation anim = AnimationManager.loadAnimationClip(animation_input.text, model, null);
                if (anim == null)
                {
                    Debug.LogError("unknown anim " + anim);
                }
                prop.playAnimationOnComponent(anim);
            }
            else
            {
                prop.playAnimSequence(animation_input.text);
            }
        }
        else //Actor
        {
            if (Configs.config_animation.Animation3D.ContainsKey(animation_input.text))
            {
                actor_controller.replaceCharacterIdle(animation_input.text);
            }
            else
            {

                actor_controller.gameObject.AddComponent<ActorAnimSequence>();
                actor_controller.gameObject.GetComponent<ActorAnimSequence>().enabled = true;
                actor_controller.gameObject.GetComponent<ActorAnimSequence>().initAnimSequence(animation_input.text, false);
            }
            model.game_object.GetComponent<Animation>().Play();
        }

    }
    public void stopAnimation()
    {
        model.game_object.GetComponent<Animation>().Stop();
    }

    public void advanceAnimSequence()
    {
        if (actor_controller != null)
        {
            if (actor_controller.gameObject.GetComponent<ActorAnimSequence>() != null)
                actor_controller.gameObject.GetComponent<ActorAnimSequence>().advanceAnimSequence();
            else
                Debug.LogError("Failed to advance");
        }
        else {
            if (model.game_object.GetComponent<PropAnimSequence>() != null)
                model.game_object.GetComponent<PropAnimSequence>().advanceAnimSequence();
            else
                Debug.LogError("Failed to advance");
        }

    }

    public void loadModel()
    {
        if (prop != null)
        {
            DestroyImmediate(prop.model.game_object);
            DestroyImmediate(prop);
            prop = null;
        }
        if (actor_controller != null)
            actor_controller.destroy();

        model = ModelManager.loadModel(model_input.text);
        model.game_object.transform.position = Vector3.zero;
        model.game_object.AddComponent<Animation>();
        prop = model.game_object.AddComponent<Prop>();
        prop.setup(model);
    }

    public void loadActor()
    {
        if (prop != null)
        {
            DestroyImmediate(prop.model.game_object);
            DestroyImmediate(prop);
            prop = null;
        }
        if (actor_controller != null)
            actor_controller.destroy();

        actor_controller = Actor.spawnActor(actor_input.text, null, actor_input.text);
        model = actor_controller.model;
        actor_controller.model.game_object.transform.position = Vector3.zero;
    }

    public void toggleLight()
    {
        directional_light.SetActive(!directional_light.activeSelf);
    }

}
