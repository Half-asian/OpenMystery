using Battlehub.RTHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Poser : MonoBehaviour
{
    public static Poser instance;
    public static event Action<bool> on_poser_enabled = delegate { };

    [SerializeField] PositionHandle position_handle;
    [SerializeField] RotationHandle rotation_handle;
    [SerializeField] ScaleHandle scale_handle;

    bool poser_enabled = false;

    string selected_actor = null;
    string selected_bone = null;
    public enum TransformGizmo
    {
        Translate,
        Rotate,
        Scale
    }
    TransformGizmo current_gizmo = TransformGizmo.Translate;


    private void Start()
    {
        instance = this;
        PoserUI.onActorSelectedEvent += OnActorSelected;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            poser_enabled = !poser_enabled;
            on_poser_enabled.Invoke(poser_enabled);
            if (poser_enabled == false)
            {
                position_handle.gameObject.SetActive(false);
                rotation_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(false);
            }
            else
            {
            }
        }

        if (!poser_enabled)
            return;


        processSelectedActor();


    }

    public void SpawnActor()
    {
        string actor_id = PoserUI.getSpawnActor();
        if (string.IsNullOrEmpty(actor_id) )
        {
            Debug.Log("string.IsNullOrEmpty(actor_id)");
            return;
        }
        if (!Configs.config_hp_actor_info.HPActorInfo.ContainsKey(actor_id))
        {
            Debug.Log("!Configs.config_hp_actor_info.HPActorInfo.ContainsKey(actor_id)");
            return;
        }
        int num = 0;
        while (Actor.getActor(actor_id + num.ToString()) != null)
            num++;

        var actor = Actor.spawnActor(actor_id, null, actor_id + num.ToString());
        actor.gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward - new Vector3(0, 0.5f, 0);
    }

    public void StopAnimating()
    {
        if (Actor.getActor(selected_actor))
            Actor.getActor(selected_actor).GetComponent<Animation>().enabled = false;
    }

    public void DestroyActor()
    {
        if (Actor.getActor(selected_actor))
        {
            Actor.destroyCharacter(selected_actor);
            selected_actor = null;
        }
    }


    private void OnActorSelected(string name)
    {
        selected_actor = name;
        selected_bone = null;
        spawnGizmo();
        if (name != null)
        {
            var actor = Actor.getActor(name);
            PoserUI.current.populateBoneDropwdown(actor.model.pose_bones.Keys.ToList());
        }
    }

    public void onBoneSelected(string bone)
    {
        selected_bone = bone;
        spawnGizmo();

    }

    private void processSelectedActor()
    {
        if (!Actor.getActor(selected_actor))
        {
            selected_actor = null;
            return;
        }

        spawnGizmo();
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current != null && current.gameObject != null && current.gameObject.GetComponent<TMP_InputField>() != null)
        {
            return;
        }
        if (selected_bone == null)
        {
            PoserUI.current.setTransformBox(Actor.getActor(selected_actor).transform);
        }
        else
        {
            PoserUI.current.setTransformBox(Actor.getActor(selected_actor).model.pose_bones[selected_bone].transform);

        }
    }

    public void changeTransform(string type, Vector3 value)
    {
        if (selected_actor != null)
        {
            if (selected_bone != null)
            {
                switch (type)
                {
                    case "position":
                        Actor.getActor(selected_actor).model.pose_bones[selected_bone].transform.position = value;
                        break;
                    case "rotation":
                        Actor.getActor(selected_actor).model.pose_bones[selected_bone].transform.eulerAngles = value;
                        break;
                    case "scale":
                        Actor.getActor(selected_actor).model.pose_bones[selected_bone].transform.localScale = value;
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case "position":
                        Actor.getActor(selected_actor).transform.position = value;
                        break;
                    case "rotation":
                        Actor.getActor(selected_actor).transform.eulerAngles = value;
                        break;
                    case "scale":
                        Actor.getActor(selected_actor).transform.localScale = value;
                        break;
                }
            }
        }
    }

    private void spawnGizmo()
    {
        if (selected_actor == null)
            return;
        var actor = Actor.getActor(selected_actor);
        Transform transform;
        if (selected_bone != null)
            transform = actor.model.pose_bones[selected_bone].transform;
        else 
            transform = actor.transform;
        switch (current_gizmo)
        {
            case TransformGizmo.Translate:
                position_handle.gameObject.SetActive(true);
                rotation_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(false);
                position_handle.transform.position = transform.position;
                position_handle.Targets = new Transform[] { transform };
                break;
            case TransformGizmo.Rotate:
                rotation_handle.gameObject.SetActive(true);
                position_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(false);
                rotation_handle.transform.position = transform.position;
                rotation_handle.Targets = new Transform[] { transform };
                break;
            case TransformGizmo.Scale:
                position_handle.gameObject.SetActive(false);
                rotation_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(true);
                scale_handle.transform.position = transform.position;
                scale_handle.Targets = new Transform[] { transform };
                break;
        }
    }

    public void changeTransformGizmo(TransformGizmo tool)
    {
        Debug.Log("changeTransformGizmo " + tool.ToString());
        current_gizmo = tool;
        if (selected_actor != null)
            spawnGizmo();

    }



}
