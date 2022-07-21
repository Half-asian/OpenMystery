using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionMatch : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        Assert.IsNotNull(_interaction.matchId, "InteractionMatch(): interaction.matchId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionMatch(): interaction.spot can't be null");
        interaction_gameobject.SetActive(true);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        setHotspot();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

        GameObject.DestroyImmediate(interaction_gameobject);

        GameStart.quidditch_manager.startMatch(config_interaction.matchId, this); //Finished is called from a callback at the end of a match
    }
}
