using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionLocationHubButton : Interaction
{
    string location_id;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction, bool should_add_enter_events)
    { throw new System.NotImplementedException(); }
    public void interactionLocationHubButtonSetup(ref string _location_id, ref Vector3 location)
    {
        InteractionManager.active_interactions.Add(this);
        location_id = _location_id;
        interaction_gameobject = gameObject;
        interaction_gameobject.name = location_id;
        interaction_gameobject.AddComponent<InteractionButton>();
        interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
        interaction_gameobject.transform.position = location;
        interaction_gameobject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        shouldShow = true;
    }

    protected override void onFinishedEnterEvents() { return; }

    public override void activate()
    {
        LocationScenarioMenu.showMenu(location_id);
    }
}
