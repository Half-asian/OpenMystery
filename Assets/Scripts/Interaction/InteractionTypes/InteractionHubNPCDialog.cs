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
    public void hubNpcDialogSetup (ref string _dialogId, ref Vector3 location)
    {
        InteractionManager.active_interactions.Add(this);
        if (_dialogId != null)
        {
            dialogId = _dialogId;
            interaction_gameobject = gameObject;
            interaction_gameobject.name = dialogId;
            interaction_gameobject.AddComponent<InteractionButton>();
            interaction_gameobject.GetComponent<InteractionButton>().interaction = this;
            interaction_gameobject.transform.position = location;
            interaction_gameobject.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
    }

    public override void onFinishedEnterEvents() {
        base.onFinishedEnterEvents();
        GameStart.dialogue_manager.activateDialogue(dialogId);
        return; 
    }
}
