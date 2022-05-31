using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationHub
{

    public static ConfigLocationHub._LocationHub current { get; private set; }

    public static void loadLocationHub(string id)
    {
        Debug.Log("Loading location hub " + id);
        current = Configs.config_location_hub.LocationHub[id];
        Location.current = null;
        Scenario.onScenarioLoaded += onScenarioLoaded;
        Scenario.Load(current.scenarioId);
    }

    public static void onScenarioLoaded()
    {
        Scenario.onScenarioLoaded -= onScenarioLoaded;
        spawnHubLocationHotspots();
    }


    public static void exitLocationHub()
    {
        current = null;
    }

    public static void spawnHubLocationHotspots()
    {
        foreach (ConfigLocation._Location location in Configs.config_location.Location.Values)
        {
            if (location.hubId == current.hubId && location.defaultScenarios != null)
            {
                if (location.unlockPredicate != null && location.unlockPredicate[0] == "false")
                    continue;

                GameObject interaction_gameobject = GameObject.Instantiate(Resources.Load<GameObject>("hud_important"), Vector3.zero, Quaternion.identity);
                InteractionLocationHubButton hub_npc_interaction = interaction_gameobject.AddComponent<InteractionLocationHubButton>();

                Vector3 hotspot_vec = Vector3.zero;
                foreach (ConfigScene._Scene.HotSpot hotspot in Scene.current.hotspots)
                {
                    if (hotspot.name == location.hotSpotId)
                        hotspot_vec = new Vector3(hotspot.position[0] * -0.01f, hotspot.position[1] * 0.01f, hotspot.position[2] * 0.01f);
                }

                hub_npc_interaction.interactionLocationHubButtonSetup(ref location.locationId, ref hotspot_vec);
            }
        }
    }

    public static void destroyLocationButtons()
    {
        foreach(Interaction i in InteractionManager.active_interactions)
        {
            if (i is InteractionLocationHubButton)
            {
                i.finish();
            }
        }
    }
}
