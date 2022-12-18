using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Events;

public class ActorHead
{


    private ActorController actor_controller;
    Transform char_a;

    public ActorHead(ActorController _actor_manager)
    {
        actor_controller = _actor_manager;
    }
    private Transform transform { get { return actor_controller.transform; } }
    private GameObject gameObject { get { return actor_controller.gameObject; } }

    private float progress = 0.0f;

    private Quaternion? start_rotation = null;
    private Quaternion? target_rotation = null;

    private bool is_head_only = false;



    public void setLookAt(ActorController target_actor)
    {
        setActorTarget(target_actor);
        is_head_only = false;
    }

    public void setTurnHeadAt(ActorController target_actor)
    {
        setActorTarget(target_actor);
        is_head_only = true;
    }

    private void setActorTarget(ActorController target_actor)
    {
        if (!target_actor.model.pose_bones.ContainsKey("jt_head_bind"))
        {
            Debug.LogError("Couldn't find jt_head_bind for lookat on " + target_actor.name);
            clearLookat();
            return;
        }

        char_a = actor_controller.model.pose_bones["jt_head_bind"];

        Vector3 target_position = target_actor.model.pose_bones["jt_head_bind"].position + new Vector3(0, 0.5f, 0) - transform.position; //Slight bump upwards
        var dest_quaternion = Quaternion.LookRotation(gameObject.transform.position + target_position - char_a.position).normalized;
        dest_quaternion = Quaternion.Euler(dest_quaternion.eulerAngles + new Vector3(0, 0, -90));
        setTargetRotation(dest_quaternion);
    }

    private void setTargetRotation(Quaternion? rotation)
    {
        progress = 0.0f;
        start_rotation = target_rotation;
        target_rotation = rotation;

        Debug.Log("Start quaternion: " + start_rotation + " " + target_rotation);
    }

    public void clearLookat()
    {
        progress = 0.0f;
        setTargetRotation(null);
    }

    public void clearTurnHeadAt()
    {
        progress = 0.0f;
        setTargetRotation(null);
    }

    public void ApplyHeadTurns()
    {
        if ( progress < 1.0f)
        {
            if (progress <= 0.5f)
            {
                progress += 4f * Time.deltaTime * (progress + 0.3f);
            }
            else
            {
                progress += 4f * Time.deltaTime * (1.0f - (progress + 0.3f - 0.5f));
            }
        }

        if (progress > 1.0f)
            progress = 1.0f;

        if (target_rotation == null)
        {
            if (start_rotation == null || progress >= 1.0f)
                return;
        }

        if (!is_head_only)
        {
            applyLooking();
        }
        else
        {
            applyTurning();
        }
    }


    private void rotateBone(Transform bone, float rotation_multiplier)
    {
        Quaternion bone_start_rotation;
        Quaternion bone_target_rotation;
        if (start_rotation != null)
            bone_start_rotation = Quaternion.Lerp(bone.rotation, (Quaternion)start_rotation, rotation_multiplier);
        else
            bone_start_rotation = bone.rotation;
        if (target_rotation != null)
            bone_target_rotation = Quaternion.Lerp(bone.rotation, (Quaternion)target_rotation, rotation_multiplier);
        else
            bone_target_rotation = bone.rotation;


        Quaternion target_direction = Quaternion.Lerp(bone_start_rotation, bone_target_rotation, progress);

        target_direction = Quaternion.Euler(new Vector3(bone.eulerAngles.x, target_direction.eulerAngles.y, bone.eulerAngles.z));

        bone.eulerAngles = target_direction.eulerAngles;

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
            {
                bone = actor_controller.model.pose_bones[bone.name];

                if (bone != null)
                {
                    bone.eulerAngles = target_direction.eulerAngles;
                }
            }
        }
    }

    private void applyLooking() {
        if (actor_controller.model.pose_bones.ContainsKey("jt_hips_bind"))
        {
            rotateBone(actor_controller.model.pose_bones["jt_hips_bind"], 0.1f);
        }

        if (actor_controller.model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
        {
            rotateBone(actor_controller.model.pose_bones["spine1_loResSpine2_bind"], 0.2f);
        }

        if (actor_controller.model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
        {
            rotateBone(actor_controller.model.pose_bones["spine1_loResSpine3_bind"], 0.4f);
        }

        if (actor_controller.model.pose_bones.ContainsKey("head1_neck_bind"))
        {
            rotateBone(actor_controller.model.pose_bones["head1_neck_bind"], 0.7f);
        }
    }

    private void applyTurning() {
        if (actor_controller.model.pose_bones.ContainsKey("head1_neck_bind"))
        {
            rotateBone(actor_controller.model.pose_bones["head1_neck_bind"], 0.7f);
        }
    }
}
