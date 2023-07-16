using Battlehub.RTHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poser : MonoBehaviour
{
    public static Poser instance;
    public static event Action<bool> on_poser_enabled = delegate { };
    public static event Action<string> on_tool_changed = delegate { };
    public static event Action on_tool_used;

    [SerializeField] PositionHandle position_handle;
    [SerializeField] RotationHandle rotation_handle;
    [SerializeField] ScaleHandle scale_handle;

    bool first_enable = false;
    bool poser_enabled = false;

    string selected_actor = null;

    public enum TransformGizmo
    {
        Translate,
        Rotate,
        Scale
    }
    GameObject transform_gizmo_gameobject = null;
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
            if (!first_enable)
            {
                firstEnable();
                first_enable = true;
            }
            if (poser_enabled == false)
                Destroy(transform_gizmo_gameobject);
        }

        if (!poser_enabled)
            return;

        if (Input.GetMouseButtonDown(1)
            && Input.GetMouseButtonDown(0))
        {
            ActivateTool();
        }
        if (selected_actor == null)
        {
            if (transform_gizmo_gameobject != null)
                Destroy(transform_gizmo_gameobject);
        }
        else
            processSelectedActor();
    }

    void firstEnable()
    {
        on_tool_changed.Invoke("SpawnActor");
    }

    private void ActivateTool()
    {
        on_tool_used.Invoke();
        Debug.Log("ActivateTool");
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
        actor.GetComponent<Animation>().enabled = false;
        Debug.Log("Spawning actor " + actor_id + num.ToString());
    }

    private void OnActorSelected(string name)
    {
        if (transform_gizmo_gameobject)
        {
            Destroy(transform_gizmo_gameobject);
            transform_gizmo_gameobject = null;
        }
        selected_actor = name;
        spawnGizmo();
    }

    private void processSelectedActor()
    {
        if (!Actor.getActor(selected_actor))
        {
            selected_actor = null;
            return;
        }

        if (transform_gizmo_gameobject == null) {
            spawnGizmo();
        }
        else
        {
            /*switch (current_gizmo)
            {
                case TransformGizmo.Translate:
                    Actor.getActor(selected_actor).transform.position = transform_gizmo_gameobject.transform.position;
                    break;
                case TransformGizmo.Rotate:
                    Actor.getActor(selected_actor).transform.rotation = transform_gizmo_gameobject.transform.rotation;
                    break;
            }*/
        }
    }

    private void spawnGizmo()
    {
        if (transform_gizmo_gameobject != null)
            Destroy(transform_gizmo_gameobject);
        switch (current_gizmo)
        {
            case TransformGizmo.Translate:
                /*transform_gizmo_gameobject = GameObject.Instantiate(Resources.Load("Poser/PoserTranslate") as GameObject);
                transform_gizmo_gameobject.transform.position = Actor.getActor(selected_actor).transform.position;
                transform_gizmo_gameobject.transform.eulerAngles = Vector3.zero;*/
                position_handle.gameObject.SetActive(true);
                rotation_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(false);
                position_handle.transform.position = Actor.getActor(selected_actor).transform.position;
                position_handle.Targets = new Transform[] { Actor.getActor(selected_actor).transform };// transform_gizmo_gameobject.transform };
                break;
            case TransformGizmo.Rotate:
                /*transform_gizmo_gameobject = GameObject.Instantiate(Resources.Load("Poser/PoserRotate") as GameObject);
                transform_gizmo_gameobject.transform.position = Actor.getActor(selected_actor).transform.position;
                transform_gizmo_gameobject.transform.rotation = Actor.getActor(selected_actor).transform.rotation;*/
                rotation_handle.gameObject.SetActive(true);
                position_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(false);
                rotation_handle.transform.position = Actor.getActor(selected_actor).transform.position;
                rotation_handle.Targets = new Transform[] { Actor.getActor(selected_actor).transform };// transform_gizmo_gameobject.transform };
                break;
            case TransformGizmo.Scale:
                position_handle.gameObject.SetActive(false);
                rotation_handle.gameObject.SetActive(false);
                scale_handle.gameObject.SetActive(true);
                scale_handle.transform.position = Actor.getActor(selected_actor).transform.position;
                scale_handle.Targets = new Transform[] { Actor.getActor(selected_actor).transform };// transform_gizmo_gameobject.transform };
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
