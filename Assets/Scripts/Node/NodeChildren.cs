using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using ModelLoading;
using static UnityEngine.ParticleSystem;

public partial class Node : MonoBehaviour
{

    protected Dictionary<string, Node> child_nodes = new Dictionary<string, Node>();
    public List<GameObject> particles = new List<GameObject>();

    private Node getChildNode(string name)
    {
        foreach(var key in child_nodes.Keys)
        {
            if (key == name)
                return child_nodes[key];
            var granchild_node = child_nodes[key].getChildNode(name);
            if (granchild_node != null)
                return granchild_node;
        }
        return null;
    }

    public void attachChildNode(string prop_model_id, string alias, string target)
    {
        Transform bone_to_attach = null;
        if (target != null)
        {
            bone_to_attach = Common.recursiveFindChild(model.jt_all_bind, target);
            if (bone_to_attach == null)
                Debug.LogError("Failed to find attach bone " + target + " on " + name);
        }

        Model prop_model = ModelManager.loadModel(prop_model_id);
        if (prop_model == null)
        {
            Debug.LogError("Failed to attach prop " + prop_model_id + " due to invalid id");
            return;
        }

        Prop prop = prop_model.game_object.AddComponent<Prop>();
        prop.setup(prop_model);




        if (alias != null)
        {
            if (child_nodes.ContainsKey(alias))
                Destroy(child_nodes[alias]);

            //Debug.Log("Adding prop " + alias);
            child_nodes[alias] = prop;
            prop.parent_node = this;
        }
        else
        {
            if (child_nodes.ContainsKey(prop_model_id))
                Destroy(child_nodes[prop_model_id]);
            child_nodes[prop_model_id] = prop;
            prop.parent_node = this;
        }
        //if (bone_to_attach.name == "jt_propCounterScale")
        //{
        //    bone_to_attach = Common.recursiveFindChild(gameObject.transform, "jt_all_bind");
        //}
        prop.model.game_object.transform.parent = transform;
        if (target != null)
        {
            ParentConstraint parent_constraint = prop.model.game_object.AddComponent<ParentConstraint>();
            parent_constraint.constraintActive = true;
            ConstraintSource constraint_source = new ConstraintSource();
            constraint_source.sourceTransform = bone_to_attach;
            constraint_source.weight = 1.0f;
            parent_constraint.AddSource(constraint_source);
        }
        prop.model.game_object.transform.localPosition = Vector3.zero;
        prop.model.game_object.transform.rotation = Quaternion.identity;
    }

    //Animation event
    public void AttachProp(string parameters)
    {
        string[] split = parameters.Split(':');
        string model_id = split[0];
        string bone_id = split[1];
        
        if (split.Length < 3)
        {
            attachChildNode(model_id, model_id, bone_id);
        }
        else
        {
            string prop_name = split[2];
            if (child_nodes.ContainsKey(prop_name))
            {
                Node prop = child_nodes[prop_name].GetComponent<Node>();
                prop.attachChildNode(model_id, model_id, bone_id);
            }
        }
    }

    //Animation event
    public void PlayPropAnim(string parameters)
    {
        string[] split = parameters.Split(':');

        playPropAnim(split[0], split[1]);
    }

    public void removeProp(string id)
    {
        if (child_nodes.ContainsKey(id))
        {
            Destroy(child_nodes[id]);
            child_nodes.Remove(id);
        }
    }

    public void playPropAnim(string prop_id, string anim_id, Dictionary<string, string> triggerReplacement = null)
    {
        Node found_node = getChildNode(prop_id);
        
        if (found_node == null)
        {
            Debug.LogError("Failed to play prop anim " + anim_id + " on a nonexistent prop " + prop_id);
            return;
        }

        HPAnimation animation = AnimationManager.loadAnimationClip(anim_id, found_node.GetComponent<Node>().model, null, triggerReplacement);
        if (animation == null) return;
        found_node.GetComponent<Node>().queueAnimationOnComponent(animation);
    }

    public void stopPropAnim(string id)
    {
        if (child_nodes.ContainsKey(id))
        {
            child_nodes[id].GetComponent<Animation>().Stop();
        }
    }

    public void destroyProps()
    {
        foreach (var g in child_nodes.Values)
        {
            DestroyImmediate(g);
        }
        foreach (GameObject particle in particles)
        {
            DestroyImmediate(particle);
        }
        child_nodes.Clear();
    }

    public void AttachParticleSystem(string parameters)
    {
        string[] split = parameters.Split(':');
        string particle_name = split[0];
        string bone_name = split[1];
        GameObject particle = null;
        if (split.Length == 3)
        {
            string prop_name = null;
            prop_name = split[2];
            if (child_nodes.ContainsKey(prop_name))
            {
                Transform bone = child_nodes[prop_name].GetComponent<Node>().model.pose_bones[bone_name];
                particle = Particle.AttachParticleSystem(particle_name, bone);
            }
            else
            {
                Debug.LogError("Could not find child node to attach particle system " + prop_name);
            }
        }
        else
        {
            if (!model.pose_bones.ContainsKey(bone_name))
            {
                Debug.LogError("Failed to attach particle system to invalid bone " + bone_name);
                return;
            }
            particle = Particle.AttachParticleSystem(particle_name, model.pose_bones[bone_name]);
        }
        if (particle != null)
            particles.Add(particle);
    }
}
