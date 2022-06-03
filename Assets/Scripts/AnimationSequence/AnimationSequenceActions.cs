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
                attachProp(action.id, action.alias, action.target);
                break;

            case "AttachBroom":
                string broom_skin_name = getBroomSkinName(GetComponent<ActorController>().actor_info.actorId);
                attachBroom(broom_skin_name, "broom", action.target);
                break;

            case "PlayBroomAnim":
                playBroomAnim(action.target);
                break;

            case "PlayPropAnim":
                playPropAnim(action.id, action.target, config_sequence.data.triggerReplacement);
                break;

            case "StopPropAnim":
                stopPropAnim(action.id);
                break;

            default:
                Debug.LogWarning("Unknown animation sequence action type of " + action.type + " in action " + action.id);
                break;
        }
    }
    protected abstract float playAnimation(string animation_id, string anim_sequence_id);
    protected abstract void attachProp(string prop_model_id, string alias, string target);
    protected abstract void attachBroom(string prop_model_id, string alias, string target);
    protected void playBroomAnim(string target)
    {
        playPropAnim("broom", target, config_sequence.data.triggerReplacement);
    }
    protected abstract void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement);

    protected abstract void stopPropAnim(string id);
}
