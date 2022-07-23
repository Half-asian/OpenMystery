using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionFastTap : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        interaction_gameobject.SetActive(false);
        activate();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        interactionComplete();
    }

}
