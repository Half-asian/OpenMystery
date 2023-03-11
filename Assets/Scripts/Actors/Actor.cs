using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ModelLoading;
using System.Globalization;
using IndividualComponents;
using System.IO;

public class Actor
{
    //Later on, change this from dictionary look up to using events
    public static Dictionary<string, ActorController> actor_controllers = new Dictionary<string, ActorController>();


    //This is just used for cleanup
    private static List<ActorController> actor_controllers_pool = new List<ActorController>(); //we can't use the character dictionary as a complete reference of all spawned characters. This is because there is no unique spawn identifier.

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
        Scenario.onScenarioCallClear += cleanup;
    }

    private static void cleanup()
    {
        foreach (ActorController ac in actor_controllers_pool)
        {
            ac.destroy();
            
        }
        actor_controllers_pool.Clear();
        actor_controllers.Clear();
    }

    public static ActorController getActor(string instance_id)
    {
        if (actor_controllers.ContainsKey(instance_id))
        {
            return actor_controllers[instance_id];
        }
        else
        {
            Debug.Log("Couldn't find actor" + instance_id);
            return null;
        }
    }

    //Actors will still spawn with invalid waypoint_id. Tested in retail.
    public static ActorController spawnActor(string hpactor_id, string waypoint_id, string instance_id)
    {
        if (actor_controllers.ContainsKey(instance_id))
        {
            respawnCharacterInScene(instance_id);
            if (waypoint_id != null)
            {
                actor_controllers[instance_id].teleportCharacter(waypoint_id);
            }
            return actor_controllers[instance_id];
        }


        ModelMaterials.lighting_layers = new List<string>();

        var waypoint = Scene.getWayPoint(waypoint_id);
        if (Scene.getWayPoint(waypoint_id) != null)
        {
            if (waypoint.lightLayerOverride != null)
            {
                ModelMaterials.lighting_layers.AddRange(waypoint.lightLayerOverride); //Figure out if more than one light layer can be applied at once
            }
        }
        
        if (ModelMaterials.lighting_layers.Count == 0)
        {
            ModelMaterials.lighting_layers.Add("CHARACTER");
        }

        if (hpactor_id == "c_avatar_female_base" || hpactor_id == "c_avatar_male_base")
            hpactor_id = "Avatar";

        //Debug.Log("Spawning character " + actor_id + " with name " + character_name);
        ActorController actor_controller = null;
        ConfigHPActorInfo._HPActorInfo config_actor = null;

        hpactor_id = ConfigActorMapping.getActorMapping(hpactor_id, Player.local_avatar_gender);

        if (hpactor_id != "Avatar" && !Configs.config_hp_actor_info.HPActorInfo.ContainsKey(hpactor_id))
            throw new System.Exception("Error: Tried to spawn an actor with invalid id " + hpactor_id);


        if (hpactor_id == "Avatar")
        {
            if (Player.local_avatar_gender == "female")                
                config_actor = Configs.config_hp_actor_info.HPActorInfo[ConfigActorMapping.getActorMapping("c_avatar_female_base", "female")];
            else
                config_actor = Configs.config_hp_actor_info.HPActorInfo[ConfigActorMapping.getActorMapping("c_avatar_male_base", "male")];

            Player.local_avatar_onscreen_name = instance_id;
            actor_controller = StaticAvatarSpawner.spawnStaticAvatar(config_actor);
            actor_controller.name = instance_id;
        }
        else
        {
            config_actor = Configs.config_hp_actor_info.HPActorInfo[hpactor_id];
            Model model = ModelManager.loadModel(config_actor.modelId);
            if (model == null)
                throw new System.Exception("Failed to load actor model " + config_actor.modelId);

            actor_controller = model.game_object.AddComponent<ActorController>();
            actor_controller.config_hpactor = config_actor;
            actor_controller.setup(model);

            if (config_actor.quidditchMaterialOptions != null)
            {
                setQuidditchProperties(actor_controller.model.game_object, config_actor);
            }

            actor_controller.model.game_object.name = instance_id;

            if (config_actor.modelPatches != null)
            {
                foreach (string patch_model_id in config_actor.modelPatches)
                {
                    actor_controller.addPatch(patch_model_id);
                }
            }
        }

        //Set linked avatar component colors
        if (config_actor.linkedComponentColorOverrides != null)
        {
            linkComponentColorOverrides(actor_controller);
        }




        if (actor_controllers != null)
            actor_controllers[instance_id] = actor_controller;


        if (Scene.scene_model != null)
            actor_controller.model.game_object.transform.SetParent(GameStart.current.actors_holder);
        actor_controller.replaceCharacterIdle(actor_controller.config_hpactor.animId_idle);
        actor_controllers_pool.Add(actor_controller);
        actor_controller.teleportCharacter(waypoint_id);
        return actor_controller;
    }

    private static void linkComponentColorOverrides(ActorController actor_controller)
    {
        var skinned_mesh_renderers = actor_controller.model.game_object.GetComponentsInChildren<SkinnedMeshRenderer>();
        var avatar_components = new AvatarComponents(Path.Combine(GlobalEngineVariables.player_folder, "Avatar.json"));
        foreach (var component in actor_controller.config_hpactor.linkedComponentColorOverrides)
        {
            switch (component)
            {
                case "eyes":
                    ComponentEyes.setExternalColorModifiers(skinned_mesh_renderers, avatar_components);
                    break;
                case "faces":
                    ComponentFaces.setExternalColorModifiers(skinned_mesh_renderers, avatar_components);
                    break;
                case "hair":
                    ComponentHair.setExternalColorModifiers(skinned_mesh_renderers, avatar_components);
                    break;
                case "lips":
                    ComponentLips.setExternalColorModifiers(skinned_mesh_renderers, avatar_components);
                    break;
                case "brows":
                    ComponentBrows.setExternalColorModifiers(skinned_mesh_renderers, avatar_components);
                    break;
            }
        }
    }

    public static void destroyCharacter(string character_name)
    {
        if (actor_controllers.ContainsKey(character_name))
        {
            actor_controllers[character_name].destroy();
            actor_controllers[character_name] = null;
        }
        actor_controllers.Remove(character_name);
    }

    public static void despawnCharacterInScene(string character_name)
    {
        if (actor_controllers.ContainsKey(character_name))
        {
            actor_controllers[character_name].gameObject.SetActive(false);
        }
    }

    public static void respawnCharacterInScene(string character_name)
    {
        actor_controllers[character_name].gameObject.SetActive(true);
        actor_controllers[character_name].setCharacterIdle();
    }

    public static string[][] serializeActors()
    {
        string[][] serialized_interactions = new string[actor_controllers_pool.Count][];
        for (int i = 0; i < actor_controllers_pool.Count; i++)
        {
            serialized_interactions[i] = actor_controllers_pool[i].GetComponent<ActorController>().toStringArray();
        }

        return serialized_interactions;
    }

    public static void spawnSerializedActors(string[][] serialized_actors)
    {
        foreach (string[] serialized_actor in serialized_actors)
        {
            ActorController am = spawnActor(serialized_actor[1], null, serialized_actor[0]);
            am.gameObject.transform.position = new Vector3(float.Parse(serialized_actor[2], CultureInfo.InvariantCulture), float.Parse(serialized_actor[3], CultureInfo.InvariantCulture), float.Parse(serialized_actor[4], CultureInfo.InvariantCulture));
            am.gameObject.transform.rotation = new Quaternion(float.Parse(serialized_actor[5], CultureInfo.InvariantCulture), float.Parse(serialized_actor[6], CultureInfo.InvariantCulture), float.Parse(serialized_actor[7], CultureInfo.InvariantCulture), float.Parse(serialized_actor[8], CultureInfo.InvariantCulture));
            am.gameObject.transform.localScale = new Vector3(float.Parse(serialized_actor[9], CultureInfo.InvariantCulture), float.Parse(serialized_actor[10], CultureInfo.InvariantCulture), float.Parse(serialized_actor[11], CultureInfo.InvariantCulture));

            Enum.TryParse(serialized_actor[12], out am.actor_state);
            am.teleportCharacter(serialized_actor[13]);
            am.replaceCharacterIdle(serialized_actor[14]);
        }
    }

    public static void spawnScenarioActors()
    {
        actor_controllers = new Dictionary<string, ActorController>();
        actor_controllers_pool = new List<ActorController>();

        if (Scenario.current.scenario_config.charSpawns != null)
        {
            foreach (ConfigScenario._Scenario.CharSpawn char_spawn in Scenario.current.scenario_config.charSpawns)
            {
                spawnActor(char_spawn.charId, char_spawn.waypointId, char_spawn.instanceId);
            }
        }

        if (Scenario.current.scenario_config.randomSpawns != null)
        {
            foreach (string[] spawn_strings in Scenario.current.scenario_config.randomSpawns)
            {
                if (spawn_strings.Length > 1)
                {
                    Debug.LogError("GameStart:ActivateScenario - more than 1 npcwaypoint listed in random spawn");
                }
                ConfigNpcWaypointSpawn._NpcWaypointSpawn spawn = Configs.config_npc_waypoint_spawn.NpcWaypointSpawn[spawn_strings[0]];

                int random_character = UnityEngine.Random.Range(0, spawn.validCharacters.Length);
                int random_sequence = UnityEngine.Random.Range(0, spawn.validSequences.Length);

                spawnActor(spawn.validCharacters[random_character], spawn.waypoint, spawn.spawnId);

                if (Configs.config_char_anim_sequence.CharAnimSequence.ContainsKey(spawn.validSequences[random_sequence]))
                {
                    actor_controllers[spawn.spawnId].gameObject.AddComponent<ActorAnimSequence>();
                    actor_controllers[spawn.spawnId].gameObject.GetComponent<ActorAnimSequence>().initAnimSequence(spawn.validSequences[random_sequence], false);
                }
                else
                {
                    actor_controllers[spawn.spawnId].replaceCharacterIdle(spawn.validSequences[random_sequence]);
                }
            }
        }

        if (Scenario.current.objective != null && Scenario.current.objective.objective_config.objectiveHubNpcs != null)
        {
            foreach (string objective_hub_npc_string in Scenario.current.objective.objective_config.objectiveHubNpcs)
            {
                ConfigHubNPC._HubNPC objective_hub_npc = Configs.config_hub_npc.HubNPC[objective_hub_npc_string];
                spawnActor(objective_hub_npc.actorId, objective_hub_npc.hubWaypoint, objective_hub_npc.hubNpcId);
            }
        }

        HubNPC.spawnScenarioHubNPCs();
    }

    private static void setQuidditchProperties(GameObject actor_gameobject, ConfigHPActorInfo._HPActorInfo actor_info)
    {
        string house;
        if (actor_info.quidditchMaterialOptions.houseId == "Avatar")
        {
            house = Player.local_avatar_house;
        }
        else if (actor_info.quidditchMaterialOptions.houseId == "QuidditchRival")
        {
            if (Player.local_avatar_house == "ravenclaw")
                house = "slytherin";
            else
                house = "ravenclaw";
        }
        else if (actor_info.quidditchMaterialOptions.houseId == "QuidditchOpponent")
        {
            if (Player.local_avatar_opponent_house != null)
            {
                house = Player.local_avatar_opponent_house;
            }
            else
            {
                house = "gryffindor";
            }
        }
        else
        {
            house = actor_info.quidditchMaterialOptions.houseId;
        }
        if (actor_info.quidditchMaterialOptions.mapping.Keys != null)
        {
            foreach (string map in actor_info.quidditchMaterialOptions.mapping.Keys)
            {
                Transform piece = actor_gameobject.transform.Find(map);
                if (piece != null)
                {
                    if (piece.GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        if (house == "ravenclaw" || (house == "Avatar" && Player.local_avatar_house == "ravenclaw"))
                        {
                            ModelMaterials.setHouseUniforms(piece.GetComponent<SkinnedMeshRenderer>().material, "ravenclaw");
                        }
                        else if (house == "slytherin" || (house == "Avatar" && Player.local_avatar_house == "slytherin"))
                        {
                            ModelMaterials.setHouseUniforms(piece.GetComponent<SkinnedMeshRenderer>().material, "slytherin");
                        }
                        else if (house == "hufflepuff" || (house == "Avatar" && Player.local_avatar_house == "hufflepuff"))
                        {
                            ModelMaterials.setHouseUniforms(piece.GetComponent<SkinnedMeshRenderer>().material, "hufflepuff");
                        }
                        else if (house == "gryffindor" || (house == "Avatar" && Player.local_avatar_house == "gryffindor"))
                        {
                            ModelMaterials.setHouseUniforms(piece.GetComponent<SkinnedMeshRenderer>().material, "gryffindor");
                        }
                        else
                        {
                            Debug.LogError("Invalid houseid " + house);
                        }
                    }
                    else
                    {
                        Debug.Log("quidditch piece no skinnedmeshrenderer");
                    }
                }
            }
        }
    }

}
