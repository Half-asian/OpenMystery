﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.TextCore.Text;

public class EventManager : MonoBehaviour
{

    public static event Action all_script_events_finished_event = delegate {};

    //public List<string> event_stack = new List<string>();
    //public float block_duration = 0.0f;

    public EventPlayer main_event_player;
    public List<EventPlayer> sequential_event_players;

    public GameObject main_camera;

    //public string blocking_message;
    //public string blocking_key;
    //public string last_finished_animation = "";
    //public bool total_block = false;

    //public List<string[]> anim_sequences_to_add = new List<string[]>();

    //public ConfigScene._Scene.Camera last_camera;

    private void Awake()
    {
        GameStart.onReturnToMenu += reset;
        Scenario.onScenarioCallClear += reset;
    }

    public void notifyCharacterAnimationComplete(string character, string animation)
    {
        main_event_player.notifyCharacterAnimationComplete(character, animation);
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyCharacterAnimationComplete(character, animation);
        }
    }

    public void notifyCamAnimFinished(string animation)
    {
        main_event_player.notifyCamAnimFinished(animation);
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyCamAnimFinished(animation);
        }
    }

    public void notifyMoveComplete(string character)
    {
        main_event_player.notifyMoveComplete(character);
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyMoveComplete(character);
        }
    }

    public void notifyScriptTrigger(string trigger)
    {
        main_event_player.notifyScriptTrigger(trigger);
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyScriptTrigger(trigger);
        }
    }

    public void notifyPropAnimationComplete(string prop, string animation)
    {
        main_event_player.notifyPropAnimationComplete(prop, animation);
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyPropAnimationComplete(prop, animation);
        }
    }

    public void notifyScreenFadeComplete()
    {
        main_event_player.notifyScreenFadeComplete();
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.notifyScreenFadeComplete();
        }
    }

    public IEnumerator waitCameraAnimation(float length, string animation)
    {
        main_event_player.last_finished_animation = "";
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.last_finished_animation = "";
        }

        yield return new WaitForSeconds(length);

        CameraManager.current.freeCamera();
        GameStart.event_manager.notifyCamAnimFinished(animation);

        CameraManager.current.simple_camera_controller.enabled = true;
        main_event_player.last_finished_animation = animation;
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.last_finished_animation = animation;
        }
    }

    public IEnumerator lookAtCountdown(string character, float time)
    {
        float start_time = Time.realtimeSinceStartup;
        while(Time.realtimeSinceStartup < start_time + time)
        {
            yield return null;
        }
        if (Actor.actor_controllers.ContainsKey(character))
            Actor.actor_controllers[character].actor_head.clearLookat();
    }

    public void startSequentialPlayer(string[] events)
    {
        EventPlayer new_event_player = gameObject.AddComponent<EventPlayer>();
        new_event_player.is_sequential_player = true;
        new_event_player.addEvent(events);
        sequential_event_players.Add(new_event_player);
    }

    public void removeSequentialPlayers()
    {
        List<EventPlayer> to_remove = new List<EventPlayer>();
        foreach(var sequential_player in sequential_event_players)
        {
            if (sequential_player is null)
                to_remove.Add(sequential_player);
        }
        foreach(var remove in to_remove)
        {
            sequential_event_players.Remove(remove);
        }
    }

    /*IEnumerator waitSequentialEvents(string[] sequences, string[][] message_and_keys)
    {
        Debug.Log("Sequential events");
        if (message_and_keys != null)
        {
            bool message_keys_complete = false;

            while (message_keys_complete == false)
            {
                for (int i = 0; i < message_and_keys.Length; i++)
                {
                    if (message_and_keys[i][0] == "CharMovementEnded")
                    {
                        if (Actors.characters.ContainsKey(message_and_keys[i][1]))
                        {
                            if (Actors.characters[message_and_keys[i][1]].character_state == CharacterManager.CharacterState.Idle)
                            {
                                message_and_keys[i][0] = "";
                                message_and_keys[i][1] = "";
                            }
                        }
                        else
                        {
                            message_and_keys[i][0] = "";
                            message_and_keys[i][1] = "";
                        }
                    }
                }

                message_keys_complete = true;
                for (int i = 0; i < message_and_keys.Length; i++)
                {
                    if (!(message_and_keys[i][0] == "" && message_and_keys[i][1] == ""))
                    {
                        message_keys_complete = false;
                    }
                }
                yield return null;
            }
        }
        if (sequences != null)
        {
            foreach (string sequence in sequences)
            {

                activateEvent(sequence);
            }
        }
    }*/


    public void Update()
    {
        main_event_player.runImmediateEvents();
        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            sequential_event_player.runImmediateEvents();
        }
        removeSequentialPlayers();
    }

    public void checkEventsActive()
    {
        if (areEventsActive() == false)
            all_script_events_finished_event.Invoke();
    }

    public bool areEventsActive()
    {
        //if (main_event_player.event_stack.Count == 0 && main_event_player.block_duration == 0.0f && main_event_player.total_block == false)
        if (main_event_player.event_stack.Count == 0)
        {
            //if (sequential_event_player.event_stack.Count == 0 && sequential_event_player.block_duration == 0.0f && sequential_event_player.total_block == false)
            return false;

        }
        return true;
    }




    public void reset()
    {
        main_event_player.reset();

        foreach (EventPlayer sequential_event_player in sequential_event_players)
        {
            Destroy(sequential_event_player);
        }
        sequential_event_players = new List<EventPlayer>();

    }

}