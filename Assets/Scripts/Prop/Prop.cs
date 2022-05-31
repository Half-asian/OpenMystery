using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Prop : PropHolder
{
    public static Dictionary<string, Prop> spawned_props = new Dictionary<string, Prop>();
    public string _name;
    public string group;
    public spawner spawned_by;
    public void setup(string _name, Model _model, spawner _spawned_by, string _group)
    {
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

    public void PlaySound(string sound) =>
    Sound.playSoundEffect(sound);

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
        Scenario.onScenarioLoaded += cleanup;
    }

    private static void cleanup()
    {
        foreach (Prop p in spawned_props.Values)
        {
            GameObject.Destroy(p.model.game_object);
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

        if (prop_locator.animation != null)
        {
            Animation prop_anim = model.game_object.AddComponent<Animation>();
            AnimationClip anim = AnimationManager.loadAnimationClip(prop_locator.animation, model, null, null);
            prop_anim.AddClip(anim, "default");
            prop_anim.wrapMode = WrapMode.Loop;
            prop_anim.Play("default");
        }

        model.game_object.name = prop_locator.name;
        Prop prop = model.game_object.AddComponent<Prop>();
        prop.setup(prop_locator.name, model, Prop.spawner.Scene, "");
        spawned_props[prop_locator.name] = prop;

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
