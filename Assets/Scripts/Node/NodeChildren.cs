using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using ModelLoading;
public partial class Node : MonoBehaviour
{

    protected Dictionary<string, Node> childNodes = new Dictionary<string, Node>();
    public List<GameObject> particles = new List<GameObject>();

    public void attachChildNode(string prop_model_id, string alias, string target)
    {
        Transform bone_to_attach = null;
        if (target != null)
        {
            bone_to_attach = Common.recursiveFindChild(model.jt_all_bind, target);
            if (bone_to_attach == null)
                Debug.LogError("Failed to find attach bone " + target);
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
            if (childNodes.ContainsKey(alias))
                Destroy(childNodes[alias]);

            //Debug.Log("Adding prop " + alias);
            childNodes[alias] = prop;
            prop.parent_node = this;
        }
        else
        {
            if (childNodes.ContainsKey(prop_model_id))
                Destroy(childNodes[prop_model_id]);
            childNodes[prop_model_id] = prop;
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
        attachChildNode(split[0], split[1], split[2]);
    }

    //Animation event
    public void PlayPropAnim(string parameters)
    {
        string[] split = parameters.Split(':');
        playPropAnim(split[0], split[1]);
    }

    public void removeProp(string id)
    {
        if (childNodes.ContainsKey(id))
        {
            Destroy(childNodes[id]);
            childNodes.Remove(id);
        }
    }

    public void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement = null)
    {
        Node found_node = null;
        childNodes.TryGetValue(id, out found_node);

        if (found_node == null && parent_node != null){
            parent_node.childNodes.TryGetValue(id, out found_node);
        }
        
        if (found_node == null)
        {
            Debug.LogError("Failed to play prop anim " + target + " on a nonexistent prop " + id);
            return;
        }

        HPAnimation animation = AnimationManager.loadAnimationClip(target, found_node.GetComponent<Node>().model, null, triggerReplacement);
        if (animation == null) return;
        found_node.GetComponent<Node>().queueAnimationOnComponent(animation);
    }

    public void stopPropAnim(string id)
    {
        if (childNodes.ContainsKey(id))
        {
            childNodes[id].GetComponent<Animation>().Stop();
        }
    }

    public void destroyProps()
    {
        foreach (var g in childNodes.Values)
        {
            DestroyImmediate(g);
        }
        foreach (GameObject particle in particles)
        {
            DestroyImmediate(particle);
        }
        childNodes.Clear();
    }

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
            if (childNodes.ContainsKey(prop_name))
            {
                Transform bone = childNodes[prop_name].GetComponent<Node>().model.pose_bones[bone_name];
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
}
