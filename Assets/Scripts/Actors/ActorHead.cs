using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public partial class ActorController
{
    [SerializeField]
    private float speed_boost = 3.0f; //0.0 is instant, 1.0 is slow, 3.0 might be normal speed?
    [SerializeField]
    private Vector3 current_rotation = Vector3.zero;
    [SerializeField]
    private Vector3 target_rotation = Vector3.zero;
    [SerializeField]
    private Vector3 target_direction = Vector3.zero;
    [SerializeField]
    private bool is_head_only = false;

    private ActorController last_actor_controller = null;
    private Prop last_prop = null;
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

    [SerializeField]
    private string target_id;

    private string[] queued_lookat = null;
    private string[] queued_turnheadat = null;
    private string[] queued_turnheadtowards = null;
    private string[] queued_turntowards = null;
    public void queueLookAt(string[] action_params)
    {
        queued_lookat = action_params;
    }
    public void queueTurnHeadAt(string[] action_params)
    {
        queued_turnheadat = action_params;
    }
    public void queueTurnHeadTowards(string[] action_params)
    {
        queued_turnheadtowards = action_params;
    }
    public void queueTurnTowards(string[] action_params)
    {
        queued_turntowards = action_params;
    }
    private void headUpdate()
    {
        if (queued_lookat != null)
        {
            lookAt(queued_lookat);
            queued_lookat = null;
        }
        if (queued_turnheadat != null)
        {
            turnHeadAt(queued_turnheadat);
            queued_turnheadat = null;
        }
        if (queued_turnheadtowards != null)
        {
            turnHeadTowards(queued_turnheadtowards);
            queued_turnheadtowards = null;
        }
        if (queued_turntowards != null)
        {
            turnTowards(queued_turntowards);
            queued_turntowards = null;
        }
    }

    public void lookAt(string[] action_params)
    {
        if (action_params.Length < 2 || action_params[0] == action_params[1])                                                   //Actor clear
        {
            clearAllLooking();
            return;
        }

        float speed = 3.0f;

        if (action_params.Length > 2)
        {
            if (!float.TryParse(action_params[2], NumberStyles.Any, CultureInfo.InvariantCulture, out speed))
                Debug.LogError("Failed to parse lookAt speed float: " + action_params[2]);

            //QuidditchS1C10P3_hoochSlowLookOrion
        }

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            setLookAt(Actor.getActor(action_params[1]), speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            setLookAt(Prop.spawned_props[action_params[1]], speed);
            return;
        }
        else                                                                            //Actor look in specific direction
        {
            try
            {
                float x = 0;
                string[] numbers = action_params[1].Split(',');
                float y = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (numbers.Length > 1)
                    x = float.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (x == 0 && y == 0)
                {
                    clearAllLooking();
                }
                else
                {
                    setLookAt(-x, y, speed);
                }
            }
            catch
            {
                Debug.LogError("Unknown second param for lookat " + action_params[0] + " " + action_params[1]);
            }
        }
    }

    private void turnHeadAt(string[] action_params)
    {
        if (action_params.Length < 2 || action_params[0] == action_params[1])                                                   //Actor clear
        {
            clearAllLooking();
            return;
        }

        float speed = 3.0f;

        if (action_params.Length > 2)
        {
            if (!float.TryParse(action_params[2], NumberStyles.Any, CultureInfo.InvariantCulture, out speed))
                Debug.LogError("Failed to parse turnHeadAt speed float: " + action_params[2]);
            //QuidditchS1C10P3_hoochSlowLookOrion
        }

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            setTurnHeadAt(
                Actor.getActor(action_params[1]), speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            setTurnHeadAt(
                Prop.spawned_props[action_params[1]], speed);
            return;
        }
        else                                                                            //Actor look in specific direction
        {
            try
            {
                float x = 0;
                string[] numbers = action_params[1].Split(',');
                float y = float.Parse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (numbers.Length > 1)
                    x = int.Parse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                if (x == 0 && y == 0)
                {
                    clearAllLooking();
                }
                else
                {
                    setTurnHeadAt(-x, y, speed);
                }
            }
            catch
            {
                Debug.LogError("Unknown second param for turnheadat " + action_params[0] + " " + action_params[1]);
            }
        }
    }

    private void turnHeadTowards(string[] action_params)
    {
        if (action_params.Length < 2 || action_params[0] == action_params[1])                                                   //Actor clear
        {
            clearAllLooking();
            return;
        }
        float speed = 3.0f;

        if (action_params.Length > 3)
        {
            if (!float.TryParse(action_params[3], NumberStyles.Any, CultureInfo.InvariantCulture, out speed))
                Debug.LogError("Failed to parse turnHeadTowards speed float: " + action_params[3]);
        }

        string bone = null;
        if (action_params.Length > 2){
            bone = action_params[2];
        }

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            setTurnHeadTowards(
                Actor.getActor(action_params[1]), bone, speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            setTurnHeadTowards(
                Prop.spawned_props[action_params[1]], bone, speed);
            return;
        }
    }

    private void turnTowards(string[] action_params)
    {
        if (action_params.Length < 2 || action_params[0] == action_params[1])                                                   //Actor clear
        {
            clearAllLooking();
            return;
        }
        float speed = 3.0f;

        if (action_params.Length > 3)
        {
            if (!float.TryParse(action_params[3], NumberStyles.Any, CultureInfo.InvariantCulture, out speed))
                Debug.LogError("Failed to parse turnHeadTowards speed float: " + action_params[3]);
        }

        string bone = null;
        if (action_params.Length > 2)
        {
            bone = action_params[2];
        }

        if (Actor.getActor(action_params[1]) != null)                      //Actor look at target actor
        {
            setTurnTowards(
                Actor.getActor(action_params[1]), bone, speed);
            return;
        }
        else if (Prop.spawned_props.ContainsKey(action_params[1]))
        {
            setTurnTowards(
                Prop.spawned_props[action_params[1]], bone, speed);
            return;
        }
    }

    public void setLookAt(ActorController target_actor, float new_speed = 3.0f)
    {
        setActorTarget(target_actor);
        is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_prop = null;
        last_vec3 = Vector3.zero;
        target_id = target_actor.name;
    }

    public void setLookAt(Prop target_prop, float new_speed = 3.0f)
    {
        setActorTarget(target_prop);
        is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = null;
        last_prop = target_prop;
        last_vec3 = Vector3.zero;
        target_id = target_prop.name;
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
        last_prop = null;
        target_id = x + " " + y;
    }

    public void setTurnHeadTowards(ActorController target_actor, string bone, float new_speed = 3.0f)
    {
        if (bone != null)
            setActorTarget(target_actor, bone);
        else
            setActorTarget(target_actor);
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_prop = null;
        last_vec3 = Vector3.zero;
        target_id = target_actor.name;
    }
    public void setTurnHeadTowards(Prop target_prop, string bone, float new_speed = 3.0f)
    {
        if (bone != null)
            setActorTarget(target_prop, bone);
        else
            setActorTarget(target_prop);
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = null;
        last_prop = target_prop;
        last_vec3 = Vector3.zero;
        target_id = target_prop.name;
    }

    public void setTurnHeadAt(ActorController target_actor, float new_speed = 3.0f)
    {
        setActorTarget(target_actor);
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_prop = null;
        last_vec3 = Vector3.zero;
        target_id = target_actor.name;
    }

    public void setTurnHeadAt(Prop target_prop, float new_speed = 3.0f)
    {
        setActorTarget(target_prop);
        is_head_only = true;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = null;
        last_prop = target_prop;
        last_vec3 = Vector3.zero;
        target_id = target_prop.name;
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
        last_prop = null;
        target_id = x + " " + y;
    }

    public void setTurnTowards(ActorController target_actor, string bone, float new_speed = 3.0f)
    {
        if (bone != null)
            setActorTarget(target_actor, bone);
        else
            setActorTarget(target_actor); is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = target_actor;
        last_prop = null;
        last_vec3 = Vector3.zero;
        target_id = target_actor.name;
    }
    public void setTurnTowards(Prop target_prop, string bone, float new_speed = 3.0f)
    {
        if (bone != null)
            setActorTarget(target_prop, bone);
        else
            setActorTarget(target_prop);
        is_head_only = false;
        speed_boost = new_speed;
        if (speed_boost == 0.0)
            speed_boost = 1000.0f;
        last_actor_controller = null;
        last_prop = target_prop;
        last_vec3 = Vector3.zero;
        target_id = target_prop.name;
    }
    public void refreshLookAts(Vector3 waypoint_euler_angles)
    {
        if (last_actor_controller != null || last_prop != null)
            setTargetRotation(Quaternion.LookRotation(target_direction).eulerAngles - waypoint_euler_angles, true);
        else
            setTargetRotation(last_vec3, true);

    }


    private void setActorTarget(ActorController target_actor, string bone = "jt_head_bind")
    {
        if (!target_actor.model.pose_bones.ContainsKey(bone))
        {
            Debug.LogError("Couldn't find " + bone + " for lookat on " + target_actor.name);
            clearAllLooking();
            return;
        }

        target_direction = target_actor.model.pose_bones[bone].position - model.pose_bones["jt_head_bind"].position;

        setTargetRotation(Quaternion.LookRotation(target_direction).eulerAngles - transform.eulerAngles);
    }

    private void setActorTarget(Prop target_prop, string bone = "jt_prop_bind")
    {
        if (!target_prop.model.pose_bones.ContainsKey(bone))
        {
            Debug.LogError("Couldn't find " + bone + " for lookat on " + target_prop.name);
            clearAllLooking();
            return;
        }

        target_direction = target_prop.model.pose_bones[bone].position - model.pose_bones["jt_head_bind"].position;

        setTargetRotation(Quaternion.LookRotation(target_direction).eulerAngles - transform.eulerAngles);
    }

    private void setTargetRotation(Vector3 rotation, bool no_delay = false)
    {
        if (Mathf.Abs(rotation.y) > 180.0f)
            rotation.y = (rotation.y > 0 ? -360.0f : 360) + rotation.y;

        if (Mathf.Abs(rotation.x) > 180.0f)
            rotation.x = (rotation.x > 0 ? -360.0f : 360) + rotation.x;

        target_rotation = rotation;

        if (no_delay == false)
        {
            delayed_y_angle = Mathf.Abs(rotation.y * 0.5f);
            y_current_delayed_angle = delayed_y_angle;
            delayed_x_angle = Mathf.Abs(rotation.x * 0.5f);
            x_current_delayed_angle = delayed_x_angle;
        }

        if (current_rotation.x < target_rotation.x)
            aim_bigger_x = true;
        else
            aim_bigger_x = false;

        if (current_rotation.y < target_rotation.y)
            aim_bigger_y = true;
        else
            aim_bigger_y = false;

    }

    public void clearAllLooking()
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
