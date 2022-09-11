using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAnimSequence : AnimationSequence
{
    Prop prop_controller;

    public override void initAnimSequence(string _anim_sequence, bool _walk)
    {
        prop_controller = GetComponent<Prop>();

        GameStart.logWrite("Activating animSequence " + _anim_sequence + " for actor " + prop_controller.name + " walk: " + _walk);

        base.initAnimSequence(_anim_sequence, _walk);
    }

    protected override float playAnimation(string animation_id, string _anim_sequence = "")
    {
        HPAnimation animation;
        if (loadedAnimations.ContainsKey(animation_id))
            animation = loadedAnimations[animation_id];
        else
        {
            animation = AnimationManager.loadAnimationClip(animation_id, prop_controller.model, null, config_sequence.data.triggerReplacement);
            loadedAnimations[animation_id] = animation;
        }

        if (animation == null) return 0.0f;

        prop_controller.playAnimationOnComponent(animation);
        return animation.anim_clip.length;
    }

    protected override void attachProp(string prop_model_id, string alias, string target)
    {
        prop_controller.attachProp(prop_model_id, alias, target);
    }

    protected override void attachBroom(string prop_model_id, string alias, string target)
    {
        throw new System.NotImplementedException();
    }

    protected override void destroyProps()
    {
        prop_controller.destroyProps();
    }

    protected override void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement)
    {
        prop_controller.playPropAnim(id, target, triggerReplacement);
    }

    protected override void stopPropAnim(string id)
    {
        prop_controller.stopPropAnim(id);
    }

}
