using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimSequence : AnimationSequence
{
    ActorController actor_controller;

    public override void initAnimSequence(string _anim_sequence, bool _walk)
    {
        actor_controller = GetComponent<ActorController>();

        base.initAnimSequence(_anim_sequence, _walk);

        if (config_sequence.isOneShot == true)
            actor_controller.actor_animation.blocked = true;   
    }

    protected override float playAnimation(string animation_id, string _anim_sequence)
    {
        actor_controller.actor_animation.anim_sequence_idle = _anim_sequence;

        AnimationClip anim_clip = AnimationManager.loadAnimationClip(animation_id, actor_controller.model, actor_controller.actor_info, config_sequence.data.triggerReplacement);
        if (anim_clip == null) return 0.0f;

        animation_component.AddClip(anim_clip, animation_id);

        if (animation_id == "p_blackboardQMovies01_transitions01_blackBoardQMoves01_staticOff")
        {
            StartCoroutine(p_blackBoardEvidence_skin_transition(1.5f, Time.realtimeSinceStartup));
        }
        animation_component.Play(animation_id);
        return anim_clip.length;
    }

    protected override void attachProp(string prop_model_id, string alias, string target)
    {
        actor_controller.attachProp(prop_model_id, alias, target);
    }

    protected override void finishSequence()
    {
        base.finishSequence();

        Debug.Log("UNBLOCK");
        actor_controller.actor_animation.unblock();
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
