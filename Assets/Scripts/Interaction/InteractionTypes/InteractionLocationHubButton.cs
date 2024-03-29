using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionLocationHubButton : Interaction
{
    string location_id;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
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
        is_active = true;
    }

    public override void onFinishedEnterEvents() {
        base.onFinishedEnterEvents();
        LocationScenarioMenu.showMenu(location_id);
        return; 
    }
}
