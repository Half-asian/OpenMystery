using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
public partial class ActorController : Node
{
    public ActorHead actor_head;
    public ActorMovement actor_movement;
    public List<Model> patches;

    private ConfigHPActorInfo._HPActorInfo _actor_info;

    public ConfigHPActorInfo._HPActorInfo actor_info
    {
        get
        {
            return _actor_info;
        }
        set
        {
            _actor_info = value;
        }
    }

    public AvatarComponents avatar_components;

    public ActorState actor_state = ActorState.Idle;

    public List<GameObject> particles = new List<GameObject>();


    private void LateUpdate()
    {
        actor_head.ApplyHeadTurns();
    }

    public new void setup(Model _model)
    {
        base.setup(_model);

        actor_head = new ActorHead(this);
        initializeAnimations();
        actor_movement = new ActorMovement(this);
        patches = new List<Model>();

    }

    //Clean up everything from last state
    public void cleanupState()
    {
        DestroyImmediate(GetComponent<ActorAnimSequence>());
        destroyProps();
        foreach (GameObject particle in particles)
        {
            GameObject.Destroy(particle);
        }
        particles = new List<GameObject>();
    }

    public void setActorState(ActorState _actor_state)
    {
        if (actor_state != _actor_state)
        {
            cleanupState();
        }

        //Set new state
        actor_state = _actor_state;
    }

    public void setCharacterIdle()
    {
        setActorState(ActorState.Idle);
        playIdleAnimation();
    }
    public void setCharacterWalk()
    {
        setActorState(ActorState.Walk);
        playWalkAnimation();
    }

    public void addPatch(string patch_model_id)
    {
        Model patch = ModelManager.loadModel(patch_model_id, model.pose_bones);
        patches.Add(patch);
        patch.game_object.transform.parent = transform;
    }

    public void PlaySound(string sound) =>
    Sound.playSoundEffect(sound);
    public void ScriptTrigger(string trigger) =>
    GameStart.event_manager.notifyScriptTrigger(trigger);

    public void destroy()
    {
        foreach(Model patch in patches)
        {
            Destroy(patch.game_object);
        }
        Destroy(model.game_object);
    }

    public void AttachParticleSystem(string parameters)
    {
        string[] split = parameters.Split(':');
        string particle_name = split[0];
        string bone_name = split[1];
        string prop_name = null;
        GameObject particle = null;
        if (split.Length == 3)
        {
            prop_name = split[2];
            if (childNodes.ContainsKey(prop_name))
            {
                Transform bone = childNodes[prop_name].GetComponent<Node>().model.pose_bones[bone_name];
                particle = Particle.AttachParticleSystem(particle_name, bone);
            }
        }
        else
        {
            particle = Particle.AttachParticleSystem(particle_name, model.pose_bones[bone_name]);
        }
        if (particle != null)
            particles.Add(particle);
    }

    //Not super advanced
    public string[] toStringArray()
    {
        string[] array =
        {
            gameObject.name,                            //0
            actor_info.actorId,                         //1
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
            actor_movement.getDestinationWaypoint(),    //13
            idle_animation,             //14
        };

        return array;

    }
}

public enum ActorState
{
    Idle,
    Walk,
    Run,
    SittingIdle,
    Success,
    RenownBoard
}