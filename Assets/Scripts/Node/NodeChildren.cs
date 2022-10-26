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
        }
        else
        {
            if (childNodes.ContainsKey(prop_model_id))
                Destroy(childNodes[prop_model_id]);
            childNodes[prop_model_id] = prop;
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

    public void removeProp(string id)
    {
        if (childNodes.ContainsKey(id))
        {
            Destroy(childNodes[id]);
            childNodes.Remove(id);
        }
    }

    public void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement)
    {
        if (!childNodes.ContainsKey(id))
        {
            Debug.LogError("Failed to play prop anim " + target + " on a nonexistent prop " + id);
            return;
        }


        HPAnimation animation = AnimationManager.loadAnimationClip(target, childNodes[id].GetComponent<Node>().model, null, triggerReplacement);
        if (animation == null) return;
        childNodes[id].GetComponent<Node>().playAnimationOnComponent(animation);
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
        childNodes.Clear();
    }
}
