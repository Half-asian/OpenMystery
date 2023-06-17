using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
public partial class ActorController : Node
{
    public List<Model> patches;

    public ConfigHPActorInfo._HPActorInfo config_hpactor;

    public AvatarComponents avatar_components;

    public ActorState actor_state = ActorState.Idle;

    private void LateUpdate()
    {
        ApplyHeadTurns();
    }

    protected override void OnUpdate()
    {
        headUpdate();
    }

    public new void setup(Model _model)
    {
        base.setup(_model);

        initializeAnimations();
        patches = new List<Model>();

    }

    //Clean up everything from last state
    private void cleanupState()
    {
        foreach (var actoranimseq in GetComponents<ActorAnimSequence>())
        {
            DestroyImmediate(actoranimseq);
        }
        destroyProps();
        //Some animations change these
        model.jt_all_bind.localPosition = Vector3.zero;
        model.jt_all_bind.localRotation = Quaternion.identity;
        if (waitForAnimateCharacterFinished != null)
            StopCoroutine(waitForAnimateCharacterFinished);
    }

    private void setActorState(ActorState _actor_state)
    {
        if (actor_state != _actor_state)
        {
            cleanupState();
        }

        //Set new state
        actor_state = _actor_state;
    }

    public void setCharacterIdle(string event_id)
    {
        modifying_events.Enqueue(event_id);
        setActorState(ActorState.Idle);
        finishMovement();
        if (this != null) //The actor might die after finishing movement
            playActorAnimation(idle_actor_anim);
    }
    public void setCharacterIdle()
    {
        setActorState(ActorState.Idle);
        finishMovement();
        if (this != null) //The actor might die after finishing movement
            playActorAnimation(idle_actor_anim);
    }
    public void setCharacterWalk()
    {
        if (actor_state == ActorState.Idle)
            cancel_crossfade = true;
        setActorState(ActorState.Walk);
        if (walk_actor_anim.id != null)
        {
            playActorAnimation(walk_actor_anim);
        }
        else if (config_hpactor.animId_walk != null)
        {
            walk_actor_anim = new ActorAnim(ActorAnim.AnimType.Regular, config_hpactor.animId_walk);
            playActorAnimation(walk_actor_anim);
        }
    }
    private void setCharacterAnimate()
    {
        setActorState(ActorState.Animate);
    }

    public void addPatch(string patch_model_id)
    {
        Model patch = ModelManager.loadModel(patch_model_id, model.pose_bones);
        patches.Add(patch);
        patch.game_object.transform.parent = transform;
    }

    public void destroy()
    {
        foreach(Model patch in patches)
        {
            DestroyImmediate(patch.game_object);
        }
        DestroyImmediate(model.game_object);
    }

    //Not super advanced
    public string[] toStringArray()
    {
        string[] array =
        {
            gameObject.name,                            //0
            config_hpactor.actorId,                         //1
            gameObject.transform.position.x.ToString(), //2
            gameObject.transform.position.y.ToString(), //3
            gameObject.transform.position.z.ToString(), //4

            gameObject.transform.rotation.x.ToString(), //5
            gameObject.transform.rotation.y.ToString(), //6
            gameObject.transform.rotation.z.ToString(), //7
            gameObject.transform.rotation.w.ToString(), //8

            gameObject.transform.localScale.x.ToString(),//9
            gameObject.transform.localScale.y.ToString(),//10
            gameObject.transform.localScale.z.ToString(),//11

            actor_state.ToString(),                     //12
            destination_waypoint_name,    //13
            config_hpactor.animId_idle,             //14
        };

        if (avatar_components != null) //Actually an avatar
        {
            array[1] = "Avatar";
        }

        return array;

    }
}

public enum ActorState
{
    Idle,
    Walk,
    Animate
}