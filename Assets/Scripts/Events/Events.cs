using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModelLoading;
using System.Globalization;
using static CocosModel;

public static class Events
{

    public static void doEventAction(string event_id, string action_type, string[] action_params, EventPlayer event_player)
    {
        switch (action_type)
        {
            case "replaceCharacterIdle":
                Actor.getActor(action_params[0])?.replaceCharacterIdle(event_id, action_params[1]);
                break;

            case "setCharacterIdle":
                Actor.getActor(action_params[0])?.setCharacterIdle(event_id);
                break;

            case "replaceCharacterIdleSequence":
                Actor.getActor(action_params[0])?.replaceCharacterIdleSequence(event_id, action_params[1]);
                break;

            case "playCharacterAnimSequence":
                Actor.getActor(action_params[0])?.playCharacterAnimSequence(event_id, action_params[1]);
                break;

            case "animateCharacter":
                Actor.getActor(action_params[0])?.animateCharacter(event_id, action_params[1],
                    action_params.Length > 2 ? int.Parse(action_params[2]) : 0
                    );
                break;

            case "replaceCharacterIdleStaggered":
                Actor.getActor(action_params[0])?.replaceCharacterIdleStaggered(event_id, action_params[1]);
                break;

            case "walkInCharacter": //Tp's character to closest connecting waypoint, then they walk to the destination
                Actor.getActor(action_params[0])?.walkInCharacter(action_params[1]);
                break;

            case "moveCharacter":
                Actor.getActor(action_params[0])?.moveCharacter(
                    action_params.Length > 1 ? action_params[1] : null, 
                    action_params.Length > 2 ? action_params[2] : null, 
                    ActorController.ActorAnim.AnimType.Regular);
                //action_params[3] int unknown

                break;

            case "turnHeadAt": //Don't move shoulders
                turnHeadAt(action_params);
                break;

            case "lookAt":
                lookAt(action_params);
                break;

            case "teleportCharacter":
                if (action_params.Length < 2) break;
                Actor.getActor(action_params[0])?.teleportCharacter(action_params[1]);
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

                string hpactor_id   = action_params.Length >= 1 ? action_params[0] : null;
                string waypoint_id  = action_params.Length >= 2 ? action_params[1] : null;
                string instance_id  = action_params.Length >= 3 ? action_params[2] : hpactor_id;

                Actor.spawnActor(hpactor_id, waypoint_id, instance_id);
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
                        Debug.LogWarning("remove/despawn prop didn't find prop " + action_params[0]);
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
                if (Actor.getActor(action_params[0]) != null)
                {

                    if (action_params[0] == Player.local_avatar_onscreen_name)
                    {
                        AvatarComponents a = Actor.getActor("Avatar").avatar_components;
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
                        foreach (SkinnedMeshRenderer smr in Actor.getActor(action_params[0]).gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            smr.enabled = false;
                        }
                    }
                }
                break;
            case "showCharacter":
                if (Actor.getActor(action_params[0]) != null)
                {

                    if (action_params[0] == Player.local_avatar_onscreen_name)
                    {
                        AvatarComponents a = Actor.getActor("Avatar").avatar_components;
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
                        foreach (SkinnedMeshRenderer smr in Actor.getActor(action_params[0]).gameObject.transform.GetComponentsInChildren<SkinnedMeshRenderer>())
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
                if (Actor.getActor(action_params[0]) != null)
                {
                    Actor.getActor(action_params[0]).gameObject.GetComponent<ActorAnimSequence>().safeAdvanceAnimSequenceTo(action_params[2]); //action_param 1 might be starting node (immediate set node).
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
                if (Actor.getActor(action_params[0]))
                {
                    Actor.getActor(action_params[0]).advanceAnimSequence();
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

            case "moveCharacterWithSequence":
                Actor.getActor(action_params[0])?.moveCharacter(
                    action_params.Length > 1 ? action_params[1] : null,
                    action_params.Length > 2 ? action_params[2] : null, 
                    ActorController.ActorAnim.AnimType.Sequence);
                break;

            case "animateProp":
                Prop.animateProp(action_params[0], action_params[1]);
                break;
            case "playPropAnimSequence":
                Prop.playPropAnimationSequence(action_params[0], action_params[1]);
                break;

            case "replaceScenarioBGMusic":
                if (Configs.playlist_dict.ContainsKey(action_params[0]))
                {
                    Sound.playBGMusic(action_params[0]);
                }
                break;


            case "equipAvatarComponent":
                Actor.getActor("Avatar")?.avatar_components.equipAvatarComponent(action_params[0]);
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
            case "stopSequentialScriptById":
                GameStart.event_manager.stopSequentialScriptById(action_params[0]);
                break;
            default:
                Debug.LogWarning("Unknown event type " + action_type);
                break;
        }
    }

    public static void doEventAction(string event_name, int event_index, string[] action_params, EventPlayer event_player)
    {
        doEventAction(event_name, Configs.config_script_events.ScriptEvents[event_name].action[event_index], action_params, event_player);
    }
    

    public static void lookAt(string[] action_params) //Up to 4 parameters. Idk what 4 is for.
    {
        if (Actor.getActor(action_params[0]) == null)                     //Check Actor exists 
        {
            Debug.LogWarning("Lookat could not find actor: " + action_params[0]);
            return;
        }

        if (action_params.Length < 2)                                                   //Actor clear
        {
            Actor.getActor(action_params[0]).clearLookat();
            return;
        }

        float speed = 3.0f;

        if (action_params.Length > 2)
        {
            float.TryParse(action_params[2], NumberStyles.Any, CultureInfo.InvariantCulture, out speed);

            //QuidditchS1C10P3_hoochSlowLookOrion
        }

        //There is a mystery param 4 as well
        //TLSQS3HouseCupP9_PennyWatchBackgroundLeave is the only case it is not set to 1

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            Actor.getActor(action_params[0]).setLookAt(
                Actor.getActor(action_params[1]), speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            Actor.getActor(action_params[0]).setLookAt(
                Prop.spawned_props[action_params[1]], speed);
            return;
        }
        else                                                                            //Actor look in specific direction
        {
            try
            {
                float x = 0;
                string[] numbers = action_params[1].Split(',');
                float y = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (numbers.Length > 1)
                    x = float.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (x == 0 && y == 0)
                {
                    Actor.getActor(action_params[0]).clearLookat();
                }
                else
                {
                    Actor.getActor(action_params[0]).setLookAt(-x, y, speed);
                }
            }
            catch
            {
                Debug.LogError("Unknown second param for lookat " + action_params[0] + " " + action_params[1]);
            }
        }
    }

    public static void turnHeadAt(string[] action_params)
    {
        if (Actor.getActor(action_params[0]) == null)                     //Check Actor exists 
        {
            Debug.LogWarning("Lookat could not find actor: " + action_params[0]);
            return;
        }

        if (action_params.Length < 2)                                                   //Actor clear
        {
            Actor.getActor(action_params[0])?.clearTurnHeadAt();
            return;
        }

        float speed = 3.0f;

        if (action_params.Length > 2)
        {
            speed = float.Parse(action_params[2], NumberStyles.Any, CultureInfo.InvariantCulture);

            //QuidditchS1C10P3_hoochSlowLookOrion
        }

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            Actor.getActor(action_params[0]).setTurnHeadAt(
                Actor.getActor(action_params[1]), speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            Actor.getActor(action_params[0]).setTurnHeadAt(
                Prop.spawned_props[action_params[1]], speed);
            return;
        }
        else                                                                            //Actor look in specific direction
        {
            try
            {
                float x = 0;
                string[] numbers = action_params[1].Split(',');
                float y = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (numbers.Length > 1)
                    x = int.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (x == 0 && y == 0)
                {
                    Actor.getActor(action_params[0]).clearLookat();
                }
                else
                {
                    Actor.getActor(action_params[0]).setTurnHeadAt(-x, y, speed);
                }
            }
            catch
            {
                Debug.LogError("Unknown second param for turnheadat " + action_params[0] + " " + action_params[1]);
            }
        }
    }
}
