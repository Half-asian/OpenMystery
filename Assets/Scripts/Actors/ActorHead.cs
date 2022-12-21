using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.XR;
using static Events;

public partial class ActorController
{
    [SerializeField]
    private float speed_boost = 3.0f; //0.0 is instant, 1.0 is slow, 3.0 might be normal speed?
    [SerializeField]
    private Vector3 current_rotation = Vector3.zero;
    [SerializeField]
    private Vector3 target_rotation = Vector3.zero;
    [SerializeField]
    private bool is_head_only = false;

    [SerializeField]
    private float multiplier = 1.0f;
    [SerializeField]
    private float multiplier2 = 1.0f;
    private ActorController last_actor_controller = null;
    private Vector3 last_vec3 = Vector3.zero;

    [SerializeField]
    float delayed_y_angle = 0.0f;
    [SerializeField]
    float y_current_delayed_angle = 0.0f;
    [SerializeField]
    float delayed_x_angle = 0.0f;
    [SerializeField]
    float x_current_delayed_angle = 0.0f;

    bool aim_bigger_x = false;
    bool aim_bigger_y = false;

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
            setActorTarget(last_actor_controller);
        else
            setTargetRotation(last_vec3);
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
        if (Mathf.Abs(rotation.y) > 180.0f)
            rotation.y = (rotation.y > 0 ? -360.0f : 360) + rotation.y;

        if (Mathf.Abs(rotation.x) > 180.0f)
            rotation.x = (rotation.x > 0 ? -360.0f : 360) + rotation.x;

        target_rotation = rotation;
        delayed_y_angle = Mathf.Abs(rotation.y * 0.5f);
        y_current_delayed_angle = delayed_y_angle;
        delayed_x_angle = Mathf.Abs(rotation.x * 0.5f);
        x_current_delayed_angle = delayed_x_angle;

        if (current_rotation.x < target_rotation.x)
            aim_bigger_x = true;
        else
            aim_bigger_x = false;

        if (current_rotation.y < target_rotation.y)
            aim_bigger_y = true;
        else
            aim_bigger_y = false;

    }

    public void clearLookat()
    {
        speed_boost = 3.0f;
        setTargetRotation(Vector3.zero);
        last_actor_controller = null;
        last_vec3 = Vector3.zero;
    }

    public void clearTurnHeadAt()
    {
        speed_boost = 3.0f;
        setTargetRotation(Vector3.zero);
        last_actor_controller = null;
        last_vec3 = Vector3.zero;
    }

    public void ApplyHeadTurns()
    {
        //Remember,     X is VERTICAL
        //and           Y is HORIZONTAL
        float y_inertia_multiplier = y_current_delayed_angle > 0.0f ? 
            Mathf.Lerp(1.0f, 0.03f, y_current_delayed_angle / delayed_y_angle) : 
            1.0f;
        float x_inertia_multiplier = x_current_delayed_angle > 0.0f ? 
            Mathf.Lerp(1.0f, 0.03f, x_current_delayed_angle / delayed_x_angle) : 
            1.0f;

        float t_x = Time.deltaTime * x_inertia_multiplier * speed_boost / 1.2f; //1.4
        float t_y = Time.deltaTime * y_inertia_multiplier * speed_boost / 1.2f;

        float new_x = Mathf.Lerp(current_rotation.x, target_rotation.x + (aim_bigger_x ? 18.0f : -18.0f), t_x);
        float new_y = Mathf.Lerp(current_rotation.y, target_rotation.y + (aim_bigger_y ? 18.0f : -18.0f), t_y);

        //Since we lerp to 1.1 or 0.9 * target rotation, we need to clamp it to 1 * target rotation
        new_y = aim_bigger_y ? 
        Mathf.Min(new_y, target_rotation.y) : 
        Mathf.Max(new_y, target_rotation.y);
        new_x = aim_bigger_x ?
        Mathf.Min(new_x, target_rotation.x) :
        Mathf.Max(new_x, target_rotation.x);


        y_current_delayed_angle -= Mathf.Abs(current_rotation.y - new_y);
        x_current_delayed_angle -= Mathf.Abs(current_rotation.x - new_x);

        current_rotation = new Vector3(new_x, new_y, 0);

        if (!is_head_only)
            applyLooking();
        else
            applyTurning();

    }


    private void rotateBone(Transform bone, Vector3 rotation, bool allow_vertical, Vector3 reference_rot)
    {
        if (allow_vertical)
            rotation = new Vector3(rotation.x, rotation.y, 0);
        else
            rotation = new Vector3(0, rotation.y, 0);

        bone.eulerAngles = reference_rot + rotation;
    }

    private void applyLooking() {

        //Collect reference bone rotations
        Vector3 jt_hips_bind_rot = Vector3.zero;
        Vector3 spine1_loResSpine2_rot = Vector3.zero;
        Vector3 spine1_loResSpine3_rot = Vector3.zero;
        Vector3 head1_neck_rot = Vector3.zero ;
        if (model.pose_bones.ContainsKey("jt_hips_bind"))
            jt_hips_bind_rot = model.pose_bones["jt_hips_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
            spine1_loResSpine2_rot = model.pose_bones["spine1_loResSpine2_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
            spine1_loResSpine3_rot = model.pose_bones["spine1_loResSpine3_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("jt_head_bind"))
            head1_neck_rot = model.pose_bones["jt_head_bind"].eulerAngles;

        if (current_rotation.y > 90.0f || current_rotation.y < -90.0f)
        {
            if (current_rotation.y < 0.0f)
            {
                if (model.pose_bones.ContainsKey("jt_hips_bind"))
                    rotateBone(model.pose_bones["jt_hips_bind"], new Vector3(current_rotation.x, current_rotation.y + 90.0f, 0.0f), false, jt_hips_bind_rot);
            }
            else
            {
                if (model.pose_bones.ContainsKey("jt_hips_bind"))
                    rotateBone(model.pose_bones["jt_hips_bind"], new Vector3(current_rotation.x, current_rotation.y - 90.0f, 0.0f), false, jt_hips_bind_rot);
            }
        }

        if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
        {
            float spine1multiplier = 0.2f;

            if (current_rotation.y > 90.0f || current_rotation.y < -90.0f)
            {
                if (current_rotation.y > 0)
                    spine1multiplier = Mathf.Lerp(spine1multiplier, 0.5f, Mathf.Abs(current_rotation.y - 90.0f) / 50.0f);
                else
                    spine1multiplier = Mathf.Lerp(spine1multiplier, 0.5f, Mathf.Abs(current_rotation.y + 90.0f) / 50.0f);
            }

            rotateBone(model.pose_bones["spine1_loResSpine2_bind"], current_rotation * spine1multiplier, false, spine1_loResSpine2_rot);
        }

        if (model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
        {

            float spine2multiplier = 0.4f;

            if (current_rotation.y > 90.0f || current_rotation.y < -90.0f)
            {
                if (current_rotation.y > 0)
                    spine2multiplier = Mathf.Lerp(spine2multiplier, 0.7f, Mathf.Abs(current_rotation.y - 90.0f) / 50.0f);
                else
                    spine2multiplier = Mathf.Lerp(spine2multiplier, 0.7f, Mathf.Abs(current_rotation.y + 90.0f) / 50.0f);
            }

            rotateBone(model.pose_bones["spine1_loResSpine3_bind"], current_rotation * spine2multiplier, false, spine1_loResSpine3_rot);

        }
        

        if (model.pose_bones.ContainsKey("jt_head_bind"))
            rotateBone(model.pose_bones["jt_head_bind"], current_rotation, true, head1_neck_rot);
    }

    private void applyTurning() {
        Vector3 jt_hips_bind_rot = Vector3.zero;

        Vector3 spine1_loResSpine2_rot = Vector3.zero;
        Vector3 spine1_loResSpine3_rot = Vector3.zero;
        Vector3 head1_neck_rot = Vector3.zero;
        if (model.pose_bones.ContainsKey("jt_hips_bind"))
            jt_hips_bind_rot = model.pose_bones["jt_hips_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
            spine1_loResSpine2_rot = model.pose_bones["spine1_loResSpine2_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("spine1_loResSpine3_bind"))
            spine1_loResSpine3_rot = model.pose_bones["spine1_loResSpine3_bind"].eulerAngles;
        if (model.pose_bones.ContainsKey("jt_head_bind"))
            head1_neck_rot = model.pose_bones["jt_head_bind"].eulerAngles;

        if (current_rotation.y > 90.0f || current_rotation.y < -90.0f)
        {
            if (current_rotation.y < 0.0f)
            {
                if (model.pose_bones.ContainsKey("jt_hips_bind"))
                    rotateBone(model.pose_bones["jt_hips_bind"], new Vector3(current_rotation.x, current_rotation.y + 90.0f, 0.0f), false, jt_hips_bind_rot);
            }
            else
            {
                if (model.pose_bones.ContainsKey("jt_hips_bind"))
                    rotateBone(model.pose_bones["jt_hips_bind"], new Vector3(current_rotation.x, current_rotation.y - 90.0f, 0.0f), false, jt_hips_bind_rot);
            }

            if (current_rotation.y < 0.0f)
            {
                if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
                    rotateBone(model.pose_bones["spine1_loResSpine2_bind"], new Vector3(current_rotation.x, current_rotation.y + 90.0f, 0.0f), false, spine1_loResSpine2_rot);
            }
            else
            {
                if (model.pose_bones.ContainsKey("spine1_loResSpine2_bind"))
                    rotateBone(model.pose_bones["spine1_loResSpine2_bind"], new Vector3(current_rotation.x, current_rotation.y - 90.0f, 0.0f), false, spine1_loResSpine2_rot);
            }
        }

        if (model.pose_bones.ContainsKey("jt_head_bind"))
        {
            rotateBone(model.pose_bones["jt_head_bind"], current_rotation, true, head1_neck_rot);
        }

            return;


    }
}
