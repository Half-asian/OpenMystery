using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoserUI : MonoBehaviour
{
    public static PoserUI current;
    private Canvas canvas;
    [SerializeField]
    GameObject spawn_actor;
    void Start()
    {
        current = this;
        canvas = GetComponent<Canvas>();
        Poser.on_poser_enabled += toggleVisible;
        Poser.on_tool_changed += onToolChanged;
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
