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

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);

        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        setHotspot();

        interaction_gameobject.SetActive(true);
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        is_active = true;
        if (Scenario.current.scenario_config.mapLocationId != null) //We're in a location
        {
            Scenario.ExitSave();
            LocationHub.loadLocationHub(Configs.config_location.Location[Location.current].hubId);
            interactionComplete();
        }
        else //We're in a hub
        {
            LocationHubMenu.showMenu();
        }
    }
}