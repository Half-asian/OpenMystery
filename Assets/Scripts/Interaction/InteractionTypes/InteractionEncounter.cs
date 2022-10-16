using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionEncounter : Interaction
{
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);
        Assert.IsNotNull(_interaction.encounterId, "InteractionEncounter(): interaction.encounterId can't be null");
        Assert.IsNotNull(_interaction.spot, "InteractionEncounter(): interaction.spot can't be null");
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        setHotspot();
        interaction_gameobject.SetActive(true);
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();
        if (Configs.config_encounter.Encounter[config_interaction.encounterId].type != "Date" && Configs.config_encounter.Encounter[config_interaction.encounterId].type != "Social")
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

    public void encounterComplete(string encounter_id, bool succeeded)
    {
        if (encounter_id == config_interaction.encounterId)
        {
            Debug.Log("Encounter complete");
            EncounterManager.onEncounterFinished -= encounterComplete;
            if (succeeded)
                interactionComplete();
            else
                interaction_gameobject.GetComponent<InteractionButton>().is_active = true;
        }
    }

}
