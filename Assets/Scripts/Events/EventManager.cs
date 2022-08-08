using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;

public class EventManager : MonoBehaviour
{

    public static event Action all_script_events_finished_event = delegate {};

    //public List<string> event_stack = new List<string>();
    //public float block_duration = 0.0f;

    public EventPlayer main_event_player;
    public EventPlayer sequential_event_player;

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
        sequential_event_player.notifyCharacterAnimationComplete(character, animation);
    }

    public void notifyCamAnimFinished(string animation)
    {
        main_event_player.notifyCamAnimFinished(animation);
        sequential_event_player.notifyCamAnimFinished(animation);
    }

    public void notifyMoveComplete(string character)
    {
        main_event_player.notifyMoveComplete(character);
        sequential_event_player.notifyMoveComplete(character);
    }

    public void notifyScriptTrigger(string trigger)
    {
        main_event_player.notifyScriptTrigger(trigger);
        sequential_event_player.notifyScriptTrigger(trigger);
    }

    public void notifyPropAnimationComplete(string prop, string animation)
    {
        main_event_player.notifyPropAnimationComplete(prop, animation);
        sequential_event_player.notifyPropAnimationComplete(prop, animation);
    }

    public IEnumerator waitCameraAnimation(float start_time, float length, string animation)
    {
        main_event_player.last_finished_animation = "";
        sequential_event_player.last_finished_animation = "";

        while (Time.realtimeSinceStartup < length + start_time)
        {
            yield return null;
        }
        CameraManager.current.freeCamera();
        GameStart.event_manager.notifyCamAnimFinished(animation);

        CameraManager.current.simple_camera_controller.enabled = true;
        main_event_player.last_finished_animation = animation;
        sequential_event_player.last_finished_animation = animation;
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

    public List<string> carvePath(List<string> visited, string destination)
    {
        List<string> final_result = new List<string>();

        if (Scene.current.waypointconnections == null)
        {
            return final_result;
        }

        foreach(ConfigScene._Scene.WayPointConnection connection in Scene.current.waypointconnections)
        {
            if (connection.connection[0] == visited[visited.Count - 1]){
                if (connection.connection[1] == destination)
                {
                    visited.Add( destination);
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
        sequential_event_player.runImmediateEvents();
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

        sequential_event_player.reset();

    }

}