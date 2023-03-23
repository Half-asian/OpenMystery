using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionTitleCard : Interaction
{
    public static event Action<string> onShowTitleCard = delegate { };
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        activate();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        onShowTitleCard.Invoke(LocalData.getLine(config_interaction.titleCardTitle));
        base.onFinishedEnterEvents();
        StartCoroutine(enumerator());
    }

    IEnumerator enumerator()
    {
        yield return new WaitForSeconds(4);
        interactionComplete();
    }


}
