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
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        base.setup(ref _interaction, should_add_enter_events);
        interaction_gameobject.SetActive(false);
        return this;
    }

    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        activate();
    }


    public override void activate()
    {
        //FocusUI.onFocusGameFinished += onFocusGameFinished;
        //FocusUI.startFocusGame();
        GameStart.current.StartCoroutine(waitFor1second());
    }

    IEnumerator waitFor1second()
    {
        yield return new WaitForSeconds(1);
        onFocusGameFinished(true);
    }


    public void onFocusGameFinished(bool focus_success)
    {
        interactionComplete(focus_success);
    }
}
