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
        "turnHeadTowards", "moveCamOnTrackCharacter", "resetCharacterIdle", "turnTowards", "moveCamOnTrack", "stopCameraAnimation", "forceScenarioSwitch", "playAttachedPropAnim",
        "hideFlatStuff", "showFlatStuff", "doGestureRecognition", "addLookupTag", "stopAnimatingProp"
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
            //Actor actions

            //3 params, possibly 4
            //3 implemented
            case "replaceCharacterIdle":
                Actor.eventReplaceCharacterIdle(event_id,action_params);
                break;

            //3 params
            //3 implemented
            case "setCharacterIdle":
                Actor.eventSetCharacterIdle(event_id, action_params);
                break;

            //2 params
            //1 implemented
            case "resetCharacterIdle":
                Actor.getActor(action_params[0])?.resetCharacterIdle(event_id);
                break;
            
            //3 params
            //2 implemented
            //todo param[2] is anim sequence node to fast forward to
            case "replaceCharacterIdleSequence":
                Actor.getActor(action_params[0])?.replaceCharacterIdleSequence(event_id, action_params[1]);
                break;

            //3 params
            //2 implemented
            case "playCharacterAnimSequence":
                Actor.getActor(action_params[0])?.playCharacterAnimSequence(event_id, action_params[1]);
                break;

            //3 params
            //3 implemented
            case "animateCharacter":
                Actor.getActor(action_params[0])?.animateCharacter(event_id, action_params[1],
                    action_params.Length > 2 ? int.Parse(action_params[2]) : 0
                    );
                break;

            //3 params
            //3 implemented
            case "replaceCharacterIdleStaggered":
                Actor.eventReplaceCharacterIdleStaggered(event_id, action_params);
                break;

            //2 params
            //3 implemented
            case "walkInCharacter": //Tp's character to closest connecting waypoint, then they walk to the destination
                Actor.getActor(action_params[0])?.walkInCharacter(action_params[1]);
                break;

            //5 params
            //3 implemented
            case "moveCharacter":
                Actor.getActor(action_params[0])?.moveCharacter(
                    action_params.Length > 1 ? action_params[1] : null, 
                    action_params.Length > 2 ? action_params[2] : null, 
                    ActorController.ActorAnim.AnimType.Regular);
                break;

            //4 params
            //4 implemented
            case "lookAt":
                Actor.eventLookAt(event_id, action_params);
                break;

            //4 params
            //4 implemented
            case "turnHeadAt":
                Actor.eventTurnHeadAt(event_id, action_params);
                break;

            //4 params
            //4 implemented
            case "turnHeadTowards":
                Actor.eventTurnHeadTowards(event_id, action_params);
                break;

            //3 to 4 params (4 params only in unused events)
            //4 implemented
            case "turnTowards":
                Actor.eventTurnTowards(event_id, action_params);
                break;

            //4 params
            //3 implemented
            case "teleportCharacter":
                Actor.eventTeleportCharacter(event_id, action_params);
                break;

            //3 params (4th possible)
            //3 implemented
            case "spawnCharacter":
                string hpactor_id = action_params.Length >= 1 ? action_params[0] : null;
                string waypoint_id = action_params.Length >= 2 ? action_params[1] : null;
                string instance_id = action_params.Length >= 3 ? action_params[2] : hpactor_id;

                Actor.spawnActor(hpactor_id, waypoint_id, instance_id);
                break;

            //1 param
            //1 implemented
            case "despawnCharacter":
                Actor.despawnCharacterInScene(action_params[0]);
                break;

            //1 param
            //1 implemented
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

            //1 param
            //1 implemented
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

            //3 params
            //??? implemented
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

            //2 params
            //1 implemented
            // param 2 is target anim sequence node
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

            //4 params
            //3 implemented
            case "moveCharacterWithSequence":
                Actor.getActor(action_params[0])?.moveCharacter(
                    action_params.Length > 1 ? action_params[1] : null,
                    action_params.Length > 2 ? action_params[2] : null,
                    ActorController.ActorAnim.AnimType.Sequence);
                break;

            //PROP ACTIONS

            //2 params (3rd possible)
            //2 implemented
            case "teleportProp":
                if (!Scene.isValidWayPoint(action_params[1]))
                {
                    Debug.LogWarning("COULDN'T FIND WAYPOINT " + action_params[1] + " IN CURRENT SCENE!");
                    break;
                }
                if (Prop.spawned_props.ContainsKey(action_params[0]))
                {
                    Scene.setGameObjectToWaypoint(Prop.spawned_props[action_params[0]].model.game_object, action_params[1]);
                }
                else
                {
                    Debug.LogWarning("COULDN'T FIND Prop " + action_params[0]);
                }
                break;

            //4 params
            //4 implemented

            case "spawnProp":
                //string model_id, string waypoint_id, string name_id, string lookuptag

                if (action_params.Length == 3)
                    Prop.spawnPropFromEvent(action_params[0], action_params[1], action_params[2], "");
                else if (action_params.Length == 4)
                    Prop.spawnPropFromEvent(action_params[0], action_params[1], action_params[2], action_params[3]);
                else
                    Prop.spawnPropFromEvent(action_params[0], action_params[1], action_params[0], "");
                break;

            //2 params
            //2 implemented
            case "removeProp":
            case "despawnProp":
                Prop.eventDespawnProp(event_id, action_params);
                break;

            //4 params
            //4 implemented
            case "attachProp": //used to attach something to a character
                {
                    var actor = Actor.getActor(action_params[0]);
                    if (actor == null)
                        break;

                    if (action_params.Length < 4)
                        actor.attachChildNode(action_params[2], action_params[2], action_params[1]);
                    else
                        actor.attachChildNode(action_params[2], action_params[3], action_params[1]);
                    break;
                }

            //1 param
            //1 implemented
            case "detachProp":
                {
                    var actor = Actor.getActor(action_params[0]);
                    if (actor == null)
                        return;

                    actor.removeChildNode(action_params[1]);
                    break;
                }

            //3 params
            //3 implemented
            case "playAttachedPropAnim":
                {
                    var actor = Actor.getActor(action_params[0]);
                    if (actor == null)
                        return;
                    actor.playPropAnim(action_params[1], action_params[2]);
                    break;
                }

            //1 param
            //1 implemented
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

            //1 param
            //1 implemented
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

            //3 params
            //2 implemented
            case "animateProp":
                Prop.animateProp(action_params[0], action_params[1]);
                break;

            //3 params
            //3 implemented
            case "playPropAnimSequence":
                string prop_id = action_params[0];
                string sequence_id = action_params[1];
                int is_group = 0;
                if (action_params.Length > 2)
                    is_group = int.Parse(action_params[2]);

                Prop.eventPlayPropAnimationSequence(prop_id, sequence_id, is_group);
                break;

            //1 param
            //1 implemented
            case "stopAnimatingProp":
                Prop.stopAnimatingProp(action_params[0]);
                break;

            //CAMERA ACTIONS

            //1 param
            //1 implemented
            case "playCameraAnimation":
                CameraManager.current.playCameraAnimation(action_params[0], true);
                break;

            //1 param
            //1 implemented
            case "stopCameraAnimation":
                if (action_params.Length > 0)
                    CameraManager.current.stopCameraAnimation(action_params[0]);
                else
                    CameraManager.current.stopCameraAnimation(null);
                break;

            //2 params
            //2 implemented
            case "playCinematicCameraAnimation":
                if (action_params.Length > 1)
                {
                    string[] focus_cam_action_params = new string[] { action_params[1], "0" };
                    CameraManager.current.focusCam(ref focus_cam_action_params);
                }

                Debug.Log("Playing cinematic camera anim " + action_params[0]);
                CameraManager.current.playCameraAnimation(action_params[0], true);
                break;

            //3 params
            //2 implemented
            case "focusCamera": //action param 1 is probably time to transition camera (lerp)
                if (event_player) event_player.last_camera = CameraManager.current.focusCam(ref action_params);
                break;

            //2 params
            //2 implemented
            case "panCamOnTrack":
                if (!float.TryParse(action_params[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float target_pos))
                    break;
                if (!float.TryParse(action_params[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float duration))
                    break;

                CameraManager.current.panCamOnTrack(target_pos, duration);
                break;

            //2 params
            //2 implemented
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

            //1 param
            //1 implemented
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

            //1 param
            //1 implemented
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

            //2 param
            //1 implemented
            case "moveCamOnTrack":
                CameraManager.current.moveCamOnTrack(action_params[0]);
                break;

            //2 params
            //1 implemented
            case "moveCamOnTrackCharacter":
                CameraManager.current.moveCamOnTrackCharacter(action_params[0]);
                break;

            //OTHER

            //1 param
            //1 implemented
            case "replaceScenarioBGMusic":
                if (Configs.playlist_dict.ContainsKey(action_params[0]))
                {
                    Sound.playBGMusic(action_params[0]);
                }
                break;

            //1 param
            //1 implemented
            case "equipAvatarComponent":
                Actor.getActor("Avatar")?.avatar_components.equipAvatarComponent(action_params[0]);
                break;
                
            //2 params
            //2 implemented
            case "wearClothingType":
                if (action_params.Length < 2)
                    Scenario.changeClothes(action_params[0], null);
                else
                    Scenario.changeClothes(action_params[0], action_params[1]);
                break;

            //1 param
            //1 implemented
            case "setQuidditchHelmetEquipped":
                Scenario.setQuidditchHelmet(action_params[0]);
                break;

            //1 param
            //1 implemented
            case "setForcedQuidditchPosition":
                Player.local_avatar_quidditch_position = action_params[0];
                break;

            //1 param
            //1 implemented
            case "setOpponentHouse":
                Player.local_avatar_opponent_house = action_params[0];
                break;

            //2 params
            //1 implemented
            case "playSound":
                Sound.playSound(action_params[0]);
                break;

            //1 param
            //1 implemented
            case "awardReward":
                Reward.getReward(action_params[0]);
                break;
            
            //3 params
            //3 implemented
            case "setContentVar":
                Scenario.setContentVar(action_params);
                break;

            //x params
            case "popupVC":

                switch (action_params[0])
                {
                    case "YearEndViewController":
                        GameStart.event_manager.main_event_player.total_block = true;
                        GameStart.event_manager.main_event_player.addCustomBlocking(new List<string> { "graduation", "graduation" });
                        Graduation.Graduate();
                        break;
                    case "YearEndRankingViewController":
                        GameStart.current.StartCoroutine(SimulatePopupModal("yearEndRewardComplete", null));
                        break;
                    case "NameChooserVC":
                        GameStart.current.StartCoroutine(SimulatePopupModal("nameInputComplete", null));
                        break;
                    case "HogwartsLetterVC":
                        GameStart.current.StartCoroutine(SimulatePopupModal("letterClosed", null));
                        break;
                    case "YearStartViewController":
                        GameStart.current.StartCoroutine(SimulatePopupModal("yearStartComplete", null));
                        break;
                    case "PreYearEndViewController":
                        break;
                    case "ExclusityModalVC":
                        GameStart.current.StartCoroutine(SimulatePopupModal("exclusivityModalClosed", null));
                        break;
                    case "BreakUpDecisionVC":
                        GameStart.current.StartCoroutine(SimulatePopupModal("breakUpModalClosed", null));
                        break;
                    case "OwlGradesVC":
                        GameStart.current.StartCoroutine(SimulatePopupModal("owlGradesClosed", null));
                        break;
                    default:
                        throw new System.Exception("Unknown popupVC param " + action_params[0]);
                }
                break;

            //1 param
            //1 implemented
            case "stopSequentialScriptById":
                GameStart.event_manager.stopSequentialScriptById(action_params[0]);
                break;

            //1 param
            //1 implemented
            case "forceScenarioSwitch":
                Scenario.Activate(action_params[0]);
                Scenario.Load(action_params[0]);
                break;


            //1 param
            //1 implemented
            case "hideFlatStuff":
                Dorm.hideFlatStuff();
                break;

            //1 param
            //1 implemented
            case "showFlatStuff":
                Dorm.showFlatStuff();
                break;

            //1 param
            //1 implemented
            case "doGestureRecognition":
                GameStart.current.StartCoroutine(SimulatePopupModal("modalClosed", "GestureCheckVC"));
                break;

            //variable length
            case "addLookupTag":
                string tag = action_params.Last();
                for(int i = 0; i < action_params.Length; i++)
                {
                    if (Prop.spawned_props.ContainsKey(action_params[i]))
                    {
                        Prop.spawned_props[action_params[i]].lookup_tags.Add(tag);
                    }
                    if (Actor.getActor(action_params[i]))
                    {
                        Actor.getActor(action_params[i]).lookup_tags.Add(tag);
                    }
                }
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

    public static IEnumerator SimulatePopupModal(string a, string b)
    {
        yield return new WaitForSeconds(2);
        GameStart.event_manager.notifyGeneric(a, b);
    }

    

}
