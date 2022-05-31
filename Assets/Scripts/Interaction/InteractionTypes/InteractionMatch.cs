using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionMatch : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.matchId, "InteractionMatch(): interaction.matchId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionMatch(): interaction.spot can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        interaction_gameobject.SetActive(false);
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
        GameObject.Destroy(interaction_gameobject);

        GameStart.quidditch_manager.startMatch(config_interaction.matchId, this); //Finished is called from a callback at the end of a match
    }
}
