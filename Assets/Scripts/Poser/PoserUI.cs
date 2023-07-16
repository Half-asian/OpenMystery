using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PoserUI : MonoBehaviour
{
    public static PoserUI current;
    public static event Action<string> onActorSelectedEvent = delegate { };

    private Canvas canvas;
    [SerializeField]
    GameObject spawn_actor;
    [SerializeField]
    Vector3 actors_list_pos;
    Dictionary<string, GameObject> actor_buttons;
    [SerializeField]
    Text active_actor_text;

    [SerializeField] Button translate_button;
    [SerializeField] Button rotate_button;
    [SerializeField] Button scale_button;

    void Start()
    {
        current = this;
        canvas = GetComponent<Canvas>();
        Poser.on_poser_enabled += toggleVisible;
        Poser.on_tool_changed += onToolChanged;
        actor_buttons = new Dictionary<string, GameObject>();
        translate_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Translate);});
        rotate_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Rotate); });
        scale_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Scale); });
    }

    private void Update()
    {
        updateActorButtons();
    }

    void updateActorButtons()
    {
        List<string> unhit_keys = actor_buttons.Keys.ToList();


        if (Actor.actor_controllers != null)
        {
            foreach (var controller in Actor.actor_controllers.Values)
            {
                if (unhit_keys.Contains(controller.name))
                {
                    unhit_keys.Remove(controller.name);
                    continue; //button already available
                }

                var actor_button = Instantiate(Resources.Load("Poser/Button") as GameObject, spawn_actor.transform);
                var button = actor_button.GetComponent<Button>();
                actor_button.GetComponentInChildren<TMPro.TMP_Text>().text = controller.name;
                button.onClick.AddListener(delegate { onActorSelected(controller.name); });
                actor_buttons[controller.name] = (actor_button);
            }
        }

        foreach (var key in unhit_keys)
        {
            Destroy(actor_buttons[key].gameObject);
            actor_buttons.Remove(key);
        }

        float offset = 0f;
        foreach(var button in actor_buttons.Values)
        {
            button.transform.position = Vector3.down * offset + actors_list_pos;
            offset += 200.0f;
        }
    }

    void onActorSelected(string name)
    {
        onActorSelectedEvent.Invoke(name);
        if (Actor.getActor(name) != null)
        {
            active_actor_text.text = "Target Actor: " + name;
        }
        else
        {
            active_actor_text.text = "Target Actor: null";
        }
    }

    void toggleVisible(bool enabled)
    {
        canvas.enabled = enabled;
    }


    void onToolChanged(string tool)
    {
        switch(tool)
        {
            case "SpawnActor":
                spawnActor();
                break;
        }
    }

    void spawnActor()
    {
        spawn_actor.SetActive(true);
        var dropdown = spawn_actor.GetComponentInChildren<SearchableDropDown>();
        dropdown.avlOptions = Configs.config_hp_actor_info.HPActorInfo.Keys.ToList();
    }

    public static string getSpawnActor()
    {
        return current.spawn_actor.GetComponentInChildren<TMPro.TMP_InputField>().text;
    }
}
