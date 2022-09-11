using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimSequence : AnimationSequence
{
    ActorController actor_controller;

    public override void initAnimSequence(string _anim_sequence, bool _walk)
    {
        actor_controller = GetComponent<ActorController>();

        GameStart.logWrite("Activating animSequence " + _anim_sequence + " for actor " + actor_controller.name + " walk: " + _walk);

        base.initAnimSequence(_anim_sequence, _walk);
    }

    protected override float playAnimation(string animation_id, string _anim_sequence)
    {
        if (walk == false)
            actor_controller.actor_animation.anim_sequence_idle = _anim_sequence;

        HPAnimation animation;
        if (loadedAnimations.ContainsKey(animation_id))
            animation = loadedAnimations[animation_id];
        else
        {
            animation = AnimationManager.loadAnimationClip(animation_id, actor_controller.model, actor_controller.actor_info, config_sequence.data.triggerReplacement);
            loadedAnimations[animation_id] = animation;
        }
        if (animation == null)
        {
            Debug.LogError("Failed to load animation clip for " + actor_controller.name + " with sequence of " + config_sequence.sequenceId + " at node index " + node_index);
            return 0.0f;
        }

        actor_controller.playAnimationOnComponent(animation);
        return animation.anim_clip.length;
    }

    protected override void attachProp(string prop_model_id, string alias, string target)
    {
        actor_controller.attachProp(prop_model_id, alias, target);
    }

    protected override void finishSequence()
    {
        if (walk == false)
            actor_controller.actor_animation.anim_sequence_idle = "";
        base.finishSequence();

        if (config_sequence.isOneShot)
        {
            actor_controller.actor_animation.setCharacterIdle();
        }

        //actor_controller.actor_animation.unblock();
    }

    protected override void attachBroom(string prop_model_id, string alias, string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.actor_info.actorId);
        attachProp(broom_skin_name, alias, target);
    }

    protected override void destroyProps()
    {
        actor_controller.destroyProps();
    }

    protected override void playPropAnim(string id, string target, Dictionary<string, string> triggerReplacement)
    {
        actor_controller.playPropAnim(id, target, triggerReplacement);
    }

    protected override void stopPropAnim(string id)
    {
        actor_controller.stopPropAnim(id);
    }
}
