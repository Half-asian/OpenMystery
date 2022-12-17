using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System.Linq;

public class Prop : Node
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

    public void playAnimation(string animation_name, Dictionary<string, string> triggerReplacement = null)
    {

        if (gameObject.GetComponent<PropAnimSequence>() != null)
        {
            Destroy(gameObject.GetComponent<PropAnimSequence>());
        }
        HPAnimation prop_anim_clip = AnimationManager.loadAnimationClip(animation_name, model, null, triggerReplacement);

        queueAnimationOnComponent(prop_anim_clip);
    }

    public void playAnimSequence(string sequence_name)
    {
        if (gameObject.GetComponent<PropAnimSequence>() == null)
        {
            gameObject.AddComponent<PropAnimSequence>();
        }
        gameObject.GetComponent<PropAnimSequence>().initAnimSequence(sequence_name, false);
    }

    protected override IEnumerator animationAlert(AnimationClip clip)
    {
        while (true)
        {
            yield return new WaitForSeconds(clip.length);
            raiseOnAnimationFinished(clip.name);

            GameStart.event_manager.notifyPropAnimationComplete(name, clip.name);
            if (clip.wrapMode != WrapMode.Loop)
            {
                yield break;
            }
        }
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
        ModelMaterials.lighting_layers = new List<string>();

        if (Scene.current.Lighting != null)
        {
            foreach (var layer in Scene.current.Lighting.layers.Values)
            {
                if (layer.objects.Contains(prop_locator.name))
                {
                    ModelMaterials.explicit_ambient = true;
                    ModelMaterials.lighting_layers.Add( layer.name);
                }
            }
            if (ModelMaterials.lighting_layers.Count == 0)
            {
                ModelMaterials.lighting_layers.Add(Scene.current.Lighting.layers.Keys.First());
            }
        }


        Model model = ModelManager.loadModel(prop_locator.reference);

        Common.setPropLocatorTransform(ref model.game_object, prop_locator);

        if (model == null)
        {
            Debug.LogWarning("Failed to spawn scene prop " + prop_locator.reference);
            return;
        }



        model.game_object.transform.SetParent(GameStart.current.props_holder);
        model.game_object.gameObject.name = prop_locator.name;
        Prop prop = model.game_object.AddComponent<Prop>();
        prop.setup(prop_locator.name, model, Prop.spawner.Scene, "");
        spawned_props[prop_locator.name] = prop;

        if (prop_locator.materials != null)
        {
            for (int c = 0; c < prop.model.game_object.transform.childCount; c++)
            {
                Transform child = prop.model.game_object.transform.GetChild(c);
                if (!prop_locator.material_dict.ContainsKey(child.name))
                {
                    continue;
                }
                Material mat = child.GetComponent<SkinnedMeshRenderer>().material;
                var material = prop_locator.material_dict[child.name];

                if (material.stringValueKeys != null)
                {
                    for (int i = 0; i < material.stringValueKeys.Length; i++)
                    {
                        mat.SetTexture(material.stringIds[i], TextureManager.loadTextureDDS(material.stringValueKeys[i]));
                        ModelMaterials.setTexSwitches(mat, material.stringIds[i]);
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

        prop.reset_animation = true;
        if (prop_locator.animation != null)
        {
            HPAnimation animation = AnimationManager.loadAnimationClip(prop_locator.animation, model, null, null);
            prop.animation_component.wrapMode = WrapMode.Loop;
            prop.queueAnimationOnComponent(animation);
        }
    }

    public static void spawnPropFromEvent(string model_id, ConfigScene._Scene.WayPoint waypoint, string name, string group)
    {
        ModelMaterials.lighting_layers = new List<string>();
        if (Scene.current.Lighting != null)
        {
            if (waypoint != null)
            {
                if (waypoint.lightLayerOverride != null)
                {
                    ModelMaterials.lighting_layers.AddRange(waypoint.lightLayerOverride); //Figure out if more than one light layer can be applied at once
                }
            }
            if (ModelMaterials.lighting_layers.Count == 0)
            {
                ModelMaterials.lighting_layers.Add(Scene.current.Lighting.layers.Keys.First());
            }
        }

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

    public static void animateProp(string prop, string animation, Dictionary<string, string> triggerReplacement = null)
    {
        if (spawned_props.ContainsKey(prop))
        {
            spawned_props[prop].playAnimation(animation, triggerReplacement);
        }
        else
        {
            Debug.LogWarning("Couldn't find prop " + prop + " in spawned props");
        }
    }

    public static void playPropAnimationSequence(string prop, string animation_sequence)
    {
        if (Prop.spawned_props.ContainsKey(prop))
        {
            Prop.spawned_props[prop].playAnimSequence(animation_sequence);
        }
        else
        {
            Debug.LogWarning("Couldn't find prop " + prop + " in spawned props");
        }
    }
}
