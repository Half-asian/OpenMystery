using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class AnimationSequence : MonoBehaviour
{
    protected void processAction(ConfigCharAnimSequence._CharAnimSequence._data.action action)
    {
        switch (action.type)
        {
            case "AttachProp":
                attachChildNode(action.id, action.alias, action.target);
                break;

            case "AttachBroom":
                attachBroom("broom", action.target);
                break;

            case "PlayBroomAnim":
                playBroomAnim(action.target);
                break;

            case "PlayPropAnim":
                playChildNodeAnim(action.id, action.target, config_sequence.data.triggerReplacement);
                break;

            case "StopPropAnim":
                stopPropAnim(action.id);
                break;

            case "RemoveProp":
                removeChildNode(action.id); //action.target is bone to move from. doesnt seem so useful
                break;

            default:
                Debug.LogWarning("Unknown animation sequence action type of " + action.type + " in action " + action.id);
                break;
        }
    }
    protected float playAnimation(string animation_id, string anim_sequence_id)
    {
        if (base_node is ActorController)
        {
            if (walk == false) {
                ActorController ac = base_node as ActorController;
                ac.idle_animation_sequence = anim_sequence_id;
            }
        }

        HPAnimation animation;
        if (loadedAnimations.ContainsKey(animation_id))
            animation = loadedAnimations[animation_id];
        else
        {
            animation = AnimationManager.loadAnimationClip(animation_id, base_node.model, null, config_sequence.data.triggerReplacement);
            loadedAnimations[animation_id] = animation;
        }

        if (animation == null) return 0.0f;

        base_node.playAnimationOnComponent(animation);
        return animation.anim_clip.length;
    }
    protected void attachChildNode(string prop_model_id, string alias, string target)
    {
        base_node.attachChildNode(prop_model_id, alias, target);
    }

    protected void removeChildNode(string prop_id)
    {
        base_node.removeProp(prop_id);
    }

    protected abstract void attachBroom(string alias, string target);
    protected void playBroomAnim(string target)
    {
        playChildNodeAnim("broom", target, config_sequence.data.triggerReplacement);
    }
    protected void playChildNodeAnim(string id, string target, Dictionary<string, string> triggerReplacement)
    {
        base_node.playPropAnim(id, target, triggerReplacement);
    }
    protected void stopPropAnim(string id)
    {
        base_node.stopPropAnim(id);
    }
}
