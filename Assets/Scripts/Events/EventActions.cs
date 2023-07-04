using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Linq;

public static class EventActions
{
    //These are most likely mistakes and shouldn't trigger unimplemented action errors
    public static readonly string[] blacklisted_actions = new string[]
    {
        ".", "Avatar", "Blocking",
        "LookAt", "MoveCharacter", "QuidditchS1C10P2_skye", "Regular", "ScriptEvents", "TLSQS1HouseCupP10_rath", "The7thMan", "TurnHeadAt", "Y5C10P2Bill",
        "Y5C10P2Jae", "Y5C10P2Merula", "animateprop", "bill", "brennan", "cam_CY_D2wallStairs", "cam_intro", "charlie", "despawn", "despawnCharacfter", 
        "f", "firstyear", "hideProp", "looAt", "lookaT", "lootAt", "moveCharater", "penny", "replaceCharacter", "replaceCharacterAnimSequence",
        "replaceCharacterAnimationSeqeucne", "replaceCharacterIdleSequenceSequence", "replacecharacterIdle", "replaceCharacterIdleStaggeredSequence",
        "replaceCharacterIdleStaggeredStaggered", "showProp", "sixthyear",
        "student18", "student3", "teleCharacter", "teleportChaacter", "teleportCharacterC", "turnHead"
    };

    public static readonly string[] implemented_actions = new string[]
    {
        "replaceCharacterIdle", "setCharacterIdle", "replaceCharacterIdleSequence", "playCharacterAnimSequence", "animateCharacter",  "replaceCharacterIdleStaggered",
         "walkInCharacter", "moveCharacter", "turnHeadAt", "lookAt", "teleportCharacter", "teleportProp", "spawnCharacter", "despawnCharacter", "spawnProp",
         "removeProp", "despawnProp", "attachProp", "detachProp", "hideEntity", "showEntity", "playCameraAnimation", "playCinematicCameraAnimation",
         "focusCamera", "panCamOnTrack", "hideCharacter", "showCharacter", "screenFadeTo", "fadeToBlack", "screenFadeFrom", "fadeFromBlack", "safeAdvanceAnimSequenceTo",
         "advanceAnimSequence", "moveCharacterWithSequence", "animateProp", "playPropAnimSequence", "replaceScenarioBGMusic", "equipAvatarComponent", "wearClothingType",
         "setQuidditchHelmetEquipped", "setForcedQuidditchPosition", "setOpponentHouse", "playSound", "awardReward", "setContentVar", "popupVC", "stopSequentialScriptById", 
        "turnHeadTowards", "moveCamOnTrackCharacter", "resetCharacterIdle", "turnTowards", "moveCamOnTrack", "stopCameraAnimation", "forceScenarioSwitch", "playAttachedPropAnim"
    };

    public static void doEventAction(string event_id, string action_type, string[] action_params, EventPlayer event_player)
    {
        if (action_type.Contains(':') || blacklisted_actions.Contains(action_type)) //This happens when the params are accidentally put as the action. Ignore these
        {
            return;
        }

        /*if (Application.isEditor && !Configs.reference_tree.script_events.Contains(event_id))
        {
            //Debug.LogError("Reference tree did not contain " +  event_id);
            //throw new System.Exception  ("Reference tree did not contain " + event_id);
        }*/

        switch (action_type)
        {
            case "replaceCharacterIdle":
                Actor.getActor(action_params[0])?.replaceCharacterIdle(event_id, action_params[1]);
                break;

            case "setCharacterIdle":
                Actor.getActor(action_params[0])?.setCharacterIdle(event_id);
                break;

            case "resetCharacterIdle":
                Actor.getActor(action_params[0])?.resetCharacterIdle(event_id);
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

            case "lookAt":
                Actor.getActor(action_params[0])?.queueLookAt(action_params);
                break;
            case "turnHeadAt": //Don't move shoulders
                Actor.getActor(action_params[0])?.queueTurnHeadAt(action_params);
                break;
            case "turnHeadTowards":
                Actor.getActor(action_params[0])?.queueTurnHeadTowards(action_params);
                break;
            case "turnTowards":
                Actor.getActor(action_params[0])?.queueTurnTowards(action_params);
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

            case "removeProp": //Probably wrong
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

            case "playAttachedPropAnim":

                var actor = Actor.getActor(action_params[0]);
                if (actor == null)
                    return;
                actor.playPropAnim(action_params[1], action_params[2]);
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
            case "stopCameraAnimation":
                CameraManager.current.stopCameraAnimation(action_params[0]);
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
            case "moveCamOnTrack":
                CameraManager.current.moveCamOnTrack(action_params[0]);
                break;
            case "moveCamOnTrackCharacter":
                CameraManager.current.moveCamOnTrackCharacter(action_params[0]);
                break;
            case "forceScenarioSwitch":
                Scenario.Activate(action_params[0]);
                Scenario.Load(action_params[0]);
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
}
