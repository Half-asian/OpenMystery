using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CocosModel;

public class Dorm
{

    static List<Node> flatStuff = new List<Node>();
    public static void LoadDorm(bool is_flat = false)
    {
        foreach(var ft in Configs.config_furniture_type.FurnitureType.Values)
        {
            if (ft.defaultComponent != null)
            {
                LoadFurnitureComponent(ft.defaultComponent, ft.waypoint, ft.category);
            }
        }

        foreach (var pet in Configs.config_pet.Pet.Values)
        {
            string waypoint_id = null;

            if (is_flat)
            {
                waypoint_id = pet.flatWaypoint;
            }
            else
            {
                waypoint_id = pet.surfaceWaypointMap[Scenario.current.scenario_config.scenarioId];
            }
            Debug.Log("Spawning pet " + pet.modelId + " " + waypoint_id);
            Prop.spawnPropFromEvent(pet.modelId, waypoint_id, pet.id, null);
            Prop.spawned_props[pet.id].playAnimSequence(pet.animSequence, null);
            Prop.spawned_props[pet.id].GetComponent<AnimationSequence>().advanceAnimSequence();
            flatStuff.Add(Prop.spawned_props[pet.id]);

            //GameStart.current.StartCoroutine(waitABit(pet.id, pet.an));
        }

    }

    public static void LoadFurnitureComponent(string component_id, string waypoint_id, string name)
    {
        var furniture_config = Configs.config_furniture.Furniture[component_id];

        if (furniture_config.modelId_default != null)
        {
            if (Scene.isValidWayPoint(waypoint_id))
            {
                Prop.spawnPropFromEvent(furniture_config.modelId_default, waypoint_id, name, null);
                flatStuff.Add(Prop.spawned_props[name]);
            }
        }
        else
            Debug.Log("furniture component " + furniture_config.componentId + " does not have modelId_default");
    }

    public static void hideFlatStuff()
    {
        List<Node> nullStuff = new List<Node>();
        foreach (var stuff in flatStuff)
        {
            if (stuff == null)
                nullStuff.Add(stuff);
            else
                stuff.model.game_object.SetActive(false);
        }
        foreach (var n in nullStuff)
        {
            flatStuff.Remove(n);
        }
    }

    public static void showFlatStuff()
    {
        List<Node> nullStuff = new List<Node>();
        foreach (var stuff in flatStuff)
        {
            if (stuff == null)
                nullStuff.Add(stuff);
            else
                stuff.model.game_object.SetActive(true);
        }
        foreach (var n in nullStuff)
        {
            flatStuff.Remove(n);
        }
    }

}
