using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModelLoading;
public class ModelInspector : MonoBehaviour
{
    Model model;
    ActorController am;
    [SerializeField]
    InputField animation_input;

    [SerializeField]
    InputField model_input;
    Prop prop;
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

        if (!string.IsNullOrEmpty(GameStart._model_inspector_model)){
            model = ModelManager.loadModel(GameStart._model_inspector_model);
            prop = model.game_object.AddComponent<Prop>();
            prop.setup(model);;
            model.game_object.transform.position = Vector3.zero;
        }
        else
        {
            am = Actor.spawnActor(GameStart._model_inspector_actor, null, "test");
        }
    }

    public void playAnimation()
    {
        if (model != null)
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
        else
        {
            if (Configs.config_animation.Animation3D.ContainsKey(animation_input.text))
            {
                am.actor_animation.replaceCharacterIdle(animation_input.text);
            }
            else
            {

                am.gameObject.AddComponent<ActorAnimSequence>();
                am.gameObject.GetComponent<ActorAnimSequence>().enabled = true;
                am.gameObject.GetComponent<ActorAnimSequence>().initAnimSequence(animation_input.text, false);
            }
        }

    }
    public void stopAnimation()
    {
        model.game_object.GetComponent<Animation>().Stop();
    }

    public void loadModel()
    {
        if (model != null)
            DestroyImmediate(model.game_object);

        model = ModelManager.loadModel(model_input.text);
        model.game_object.transform.position = Vector3.zero;
        model.game_object.AddComponent<Animation>();

    }

}
