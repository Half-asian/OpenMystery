using System.Collections.Generic;
using UnityEngine;

public class ActorController : PropHolder
{
    public ActorHead actor_head;
    public ActorAnimation actor_animation;
    public ActorMovement actor_movement;
    public Animation animation_component;
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

    public void setup(Model _model)
    {
        model = _model;
        if (model.game_object.GetComponent<Animation>() == null)
            animation_component = model.game_object.AddComponent<Animation>();
        actor_head = new ActorHead(this);
        actor_animation = model.game_object.AddComponent<ActorAnimation>();
        actor_animation.actor_controller = this;
        actor_movement = new ActorMovement(this);
        patches = new List<Model>();
    }

    public void addPatch(string patch_model_id)
    {
        Model patch = ModelManager.loadModel(patch_model_id, model.pose_bones);
        patches.Add(patch);
        patch.game_object.transform.parent = transform;
    }


    /*public void rootMotionMove(string movement_string)
    {
        string[] floats = movement_string.Split(',');
        Vector3 delta = new Vector3(float.Parse(floats[0]) / 100, float.Parse(floats[1]) / 100, float.Parse(floats[2])) / 100;

        actor_movement.MoveOverTime(delta, float.Parse(floats[3]));
    }*/
    public void PlaySound(string sound) =>
    Sound.playSoundEffect(sound);



    public void destroy()
    {
        foreach(Model patch in patches)
        {
            Destroy(patch.game_object);
        }
        Destroy(model.game_object);
    }

    private void LateUpdate()
    {
        actor_head.ApplyHeadTurns();
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
            actor_animation.animId_idle,                //14
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