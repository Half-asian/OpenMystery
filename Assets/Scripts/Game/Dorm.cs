using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CocosModel;

public class Dorm
{
    /*public static void LoadDorm()
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
            string waypoint_id = pet.surfaceWaypointMap[Scenario.current.scenario_config.scenarioId];

            Prop.spawnPropFromEvent(pet.modelId, waypoint_id, pet.id, null);
            Prop.spawned_props[pet.id].playAnimSequence(pet.animSequence);
            Prop.spawned_props[pet.id].GetComponent<AnimationSequence>().advanceAnimSequence();
            //GameStart.current.StartCoroutine(waitABit(pet.id, pet.an));
        }

    }

    public static void LoadFurnitureComponent(string component_id, string waypoint_id, string name)
    {
        var furniture_config = Configs.config_furniture.Furniture[component_id];

        if (furniture_config.modelId_default != null)
            Prop.spawnPropFromEvent(furniture_config.modelId_default, waypoint_id, name, null);
        else
            Debug.Log("furniture component " + furniture_config.componentId + " does not have modelId_default");
    }

    static IEnumerator waitABit(string pet_id, string animation)
    {
        yield return new WaitForSeconds(3);
        Prop.spawned_props[pet_id].playAnimSequence(animation);
    }
    */
}
