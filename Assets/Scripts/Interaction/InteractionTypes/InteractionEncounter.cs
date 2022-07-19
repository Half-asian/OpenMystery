using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionEncounter : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    {
        Assert.IsNotNull(_interaction.encounterId, "InteractionEncounter(): interaction.encounterId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionEncounter(): interaction.spot can't be null");
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
        if (Configs.config_encounter.Encounter[config_interaction.encounterId].type != "Date")
        {
            interactionComplete();
        }
        else
        {
            EncounterManager.onEncounterFinished += encounterComplete;
            GameStart.encounter_manager.activateEncounter(config_interaction.encounterId);
        }
        //Interaction is called from a callback.
        //finished();
    }

    public void encounterComplete(string encounter_id)
    {
        if (encounter_id == config_interaction.encounterId)
        {
            EncounterManager.onEncounterFinished -= encounterComplete;
            interactionComplete();
        }
    }

}
