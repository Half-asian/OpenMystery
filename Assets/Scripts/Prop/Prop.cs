using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System.Linq;
using static ConfigScene._Scene;

public class Prop : Node
{
    public static Dictionary<string, Prop> spawned_props = new Dictionary<string, Prop>();
    public string _name;
    public List<string> lookup_tags;
    public spawner spawned_by;
    public void setup(string _name, Model _model, spawner _spawned_by, string lookup_tag)
    {
        base.setup(_model);
        this._name = _name;
        model = _model;
        spawned_by = _spawned_by;
        lookup_tags = new List<string>();
        if (lookup_tag != null)
        {
            lookup_tags.Add(lookup_tag);
        }
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

    public void playAnimSequence(string sequence_name, string starting_node)
    {
        if (gameObject.GetComponent<PropAnimSequence>() == null)
        {
            gameObject.AddComponent<PropAnimSequence>();
        }
        gameObject.GetComponent<PropAnimSequence>().initAnimSequence(sequence_name, false, starting_node);
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
        prop.setup(prop_locator.name, model, Prop.spawner.Scene, null);
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
                UnityEngine.Material mat = null;
                var smr = child.GetComponent<SkinnedMeshRenderer>();
                var mr = child.GetComponent<MeshRenderer>();
                if (smr != null)
                    mat = smr.material;
                else if (mr != null)
                    mat = mr.material;
                else
                    return;

                var material = prop_locator.material_dict[child.name];

                if (material.stringValueKeys != null)
                {
                    for (int i = 0; i < material.stringValueKeys.Length; i++)
                    {
                        mat.SetTexture(material.stringIds[i], TextureManager.loadTexture(material.stringValueKeys[i]));
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

        prop.cancel_crossfade = true;
        if (prop_locator.animation != null)
        {
            HPAnimation animation = AnimationManager.loadAnimationClip(prop_locator.animation, model, null, null);
            if (animation != null)
            {
                prop.animation_component.wrapMode = WrapMode.Loop;
                prop.queueAnimationOnComponent(animation);
            }
        }
    }

    public static void spawnPropFromEvent(string model_id, string waypoint_id, string name, string group)
    {
        ModelMaterials.lighting_layers = new List<string>();
        if (Scene.current.Lighting != null)
        {
            if (Scene.getWayPointData(waypoint_id, out var waypoint))
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
            Debug.Log("Loading model " + model_id);
            Model model = ModelManager.loadModel(model_id);
            prop = model.game_object.AddComponent<Prop>();
            prop.setup(name, model, spawner.Event, group);
            model.game_object.name = name;
            spawned_props[name] = prop;
        }

        prop.model.game_object.transform.SetParent(GameStart.current.props_holder);

        Scene.setGameObjectToWaypoint(prop.model.game_object, waypoint_id);
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

    public static void stopAnimatingProp(string prop)
    {
        if (spawned_props.ContainsKey(prop))
        {
            spawned_props[prop].animation_component.Stop();
        }
    }

    public static void eventPlayPropAnimationSequence(string prop_id, string animation_sequence, int is_group, string starting_node)
    {
        if (is_group == 1)
        {
            foreach (var prop in spawned_props.Values)
            {
                if (prop.lookup_tags.Contains(prop_id))
                {
                    prop.playAnimSequence(animation_sequence, starting_node);
                }
            }
        }
        else
        {
            if (spawned_props.ContainsKey(prop_id))
            {
                spawned_props[prop_id].playAnimSequence(animation_sequence, starting_node);
            }
        }
    }

    public static void eventDespawnProp(string event_id, string[] action_params)
    {
        string prop_id = action_params[0];
        int is_lookup_tag_mode = 0;
        if (action_params.Length > 2)
        {
            int.TryParse(action_params[1], out is_lookup_tag_mode);
        }
        despawnProp(event_id, prop_id, is_lookup_tag_mode == 1);
    }

    private static void despawnProp(string event_id, string prop_id, bool is_lookup_tag_mode)
    {
        if (is_lookup_tag_mode)
        {
            List<string> props_to_destroy = new List<string>();
            foreach (string p_key in Prop.spawned_props.Keys)
            {
                if (Prop.spawned_props[p_key].lookup_tags.Contains(prop_id))
                {
                    GameObject.Destroy(Prop.spawned_props[p_key].model.game_object);
                    props_to_destroy.Add(p_key);
                }
            }
            foreach (string p_key in props_to_destroy)
            {
                Prop.spawned_props.Remove(p_key);
            }
        }
        else
        {
            if (Prop.spawned_props.ContainsKey(prop_id))
            {
                GameObject.Destroy(Prop.spawned_props[prop_id].model.game_object);
                Prop.spawned_props.Remove(prop_id);
            }
        }
    }

}
