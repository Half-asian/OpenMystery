using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimSequence : AnimationSequence
{
    ActorController actor_controller;

    public override void initAnimSequence(string _anim_sequence, bool _walk)
    {
        actor_controller = GetComponent<ActorController>();
        base_node = actor_controller;

        //Debug.Log("ACTOR_ANIM_SEQUENCE: Activating animSequence " + _anim_sequence + " for actor " + actor_controller.name + " walk: " + _walk);

        base.initAnimSequence(_anim_sequence, _walk);
    }

    protected override void finishSequence()
    {
        string exitAnim = config_sequence.data.exitAnim;
        bool isOneShot = config_sequence.isOneShot;
        base.finishSequence();

        if (isOneShot)
        {
            actor_controller.markCurrentAnimationFinished();
            actor_controller.setCharacterIdle();
        }
        if (exitAnim != null)
        {
            actor_controller.replaceCharacterIdle("Engine finishSequence", exitAnim);
        }

        //If not one shot, freeze
        //Reference Y5 Chapter 22 P6 Charlie prefect bath, charlies frozen
        //If one shot, repeat
        //Reference Y4 Chapter 3 bowtruckle class, bowtruckle feeding bug
    }

    protected override float playAnimation(string animation_id, string anim_sequence_id)
    {
        HPAnimation animation;
        if (loadedAnimations.ContainsKey(animation_id))
            animation = loadedAnimations[animation_id];
        else
        {
            animation = AnimationManager.loadAnimationClip(animation_id, base_node.model, actor_controller.config_hpactor, config_sequence.data.triggerReplacement);
            loadedAnimations[animation_id] = animation;
        }

        if (animation == null) return 0.0f;

        base_node.queueAnimationOnComponent(animation);
        return animation.anim_clip.length;
    }

    protected override void attachBroom(string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.config_hpactor.actorId);
        attachChildNode(broom_skin_name, null, target);
    }

    protected override void playBroomAnim(string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.config_hpactor.actorId);
        playChildNodeAnim(broom_skin_name, target, config_sequence.data.triggerReplacement);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
