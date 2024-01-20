using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UiManager : MonoBehaviour
{

    private UI.IGoalDropdown _goal_dropdown;
    private UI.GoalPopup _goal_popup;
    private UiImageLoader _ui_image_loader;

    public GameObject goal_popup_gameobject;
    public GameObject tlsq_menu_gameobject;
    public GameObject goal_dropdown_gameobject;

    public GameObject settings_menu;

    public GameObject next_area_button;

    public Text please_wait_text;
    public Text press_space_text;

    [SerializeField]
    GameObject exit_to_menu_button;
    [SerializeField]
    GameObject pause_menu;
    [SerializeField]
    GameObject main_menu;

    [SerializeField]
    bool should_show_exit = false;
    [SerializeField]
    bool should_show_popup = false;
    [SerializeField]
    bool should_show_next = false;

    UnityEvent<string> _event_goal_dropdown_changed;

    private void Awake()
    {
        _goal_dropdown = GetComponent<UI.IGoalDropdown>();
        _goal_popup = GetComponent<UI.GoalPopup>();
        _event_goal_dropdown_changed = new UnityEvent<string>();
        _event_goal_dropdown_changed.AddListener(showPopup);
        _ui_image_loader = GetComponent<UiImageLoader>();

    }

    private void checkRemovePopup()
    {
        var current_goal = Goal.getGoalById(_goal_popup.latest_goal.goal_id);
        var current_assignment = Assignment.active_assignments.Count > 0 ? Assignment.active_assignments[0] : null;
        if (current_goal == null && current_assignment == null)
        {
            return;
        }
        string current_ob_scenario;

        if (current_goal != null)
            current_ob_scenario = current_goal.active_objective.objective_config.objectiveScenario;
        else
        {
            if (current_assignment.active_objective == null)
            {
                should_show_next = false;
                return;
            }
            current_ob_scenario = current_assignment.active_objective.objective_config.objectiveScenario;
        }

        if (Scenario.current == null || Scenario.current.scenario_config == null)
        {
            return;
        }

        if (current_ob_scenario == null && current_goal.active_objective.objective_config.objectiveHubNpcs == null)
        {
            if (Scenario.current.scenario_config != null)
                if (Scenario.current.scenario_config.scenarioId == "NUX_TrainScene")
                {
                    should_show_popup = false;
                    should_show_next = false;
                }

            return;
        }

        if (Scenario.current.scenario_config.scenarioId == current_ob_scenario)
        {
            should_show_popup = false;
            should_show_next = false;
        }
        if (LocationHub.current != null && current_goal.active_objective.objective_config.objectiveHubNpcs != null) {
            bool hub_npcs_found = false;
            foreach (var hubnpc in current_goal.active_objective.objective_config.objectiveHubNpcs)
            {
                if (Configs.config_hub_npc.HubNPC[hubnpc].hubId == LocationHub.current.hubId)
                {
                    hub_npcs_found = true;
                }
            }
            if (hub_npcs_found == true)
            {
                should_show_popup = false;
                should_show_next = false;
            }
        }
        
    }

    private void Update()
    {
        checkRemovePopup();
        if (should_show_popup)
        {
            if (DialogueManager.in_dialogue || EventManager.are_events_active || LoadingScreenCanvas.is_loading)
                goal_popup_gameobject.SetActive(false);
            else
                goal_popup_gameobject.SetActive(true);
        }
        else
            goal_popup_gameobject.SetActive(false);

        if (should_show_exit)
        {
            if (DialogueManager.in_dialogue || EventManager.are_events_active || LoadingScreenCanvas.is_loading)
                exit_to_menu_button.SetActive(false);
            else
                exit_to_menu_button.SetActive(true);
        }
        else
            exit_to_menu_button.SetActive(false);

        if (should_show_next)
        {
            if (DialogueManager.in_dialogue || EventManager.are_events_active || LoadingScreenCanvas.is_loading)
                next_area_button.SetActive(false);
            else
                next_area_button.SetActive(true);
        }
        else
            next_area_button.SetActive(false);
    }

    public void showExitMenuButton()
    {
        should_show_exit = true;
    }

    public void showNextButton()
    {
        should_show_next = true;
    }

    public void showPopup(string goal_id)
    {
        if (goal_id == null)
            return;
        if (Configs.config_goal.Goals.TryGetValue(goal_id, out var goal))
        {
            _goal_popup.setPopup(goal);
        }
        else if (Configs.config_assignment.Assignment.TryGetValue(goal_id, out var assignment))
        {
            _goal_popup.setPopup(assignment);
        }
        else
        {
            throw new Exception("Failed to show popup due to invalid goal id that wasn't a goal or assignment");
        }
        should_show_popup = true;
    }

    public void showPopup(ConfigGoal.Goal goal) => showPopup(goal.goal_id);
    public void setup()
    {
        StartCoroutine(_ui_image_loader.setup());
    }

    public void setMenu()
    {
        _goal_dropdown.setup(_event_goal_dropdown_changed);
        _goal_popup.closePopup();
    }

    public void nextAreaButtonClicked()
    {
        LoadingScreenCanvas.current.showImage();
        GameStart.ui_manager.next_area_button.SetActive(false);
        StartCoroutine(nextAreaButtonClickedDelay());
    }

    IEnumerator nextAreaButtonClickedDelay()
    {
        yield return null;
        if (Assignment.active_assignments.Count == 0)
        {
            Goal g = Goal.getGoalById(_goal_popup.latest_goal.goal_id);
            g.loadObjectiveScenario();
        }
        else
        {
            Assignment assignment = Assignment.active_assignments[0];
            assignment.loadCurrentObjectiveScenario();
        }
    }


    public void exitToMenuButtonClicked()
    {
        GameStart.ui_manager.exit_to_menu_button.SetActive(false);
        GameStart.current.GetComponent<GameStart>().cleanUp();
        GameStart.ui_manager.pause_menu.SetActive(false);
        GameStart.ui_manager.main_menu.SetActive(true);
        should_show_exit = false;
        should_show_popup = false;
        ScreenFade.fadeFrom(1, Color.clear, true);
        StartCoroutine(fadeBackup());
    }

    IEnumerator fadeBackup()
    {
        yield return new WaitForSeconds(1);
        ScreenFade.fadeFrom(0.001f, Color.clear, true);
    }

    public void settingsButtonClicked()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    public void chapterSelectButtonClicked()
    {
        //always use this for now
        //if (GlobalEngineVariables.launch_mode == "tlsq")
        tlsq_menu_gameobject.SetActive(!tlsq_menu_gameobject.activeSelf);
        goal_popup_gameobject.SetActive(!goal_popup_gameobject.activeSelf);
        goal_dropdown_gameobject.SetActive(!goal_dropdown_gameobject.activeSelf);
    }

    public void startQuestButtonClicked()
    {
        string goal_chain = _goal_dropdown.getCurrentGoalChainId();
        GoalChainType goal_chain_type = _goal_dropdown.getCurrentGoalChainType();
        int goal_index = _goal_dropdown.getCurrentGoalIndex(goal_chain_type);
        if (goal_chain != null) GameObject.Find("MainMenuCanvas").GetComponent<MainMenu>().enterGoalID(goal_chain, goal_index, goal_chain_type);
        _goal_popup.closePopup();
        should_show_popup = false;
        should_show_next = false;
    }




}

namespace UI
{
    interface IGoalDropdown
    {
        void setup(UnityEvent<string> event_goal_dropdown_changed);
        string getCurrentGoalChainId();
        string getCurrentGoalId();
        int getCurrentGoalIndex(GoalChainType type);
        GoalChainType getCurrentGoalChainType();
    }

    interface IQuestBox
    {
        void setQuest(string goal_chain_id);
    }
}

