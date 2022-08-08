using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using ModelLoading;
public class Prop : ModelHolder
{
    public static Dictionary<string, Prop> spawned_props = new Dictionary<string, Prop>();
    public string _name;
    public string group;
    public spawner spawned_by;
    public void setup(string _name, Model _model, spawner _spawned_by, string _group)
    {
        base.setup(_model);
        this._name = _name;
        model = _model;
        spawned_by = _spawned_by;
        group = _group;
    }

    public enum spawner
    {
        Scene,
        Event,
    }

    public void playAnimation(string animation_name)
    {

        if (gameObject.GetComponent<PropAnimSequence>() != null)
        {
            Destroy(gameObject.GetComponent<PropAnimSequence>());
        }
        HPAnimation prop_anim_clip = AnimationManager.loadAnimationClip(animation_name, model, null);

        playAnimationOnComponent(prop_anim_clip);
    }

    public void playAnimSequence(string sequence_name)
    {
        if (gameObject.GetComponent<PropAnimSequence>() == null)
        {
            gameObject.AddComponent<PropAnimSequence>();
        }
        gameObject.GetComponent<PropAnimSequence>().initAnimSequence(sequence_name, false);
    }

    public void onAnimationFinished(string animation_name)
    {
        GameStart.event_manager.notifyPropAnimationComplete(_name, animation_name);
    }

    public List<GameObject> particles = new List<GameObject>();

    public void PlaySound(string sound) =>
    Sound.playSoundEffect(sound);

    public void ScriptTrigger(string trigger) =>
    GameStart.event_manager.notifyScriptTrigger(trigger);
    public void AttachParticleSystem(string parameters)
    {
        string[] split = parameters.Split(':');
        string particle_name = split[0];
        string bone_name = split[1];
        string prop_name = null;
        GameObject particle = null;
        if (split.Length == 3)
        {
            prop_name = split[2];
            if (props.ContainsKey(prop_name))
            {
                Transform bone = props[prop_name].pose_bones[bone_name];
                particle = Particle.AttachParticleSystem(particle_name, bone);
            }
        }
        else
        {
            particle = Particle.AttachParticleSystem(particle_name, model.pose_bones[bone_name]);
        }
        if (particle != null)
            particles.Add(particle);
    }

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
        Scenario.onScenarioCallClear += cleanup;
    }

    private static void cleanup()
    {
        foreach (Prop p in spawned_props.Values)
        {
            GameObject.Destroy(p.model.game_object);
            foreach (GameObject pa in p.particles)
                Destroy(pa);
        }
        Prop.spawned_props.Clear();


    }

    public static void spawnScenarioProps()
    {
        if (Scene.current.proplocator_dict != null)
        {
            foreach (ConfigScene._Scene.PropLocator prop_locator in Scene.current.proplocator_dict.Values)
            {
                spawnPropFromLocator(prop_locator);
            }
        }
    }

    public static void spawnPropFromLocator(ConfigScene._Scene.PropLocator prop_locator)
    {
        Model model = ModelManager.loadModel(prop_locator.reference);

        Common.setWaypointTransform(ref model.game_object, prop_locator);

        if (model == null)
        {
            Debug.LogWarning("Failed to spawn scene prop " + prop_locator.reference);
            return;
        }

        model.game_object.transform.parent = Scene.scene_model.game_object.transform;
        model.game_object.gameObject.name = prop_locator.name;
        Prop prop = model.game_object.AddComponent<Prop>();
        prop.setup(prop_locator.name, model, Prop.spawner.Scene, "");
        spawned_props[prop_locator.name] = prop;

        if (prop_locator.animation != null)
        {
            HPAnimation animation = AnimationManager.loadAnimationClip(prop_locator.animation, model, null, null);
            prop.animation_component.wrapMode = WrapMode.Loop;
            prop.playAnimationOnComponent(animation);
        }
    }

    public static void spawnPropFromEvent(string model_id, ConfigScene._Scene.WayPoint waypoint, string name, string group)
    {
        Prop prop = null;
        if (spawned_props.ContainsKey(name))
        {
            prop = spawned_props[name];
        }
        else
        {
            Model model = ModelManager.loadModel(model_id);
            prop = model.game_object.AddComponent<Prop>();
            prop.setup(name, model, spawner.Event, group);
            model.game_object.name = name;
            spawned_props[name] = prop;
        }

        prop.model.game_object.transform.parent = Scene.scene_model.game_object.transform;
        Common.setWaypointTransform(ref prop.model.game_object, waypoint);
    }

    public static void attachPropFromEvent(string character, string bone, string model_id, string name = null)
    {
        if (name == null)
            name = model_id;

        if (!Actor.actor_controllers.ContainsKey(character))
            return;

        Prop prop;
        if (Prop.spawned_props.ContainsKey(name))
        {
            prop = spawned_props[name];
        }
        else
        {
            Model model = ModelManager.loadModel(model_id);
            prop = model.game_object.AddComponent<Prop>();
            prop.setup(name, model, spawner.Event, "");
            model.game_object.name = name;
            spawned_props[name] = prop;
        }

        prop.model.game_object.transform.parent =  Actor.actor_controllers[character].model.pose_bones[bone];
        prop.model.game_object.transform.localPosition = Vector3.zero;
        prop.model.game_object.transform.localRotation = Quaternion.identity;
    }

    public static void detachPropFromEvent(string character, string name)
    {
        if (Prop.spawned_props.ContainsKey(name))
        {
            GameObject.Destroy(Prop.spawned_props[name].model.game_object);

            Prop.spawned_props.Remove(name);
        }
    }
}
