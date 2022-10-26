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
        //Debug.Log("ACTOR_ANIM_SEQUENCE: " + actor_controller.name + " has finished sequence.");
        base.finishSequence();

        if (config_sequence.isOneShot)
        {
            actor_controller.setCharacterIdle();
        }
        //If not one shot, freeze
        //Reference Y5 Chapter 22 P6 Charlie prefect bath, charlies frozen
        //If one shot, repeat
        //Reference Y4 Capter 3 bowtruckle class, bowtruckle feeding bug
    }

    protected override void attachBroom(string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.actor_info.actorId);
        attachChildNode(broom_skin_name, null, target);
    }

    protected override void playBroomAnim(string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.actor_info.actorId);
        playChildNodeAnim(broom_skin_name, target, config_sequence.data.triggerReplacement);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
