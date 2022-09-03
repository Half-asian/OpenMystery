using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using ModelLoading;
public partial class ModelHolder : MonoBehaviour 
{

    protected Dictionary<string, GameObject> props = new Dictionary<string, GameObject>();



    public void attachProp(string prop_model_id, string alias, string target)
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

        ModelHolder prop = prop_model.game_object.AddComponent<ModelHolder>();
        prop.setup(prop_model);




        if (alias != null)
        {
            if (props.ContainsKey(alias))
                Destroy(props[alias]);

            Debug.Log("Adding prop " + alias);
            props[alias] = prop.model.game_object;
        }
        else
        {
            if (props.ContainsKey(prop_model_id))
                Destroy(props[prop_model_id]);
            props[prop_model_id] = prop.model.game_object;
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
        if (props.ContainsKey(id))
        {
            Destroy(props[id]);
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


        HPAnimation animation = AnimationManager.loadAnimationClip(target, props[id].GetComponent<ModelHolder>().model, null, triggerReplacement);
        if (animation == null) return;
        props[id].GetComponent<ModelHolder>().playAnimationOnComponent(animation);
    }

    public void stopPropAnim(string id)
    {
        if (props.ContainsKey(id))
        {
            props[id].GetComponent<Animation>().Stop();
        }
    }

    public void destroyProps()
    {
        foreach (var g in props.Values)
        {
            Destroy(g);
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
