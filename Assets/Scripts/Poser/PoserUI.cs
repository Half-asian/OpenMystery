using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PoserUI : MonoBehaviour
{
    public static PoserUI current;
    public static event Action<string> onActorSelectedEvent = delegate { };
    private Canvas canvas;
    [SerializeField] GameObject actor_tool;
    [SerializeField] GameObject bone_tool;
    [SerializeField] GameObject base_panel;

    Dictionary<string, GameObject> actor_buttons;
    List<GameObject> bone_buttons = new List<GameObject>();

    [SerializeField] Button actor_tool_button;
    [SerializeField] Button bone_tool_button;

    [SerializeField] Button translate_button;
    [SerializeField] Button rotate_button;
    [SerializeField] Button scale_button;
    [SerializeField] Button spawn_button;
    [SerializeField] Button stop_animating_button;
    [SerializeField] Button destroy_button;
    [SerializeField] Transform actor_button_parent;
    [SerializeField] Transform bone_button_parent;
    [SerializeField] Text active_actor_text;
    [SerializeField] TMP_InputField pos_x;
    [SerializeField] TMP_InputField pos_y;
    [SerializeField] TMP_InputField pos_z;
    [SerializeField] TMP_InputField rot_x;
    [SerializeField] TMP_InputField rot_y;
    [SerializeField] TMP_InputField rot_z;
    [SerializeField] TMP_InputField sca_x;
    [SerializeField] TMP_InputField sca_y;
    [SerializeField] TMP_InputField sca_z;
    [SerializeField] SearchableDropDown actor_dropdown;
    void Start()
    {
        current = this;
        canvas = GetComponent<Canvas>();
        Poser.on_poser_enabled += toggleVisible;
        actor_buttons = new Dictionary<string, GameObject>();
        actor_tool_button.onClick.AddListener(showActorTool);
        bone_tool_button.onClick.AddListener(showBoneTool);
        translate_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Translate);});
        rotate_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Rotate); });
        scale_button.onClick.AddListener(delegate { Poser.instance.changeTransformGizmo(Poser.TransformGizmo.Scale); });
        spawn_button.onClick.AddListener(Poser.instance.SpawnActor);
        stop_animating_button.onClick.AddListener(Poser.instance.StopAnimating);
        destroy_button.onClick.AddListener(Poser.instance.DestroyActor);
        Actor.OnActorsModified += updateActorButtons;
        pos_x.onValueChanged.AddListener(delegate { changeTransform("position"); });
        pos_y.onValueChanged.AddListener(delegate { changeTransform("position"); });
        pos_z.onValueChanged.AddListener(delegate { changeTransform("position"); });
        rot_x.onValueChanged.AddListener(delegate { changeTransform("rotation"); });
        rot_y.onValueChanged.AddListener(delegate { changeTransform("rotation"); });
        rot_z.onValueChanged.AddListener(delegate { changeTransform("rotation"); });
        sca_x.onValueChanged.AddListener(delegate { changeTransform("scale"); });
        sca_y.onValueChanged.AddListener(delegate { changeTransform("scale"); });
        sca_z.onValueChanged.AddListener(delegate { changeTransform("scale"); });
        GameStart.onConfigsLoaded += setup;
    }

    void setup()
    {
        actor_dropdown.avlOptions = Configs.config_hp_actor_info.HPActorInfo.Keys.ToList();
    }

    public void populateBoneDropwdown(List<string> bones)
    {
        foreach(var old_button in bone_buttons)
        {
            Destroy(old_button);
        }
        bone_buttons.Clear();
        foreach(var b in bones) {
            var bone_button = Instantiate(Resources.Load("Poser/Button") as GameObject, bone_button_parent.transform);
            var button = bone_button.GetComponent<Button>();
            bone_button.GetComponentInChildren<TMPro.TMP_Text>().text = b;
            button.onClick.AddListener(delegate { Poser.instance.onBoneSelected(b); active_actor_text.text = "Target Bone: " + b;            });
            bone_buttons.Add(bone_button);
        }

        buttonSpace(bone_buttons);
    }

    private void buttonSpace(List<GameObject> buttons)
    {
        float y_offset = 0;
        float x_offset = 0;
        foreach (var button in buttons)
        {
            button.transform.localPosition = new Vector3(x_offset, y_offset, 0);
            y_offset -= 15;
            if (y_offset < -450)
            {
                y_offset = 0;
                x_offset += 100;
            }
        }
    }


    void showActorTool()
    {
        actor_tool.SetActive(true);
        bone_tool.SetActive(false);
    }
    void showBoneTool()
    {
        actor_tool.SetActive(false);
        bone_tool.SetActive(true);
    }

    void changeTransform(string type)
    {
        switch (type)
        {
            case "position":
                if (!float.TryParse(pos_x.text, out var px))
                    return;
                if (!float.TryParse(pos_y.text, out var py))
                    return;
                if (!float.TryParse(pos_z.text, out var pz))
                    return;
                Vector3 pos = new Vector3(px, py, pz);
                Poser.instance.changeTransform("position", pos);
                break;
            case "rotation":
                if (!float.TryParse(rot_x.text, out var rx))
                    return;
                if (!float.TryParse(rot_y.text, out var ry))
                    return;
                if (!float.TryParse(rot_z.text, out var rz))
                    return;
                Vector3 rot = new Vector3(rx, ry, rz);
                Poser.instance.changeTransform("rotation", rot);
                break;
            case "scale":
                if (!float.TryParse(sca_x.text, out var sx))
                    return;
                if (!float.TryParse(sca_y.text, out var sy))
                    return;
                if (!float.TryParse(sca_z.text, out var sz))
                    return;
                Vector3 sca = new Vector3(sx, sy, sz);
                Poser.instance.changeTransform("scale", sca);
                break;
        }



    }

    void updateActorButtons()
    {
        if (GameStart.current.model_inspector == true)
            return;
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

                var actor_button = Instantiate(Resources.Load("Poser/Button") as GameObject, actor_button_parent.transform);
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
        buttonSpace(actor_buttons.Values.ToList());
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



    public void setTransformBox(Transform transform)
    {
        pos_x.SetTextWithoutNotify(transform.position.x.ToString());
        pos_y.SetTextWithoutNotify(transform.position.y.ToString());
        pos_z.SetTextWithoutNotify(transform.position.z.ToString());
        rot_x.SetTextWithoutNotify(transform.eulerAngles.x.ToString());
        rot_y.SetTextWithoutNotify(transform.eulerAngles.y.ToString());
        rot_z.SetTextWithoutNotify(transform.eulerAngles.z.ToString());
        sca_x.SetTextWithoutNotify(transform.localScale.x.ToString());
        sca_y.SetTextWithoutNotify(transform.localScale.y.ToString());
        sca_z.SetTextWithoutNotify(transform.localScale.z.ToString());
    }

    void toggleVisible(bool enabled)
    {
        base_panel.SetActive(enabled);
    }

    public static string getSpawnActor()
    {
        return current.actor_tool.GetComponentInChildren<TMPro.TMP_InputField>().text;
    }
}
