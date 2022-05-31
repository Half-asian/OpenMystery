using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPlayer : MonoBehaviour
{

    public List<string> event_stack = new List<string>();
    public List<string[]> anim_sequences_to_add = new List<string[]>();
    public ConfigScene._Scene.Camera last_camera;

    public string last_finished_animation = "";
    public float block_duration = 0.0f;
    public string blocking_message;
    public string blocking_key;
    public bool total_block = false;

    [SerializeField] private bool is_sequential_player;

    public float activateEvent(string event_name)
    {

        if (Configs.config_script_events.ScriptEvents[event_name].shouldRun != null)
        {
            if (Configs.config_script_events.ScriptEvents[event_name].shouldRun != "")
            {
                if (Predicate.parsePredicate(Configs.config_script_events.ScriptEvents[event_name].shouldRun) == false)
                {
                    //Debug.Log("predicate fail " + event_config.ScriptEvents[event_name].shouldRun);
                    return 0.0f;
                }
                else
                {
                    //Debug.Log("predicate pass " + event_config.ScriptEvents[event_name].shouldRun);

                }
            }
        }

        Debug.Log("Event " + event_name);
        Log.write("Event " + event_name);

        float event_time = 0.0f;
        if (!Configs.config_script_events.ScriptEvents.ContainsKey(event_name))
        {
            Debug.Log("Couldn't find event " + event_name);
            return 0.0f;
        }

        if (Configs.config_script_events.ScriptEvents[event_name].action != null)
        {

            for (int event_index = 0; event_index < Configs.config_script_events.ScriptEvents[event_name].action.Length; event_index++)
            {

                string[] action_params;
                if (Configs.config_script_events.ScriptEvents[event_name].param != null)
                    if (event_index < Configs.config_script_events.ScriptEvents[event_name].param.Length)
                        action_params = Configs.config_script_events.ScriptEvents[event_name].param[event_index].Split(':');
                    else
                        continue;
                else
                    action_params = new string[0];

                event_time += Events.doEventAction(event_name, event_index, action_params, this);
            }
        }

        if (Configs.config_script_events.ScriptEvents[event_name].type == "Blocking" && Configs.config_script_events.ScriptEvents[event_name].duration == 0.0f && Configs.config_script_events.ScriptEvents[event_name].messageAndKeys != null) //There are some blocking events with no way to exit and no time. Just leftover junk maybe?
        {
            total_block = true;
        }

        if (Configs.config_script_events.ScriptEvents[event_name].type == "Blocking")
        {
            event_time = Configs.config_script_events.ScriptEvents[event_name].duration;

            if (Configs.config_script_events.ScriptEvents[event_name].messageAndKeys != null)
            {
                blocking_message = Configs.config_script_events.ScriptEvents[event_name].messageAndKeys[0][0];
                if (Configs.config_script_events.ScriptEvents[event_name].messageAndKeys[0].Length >= 2)
                {
                    blocking_key = Configs.config_script_events.ScriptEvents[event_name].messageAndKeys[0][1];
                }
                else
                {
                    blocking_key = blocking_message;
                }
            }

        }
        else
        {
            if (Configs.config_script_events.ScriptEvents[event_name].duration != 0.0f)
            {
                //Debug.Log("Non-blocking event with non 0 duration " + event_name);
            }
        }


        if (Configs.config_script_events.ScriptEvents[event_name].type == "Sequential")
        {
            Debug.Log("Starting sequential");
            //StartCoroutine(waitSequentialEvents(Configs.config_script_events.ScriptEvents[event_name].sequenceIds, Configs.config_script_events.ScriptEvents[event_name].messageAndKeys)); //We need to complete message and keys before activating sequences
            if (Configs.config_script_events.ScriptEvents[event_name].sequenceIds != null)
            {
                GameStart.event_manager.sequential_event_player.blocking_key = "";
                GameStart.event_manager.sequential_event_player.total_block = false;
                GameStart.event_manager.sequential_event_player.event_stack = new List<string>();
                GameStart.event_manager.sequential_event_player.block_duration = 0.0f;
                GameStart.event_manager.sequential_event_player.blocking_message = "";
                GameStart.event_manager.sequential_event_player.event_stack.AddRange(Configs.config_script_events.ScriptEvents[event_name].sequenceIds);
            }
        }
        return event_time;
    }

    public void runImmediateEvents()
    {
        block_duration -= Time.deltaTime;

        if (block_duration <= 0.0f)
        {
            if (!is_sequential_player) GameStart.dialogue_manager.waiting_for_dialogue = true;
            block_duration = 0.0f;
        }

        if (block_duration > 0.0f && !is_sequential_player)
        {
            if (!GameStart.dialogue_manager.in_bubble)
            {
                GameStart.dialogue_manager.waiting_for_dialogue = false;
                GameStart.dialogue_manager.ui_dialogue.SetActive(false);
            }
        }

        if (block_duration > 1000 && !is_sequential_player) //Cast a spell
        {
            block_duration = 1f;
        }

        while (event_stack.Count != 0 && block_duration == 0.0f && total_block == false)
        {
            block_duration = activateEvent(event_stack[0]);
            event_stack.RemoveAt(0);

        }

        if (blocking_key == "ScreenFadeComplete")
        {
            blocking_message = "";
            blocking_key = "";
            block_duration = 0.85f;
            total_block = false;
        }
        if (blocking_message == "ScreenFadeComplete")
        {
            blocking_message = "";
            blocking_key = "";
            block_duration = 0.85f;
            total_block = false;
        }

        if (blocking_message == "CamAnimFinished") //Sometimes, a blocking key is called after the animation has finished playing. May be related to very precise timing.
        {
            if (blocking_key == last_finished_animation)
            {
                total_block = false;
                blocking_message = "";
                blocking_key = "";
                block_duration = 0.00f;
            }
        }

        for (int i = anim_sequences_to_add.Count - 1; i >= 0; i--)
        {
            if (Actor.actor_controllers.ContainsKey(anim_sequences_to_add[i][0]))
            {
                if (Actor.actor_controllers[anim_sequences_to_add[i][0]].actor_state == ActorState.Idle)
                {
                    Debug.Log("late anim sequence set for " + anim_sequences_to_add[i][0] + " " + anim_sequences_to_add[i][1]);
                    if (Actor.actor_controllers[anim_sequences_to_add[i][0]].gameObject.GetComponent<ActorAnimSequence>() == null)
                    {
                        Actor.actor_controllers[anim_sequences_to_add[i][0]].gameObject.AddComponent<ActorAnimSequence>();
                    }
                    Actor.actor_controllers[anim_sequences_to_add[i][0]].gameObject.GetComponent<ActorAnimSequence>().initAnimSequence(anim_sequences_to_add[i][1], false);
                    anim_sequences_to_add.RemoveAt(i);
                }
            }
            else
            {
                anim_sequences_to_add.RemoveAt(i);
            }

        }
    }

    public void reset()
    {
        event_stack.Clear();
        total_block = false;
        block_duration = 0.0f;
        blocking_message = "";
        blocking_key = "";
    }

    public IEnumerator waitPropAnimation(float start_time, float length, string prop, string animation)
    {
        while (Time.realtimeSinceStartup < length + start_time)
        {
            yield return null;
        }
        notifyPropAnimationComplete(prop, animation);
    }

    public IEnumerator waitCharacterAnimation(float start_time, float length, string character, string animation)
    {
        while (Time.realtimeSinceStartup < length + start_time)
        {
            yield return null;
        }
        notifyCharacterAnimationComplete(character, animation);
    }

    public IEnumerator waitSafeAdvanceAnimSequenceToCoroutine(string destination) //This is a stub
    {
        yield return null;
        notifySequenceNodeExited(destination);
    }

    public void notifyMoveComplete(string character)
    {
        if (blocking_message == "CharMovementEnded" && blocking_key == character)
        {
            removeBlock();
        }
    }

    public void notifyCamAnimFinished(string animation)
    {
        if (blocking_message == "CamAnimFinished" && blocking_key == animation)
        {
            removeBlock();
        }
    }
    public void notifyCharacterAnimationComplete(string character, string animation)
    {
        if (blocking_message == "CharAnimEnded" && blocking_key == character + ":" + animation)
        {
            removeBlock();
        }
    }

    public void notifyPropAnimationComplete(string prop, string animation)
    {
        if (blocking_message == "PropAnimEnded" && blocking_key == prop + ":" + animation)
        {
            removeBlock();
        }
    }

    public void notifySequenceNodeExited(string node_name)
    {
        if (blocking_message == "SequenceNodeExited" && blocking_key == node_name)
        {
            removeBlock();
        }
    }

    private void removeBlock()
    {
        total_block = false;
        blocking_message = "";
        blocking_key = "";
        block_duration = 0.00f;
        runImmediateEvents();
    }
}
