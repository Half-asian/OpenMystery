using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionScenarioTransition : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.scenarioId, "InteractionScenarioTransition(): interaction.scenarioId can't be null");
        base.setup(ref _interaction, should_add_enter_events);
        return this;
    }
    protected override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        activate();
    }
    public override void activate()
    {
        Scenario.Activate(config_interaction.scenarioId, Scenario.current.objective);
        Scenario.Load(config_interaction.scenarioId);
    }

}
