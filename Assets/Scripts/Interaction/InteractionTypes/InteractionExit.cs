using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionExit : Interaction
{
    public static event Action InteractionExitCalled = delegate {};

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        base.setup(ref _interaction, should_add_enter_events);
        config_interaction.autoSelect = false;

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
        shouldShow = true;
        if (Scenario.current.scenario_config.mapLocationId != null) //We're in a location
        {
            Scenario.ExitSave();
            LocationHub.loadLocationHub(Location.current.hubId);
            interactionComplete();
        }
        else //We're in a hub
        {
            LocationHubMenu.showMenu();
        }
    }
}