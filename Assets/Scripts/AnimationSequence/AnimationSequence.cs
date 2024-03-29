using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class AnimationSequence : MonoBehaviour
{
    //Although it is known as a character animation sequence, it is also used for props.
    public ConfigCharAnimSequence._CharAnimSequence config_sequence;

    protected Animation animation_component;
    //the anim sequence node
    public int node_index = 0;
    int node_repeated = 0;

    public Dictionary<string, HPAnimation> loadedAnimations = new Dictionary<string, HPAnimation>();

    public bool walk;

    string current_animation_name;

    //Not the anim sequence node
    protected Node base_node;

    public HPAnimation current_animation;

    protected Dictionary<string, Node> local_props = new Dictionary<string, Node>();

    public virtual void initAnimSequence(string _anim_sequence, bool _walk, string override_start_node)
    {
        base_node.onAnimationFinished += WaitForAnimation;
        walk = _walk;
        //Find the animation sequence in the config

        animation_component = GetComponent<Animation>();

        if (Configs.config_char_anim_sequence.CharAnimSequence.ContainsKey(_anim_sequence))
        {
            config_sequence = Configs.config_char_anim_sequence.CharAnimSequence[_anim_sequence];
        }
        else
        {
            Debug.LogError("Tried to load invalid animation sequence: " + _anim_sequence);
            return;
        }

        if (config_sequence.useStagger) //If stagger is true, do not crossfade
        {
            base_node.cancel_crossfade = true;
        }

        //Find index of starting node

        string starting_node = override_start_node != null ? override_start_node : config_sequence.data.startEdge.destinationId;

        for (int i = 0; i < config_sequence.data.nodes.Length; i++)
            if (config_sequence.data.nodes[i].nodeId == starting_node)
                node_index = i;

        //Perform the entry actions

        if (config_sequence.data.startEdge.actions != null)
            foreach (var action in config_sequence.data.startEdge.actions)
                processAction(action);


        activateNode();

        return;
    }

    private int getNextNodeIndex()
    {
        node_repeated = 0;
        //Find the new node
        int edge_index = -1;

        int weight_total = 0;
        int generated_weight = Random.Range(1, 100);

        for (int i = 0; i < config_sequence.data.nodes[node_index].edges.Length; i++) //Choose an edge to branch
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
        return new_node_index;
    }

    public void advanceAnimSequence()
    {
        int new_node_index = getNextNodeIndex();

        if (config_sequence.data.nodes[node_index].edges[config_sequence.data.nodes[node_index].edges.Length - 1].actions != null) //activate the actions
        {
            foreach (var action in config_sequence.data.nodes[node_index].edges[config_sequence.data.nodes[node_index].edges.Length - 1].actions)
                processAction(action);
        }

        node_index = new_node_index;
        activateNode();
    }

    public void activateNode()
    {
        node_repeated++;
        if (node_index == -1) //We have reached TheEnd
        {
            finishSequence();
            return;
        }

        //Activate the actions

        if (config_sequence.data.nodes[node_index].entryActions != null)
            foreach (var action in config_sequence.data.nodes[this.node_index].entryActions)
                processAction(action);


        //Play the main animation
        if (config_sequence.data.nodes[node_index].walkAnimName != null)
            current_animation_name = config_sequence.data.nodes[node_index].walkAnimName;
        else if (config_sequence.data.nodes[node_index].animName != null)
            current_animation_name = config_sequence.data.nodes[node_index].animName;
        else
            return;
        playAnimation(current_animation_name, config_sequence.sequenceId);
    }

    private void cleanup()
    {
        base_node.onAnimationFinished -= WaitForAnimation;
        destroyPropsMadeFromSequence();
        config_sequence = null;
    }

    protected virtual void finishSequence()
    {
        if (config_sequence.isOneShot)
        {
            cleanup();
            Destroy(this);
        }
    }

    protected virtual void OnDestroy()
    {
        cleanup();
    }

    private void WaitForAnimation(string animation_name)
    {
        if (config_sequence == null)
            return;

        if (node_index == -1)
            return;

        if (config_sequence.data.nodes[node_index].exitActions != null)
            foreach (var action in config_sequence.data.nodes[node_index].exitActions)
                processAction(action);

        //If we're a one shot and the next is theend, ignore block

        if (config_sequence.data.nodes[node_index].blocking == true)
        {
            if (!(config_sequence.isOneShot && getNextNodeIndex() == -1))
                return;
        }

        // at this point, the animation has completed
        // min loops unimplemented. No idea how it works.
        if (node_repeated >= config_sequence.data.nodes[node_index].maxLoops)
            advanceAnimSequence();
        else
            activateNode();

    }
    public void safeAdvanceAnimSequenceTo(string destination) //THIS IS USED ONCE IN THE GAME. NO CLUE. THIS IS A STUB TO PREVENT A SOFT LOCK.
    {
        advanceAnimSequence();
    }

    public void destroyPropsMadeFromSequence()
    {
        foreach (var prop_name in local_props.Keys)
        {
            if (prop_name != null)
            {
                base_node.removeChildNode(prop_name);
            }
        }
        local_props.Clear();
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
