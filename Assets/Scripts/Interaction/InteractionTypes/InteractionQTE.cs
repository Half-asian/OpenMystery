using UnityEngine;
using System.Collections;
public class InteractionQTE : Interaction
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
        GameStart.current.StartCoroutine(delay());

    }

    private IEnumerator delay()
    {
        yield return new WaitForSeconds(3);
        interactionComplete();
    }
}
