using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModelLoading;
using System.Globalization;
using static CocosModel;

public static class Events
{

    public static void doEventAction(string action_type, string[] action_params, EventPlayer event_player)
    {
        switch (action_type)
        {
            case "animateCharacter":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Actor.actor_controllers[action_params[0]].animateCharacter(action_params[1]);
                }
                else
                {
                    Debug.Log("Couldn't find actor " + action_params[0] + " in scene.");
                }

                break;

            case "replaceCharacterIdleStaggered":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Actor.actor_controllers[action_params[0]].replaceCharacterIdleStaggered(action_params[1]);

                }
                break;
            case "replaceCharacterIdle":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Actor.actor_controllers[action_params[0]].replaceCharacterIdle(action_params[1]);
                }

                break;

            case "setCharacterIdle":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    if (Actor.actor_controllers.ContainsKey(action_params[0]))
                    {
                        Actor.actor_controllers[action_params[0]].setCharacterIdle();
                    }
                }
                break;


            case "walkInCharacter": //Tp's character to closest connecting waypoint, then they walk to the destination
                if (!Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Debug.LogError("COULDN'T FIND CHARACTER " + action_params[0] + " IN CURRENT SCENE!");
                    break;
                }
                if (!Scene.current.waypoint_dict.ContainsKey(action_params[1]))
                {
                    Debug.LogError("COULDN'T FIND WAYPOINT " + action_params[1] + " IN CURRENT SCENE!");
                    break;
                }
                string waypoint_connector = null;
                if (Scene.current.waypointconnections != null)
                {
                    foreach (var w in Scene.current.waypointconnections)
                    {
                        if (w.connection[1] == action_params[1])
                            waypoint_connector = w.connection[0];
                        else if (w.connection[0] == action_params[1])
                            waypoint_connector = w.connection[1];
                    }
                }
                if (waypoint_connector == null)
                {
                    Debug.LogError("Couldn't find a connector to waypoint " + action_params[1]);
                }
                else
                {
                    Actor.actor_controllers[action_params[0]].teleportCharacter(waypoint_connector);
                }
                moveCharacter(action_params);
                break;

            case "moveCharacter":
                moveCharacter(action_params);

                break;
            case "turnHeadAt": //Don't move shoulders
                turnHeadAt(action_params);
                break;
            case "lookAt":
                lookAt(action_params);
                break;
            case "teleportCharacter":
                if (action_params.Length < 2)
                    break;
                if (!Scene.current.waypoint_dict.ContainsKey(action_params[1]))
                {
                    Debug.LogWarning("COULDN'T FIND WAYPOINT " + action_params[1] + " IN CURRENT SCENE!");
                    break;
                }

                if (!Actor.actor_controllers.ContainsKey(action_params[0]) || Actor.actor_controllers[action_params[0]].gameObject == null)
                {
                    Debug.Log("Couldn't find character " + action_params[0] + " in characters.");
                    break;
                }

                Actor.actor_controllers[action_params[0]].teleportCharacter(action_params[1]);

                Actor.actor_controllers[action_params[0]].actor_head.clearTurnHeadAt();
                Actor.actor_controllers[action_params[0]].actor_head.clearLookat();
                break;

            case "teleportProp":
                if (!Scene.current.waypoint_dict.ContainsKey(action_params[1]))
                {
                    Debug.LogWarning("COULDN'T FIND WAYPOINT " + action_params[1] + " IN CURRENT SCENE!");
                    break;
                }
                ConfigScene._Scene.WayPoint waypoint_c = Scene.current.waypoint_dict[action_params[1]];
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Common.setWaypointTransform(ref Prop.spawned_props[action_params[0]].model.game_object, waypoint_c);
                }
                else
                {
                    Debug.LogWarning("COULDN'T FIND Prop " + action_params[0]);
                }
                break;

            case "spawnCharacter":
                if (action_params.Length > 2)
                    Actor.spawnActor(action_params[0], action_params[1], action_params[2]);
                else
                    Actor.spawnActor(action_params[0], action_params[1], action_params[0]);
                break;

            case "despawnCharacter":
                Actor.despawnCharacterInScene(action_params[0]);
                break;

            case "spawnProp":
                //string model_id, string waypoint_id, string name_id, string group_id

                if (Scene.current.waypoint_dict.ContainsKey(action_params[1]))
                {
                    if (action_params.Length == 3)
                        Prop.spawnPropFromEvent(action_params[0], Scene.current.waypoint_dict[action_params[1]], action_params[2], "");
                    else if (action_params.Length == 4)
                        Prop.spawnPropFromEvent(action_params[0], Scene.current.waypoint_dict[action_params[1]], action_params[2], action_params[3]);
                    else
                        Prop.spawnPropFromEvent(action_params[0], Scene.current.waypoint_dict[action_params[1]], action_params[0], "");
                }
                break;

            case "removeProp":
            case "despawnProp":
                //string despawn_id
                //int mode
                //mode will be 1 if first id is the group id. Otherwise, will be prop id.

                if (action_params.Length == 2)
                {
                    if (action_params[1] != "1")
                    {
                        Debug.LogError("Unknown mode for despawnProp");
                        break;
                    }

                    List<string> props_to_destroy = new List<string>();
                    foreach (string p_key in Prop.spawned_props.Keys)
                    {
                        if (Prop.spawned_props[p_key].group == action_params[0])
                        {
                            GameObject.Destroy(Prop.spawned_props[p_key].model.game_object);
                            props_to_destroy.Add(p_key);
                        }
                    }

                    foreach (string p_key in props_to_destroy)
                    {
                        Prop.spawned_props.Remove(p_key);
                    }
                }

                else if (action_params.Length == 1)
                {
                    if (Prop.spawned_props.ContainsKey(action_params[0]))
                    {
                        GameObject.Destroy(Prop.spawned_props[action_params[0]].model.game_object);
                        Prop.spawned_props.Remove(action_params[0]);
                    }
                    else
                    {
                        Debug.LogError("remove/despawn prop didn't find prop " + action_params[0]);
                    }
                }

                else
                {
                    Debug.LogError("Unknown param length for despawnProp");
                }

                break;

            case "attachProp": //used to attach something to a character
                if (action_params.Length < 4)
                    Prop.attachPropFromEvent(action_params[0], action_params[1], action_params[2]);
                else
                    Prop.attachPropFromEvent(action_params[0], action_params[1], action_params[2], action_params[3]);
                break;

            case "detachProp":
                Prop.detachPropFromEvent(action_params[0], action_params[1]);
                break;

            case "hideEntity":
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Prop.spawned_props[action_params[0]].model.game_object.SetActive(false);
                }
                else
                {
                    Debug.Log("remove/despawn prop didn't find prop " + action_params[0]);
                }
                break;
            case "showEntity":
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Prop.spawned_props[action_params[0]].model.game_object.SetActive(true);
                }
                else
                {
                    Debug.Log("remove/despawn prop didn't find prop " + action_params[0]);
                }
                break;


            case "playCameraAnimation":

                Debug.Log("Playing camera anim " + action_params[0]);
                CameraManager.current.playCameraAnimation(action_params[0], true);
                break;

            case "playCinematicCameraAnimation":
                if (action_params.Length > 1)
                {
                    string[] focus_cam_action_params = new string[] { action_params[1], "0" };
                    CameraManager.current.focusCam(ref focus_cam_action_params);
                }

                Debug.Log("Playing cinematic camera anim " + action_params[0]);
                CameraManager.current.playCameraAnimation(action_params[0], true);
                break;

            case "focusCamera": //action param 1 is probably time to transition camera (lerp)
                if (event_player) event_player.last_camera = CameraManager.current.focusCam(ref action_params);
                break;

            case "panCamOnTrack":
                if (action_params[0] != "0:0" && event_player != null) //no clue what this means but it seems faulty
                    CameraManager.current.panCamOnTrack(event_player.last_camera.animation);
                break;

            case "hideCharacter":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {

                    if (action_params[0] == Player.local_avatar_onscreen_name)
                    {
                        AvatarComponents a = Actor.actor_controllers["Avatar"].avatar_components;
                        foreach (SkinnedMeshRenderer smr in a.base_model.game_object.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.enabled = false;
                        }
                        foreach (AvatarComponent i in a.components.Values)
                        {
                            i.hideComponent();
                        }
                    }
                    else
                    {
                        foreach (SkinnedMeshRenderer smr in Actor.actor_controllers[action_params[0]].gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.enabled = false;
                        }
                    }
                }
                break;
            case "showCharacter":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {

                    if (action_params[0] == Player.local_avatar_onscreen_name)
                    {
                        AvatarComponents a = Actor.actor_controllers["Avatar"].avatar_components;
                        foreach (SkinnedMeshRenderer smr in a.base_model.game_object.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.enabled = true;
                        }
                        foreach (AvatarComponent i in a.components.Values)
                        {
                            i.showComponent();
                        }
                    }
                    else
                    {
                        foreach (SkinnedMeshRenderer smr in Actor.actor_controllers[action_params[0]].gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.enabled = true;
                        }
                    }
                }
                break;

            case "screenFadeTo":
                if (Scenario.block_screenfades == true)
                {
                    break;
                }
                float ffade_to_time = 1.0f;
                if (action_params.Length >= 1)
                    ffade_to_time = float.Parse(action_params[0], CultureInfo.InvariantCulture);
                string color_string = action_params[1];
                int r = int.Parse(color_string.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(color_string.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(color_string.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                ScreenFade.fadeTo(ffade_to_time, new Color(r / 255.0f, g / 255.0f, b / 255.0f));
                break;

            case "fadeToBlack":
                if (Scenario.block_screenfades == true)
                {
                    break;
                }
                float fade_to_time = 1.0f;
                if (action_params.Length >= 1)
                {
                    bool success = float.TryParse(action_params[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float result);
                    if (success == false)
                        throw new System.Exception("fadeToBlack: Failed to parse " + action_params[0] + " as a float.");
                    else
                        fade_to_time = result;
                }
                ScreenFade.fadeTo(fade_to_time, Color.black);
                break;

            case "screenFadeFrom":
            case "fadeFromBlack":
                if (Scenario.block_screenfades == true)
                {
                    break;
                }
                float fade_from_time = 1.0f;
                if (action_params.Length >= 1)
                {
                    bool success = float.TryParse(action_params[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float result);
                    if (success == false)
                        throw new System.Exception("fadeFromBlack: Failed to parse " + action_params[0] + " as a float.");
                    else
                        fade_from_time = result;
                }
                ScreenFade.fadeFrom(fade_from_time, Color.black);
                break;

            case "safeAdvanceAnimSequenceTo": //This shit is used ONCE in the entire game
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Actor.actor_controllers[action_params[0]].gameObject.GetComponent<ActorAnimSequence>().safeAdvanceAnimSequenceTo(action_params[2]); //action_param 1 might be starting node (immediate set node).
                }
                else
                {
                    if (Prop.spawned_props.ContainsKey(action_params[0]))
                    {
                        Prop.spawned_props[action_params[0]].model.game_object.GetComponent<PropAnimSequence>().safeAdvanceAnimSequenceTo(action_params[2]);
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't find prop " + action_params[0] + " in spawned props");
                    }
                }
                if (event_player)
                    GameStart.event_manager.StartCoroutine(event_player.waitSafeAdvanceAnimSequenceToCoroutine(action_params[2]));

                break;

            case "advanceAnimSequence":
                if (Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    if (Actor.actor_controllers[action_params[0]].gameObject != null)
                    {
                        if (Actor.actor_controllers[action_params[0]].gameObject.GetComponent<ActorAnimSequence>() != null)
                            Actor.actor_controllers[action_params[0]].gameObject.GetComponent<ActorAnimSequence>().advanceAnimSequence();
                    }
                }
                else
                {
                    if (Prop.spawned_props.ContainsKey(action_params[0]))
                    {
                        if (Prop.spawned_props[action_params[0]].model.game_object.GetComponent<PropAnimSequence>() != null)
                            Prop.spawned_props[action_params[0]].model.game_object.GetComponent<PropAnimSequence>().advanceAnimSequence();
                        else
                            Debug.LogWarning("Prop " + action_params[0] + " did not have an anim sequence");
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't find prop " + action_params[0] + " in spawned props");
                    }
                }
                break;
            case "replaceCharacterIdleSequence":
            case "playCharacterAnimSequence":
                if (!Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Debug.LogError("Couldn't find actor " + action_params[0]);
                    return;
                }

                Actor.actor_controllers[action_params[0]].replaceCharacterIdleSequence(action_params[1]);

                break;

            case "moveCharacterWithSequence":

                if (!Actor.actor_controllers.ContainsKey(action_params[0]))
                {
                    Debug.LogError("Couldn't find actor " + action_params[0]);
                    return;
                }
                string[] new_action_params = new string[] { action_params[0], action_params[1] };
                moveCharacter(new_action_params);
                Actor.actor_controllers[action_params[0]].replaceCharacterWalkSequence(action_params[2]);
                break;

            case "animateProp":
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Prop.spawned_props[action_params[0]].playAnimation(action_params[1]);
                }
                else
                {
                    Debug.LogWarning("Couldn't find prop " + action_params[0] + " in spawned props");
                }
                break;
            case "playPropAnimSequence":
                Debug.Log("playPromAnimSequence " + action_params[0] + " " + action_params[1]);
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Prop.spawned_props[action_params[0]].playAnimSequence(action_params[1]);
                }
                else
                {
                    Debug.LogWarning("Couldn't find prop " + action_params[0] + " in spawned props");
                }
                break;

            case "replaceScenarioBGMusic":
                if (Configs.playlist_dict.ContainsKey(action_params[0]))
                {
                    Sound.playBGMusic(action_params[0]);
                }
                break;


            case "equipAvatarComponent":
                Actor.actor_controllers["Avatar"].avatar_components.equipAvatarComponent(action_params[0]);

                break;
            case "wearClothingType":
                Debug.Log("wearClothingType " + action_params[0]);
                if (action_params.Length < 2)
                    Scenario.changeClothes(action_params[0], null);
                else
                    Scenario.changeClothes(action_params[0], action_params[1]);
                break;
            case "setQuidditchHelmetEquipped":
                Scenario.setQuidditchHelmet(action_params[0]);
                break;
            case "setForcedQuidditchPosition":
                Player.local_avatar_quidditch_position = action_params[0];
                break;
            case "setOpponentHouse":
                Player.local_avatar_opponent_house = action_params[0];
                break;

            case "playSound":
                Sound.playSound(action_params[0]);
                break;

            case "awardReward":
                Reward.getReward(action_params[0]);
                break;

            case "setContentVar":
                Scenario.setContentVar(action_params);
                break;

            case "popupVC":
                if (action_params[0] == "YearEndViewController")
                {
                    GameStart.event_manager.main_event_player.total_block = true;
                    GameStart.event_manager.main_event_player.addCustomBlocking(new List<string> { "graduation", "graduation" });
                    Graduation.Graduate();
                }
                break;

            default:
                Debug.LogWarning("Unknown event type " + action_type);
                break;
        }
    }

    public static void doEventAction(string event_name, int event_index, string[] action_params, EventPlayer event_player)
    {
        doEventAction(Configs.config_script_events.ScriptEvents[event_name].action[event_index], action_params, event_player);
    }

    public static List<string> carvePath(List<string> visited, string destination)
    {
        List<string> final_result = new List<string>();

        if (Scene.current.waypointconnections == null)
        {
            return final_result;
        }

        foreach (ConfigScene._Scene.WayPointConnection connection in Scene.current.waypointconnections)
        {
            //Check if the waypoint connection defines invalid waypoints
            if (!Scene.current.waypoint_dict.ContainsKey(connection.connection[0]) ||
                !Scene.current.waypoint_dict.ContainsKey(connection.connection[1]))
                continue;

            if (connection.connection[0] == visited[visited.Count - 1])
            {
                if (connection.connection[1] == destination)
                {
                    visited.Add(destination);
                    return visited;
                }
                else if (!visited.Contains(connection.connection[1]))
                {
                    List<string> temp = new List<string>(visited);
                    temp.Add(connection.connection[1]);
                    List<string> result = carvePath(temp, destination);

                    if (result[result.Count - 1] == destination) //we found a path
                    {
                        string s_path = "";
                        foreach (string s in result)
                        {
                            s_path += s + " ";
                        }
                        if (final_result.Count != 0) //we already found a path
                        {
                            if (result.Count < final_result.Count)
                            {
                                final_result = result; //The new path is shorter
                            }
                        }
                        else
                        {

                            final_result = result; //we hadn't found a path, now we have
                        }
                    }
                }
            }
            else if (connection.connection[1] == visited[visited.Count - 1])
            {
                if (connection.connection[0] == destination)
                {
                    visited.Add(destination);
                    return visited;
                }
                else if (!visited.Contains(connection.connection[0]))
                {
                    List<string> temp = new List<string>(visited);
                    temp.Add(connection.connection[0]);
                    List<string> result = carvePath(temp, destination);

                    if (result[result.Count - 1] == destination) //we found a path
                    {
                        string s_path = "";
                        foreach (string s in result)
                        {
                            s_path += s + " ";
                        }
                        if (final_result.Count != 0) //we already found a path
                        {
                            if (result.Count < final_result.Count)
                            {
                                final_result = result; //The new path is shorter
                            }
                        }
                        else
                        {
                            final_result = result; //we hadn't found a path, now we have
                        }
                    }
                }
            }
        }
        if (final_result.Count == 0)
        {
            return visited;
        }
        return final_result;
    }

    public class WayPoint
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    public static void moveCharacter(string[] action_params)
    {
        if (!Actor.actor_controllers.ContainsKey(action_params[0]) || Actor.actor_controllers[action_params[0]].gameObject == null)
        {
            Debug.LogError("Couldn't find character " + action_params[0] + " in characters.");
            return;
        }

        ActorController actor_controller = Actor.actor_controllers[action_params[0]];

        if (!Scene.current.waypoint_dict.ContainsKey(action_params[1]))
        {
            Debug.LogWarning("COULDN'T FIND WAYPOINT " + action_params[1] + " IN CURRENT SCENE! for character move");
            return;
        }

        List<string> visited = new List<string>();
        string current_waypoint = actor_controller.getDestinationWaypoint();
        if (current_waypoint != null)
        {
            visited.Add(current_waypoint);
            if (current_waypoint == action_params[1]) //We are already at the destination
                return;
        }

        List<string> path = carvePath(visited, action_params[1]);

        string s_path = "";
        foreach (string s in path)
        {
            s_path += s + " ";
        }

        Debug.Log(action_params[0] + " Walking: " + s_path + ". Starting at " + actor_controller.getDestinationWaypoint() + " going to " + action_params[1]);

        if (path.Count != 0)
        {
            if (path[path.Count - 1] != action_params[1])//Did not find a path
            {
                path.Clear();
                path.Add(action_params[1]); //Change to a direct route
            }
        }
        else
        {
            path.Clear();
            path.Add(action_params[1]); //Change to a direct route
        }

        if (action_params.Length < 3)
        {
            actor_controller.moveCharacter(path, 0.0f);
            return;
        }

        if (action_params[2] != "")
        {
            List<string> running_anims =new List<string> { "walk_wheelchairStudent", "c_Stu_Jog01", "flyingOnBroom", "c_Stu_Run01", "flyingOnBroom" };
            //There is an animation included
            if (running_anims.Contains(action_params[2])) 
                actor_controller.moveCharacter(path, 1.3f);
            else
                actor_controller.moveCharacter(path, 0.0f);
            actor_controller.replaceCharacterWalk(action_params[2]);
        }
        else
        {
            actor_controller.moveCharacter(path, 0.0f);
        }

        //action_params[3] int unknown
    }
    public class Looking
    {
        public ActorController character;
        public float progress;
        public float destination_progress;
        public Vector3 looking_position;
    }
    public static void lookAt(string[] action_params) //Up to 4 parameters. Idk what 4 is for.
    {
        //Debug.Log("lookAt " + string.Join(",", action_params));
        if (action_params.Length < 2)
        {
            if (Actor.actor_controllers.ContainsKey(action_params[0]))
            {
                Actor.actor_controllers[action_params[0]].actor_head.clearLookat();
            }
            return;
        }

        if (!Actor.actor_controllers.ContainsKey(action_params[0]))
        {
            Debug.LogError("Lookat could not find actor" + action_params[0]);
            return;
        }


        if (!Actor.actor_controllers.ContainsKey(action_params[1]))
        {
            Debug.LogError("Lookat could not find actor" + action_params[1]);
            Actor.actor_controllers[action_params[0]].actor_head.clearLookat();
            return;
        }

        Looking new_looking = new Looking();
        new_looking.character = Actor.actor_controllers[action_params[1]];

        new_looking.progress = 0.0f;

        Actor.actor_controllers[action_params[0]].actor_head.setLookat(new_looking);

        
        if (action_params.Length > 2)
        {
            float.TryParse(action_params[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float action_params_2_float);

            //QuidditchS1C10P3_hoochSlowLookOrion
            //Action param 2 refers how long to lerp to looking at character.
            //TODO
        }
    }

    public static void turnHeadAt(string[] action_params)
    {
        if (action_params.Length < 2)
        {
            if (Actor.actor_controllers.ContainsKey(action_params[0]))
            {
                Actor.actor_controllers[action_params[0]].actor_head.clearTurnHeadAt();
            }
            return;
        }

        if (Actor.actor_controllers.ContainsKey(action_params[0]))
        {
            if (Actor.actor_controllers.ContainsKey(action_params[1]))
            {

                Looking new_looking = new Looking();
                new_looking.character = Actor.actor_controllers[action_params[1]];

                new_looking.progress = 0.0f;
                Actor.actor_controllers[action_params[0]].actor_head.setTurnHeadAt(new_looking);
            }
            else
            {
                Actor.actor_controllers[action_params[0]].actor_head.clearTurnHeadAt();
            }
        }
    }
}
