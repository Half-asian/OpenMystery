using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static ConfigScene._Scene;

public class HubNPC
{
    public static Dictionary<string, HubNPC> activeHubNPCs = new Dictionary<string, HubNPC>();
    public ConfigHubNPC._HubNPC config_hubnpc;

    #region static methods
    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        activeHubNPCs = new Dictionary<string, HubNPC>();
    }

    public static void addHubNPC(string hubnpc_id)
    {
        if (!Configs.config_hub_npc.HubNPC.ContainsKey(hubnpc_id))
            throw new Exception("Tried to activate invalid hubnpc with id " + hubnpc_id);

        ConfigHubNPC._HubNPC hubnpc_config = Configs.config_hub_npc.HubNPC[hubnpc_id];
        addHubNPC(hubnpc_config);
    }

    public static void addHubNPC(ConfigHubNPC._HubNPC _config_hubnpc)
    {
        HubNPC new_hubnpc = new HubNPC();
        new_hubnpc.config_hubnpc = _config_hubnpc;
        activeHubNPCs[new_hubnpc.config_hubnpc.hubNpcId] = new_hubnpc;
    }

    public static void removeHubNPC(string hubnpc_id)
    {
        if (!Configs.config_hub_npc.HubNPC.ContainsKey(hubnpc_id))
            throw new Exception("Tried to deactivate invalid hubnpc with id " + hubnpc_id);

        activeHubNPCs.Remove(hubnpc_id);
    }

    public static HubNPC getHubNPC(string hubnpc_id)
    {
        if (activeHubNPCs.ContainsKey(hubnpc_id))
            return activeHubNPCs[hubnpc_id];
        return null;
    }

    public static void spawnScenarioHubNPCs()
    {
        if (LocationHub.current != null) //We are in a hub
        {
            foreach(HubNPC hubnpc in activeHubNPCs.Values)
            {
                ConfigHubNPC._HubNPC config = hubnpc.config_hubnpc;
                if (hubnpc.config_hubnpc.hubId == LocationHub.current.hubId)
                {
                    Actor.spawnActor(config.actorId, config.hubWaypoint, config.hubNpcId);
                    if (config.avatarWaypoint != null)
                    {
                        Actor.getActor("Avatar")?.teleportCharacter(config.avatarWaypoint);
                    }
                    if (config.primaryDialogue != null)
                    {
                        hubnpc.activateDialogue();
                    }
                }
            }
        }
    }
    #endregion

    //TODO: Set up a priority system. This is needed when we can run multiple quests at the same time.
    //Otherwise two hub npcs could start dialogue simultaneously.
    private void activateDialogue()
    {
        if (config_hubnpc.autoStartDialogue == true)
        {
            GameStart.dialogue_manager.activateDialogue(config_hubnpc.primaryDialogue);
        }
        else
        {
            GameStart.interaction_manager.spawnHubNPCInteraction(ref config_hubnpc.primaryDialogue, config_hubnpc.hubWaypoint);
        }
    }
}
