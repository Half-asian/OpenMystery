using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LocationScenarioMenu : MonoBehaviour
{

    static LocationScenarioMenu singleton;

    [SerializeField]
    GameObject menu;
    [SerializeField]
    private Button button_go;
    [SerializeField]
    private Button button_cancel;
    [SerializeField]
    private Dropdown dropdown;

    LocationScenarioMenu()
    {
        singleton = this;
    }

    public void loadButtons(string location_id)
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();

        ConfigLocation._Location location = Configs.config_location.Location[location_id];

        if (location.defaultScenarios != null) //Pre-add these
        {
            foreach(var default_scenario in location.defaultScenarios)
            {
                if (Configs.config_scenario.Scenario[default_scenario].activeDefaultPredicate == null ||
                    Predicate.parsePredicate(Configs.config_scenario.Scenario[default_scenario].activeDefaultPredicate))
                {
                    options.Add(default_scenario);
                    break;
                }
            }
        }

        if (Location.activeScenarios.ContainsKey(location))
        {
            foreach (Scenario scenario in Location.activeScenarios[location])
            {
                options.Add(scenario.scenario_config.scenarioId);
            }
        }
        dropdown.AddOptions(options);
    }

    public static void showMenu(string location_id)
    {
        singleton.menu.SetActive(true);
        singleton.loadButtons(location_id);
    }

    public void goButtonClicked()
    {
        menu.SetActive(false);
        LocationHub.destroyLocationButtons();
        Scenario.Load(dropdown.options[dropdown.value].text); //Load the scenario. Default scenarios will create new temp scenarios, otherwise will load pre activated scenario.
    }

    public void closeMenu()
    {
        menu.SetActive(false);
    }
}
