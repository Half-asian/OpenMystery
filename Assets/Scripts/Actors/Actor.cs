using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Actor
{
    //Later on, change this from dictionary look up to using events
    public static Dictionary<string, ActorController> actor_controllers = new Dictionary<string, ActorController>();


    //This is just used for cleanup
    private static List<ActorController> actor_controllers_pool = new List<ActorController>(); //we can't use the character dictionary as a complete reference of all spawned characters. This is because there is no unique spawn identifier.

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
        Scenario.onScenarioLoaded += cleanup;
    }

    private static void cleanup()
    {
        foreach (ActorController ac in actor_controllers_pool)
        {
            ac.destroy();
        }
        actor_controllers_pool = new List<ActorController>();
        actor_controllers = new Dictionary<string, ActorController>();
    }

    public static ActorController spawnActor(string actor_id, string waypoint_id, string character_name)
    {
        if (actor_id == "c_avatar_female_base" || actor_id == "c_avatar_male_base")
            actor_id = "Avatar";

        //Debug.Log("Spawning character " + actor_id + " with name " + character_name);
        ActorController actor_controller = null;
        ConfigHPActorInfo._HPActorInfo config_actor = null;

        actor_id = ConfigActorMapping.getActorMapping(actor_id, Player.local_avatar_gender);

        if (actor_id != "Avatar" && !Configs.config_hp_actor_info.HPActorInfo.ContainsKey(actor_id))
            throw new System.Exception("Error: Tried to spawn an actor with invalid id " + actor_id);


        if (actor_id == "Avatar")
        {
            if (Player.local_avatar_gender == "female")
                config_actor = Configs.config_hp_actor_info.HPActorInfo["c_avatar_female_base"]; //Upgrade later to match wands
            else
                config_actor = Configs.config_hp_actor_info.HPActorInfo["c_avatar_male_base"]; //Upgrade later to match wands

            Player.local_avatar_onscreen_name = character_name;
            actor_controller = StaticAvatarSpawner.spawnStaticAvatar();
            if (Player.local_avatar_clothing_type != null)
            {
                Player.changeClothes(Player.local_avatar_clothing_type, Player.local_avatar_secondary_clothing_option);
            }
        }
        else
        {
            config_actor = Configs.config_hp_actor_info.HPActorInfo[actor_id];
            Model model = ModelManager.loadModel(config_actor.modelId);
            if (model == null)
                throw new System.Exception("Failed to load actor model " + config_actor.modelId);

            actor_controller = model.game_object.AddComponent<ActorController>();
            actor_controller.setup(model);
            actor_controller.actor_info = config_actor;
            actor_controller.actor_animation.animId_idle = config_actor.animId_idle;

            if (config_actor.quidditchMaterialOptions != null)
            {
                setQuidditchProperties(actor_controller.model.game_object, config_actor);
            }

            actor_controller.model.game_object.name = character_name;

            if (config_actor.modelPatches != null)
            {
                foreach (string patch_model_id in config_actor.modelPatches)
                {
                    actor_controller.addPatch(patch_model_id);
                }
            }
        }



        if (actor_controllers != null)
            actor_controllers[character_name] = actor_controller;

        if (waypoint_id != null)
        {
            Scene.setGameObjectToWaypoint(actor_controller.model.game_object, waypoint_id);
        }

        if (Scene.scene_model != null)
            actor_controller.model.game_object.transform.parent = Scene.scene_model.game_object.transform;
        actor_controller.actor_animation.replaceCharacterIdle(actor_controller.actor_info.animId_idle);
        actor_controller.actor_animation.setCharacterIdle();
        actor_controllers_pool.Add(actor_controller);
        return actor_controller;
    }

    public static void despawnCharacterInScene(string character_name)
    {
        if (actor_controllers.ContainsKey(character_name))
        {
            actor_controllers[character_name].destroy();
            actor_controllers_pool.Remove(actor_controllers[character_name]);
            actor_controllers.Remove(character_name);
        }
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
            am.gameObject.transform.position = new Vector3(float.Parse(serialized_actor[2]), float.Parse(serialized_actor[3]), float.Parse(serialized_actor[4]));
            am.gameObject.transform.rotation = new Quaternion(float.Parse(serialized_actor[5]), float.Parse(serialized_actor[6]), float.Parse(serialized_actor[7]), float.Parse(serialized_actor[8]));
            am.gameObject.transform.localScale = new Vector3(float.Parse(serialized_actor[9]), float.Parse(serialized_actor[10]), float.Parse(serialized_actor[11]));

            Enum.TryParse(serialized_actor[12], out am.actor_state);
            am.actor_movement.destination_waypoint = serialized_actor[13];
            am.actor_animation.replaceCharacterIdle(serialized_actor[14]);
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
                    actor_controllers[spawn.spawnId].actor_animation.replaceCharacterIdle(spawn.validSequences[random_sequence]);
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
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_ravenclaw", 1);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_slytherin", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_hufflepuff", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_gryffindor", 0);

                        }
                        else if (house == "slytherin" || (house == "Avatar" && Player.local_avatar_house == "slytherin"))
                        {
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_ravenclaw", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_slytherin", 1);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_hufflepuff", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_gryffindor", 0);
                        }
                        else if (house == "hufflepuff" || (house == "Avatar" && Player.local_avatar_house == "hufflepuff"))
                        {
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_ravenclaw", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_slytherin", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_hufflepuff", 1);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_gryffindor", 0);
                        }
                        else if (house == "gryffindor" || (house == "Avatar" && Player.local_avatar_house == "gryffindor"))
                        {
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_ravenclaw", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_slytherin", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_hufflepuff", 0);
                            piece.GetComponent<SkinnedMeshRenderer>().material.SetInt("is_gryffindor", 1);
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
