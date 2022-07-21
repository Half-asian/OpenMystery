using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionProject : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        Assert.IsNotNull(_interaction.projectId, "InteractionProject(): interaction.projectId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionProject(): interaction.spot can't be null");
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;

        setHotspot();
        interaction_gameobject.SetActive(true);

        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        Project.startProject(config_interaction.projectId);
        interactionComplete();
    }
}
