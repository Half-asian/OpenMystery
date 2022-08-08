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
        //DestroyImmediate(model.game_object);

        ModelManager.loadModel(model_input.text);
        model.game_object.transform.position = Vector3.zero;
        model.game_object.AddComponent<Animation>();

    }

}
