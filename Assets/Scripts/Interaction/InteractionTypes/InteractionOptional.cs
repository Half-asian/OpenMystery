using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionOptional : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        base.setup(ref _interaction, false);
        interaction_gameobject.SetActive(true);
        return this;
    }

    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        interaction_gameobject.SetActive(true);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        setHotspot();
    }

    public override void activate()
    {
        base.addEnterEvents();
    }
    
}
