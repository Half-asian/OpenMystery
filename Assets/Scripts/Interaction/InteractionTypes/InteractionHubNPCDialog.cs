using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
public class InteractionHubNPCDialog : Interaction
{
    string dialogId;
    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    { throw new System.NotImplementedException(); }
    public void hubNpcDialogSetup (ref string _dialogId, string waypoint_id)
    {
        InteractionManager.active_interactions.Add(this);
        if (_dialogId != null)
        {
            dialogId = _dialogId;
            interaction_gameobject = gameObject;
            interaction_gameobject.name = dialogId;
            interaction_gameobject.AddComponent<InteractionButton>();
            interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
            Scene.setGameObjectToWaypoint(interaction_gameobject, waypoint_id);
            interaction_gameobject.transform.localScale = new Vector3(0.03f, 1, 0.03f);
            interaction_gameobject.transform.Translate(new Vector3(0, 1.1f, 0));
            interaction_gameobject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
    }

    public override void onFinishedEnterEvents() {
        base.onFinishedEnterEvents();
        GameStart.dialogue_manager.activateDialogue(dialogId);
        return; 
    }
}
