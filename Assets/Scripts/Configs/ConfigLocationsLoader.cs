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

    public override void combine(List<ConfigLocation> other_list)
    {
        throw new NotImplementedException();
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

    public override void combine(List<ConfigLocationHub> other_list)
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

    public override void combine(List<ConfigHubNPC> other_list)
    {
        throw new NotImplementedException();
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

    public override void combine(List<ConfigNpcWaypointSpawn> other_list)
    {
        throw new NotImplementedException();
    }
}

class ConfigLocationsLoader
{
    public static async Task loadConfigsAsync()
    {
        Configs.config_location = await ConfigLocation.CreateFromJSONAsync(Common.getConfigPath("Locations-"));
        Configs.config_location_hub = await ConfigLocationHub.CreateFromJSONAsync(Common.getConfigPath("Locations-"));
        Configs.config_hub_npc = await ConfigHubNPC.CreateFromJSONAsync(Common.getConfigPath("HubNpcs-"));
        Configs.config_npc_waypoint_spawn = await ConfigNpcWaypointSpawn.CreateFromJSONAsync(Common.getConfigPath("NpcWaypointSpawn-"));
    }
}

