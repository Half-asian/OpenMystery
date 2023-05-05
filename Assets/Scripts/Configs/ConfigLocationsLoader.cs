using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigLocation : Config<ConfigLocation>
{
    public class _Location
    {
        public string[] additionalScenarioPredicates;
        public string[] additionalScenarios;
        public string ambientId;
        public string[] defaultScenarios;
        public string explorePredicate;
        public string hotSpotId;
        public string hubId;
        public string icon;
        public string iconInactive;
        public string locationId;
        public string locationName;
        public string[] lockedDescription;
        public string musicPlaylistId;
        public string[] unlockPredicate;
    }

    public Dictionary<string, _Location> Location;

    public override ConfigLocation combine(List<ConfigLocation> other_list)
    {
        throw new NotImplementedException();
    }
    public static void getConfig()
    {
        Configs.config_location = getJObjectsConfigsListST("Location");
    }
}

public class ConfigLocationHub : Config<ConfigLocationHub>
{
    public class _LocationHub
    {
        public string hubId;
        public string loaderImg;
        public string name;
        public string npcWaypoint;
        public int order;
        public string scenarioId;
        public string showFilter;
    }

    public Dictionary<string, _LocationHub> LocationHub;

    public override ConfigLocationHub combine(List<ConfigLocationHub> other_list)
    {
        throw new NotImplementedException();
    }
}

public class ConfigHubNPC : Config<ConfigHubNPC>
{
    public class _HubNPC
    {
        public string actorId;
        public bool autoStartDialogue;
        public string avatarWaypoint;
        public string hubId;
        public string hubNpcId;
        public string hubWaypoint;
        public string primaryDialogue;
        public int priority;
    }

    public Dictionary<string, _HubNPC> HubNPC;

    public override ConfigHubNPC combine(List<ConfigHubNPC> other_list)
    {
        throw new NotImplementedException();
    }
    public static void getConfig()
    {
        Configs.config_hub_npc = getJObjectsConfigsListST("HubNpc");
    }
    public static void getAllReferences(string hub_npc_id, ref ReferenceTree reference_tree)
    {
        if (!reference_tree.hub_npcs.Contains(hub_npc_id))
            reference_tree.hub_npcs.Add(hub_npc_id);
        else
            return;

        var hub_npc = Configs.config_hub_npc.HubNPC[hub_npc_id];
        if (hub_npc.primaryDialogue != null)
        {
            ConfigHPDialogueLine.getAllReferences(DialogueManager.getFirstDialogueLine(hub_npc.primaryDialogue), ref reference_tree);
        }

    }
}

public class ConfigNpcWaypointSpawn : Config<ConfigNpcWaypointSpawn>
{
    public class _NpcWaypointSpawn
    {
        public string spawnId;
        public string[] validCharacters;
        public string[] validSequences;
        public string waypoint;
    }

    public Dictionary<string, _NpcWaypointSpawn> NpcWaypointSpawn;

    public override ConfigNpcWaypointSpawn combine(List<ConfigNpcWaypointSpawn> other_list)
    {
        throw new NotImplementedException();
    }
    public static void getConfig()
    {
        Configs.config_npc_waypoint_spawn = getJObjectsConfigsListST("NpcWaypointSpawn");
    }
}

