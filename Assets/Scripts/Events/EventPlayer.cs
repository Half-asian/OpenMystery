using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static EventPlayer.ActiveMessageKeys;

public class EventPlayer : MonoBehaviour
{
    public string id = null;

    [Serializable]
    public class ActiveMessageKeys
    {
        [Serializable]
        public class MessageKey {
            public MessageKey(string _message, List<string> _keys)
            {
                message = _message;
                keys = _keys;
            }
            public string message;
            public List<string> keys;
        }
        public List<MessageKey> message_key_set = new List<MessageKey>();

        public void add(List<string> strings)
        {
            string message = strings[0];
            List<string> keys = strings.GetRange(1, strings.Count - 1);
            
            message_key_set.Add(new MessageKey(message, keys));
        }

        public int Count => message_key_set.Count;

        public void clear()
        {
            message_key_set.Clear();
        }

        //Returns true if we removed a block
        public bool checkRemoveMessageKey(string message, string key)
        {
            if (key == null)
                return checkRemoveMessageKey(message);
                
            MessageKey found_message_key = null;
            foreach (var message_key in message_key_set)
            {
                if (message_key.message == message && message_key.keys.Contains(key))
                {
                    found_message_key = message_key;
                    break;
                }
            }
            if (found_message_key != null)
            {
                print();
                //message_key_set.Remove(found_message_key);
                message_key_set.Clear(); //We actually only need to remove one message key to remove the block.
                print();
                return true;
            }
            return false;
        }
        private bool checkRemoveMessageKey(string message)
        {
            MessageKey found_message_key = null;
            foreach (var message_key in message_key_set)
            {
                if (message_key.message == message)
                {
                    found_message_key = message_key;
                    break;
                }
            }
            if (found_message_key != null)
            {
                print();
                //message_key_set.Remove(found_message_key);
                message_key_set.Clear(); //We actually only need to remove one message key to remove the block.
                print();
                return true;
            }
            return false;
        }

        public void print()
        {
            if (Count == 0)
            {
                return;
            }
            int count = 0;
            foreach(var messagekey in message_key_set)
            {
                string line = "Index: " + count + " " + messagekey.message + " ";
                if (messagekey.keys != null)
                {
                    foreach(var key in messagekey.keys)
                    {
                        line += key;
                    }
                }
                count++;
                Debug.Log(line);
            }
        }

        public void update()
        {
            List<string> actorsAwaitingMovement = new List<string>();
            bool waiting_cam_anim = false;
            foreach (var message in message_key_set)
            {
                if (message.message == "CharMovementEnded")
                {
                    actorsAwaitingMovement.AddRange(message.keys);
                }
                if (message.message == "CamAnimFinished")
                    waiting_cam_anim = true;
            }
            foreach(var actor_instance_id in actorsAwaitingMovement)
            {
                if (Actor.getActor(actor_instance_id) == null || Actor.getActor(actor_instance_id).actor_state != ActorState.Walk)
                {
                    GameStart.event_manager.notifyMoveComplete(actor_instance_id);
                }
            }

            if (waiting_cam_anim)
                GameStart.event_manager.notifyCamAnimFinished(GameStart.event_manager.last_finished_animation);


        }

    }

    [SerializeField]
    private List<(ActiveMessageKeys, string, string[], float)> awaiting_sequential_players = new List<(ActiveMessageKeys, string, string[], float)>();
    [SerializeField]
    ActiveMessageKeys blocking_message_keys = new ActiveMessageKeys();

    public List<string> event_stack = new List<string>();
    public List<string[]> anim_sequences_to_add = new List<string[]>();
    public ConfigScene._Scene.Camera last_camera;

    public string last_finished_animation = "";
    public float block_duration = 0.0f;
    public bool total_block = false;


    public bool is_sequential_player;



    public void addEvents(IEnumerable<string> events_ids)
    {
        foreach (string event_id in events_ids)
            Debug.Log("Adding event to stack " + event_id);
        event_stack.AddRange(events_ids);
    }


    public float activateEvent(string event_name)
    {
        if (Configs.config_script_events.ScriptEvents.ContainsKey(VariantManager.getVariantForId(event_name)))
            event_name = VariantManager.getVariantForId(event_name);

        if (!Configs.config_script_events.ScriptEvents.ContainsKey(event_name))
        {
            Debug.Log("Couldn't find event " + event_name);
            return 0.0f;
        }

        Debug.Log("Activating Event " + event_name + " is sequential player: " + is_sequential_player + " Frame: " + Time.frameCount);

        var script_event = Configs.config_script_events.ScriptEvents[event_name];
        float event_time = 0.0f;

        if (string.IsNullOrEmpty(script_event.shouldRun) || Predicate.parsePredicate(script_event.shouldRun) == true)
        {
            if (script_event.action != null)
            {
                for (int event_index = 0; event_index < script_event.action.Length; event_index++)
                {

                    string[] action_params;
                    if (script_event.param != null)
                        if (event_index < script_event.param.Length)
                            action_params = script_event.param[event_index].Split(':');
                        else
                            continue;
                    else
                        action_params = new string[0];

                    EventActions.doEventAction(event_name, event_index, action_params, this);
                }
            }
            event_time = script_event.Duration;
        }

        //if (Configs.config_script_events.ScriptEvents[event_name].type == "Blocking" && Configs.config_script_events.ScriptEvents[event_name].Duration == 0.0f && Configs.config_script_events.ScriptEvents[event_name].messageAndKeys != null) //There are some blocking events with no way to exit and no time. Just leftover junk maybe?
        //{
        //    total_block = true;
        //}

        //Some interactions are erroneously marked as sequential
        //Probably function like blocking instead

        if (script_event.type == "Sequential" && script_event.sequenceIds == null)
        {
            script_event.type = "Blocking";
        }

        if (script_event.type == "Blocking")
        {
            if (script_event.messageAndKeys != null)
            {
                foreach (var message_key_sets in script_event.messageAndKeys)
                {
                    blocking_message_keys.add(new List<string>(message_key_sets));
                }
                blocking_message_keys.print();
            }
        }
        else
        {
            event_time = 0.0f;
        }

        //Year 6 Chapter 12 Part 1 is the ultimate challenge of whether sequential players and looping events are implemented correctly.

        if (script_event.type == "Sequential" && is_sequential_player == false) //Sequential players cannot start more sequential players 
        {
            Debug.Log("Starting sequential from event " + script_event.eventId);
            if (script_event.messageAndKeys != null || script_event.duration != null)
            {
                ActiveMessageKeys sequential_queue_message_keys = null;
                if (script_event.messageAndKeys != null && script_event.messageAndKeys.Length != 0)
                {
                    sequential_queue_message_keys = new ActiveMessageKeys();
                    foreach (var message_key_sets in script_event.messageAndKeys)
                    {
                        sequential_queue_message_keys.add(new List<string>(message_key_sets));
                    }
                }
                awaiting_sequential_players.Add((sequential_queue_message_keys, script_event.eventId, script_event.sequenceIds, script_event.duration != null ? Time.realtimeSinceStartup + script_event.Duration : float.MaxValue));
            }
            else
                GameStart.event_manager.startSequentialPlayer(script_event.eventId, script_event.sequenceIds);
            event_time = 0.0f;//Sequential players continue immediately. Duration/blocking keys are only for starting the sequences.
        }

        return event_time;
    }

    public void runImmediateEvents()
    {
        blocking_message_keys.update();

        List<(ActiveMessageKeys, string, string[], float)> to_remove = new List<(ActiveMessageKeys, string, string[], float)>();
        var copy = awaiting_sequential_players.ToArray();
        foreach(var seq in copy)
        {
            if (seq.Item1 != null)
                seq.Item1.update();

            if (Time.realtimeSinceStartup > seq.Item4)
            {
                GameStart.event_manager.startSequentialPlayer(seq.Item2, seq.Item3);
                to_remove.Add(seq);
            }
        }
        foreach (var remove in to_remove)
        {
            awaiting_sequential_players.Remove(remove);
        }


        block_duration -= Time.deltaTime;

        if (block_duration <= 0.0f)
        {
            if (total_block == false)
                blocking_message_keys.clear();
            block_duration = 0.0f;
        }

        if (event_stack.Count == 0 && is_sequential_player)
        {
            Destroy(this);
            return;
        }

        while (event_stack.Count != 0 && block_duration == 0.0f && total_block == false)
        {
            string event_id = event_stack[0];
            event_stack.RemoveAt(0);
            block_duration = activateEvent(event_id);

        }



        for (int i = anim_sequences_to_add.Count - 1; i >= 0; i--)
        {
            var actor = Actor.getActor(anim_sequences_to_add[i][0]);
            if (actor != null)
            {
                if (actor.actor_state == ActorState.Idle)
                {
                    Debug.Log("late anim sequence set for " + anim_sequences_to_add[i][0] + " " + anim_sequences_to_add[i][1]);
                    if (actor.gameObject.GetComponent<ActorAnimSequence>() == null)
                    {
                        actor.gameObject.AddComponent<ActorAnimSequence>();
                    }
                    actor.gameObject.GetComponent<ActorAnimSequence>().initAnimSequence(anim_sequences_to_add[i][1], false);
                    anim_sequences_to_add.RemoveAt(i);
                }
            }
            else
            {
                anim_sequences_to_add.RemoveAt(i);
            }

        }
        if (GameStart.event_manager != null) {
            GameStart.event_manager.checkEventsActive();
        }
        else {
            GameStart.event_manager = GetComponent<EventManager>();
        }

    }

    public void reset()
    {
        event_stack.Clear();
        total_block = false;
        block_duration = 0.0f;
        blocking_message_keys = new ActiveMessageKeys();
        awaiting_sequential_players = new List<(ActiveMessageKeys, string, string[], float)>();
    }

    public void addCustomBlocking(List<string> strings)
    {
        blocking_message_keys.add(strings);
    }

    public void removeCustomBlockings()
    {
        blocking_message_keys.clear();
        total_block = false;
    }

    public IEnumerator waitSafeAdvanceAnimSequenceToCoroutine(string destination) //This is a stub
    {
        yield return null;
        notifySequenceNodeExited(destination);
    }


    private void processSequentialBlocks(string message, string key)
    {
        List<(ActiveMessageKeys, string, string[], float)> to_remove = new List<(ActiveMessageKeys, string, string[], float)>();
        foreach (var awaiting in awaiting_sequential_players)
        {
            if (awaiting.Item1 != null)
                awaiting.Item1.checkRemoveMessageKey(message, key);
            
            if ((awaiting.Item1 != null && awaiting.Item1.Count == 0))
            {
                GameStart.event_manager.startSequentialPlayer(awaiting.Item2, awaiting.Item3);
                to_remove.Add(awaiting);
            }
        }
        foreach (var remove in to_remove)
        {
            awaiting_sequential_players.Remove(remove);
        }
    }

    private void processNotifyBlocks(string message, string key)
    {
        processSequentialBlocks(message, key);
        
        if (blocking_message_keys.checkRemoveMessageKey(message, key) && blocking_message_keys.Count == 0)
            removeBlock();
    }

    public void notifyCharacterAnimationComplete(string character, string animation) => processNotifyBlocks("CharAnimEnded", character + ":" + animation);
    public void notifyMoveComplete(string character) => processNotifyBlocks("CharMovementEnded", character);
    public void notifyCamAnimFinished(string animation) => processNotifyBlocks("CamAnimFinished", animation);
    public void notifyPropAnimationComplete(string prop, string animation) => processNotifyBlocks("PropAnimEnded", prop + ":" + animation);
    public void notifySequenceNodeExited(string node_name) => processNotifyBlocks("SequenceNodeExited", node_name);
    public void notifyScriptTrigger(string trigger) => processNotifyBlocks("AnimationScriptTriggerHit", trigger);
    public void notifyScreenFadeComplete() => processNotifyBlocks("ScreenFadeComplete", null);

    public void notifyGestureRecognitionComplete() => processNotifyBlocks("modalClosed", "GestureCheckVC");
    private void removeBlock()
    {
        total_block = false;
        block_duration = 0.00f;
        runImmediateEvents();
    }
}
