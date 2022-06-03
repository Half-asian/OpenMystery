using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PropHolder : MonoBehaviour
{
    public Model model;

    protected Dictionary<string, Model> props = new Dictionary<string, Model>();



    public void attachProp(string prop_model_id, string alias, string target)
    {
        Transform bone_to_attach = null;
        if (target != null)
        {
            bone_to_attach = Common.recursiveFindChild(model.jt_all_bind, target);
            if (bone_to_attach == null)
                Debug.LogError("Failed to find attach bone " + target);
        }

        Model prop = ModelManager.loadModel(prop_model_id);

        if (prop == null)
        {
            Debug.LogError("Failed to attach prop " + prop_model_id + " due to invalid id");
            return;
        }
        prop.game_object.AddComponent<Animation>();
        prop.game_object.AddComponent<Prop>().model = prop;

        if (alias != null)
        {
            if (props.ContainsKey(alias))
                Destroy(props[alias].game_object);

            Debug.Log("Adding prop " + alias);
            props[alias] = prop;
        }
        else
        {
            if (props.ContainsKey(prop_model_id))
                Destroy(props[prop_model_id].game_object);
            Debug.Log("Adding prop " + prop_model_id);
            props[prop_model_id] = prop;
        }
        //if (bone_to_attach.name == "jt_propCounterScale")
        //{
        //    bone_to_attach = Common.recursiveFindChild(gameObject.transform, "jt_all_bind");
        //}
        prop.game_object.transform.parent = transform;
        if (target != null)
        {
            ParentConstraint parent_constraint = prop.game_object.AddComponent<ParentConstraint>();
            parent_constraint.constraintActive = true;
            ConstraintSource constraint_source = new ConstraintSource();
            constraint_source.sourceTransform = bone_to_attach;
            constraint_source.weight = 1.0f;
            parent_constraint.AddSource(constraint_source);
        }
        prop.game_object.transform.localPosition = Vector3.zero;
        prop.game_object.transform.rotation = Quaternion.identity;
    }

    public void removeProp(string id)
    {
        if (props.ContainsKey(id))
        {
            Destroy(props[id].game_object);
            props.Remove(id);
        }
    }

    public void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement)
    {
        if (!props.ContainsKey(id))
        {
            Debug.LogError("Failed to play prop anim " + target + " on a nonexistent prop " + id);
            return;
        }

        AnimationClip anim_clip = AnimationManager.loadAnimationClip(target, props[id], null, triggerReplacement);
        if (anim_clip == null) return;

        props[id].game_object.GetComponent<Animation>().AddClip(anim_clip, "default");
        props[id].game_object.GetComponent<Animation>().Play("default");
    }

    public void stopPropAnim(string id)
    {
        if (props.ContainsKey(id))
        {
            props[id].game_object.GetComponent<Animation>().Stop();
        }
    }

    public void destroyProps()
    {
        foreach (Model g in props.Values)
        {
            Destroy(g.game_object);
        }
        props.Clear();
    }

    //Animation Events
    public void AttachProp(string paramaters)
    {
        string[] split = paramaters.Split(':');
        attachProp(split[0], split[1], split[2]);
    }



    public void PlayPropAnim(string paramaters)
    {
        string[] split = paramaters.Split(':');
        playPropAnim(split[0], split[1], null);
    }


}
