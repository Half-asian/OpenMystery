using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
public class InteractionFocus : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        FocusUI.onFocusGameFinished += onFocusGameFinished;
        FocusUI.startFocusGame();
    }


    public void onFocusGameFinished(bool focus_success)
    {
        interactionComplete(focus_success);
    }
}
