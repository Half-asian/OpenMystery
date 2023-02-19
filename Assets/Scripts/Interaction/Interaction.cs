using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using static ConfigInteraction;

/*
Interaction Class Lifespan

Wait for enter events to finish
Spawn Group members - Only if group
Wait for task to complete
Spawn leads to
Wait for leads to complete
Wait for exit events to finish
Return groupProgress to parent
*/

public abstract class Interaction : MonoBehaviour
{
    [SerializeField]
    public ConfigInteraction.Interaction config_interaction { get; set; }
    public InteractionGroup parent_group_interaction { get; set; }
    public InteractionAutotuneGroup parent_autotune_group_interaction { get; set; }
    public GameObject interaction_gameobject { get; protected set; }

    public System.Guid guid = System.Guid.NewGuid();
    public System.Guid parent_group_guid = System.Guid.Empty;
    public System.Guid parent_autotune_group_guid = System.Guid.Empty;

    [SerializeField]
    public int group_progress = 0;
    public bool is_active = true;
    public bool should_onFinishedEnterEvents_when_respawned = true;
    public bool can_add_project_progress = true; //Autotune interactions are not allowed to add project progress. That is controlled by the autotune group.

    public static event Action<string> interaction_finished_event;

    private void Update()
    {
        if (config_interaction != null) //Real interaction
        {
            if (config_interaction.AutoSelect == true && is_active == true) //Autoselect
            {
                activate();
            }
        }
    }

    public string[] toStringArray()
    {
        Debug.Log("Serializing interaction " + name + " isactive: " + is_active);
        return new string[] {
            config_interaction.id,
            guid.ToString(),
            parent_group_guid.ToString(),
            parent_autotune_group_guid.ToString(),
            group_progress.ToString(),
            is_active.ToString(),
            should_onFinishedEnterEvents_when_respawned.ToString()
        };
    }

    public virtual Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        config_interaction = _interaction;
        interaction_gameobject = gameObject;
        interaction_gameobject.name = _interaction.id;

        return this;
    }

    public void activate()
    {
        is_active = false;
        if (config_interaction != null)
        {
            Debug.Log("Activating interaction " + config_interaction.id);

            if (config_interaction.enterEvents != null)
            {
                GameStart.event_manager.main_event_player.addEvents(config_interaction.enterEvents);
            }
        }
        EventManager.all_script_events_finished_event += onFinishedEnterEvents;
    }
    public virtual void onFinishedEnterEvents()
    {
        if (config_interaction != null) //These can be fake interactions
            Debug.Log("onFinishedEnterEvents " + config_interaction.id);
        EventManager.all_script_events_finished_event -= onFinishedEnterEvents;
    }
    public void interactionComplete(bool success = true)
    {
        Scenario.completeInteraction(config_interaction.id);

        addExitEvents(success);
    }
    protected void addExitEvents(bool success)
    {
        Assert.IsNotNull(config_interaction, "finished interaction was null");

        Debug.Log("addExitEvents " + config_interaction.id);

        //This interaction has an immediate exit.
        if (config_interaction.exitEvents == null
            && config_interaction.successEvents == null
            && config_interaction.failEvents == null
            && config_interaction.qteSuccessEvents == null
            && config_interaction.qteFailEvents == null)
        {
            onFinishedExitEvents();
            return;
        }

        if (success == true)
        {
            if (config_interaction.qteSuccessEvents != null)
                GameStart.event_manager.main_event_player.addEvents(config_interaction.qteSuccessEvents);
            if (config_interaction.successEvents != null)
                GameStart.event_manager.main_event_player.addEvents(config_interaction.successEvents);
        }
        else
        {
            if (config_interaction.qteFailEvents != null)
                GameStart.event_manager.main_event_player.addEvents(config_interaction.qteFailEvents);
            if (config_interaction.failEvents != null)
                GameStart.event_manager.main_event_player.addEvents(config_interaction.failEvents);
        }

        if (config_interaction.exitEvents != null)
            GameStart.event_manager.main_event_player.addEvents(config_interaction.exitEvents);

        EventManager.all_script_events_finished_event += onFinishedExitEvents;

    }
    protected virtual void onFinishedExitEvents()
    {
        if (config_interaction != null) //These can be fake interactions
            Debug.Log("onFinishedExitEvents " + config_interaction.id);
        EventManager.all_script_events_finished_event -= onFinishedExitEvents;

        if (config_interaction.leadsTo != null)
        {
            spawnLeadsTo();
        }
        else
        {
            complete();
        }
    }
    public void spawnLeadsTo()
    {
        Debug.Log("spawnLeadsTo");
        GameObject leadsto = null;
        if (config_interaction.leadsToPredicate != null)
        {
            int best_match_leads_to = config_interaction.leadsTo.Length - 1;
            if (config_interaction.leadsToPredicate != null)
            {
                for (int i = config_interaction.leadsToPredicate.Length - 1; i >= 0; i--)
                {
                    if (Predicate.parsePredicate(config_interaction.leadsToPredicate[i]))
                    {
                        best_match_leads_to = i;
                    }
                }
            }
            if (config_interaction.leadsTo[best_match_leads_to] == config_interaction.id)
            {
                gameObject.transform.name = "old_" + gameObject.transform.name;
            }

            Debug.Log("Activating a leads to " + config_interaction.leadsTo[best_match_leads_to]);

            leadsto = GameStart.interaction_manager.spawnInteraction(config_interaction.leadsTo[best_match_leads_to]);
        }
        else
        {
            if (config_interaction.leadsTo[0] != "exit")
            {
                Debug.Log("Activating leads to " + config_interaction.leadsTo[0]);

                leadsto = GameStart.interaction_manager.spawnInteraction(config_interaction.leadsTo[0]);
            }
            Debug.Log("Activating leads to exit. We are done.");
        }
        if (leadsto != null && config_interaction.type != "Group" && config_interaction.type != "AutotuneGroup")
        {
            if (parent_group_interaction != null)
            {
                parent_group_interaction.addMemberInteraction(leadsto.GetComponent<Interaction>());
            }
        }

        complete();
    }

    public void complete()
    {
        if (config_interaction != null) //These can be fake interactions
            Debug.Log("Interaction complete " + config_interaction.id);
        if (config_interaction == null) return;

        if (config_interaction.successReward != null)
        {
            Reward.getReward(config_interaction.successReward);
        }
        interaction_finished_event?.Invoke(config_interaction.id);

        if (parent_group_interaction != null)
            parent_group_interaction.memberInteractionFinished(this);
        if (parent_autotune_group_interaction != null)
            parent_autotune_group_interaction.memberInteractionComplete(this);
        if (can_add_project_progress)
            Project.addProgress(config_interaction.ProjectProgress);


        if (parent_group_interaction == null && parent_autotune_group_interaction == null)
        {
            destroy();
        }
    }

    public virtual void destroy()
    {
        InteractionManager.active_interactions.Remove(this);
        GameObject.DestroyImmediate(interaction_gameobject);
        GameStart.interaction_manager.onInteractionDestroyed(this);
    }

    protected void setHotspot()
    {
        if (config_interaction.spot != null) {

            Scene.setGameObjectToHotspot(interaction_gameobject, config_interaction.spot);
        }
    }
}
