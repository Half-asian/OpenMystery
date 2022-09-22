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

        GameStart.logWrite("Activating animSequence " + _anim_sequence + " for actor " + actor_controller.name + " walk: " + _walk);

        base.initAnimSequence(_anim_sequence, _walk);
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
    }

    protected override void attachBroom(string prop_model_id, string alias, string target)
    {
        string broom_skin_name = getBroomSkinName(actor_controller.actor_info.actorId);
        attachChildNode(broom_skin_name, alias, target);
    }
}
