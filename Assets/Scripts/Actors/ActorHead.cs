using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.XR;
using static Events;

public partial class ActorController
{
    [SerializeField]
    private float progress = 0.0f;
    [SerializeField]
    private float speed_boost = 3.0f; //0.0 is instant, 1.0 is slow, 3.0 might be normal speed?
    [SerializeField]
    private Vector3 start_rotation = Vector3.zero;
    [SerializeField]
    private Vector3 target_rotation = Vector3.zero;
    [SerializeField]
    private bool is_head_only = false;

    [SerializeField]
    private float multiplier = 1.0f;

    private ActorController last_actor_controller = null;
    private Vector3 last_vec3 = Vector3.zero;

    public void setLookAt(ActorController target_actor, float new_speed = 3.0f)
    {
        setActorTarget(target_actor);
        is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_vec3 = Vector3.zero;
    }

    public void setLookAt(float x, float y, float new_speed = 3.0f)
    {
        setTargetRotation(new Vector3(x, y, 0));
        is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_vec3 = new Vector3(x, y, 0);
        last_actor_controller = null;
    }

    public void setTurnHeadAt(ActorController target_actor, float new_speed = 3.0f)
    {
        setActorTarget(target_actor);
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_vec3 = Vector3.zero;
    }

    public void setTurnHeadAt(float x, float y, float new_speed = 3.0f)
    {
        setTargetRotation(new Vector3(x, y, 0));
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_vec3 = new Vector3(x, y, 0);
        last_actor_controller = null;
    }

    public void refreshLookAts()
    {
        if (last_actor_controller != null)
        {
            setActorTarget(last_actor_controller);
        }
        else
        {
            setTargetRotation(last_vec3);
        }
    }


    private void setActorTarget(ActorController target_actor)
    {
        if (!target_actor.model.pose_bones.ContainsKey("jt_head_bind"))
        {
            Debug.LogError("Couldn't find jt_head_bind for lookat on " + target_actor.name);
            clearLookat();
            return;
        }

        Vector3 targetDirection = target_actor.model.pose_bones["jt_head_bind"].position - model.pose_bones["jt_head_bind"].position;

        setTargetRotation(Quaternion.LookRotation(targetDirection).eulerAngles - transform.eulerAngles);
    }

    private void setTargetRotation(Vector3 rotation)
    {
        progress = 0.0f;
        start_rotation = target_rotation;
        target_rotation = rotation;
    }

    public void clearLookat()
    {
        progress = 0.0f;
        speed_boost = 3.0f;
        setTargetRotation(Vector3.zero);
        last_actor_controller = null;
        last_vec3 = Vector3.zero;
    }

    public void clearTurnHeadAt()
    {
        progress = 0.0f;
        speed_boost = 3.0f;
        setTargetRotation(Vector3.zero);
        last_actor_controller = null;
        last_vec3 = Vector3.zero;
    }

    public void ApplyHeadTurns()
    {
        if (progress < 0.0f) //This should never happen
            progress = 0.0f;

        if ( progress < 1.0f)
        {
            if (progress <= 0.5f)
                progress += 4.0f * Time.deltaTime * (progress + 0.3f) * (speed_boost / 3);
            else
                progress += 4.0f * Time.deltaTime * (1.0f - (progress + 0.3f - 0.5f)) * (speed_boost / 3);
        }

        if (progress > 1.0f)
            progress = 1.0f;

        if (!is_head_only)
            applyLooking();
        else
            applyTurning();
    }


    private void rotateBone(Transform bone, float rotation_multiplier, bool allow_vertical, Vector3 reference_rot)
    {
        Quaternion bone_start_rotation;
        Quaternion bone_target_rotation;
        bone_start_rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(start_rotation), rotation_multiplier);
        bone_target_rotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(target_rotation), rotation_multiplier);

        if (allow_vertical)
            bone_target_rotation = Quaternion.Euler(new Vector3(bone_target_rotation.eulerAngles.x, bone_target_rotation.eulerAngles.y, 0));
        else
            bone_target_rotation = Quaternion.Euler(new Vector3(0, bone_target_rotation.eulerAngles.y, 0));

        bone.eulerAngles = reference_rot + Quaternion.Lerp(bone_start_rotation, bone_target_rotation, progress).eulerAngles;

        //What the fuck is this for?
        /*for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
            {
                bone = model.pose_bones[bone.name];

                if (bone != null)
                {
                    bone.eulerAngles = reference_rot + Quaternion.Lerp(bone_start_rotation, bone_target_rotation, progress).eulerAngles;
                }
            }
        }*/
    }

    private void applyLooking() {

        //Collect reference bone rotations
        Vector3 spine1_loResSpine2_rot = Vector3.zero;
        Vector3 spine1_loResSpine3_rot = Vector3.zero;
        Vector3 head1_neck_rot = Vector3.zero ;
        if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
            spine1_loResSpine2_rot = model.pose_bones["spine1_loResSpine2_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
            spine1_loResSpine3_rot = model.pose_bones["spine1_loResSpine3_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("head1_neck_bind"))
            head1_neck_rot = model.pose_bones["head1_neck_bind"].eulerAngles;

        if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
            rotateBone(model.pose_bones["spine1_loResSpine2_bind"], 0.2f, false, spine1_loResSpine2_rot);

        if (model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
            rotateBone(model.pose_bones["spine1_loResSpine3_bind"], 0.4f, false, spine1_loResSpine3_rot);

        if (model.pose_bones.ContainsKey("head1_neck_bind"))
            rotateBone(model.pose_bones["head1_neck_bind"], 1, true, head1_neck_rot);
    }

    private void applyTurning() {
        if (model.pose_bones.ContainsKey("head1_neck_bind"))
        {
            Vector3 head1_neck_rot = model.pose_bones["head1_neck_bind"].eulerAngles;
            rotateBone(model.pose_bones["head1_neck_bind"], 1, true, head1_neck_rot);
        }
    }
}
