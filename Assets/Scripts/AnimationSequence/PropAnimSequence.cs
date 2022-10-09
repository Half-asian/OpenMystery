using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAnimSequence : AnimationSequence
{
    Prop prop_controller;

    public override void initAnimSequence(string _anim_sequence, bool _walk)
    {
        prop_controller = GetComponent<Prop>();
        base_node = prop_controller;

        Debug.Log("Activating animSequence " + _anim_sequence + " for actor " + prop_controller.name + " walk: " + _walk);

        base.initAnimSequence(_anim_sequence, _walk);
    }

    protected override void attachBroom(string prop_model_id, string alias, string target)
    {
        throw new System.NotImplementedException();
    }
}
