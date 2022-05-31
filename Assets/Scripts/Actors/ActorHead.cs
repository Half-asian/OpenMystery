using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorHead
{


    private ActorController actor_manager;
    public string looking_at;
    public Events.Looking looking;
    public Events.Looking turning;
    Transform char_a;

    public ActorHead(ActorController _actor_manager)
    {
        actor_manager = _actor_manager;
    }
    private Transform transform { get { return actor_manager.transform; } }
    private GameObject gameObject { get { return actor_manager.gameObject; } }

    public void setLookat(Events.Looking _looking)
    {
        looking = _looking;
        looking.destination_progress = 1.0f;
        if (looking.character.name == gameObject.name) //A character looking at themselves is the same as removing lookat.
        {
            looking = null;
            return;
        }

        try
        {
            looking.looking_position = looking.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind").position - transform.position;//looking.character.transform.position - transform.position;
            looking_at = looking.character.name;
        }
        catch
        {
            //Debug.Log("failed to set look at " + e);
            looking = null;
        }
    }

    public void setTurnHeadAt(Events.Looking _turnHeadAt)
    {
        turning = _turnHeadAt;
        turning.destination_progress = 1.0f;
        try
        {
            turning.looking_position = turning.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind").position - transform.position;//looking.character.transform.position - transform.position;
        }
        catch
        {
            turning = null;
        }
    }

    public void clearLookat()
    {
        if (looking != null)
            looking.destination_progress = 0.0f;
        looking_at = null;
    }

    public void clearTurnHeadAt()
    {
        if (turning != null)
            turning.destination_progress = 0.0f;
    }

    public void ApplyHeadTurns()
    {
        if (looking != null)
        {
            if (looking.character == null)
            {
                looking = null;
                return;
            }
            looking.looking_position = looking.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind").position - transform.position;

            if (looking.progress < 1.0f && looking.progress < looking.destination_progress)
            {
                if (looking.progress <= 0.5f)
                {
                    looking.progress += 4f * Time.deltaTime * (looking.progress + 0.3f);
                }
                else
                {
                    looking.progress += 4f * Time.deltaTime * (1.0f - (looking.progress + 0.3f - 0.5f));
                }
            }

            if (looking.progress > 1.0f)
                looking.progress = 1.0f;

            else if (looking.progress > 0.0f && looking.progress > looking.destination_progress)
            {
                if (looking.progress >= 0.5f)
                {
                    looking.progress -= 4f * Time.deltaTime * (looking.progress + 0.3f);
                }
                else
                {
                    looking.progress -= 4f * Time.deltaTime * (1.0f - (looking.progress + 0.3f - 0.5f));
                }
            }

            if (looking.progress < 0.0f)
                looking.progress = 0.0f;

            char_a = gameObject.transform.Find("Armature/jt_all_bind/jt_hips_bind");

            if (char_a == null)
            {
                looking = null;
                return;
            }

            //jt hips bind

            Quaternion target_hips_direction = Quaternion.LookRotation(gameObject.transform.position + looking.looking_position - char_a.position).normalized;

            target_hips_direction = Quaternion.Euler(target_hips_direction.eulerAngles + new Vector3(0, 0, -90));

            target_hips_direction = Quaternion.LerpUnclamped(char_a.rotation, target_hips_direction, looking.progress / 6f);

            target_hips_direction = Quaternion.Euler(new Vector3(char_a.eulerAngles.x, target_hips_direction.eulerAngles.y, char_a.eulerAngles.z));

            char_a.eulerAngles = target_hips_direction.eulerAngles;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
                {
                    char_a = gameObject.transform.GetChild(i).Find("Armature/jt_all_bind/jt_hips_bind");
                    if (char_a != null)
                    {
                        char_a.eulerAngles = target_hips_direction.eulerAngles;
                    }
                }
            }

            //ROTATE SPINE 2

            char_a = gameObject.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind");

            if (char_a == null)
            {
                looking = null;
                return;
            }

            //char_b = looking.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind");
            Quaternion target_spine2_direction = Quaternion.LookRotation(gameObject.transform.position + looking.looking_position - char_a.position).normalized;

            target_spine2_direction = Quaternion.Euler(target_spine2_direction.eulerAngles + new Vector3(0, 0, -90));

            target_spine2_direction = Quaternion.LerpUnclamped(char_a.rotation, target_spine2_direction, looking.progress / 4.5f);

            target_spine2_direction = Quaternion.Euler(new Vector3(char_a.eulerAngles.x, target_spine2_direction.eulerAngles.y, char_a.eulerAngles.z));

            char_a.eulerAngles = target_spine2_direction.eulerAngles;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
                {
                    char_a = gameObject.transform.GetChild(i).Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind");
                    if (char_a != null)
                    {
                        char_a.eulerAngles = target_spine2_direction.eulerAngles;
                    }
                }
            }


            //ROTATE SPINE 3

            char_a = gameObject.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind");
            //char_b = looking.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind");
            if (char_a == null)
            {
                looking = null;
                return;
            }

            Quaternion target_spine3_direction = Quaternion.LookRotation(gameObject.transform.position + looking.looking_position - char_a.position).normalized;

            target_spine3_direction = Quaternion.Euler(target_spine3_direction.eulerAngles + new Vector3(0, 0, -90));

            target_spine3_direction = Quaternion.LerpUnclamped(char_a.rotation, target_spine3_direction, looking.progress / 4f);

            //target_spine3_direction = Quaternion.Euler(new Vector3(char_a.eulerAngles.x, target_spine3_direction.eulerAngles.y, char_a.eulerAngles.z));

            char_a.eulerAngles = target_spine3_direction.eulerAngles;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
                {
                    char_a = gameObject.transform.GetChild(i).Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind");
                    if (char_a != null)
                    {
                        char_a.eulerAngles = target_spine3_direction.eulerAngles;
                    }
                }
            }

            //ROTATE NECK
            char_a = gameObject.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");

            if (char_a == null)
            {
                looking = null;
                return;
            }

            //char_b = looking.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");

            Quaternion target_neck_direction = Quaternion.LookRotation(gameObject.transform.position + looking.looking_position - char_a.position).normalized;

            target_neck_direction = Quaternion.Euler(target_neck_direction.eulerAngles + new Vector3(20, 0, -90));

            target_neck_direction = Quaternion.LerpUnclamped(char_a.rotation, target_neck_direction, looking.progress / 1.3f);

            target_neck_direction = Quaternion.Euler(new Vector3(char_a.eulerAngles.x, target_neck_direction.eulerAngles.y, char_a.eulerAngles.z));

            char_a.eulerAngles = target_neck_direction.eulerAngles;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
                {
                    char_a = gameObject.transform.GetChild(i).Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");
                    if (char_a != null)
                    {
                        char_a.eulerAngles = target_neck_direction.eulerAngles;
                    }
                }
            }

            if (looking.progress <= 0.0f && looking.progress == 0.0f)
            {
                looking = null;
            }
        }

        if (turning != null)
        {
            if (turning.character == null)
            {
                turning = null;
                return;
            }
            Transform head_bind_pos = turning.character.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind");

            if (head_bind_pos != null) turning.looking_position = head_bind_pos.position - transform.position;

            if (turning.progress < 1.0f && turning.progress < turning.destination_progress)
            {
                if (turning.progress <= 0.5f)
                {
                    turning.progress += 4f * Time.deltaTime * (turning.progress + 0.3f);
                }
                else
                {
                    turning.progress += 4f * Time.deltaTime * (1.0f - (turning.progress + 0.3f - 0.5f));
                }
            }

            if (turning.progress > 1.0f)
                turning.progress = 1.0f;

            else if (turning.progress > 0.0f && turning.progress > turning.destination_progress)
            {
                if (turning.progress >= 0.5f)
                {
                    turning.progress -= 4f * Time.deltaTime * (turning.progress + 0.3f);
                }
                else
                {
                    turning.progress -= 4f * Time.deltaTime * (1.0f - (turning.progress + 0.3f - 0.5f));
                }
            }

            if (turning.progress < 0.0f)
                turning.progress = 0.0f;


            //ROTATE NECK

            char_a = gameObject.transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");
            //char_b = looking..transform.Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");
            if (char_a == null)
            {
                turning = null;
                return;
            }

            Quaternion target_neck_direction = Quaternion.LookRotation(gameObject.transform.position + turning.looking_position - char_a.position).normalized;

            target_neck_direction = Quaternion.Euler(target_neck_direction.eulerAngles + new Vector3(20, 0, -90));

            target_neck_direction = Quaternion.LerpUnclamped(char_a.rotation, target_neck_direction, turning.progress * 0.7f);

            char_a.eulerAngles = target_neck_direction.eulerAngles;
            char_a.localPosition = char_a.localPosition + new Vector3(0, 0, Mathf.Abs(target_neck_direction.eulerAngles.y * 0.008f));

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                if (gameObject.transform.GetChild(i).GetComponent<Animation>() != null)
                {
                    char_a = gameObject.transform.GetChild(i).Find("Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind");
                    if (char_a != null)
                    {
                        char_a.eulerAngles = target_neck_direction.eulerAngles;
                        char_a.localPosition = char_a.localPosition + new Vector3(0, 0, target_neck_direction.eulerAngles.y * 0.008f);
                    }
                }
            }

            if (turning.progress <= 0.0f && turning.progress == 0.0f)
            {
                turning = null;
            }
        }
    }
}
