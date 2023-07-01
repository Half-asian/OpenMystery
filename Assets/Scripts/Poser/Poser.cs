using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poser : MonoBehaviour
{
    public static Poser instance;
    public static event Action<bool> on_poser_enabled;
    public static event Action<string> on_tool_changed;
    public static event Action on_tool_used;

    bool first_enable = false;
    bool poser_enabled = false;

    private void Start()
    {
        instance = this;
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
        }

        if (Input.GetMouseButton(1)
            && Input.GetMouseButton(0))
        {
            ActivateTool();
        }

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

}
