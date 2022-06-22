using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LocationHubMenu : MonoBehaviour
{

    static LocationHubMenu singleton;

    [SerializeField]
    private Button button_go;
    [SerializeField]
    private Button button_cancel;
    [SerializeField]
    private Dropdown dropdown;

    private void Awake()
    {
        singleton = this;
        loadButtons();
    }

    public void loadButtons()
    {
        dropdown.ClearOptions();
         
        List<string> options = new List<string>();

        foreach (ConfigLocationHub._LocationHub lh in Configs.config_location_hub.LocationHub.Values)
        {
            options.Add(lh.hubId);
        }

        dropdown.AddOptions(options);
    }

    public static void showMenu()
    {
        singleton.gameObject.SetActive(true);
    }

    public void goButtonClicked()
    {
        gameObject.SetActive(false);
        LocationHub.destroyLocationButtons();
        LocationHub.loadLocationHub(dropdown.options[dropdown.value].text);
    }

    public void closeMenu()
    {
        gameObject.SetActive(false);
    }
}
