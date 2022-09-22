using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class AnimationSequence : MonoBehaviour
{
    //Although it is known as a character animation sequence, it is also used for props.
    public ConfigCharAnimSequence._CharAnimSequence config_sequence;

    protected Animation animation_component;

    public int node_index = 0;

    public bool waiting = false;

    public IEnumerator wait_for_animation = null;

    public Dictionary<string, HPAnimation> loadedAnimations = new Dictionary<string, HPAnimation>();

    public bool walk;

    string current_animation_name;

    protected Node base_node;

    public virtual void initAnimSequence(string _anim_sequence, bool _walk)
    {

        destroyProps();
        walk = _walk;
        //Find the animation sequence in the config

        animation_component = GetComponent<Animation>();

        waiting = false;

        if (wait_for_animation != null)
        {
            StopCoroutine(wait_for_animation);
        }

        if (Configs.config_char_anim_sequence.CharAnimSequence.ContainsKey(_anim_sequence))
        {
            config_sequence = Configs.config_char_anim_sequence.CharAnimSequence[_anim_sequence];
        }
        else
        {
            Debug.LogError("Tried to load invalid animation sequence: " + _anim_sequence);
            return;
        }

        //Find index of starting node

        for (int i = 0; i < config_sequence.data.nodes.Length; i++)
            if (config_sequence.data.nodes[i].nodeId == config_sequence.data.startEdge.destinationId)
                node_index = i;

        //Perform the entry actions

        if (config_sequence.data.startEdge.actions != null)
            foreach (ConfigCharAnimSequence._CharAnimSequence._data.action action in config_sequence.data.startEdge.actions)
                processAction(action);

        activateNode(node_index);

        return;
    }

    public void advanceAnimSequence()
    {
        //Find the new node
        int edge_index = -1;

        int weight_total = 0;
        int generated_weight = Random.Range(1, 100);

        for(int i = 0; i < config_sequence.data.nodes[node_index].edges.Length; i++) //Choose an edge to branch
        {
            weight_total += config_sequence.data.nodes[node_index].edges[i].weight;
            if (generated_weight <= weight_total || i == config_sequence.data.nodes[node_index].edges.Length - 1)
            {
                edge_index = i;
                break;
            }
        }

        int new_node_index = -1;

        for (int i = 0; i < config_sequence.data.nodes.Length; i++) //Find the node that matches the edge id
        {
            if (config_sequence.data.nodes[i].nodeId == config_sequence.data.nodes[node_index].edges[edge_index].destinationId)
                new_node_index = i;
        }

        if (config_sequence.data.nodes[node_index].edges[config_sequence.data.nodes[node_index].edges.Length - 1].actions != null) //activate the actions
        {
            foreach (ConfigCharAnimSequence._CharAnimSequence._data.action action in config_sequence.data.nodes[node_index].edges[config_sequence.data.nodes[node_index].edges.Length - 1].actions)
                processAction(action);
        }

        activateNode(new_node_index);
    }

    public void activateNode(int node_index)
    {
        if (node_index == -1) //We have reached TheEnd
        {
            finishSequence();
            return;
        }

        this.node_index = node_index;

        //Activate the actions

        if (config_sequence.data.nodes[this.node_index].entryActions != null)
            foreach (ConfigCharAnimSequence._CharAnimSequence._data.action action in config_sequence.data.nodes[this.node_index].entryActions)
                processAction(action);


        //Play the main animation
        //if (walk)
        if (config_sequence.data.nodes[this.node_index].walkAnimName != null)
            current_animation_name = config_sequence.data.nodes[this.node_index].walkAnimName;
        else
            current_animation_name = config_sequence.data.nodes[this.node_index].animName;

        float animation_length = playAnimation(current_animation_name, config_sequence.sequenceId);

        if (config_sequence.data.nodes[this.node_index].blocking == false)
        {
            waiting = true;
            wait_for_animation = WaitForAnimation(animation_length, current_animation_name);
            StartCoroutine(wait_for_animation);
        }
    }

    protected virtual void finishSequence()
    {
        Destroy(this);
        return;
    }

    protected IEnumerator WaitForAnimation(float clip_time, string animation_name)
    {
        yield return new WaitForSeconds(clip_time);

        // at this point, the animation has completed
        // so at this point, do whatever you wish...

        if (waiting == true)
        {
            waiting = false;
            advanceAnimSequence();
        }
        else
        {
            Debug.Log("Wtf");
        }
    }
    public void safeAdvanceAnimSequenceTo(string destination) //THIS IS USED ONCE IN THE GAME. NO CLUE. THIS IS A STUB TO PREVENT A SOFT LOCK.
    {
        advanceAnimSequence();
    }

    protected void destroyProps()
    {
        base_node.destroyProps();
    }

    protected static string getBroomSkinName(string actor_id)
    {
        if (Configs.config_quidditch_broom_info.QuidditchBroomInfo.ContainsKey(actor_id))
        {
            if (Configs.config_quidditch_broom_info.QuidditchBroomInfo[actor_id].predicates != null)
            {
                for (int i = 0; i < Configs.config_quidditch_broom_info.QuidditchBroomInfo[actor_id].predicates.Length; i++)
                {
                    if (Predicate.parsePredicate(Configs.config_quidditch_broom_info.QuidditchBroomInfo[actor_id].predicates[i]) == true)
                    {
                        return Configs.config_quidditch_broom_info.QuidditchBroomInfo[actor_id].brooms[i];
                    }
                }
            }
            return Configs.config_quidditch_broom_info.QuidditchBroomInfo[actor_id].defaultBroom;
        }
        return Configs.config_quidditch_broom_info.QuidditchBroomInfo["default"].defaultBroom;
    }

}
