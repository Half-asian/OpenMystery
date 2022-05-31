using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionProject : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.projectId, "InteractionProject(): interaction.projectId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionProject(): interaction.spot can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;

        setHotspot();
        interaction_gameobject.SetActive(false);
        return this;
    }

    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        interaction_gameobject.SetActive(true);

    }

    public override void activate()
    {
        Debug.Log("Activate interaction project called");
        Project.startProject(config_interaction.projectId);
        interactionComplete();
    }
}
