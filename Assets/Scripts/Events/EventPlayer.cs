using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class EventPlayer : MonoBehaviour
{
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
                Debug.Log("Going to remove message " + found_message_key.message + " " + found_message_key.keys[0] + " THe count is currently " + Count);
                message_key_set.Remove(found_message_key);
                Debug.Log("Removed message " + found_message_key.message + " " + found_message_key.keys[0] + " THe count is now " + Count);
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
                Debug.Log("Going to remove message " + found_message_key.message + " THe count is currently " + Count);
                message_key_set.Remove(found_message_key);
                Debug.Log("Removed message " + found_message_key.message + " THe count is now " + Count);
                print();
                return true;
            }
            return false;
        }

        public void print()
        {
            if (Count == 0)
            {
                Debug.Log("There are no messagekeys");
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
                Debug.Log(line);
                count++;
            }


        }

    }

    [SerializeField]
    private List<(ActiveMessageKeys, string[])> awaiting_sequential_players = new List<(ActiveMessageKeys, string[])>();
    [SerializeField]
    ActiveMessageKeys blocking_message_keys = new ActiveMessageKeys();

    public List<string> event_stack = new List<string>();
    public List<string[]> anim_sequences_to_add = new List<string[]>();
    public ConfigScene._Scene.Camera last_camera;

    public string last_finished_animation = "";
    public float block_duration = 0.0f;
    public bool total_block = false;


    public bool is_sequential_player;
    int anim_block_miss_count = 0;



    public void addEvents(IEnumerable<string> events_ids)
    {
        foreach (string event_id in events_ids)
            Debug.Log("Adding event to stack " + event_id);
        event_stack.AddRange(events_ids);
    }


    public float activateEvent(string event_name)
    {
        if (!Configs.config_script_events.ScriptEvents.ContainsKey(event_name))
        {
            Debug.Log("Couldn't find event " + event_name);
            return 0.0f;
        }

        Debug.Log("Activating Event " + event_name + " is sequential player: " + is_sequential_player);

        if (!string.IsNullOrEmpty( Configs.config_script_events.ScriptEvents[event_name].shouldRun))
        {
            if (Predicate.parsePredicate(Configs.config_script_events.ScriptEvents[event_name].shouldRun) == false)
            {
                return 0.0f;
            }
        }

        float event_time = 0.0f;
        var script_event = Configs.config_script_events.ScriptEvents[event_name];

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

                Events.doEventAction(event_name, event_index, action_params, this);
            }
        }

        if (Configs.config_script_events.ScriptEvents[event_name].type == "Blocking" && Configs.config_script_events.ScriptEvents[event_name].Duration == 0.0f && Configs.config_script_events.ScriptEvents[event_name].messageAndKeys != null) //There are some blocking events with no way to exit and no time. Just leftover junk maybe?
        {
            total_block = true;
        }



        if (script_event.type == "Blocking")
        {
            event_time = script_event.Duration;

            if (script_event.messageAndKeys != null)
            {
                anim_block_miss_count = 0;
                foreach (var message_key_sets in script_event.messageAndKeys)
                {
                    Debug.Log("Adding block " + message_key_sets[0]);
                    blocking_message_keys.add(new List<string>(message_key_sets));
                }
                blocking_message_keys.print();
            }

        }

        if (script_event.type == "Sequential")
        {
            Debug.Log("Starting sequential");
            /*if (script_event.messageAndKeys != null)
            {
                ActiveMessageKeys sequential_queue_message_keys = new ActiveMessageKeys();
                foreach (var message_key_sets in script_event.messageAndKeys)
                {
                    sequential_queue_message_keys.add(new List<string>(message_key_sets));
                }
                awaiting_sequential_players.Add((sequential_queue_message_keys, script_event.sequenceIds));
            }
            else*/
                GameStart.event_manager.startSequentialPlayer(script_event.sequenceIds);
        }

        return event_time;
    }

    public void runImmediateEvents()
    {
        if (blocking_message_keys.checkRemoveMessageKey("CamAnimFinished", last_finished_animation))
        {//Sometimes, a blocking key is called after the animation has finished playing. May be related to very precise timing.
            removeBlock();
            return;
        }

        block_duration -= Time.deltaTime;

        if (block_duration <= 0.0f)
        {
            if (total_block == false)
                blocking_message_keys.clear();
            block_duration = 0.0f;
        }

        if (block_duration > 1000 && !is_sequential_player) //Cast a spell
        {
            block_duration = 1f;
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
        if (GameStart.event_manager != null) {
            GameStart.event_manager.checkEventsActive();
        }
        else {
            GameStart.event_manager = GetComponent<EventManager>();
        }
    }

    public void reset()
    {
        if (is_sequential_player)
        {
            Debug.LogError("Reset sequential");
        }
        else
        {
            Debug.LogError("Reset event non");
        }
        event_stack.Clear();
        total_block = false;
        block_duration = 0.0f;
        blocking_message_keys = new ActiveMessageKeys();
        awaiting_sequential_players = new List<(ActiveMessageKeys, string[])>();
    }

    public void addCustomBlocking(List<string> strings)
    {
        blocking_message_keys.add(strings);
    }

    public IEnumerator waitSafeAdvanceAnimSequenceToCoroutine(string destination) //This is a stub
    {
        yield return null;
        notifySequenceNodeExited(destination);
    }


    private void processSequentialBlocks(string message, string key)
    {
        List<(ActiveMessageKeys, string[])> to_remove = new List<(ActiveMessageKeys, string[])>();
        foreach (var awaiting in awaiting_sequential_players)
        {
            awaiting.Item1.checkRemoveMessageKey(message, key);
            if (awaiting.Item1.Count == 0)
            {
                GameStart.event_manager.startSequentialPlayer(awaiting.Item2);
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

    public void notifyCharacterAnimationComplete(string character, string animation)
    {
        //Debug.Log("EVENT_PLAYER: CharAnimEnded " + character + animation);
        processSequentialBlocks("CharAnimEnded", character + ":" + animation);

        if (blocking_message_keys.Count == 0)
            return;
        bool removed = blocking_message_keys.checkRemoveMessageKey("CharAnimEnded", character + ":" + animation);
        if (blocking_message_keys.Count == 0)
        {
            removeBlock();
        }
        if (removed == true)
        {
            return;
        }

        /*Hack*/
        /*It seems that if the animation misses 3 times, then remove block anyway*/

        foreach (ActiveMessageKeys.MessageKey a in blocking_message_keys.message_key_set)
        {
            if (a.message == "CharAnimEnded")
            {
                if (a.keys[0].Split(':')[0] == character)
                {
                    anim_block_miss_count++;
                    if (anim_block_miss_count >= 3)
                    {
                        anim_block_miss_count = 0;
                        blocking_message_keys.checkRemoveMessageKey("CharAnimEnded", a.keys[0]);
                    }
                    break;
                }
            }
        }
    }
    public void notifyMoveComplete(string character) => processNotifyBlocks("CharMovementEnded", character);
    public void notifyCamAnimFinished(string animation)
    {

        processNotifyBlocks("CamAnimFinished", animation);

    }
    public void notifyPropAnimationComplete(string prop, string animation) {
        //Debug.Log("EVENT_PLAYER: PropAnimEnded " + prop + animation);

        processNotifyBlocks("PropAnimEnded", prop + ":" + animation);

    }
    public void notifySequenceNodeExited(string node_name) => processNotifyBlocks("SequenceNodeExited", node_name);
    public void notifyScriptTrigger(string trigger) => processNotifyBlocks("AnimationScriptTriggerHit", trigger);
    public void notifyScreenFadeComplete() => processNotifyBlocks("ScreenFadeComplete", null);
    private void removeBlock()
    {
        if (is_sequential_player)
        {
            Debug.Log("Remove block sequential");
        }
        else
        {
            Debug.Log("Remove block");
        }
        total_block = false;
        block_duration = 0.00f;
        runImmediateEvents();
    }
}
