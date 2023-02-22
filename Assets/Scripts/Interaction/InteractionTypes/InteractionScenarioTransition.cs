using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionScenarioTransition : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        Assert.IsNotNull(_interaction.scenarioId, "InteractionScenarioTransition(): interaction.scenarioId can't be null");
        base.setup(ref _interaction);
        interaction_gameobject.SetActive(true);
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        if (config_interaction.spot != null) setHotspot();
        return this;
    }
    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

        if (Scenario.current != null)
        {
            Scenario.Activate(config_interaction.scenarioId, Scenario.current.objective);
            Scenario.Load(config_interaction.scenarioId);
        }
    }
}
