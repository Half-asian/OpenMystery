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

    public GameObject exit_to_menu_button;
    public GameObject pause_menu;
    public GameObject main_menu;

    UnityEvent<ConfigGoal.Goal> _event_goal_dropdown_changed;

    private void Awake()
    {
        _goal_dropdown = GetComponent<UI.IGoalDropdown>();
        _goal_popup = GetComponent<UI.GoalPopup>();
        _event_goal_dropdown_changed = new UnityEvent<ConfigGoal.Goal>();
        _event_goal_dropdown_changed.AddListener(showPopup);
        _ui_image_loader = GetComponent<UiImageLoader>();
        DialogueManager.onDialogueStartedEvent += hideShit;
    }

    private void hideShit()
    {
        if (_goal_popup.gameObject.activeSelf)
        {
            _goal_popup.closePopup();
            if (Goal.getGoalById(_goal_popup.latest_goal.goal_id) == null)
                DialogueManager.onDialogueFinishedEventPrimary += showPopup;
        }
        if (GameStart.ui_manager.next_area_button.activeSelf)
        {
            GameStart.ui_manager.next_area_button.SetActive(false);
            DialogueManager.onDialogueFinishedEventPrimary += showNextAreaButton;
        }
        if (GameStart.ui_manager.exit_to_menu_button.activeSelf)
        {
            GameStart.ui_manager.exit_to_menu_button.SetActive(false);
            DialogueManager.onDialogueFinishedEventPrimary += showExitMenuButton;
        }
    }

    private void showNextAreaButton(string rubbish)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= showNextAreaButton;
        GameStart.ui_manager.next_area_button.SetActive(true);
    }

    private void showExitMenuButton(string rubbish)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= showExitMenuButton;
        GameStart.ui_manager.exit_to_menu_button.SetActive(true);
    }

    public void setup()
    {
        StartCoroutine(_ui_image_loader.setup());
    }

    public void setMenu()
    {
        _goal_dropdown.setup(_event_goal_dropdown_changed);
        _goal_popup.closePopup();
    }
    public void exitPopup()
    {

        GameStart.ui_manager.next_area_button.SetActive(false);
        GameStart.ui_manager.exit_to_menu_button.SetActive(true);
    }

    public void nextAreaButtonClicked()
    {
        LoadingScreenCanvas.current.showImage();
        GameStart.ui_manager.next_area_button.SetActive(false);
        closePopup();
        StartCoroutine(nextAreaButtonClickedDelay());
    }

    IEnumerator nextAreaButtonClickedDelay()
    {
        yield return null;
        Goal g = Goal.getGoalById(_goal_popup.latest_goal.goal_id);
        g.loadObjectiveScenario();
    }


    public void exitToMenuButtonClicked()
    {
        GameStart.ui_manager.exit_to_menu_button.SetActive(false);
        GameStart.current.GetComponent<GameStart>().cleanUp();
        GameStart.ui_manager.pause_menu.SetActive(false);
        GameStart.ui_manager.main_menu.SetActive(true);
        ScreenFade.fadeFrom(0.001f, Color.clear, true);
    }

    public void settingsButtonClicked()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    public void chapterSelectButtonClicked()
    {
        if (GlobalEngineVariables.launch_mode == "tlsq")
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
    }

    public void showPopup(string rubbish)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= showPopup;
        _goal_popup.setPopup(_goal_popup.latest_goal);
    }
    public void showPopup(ConfigGoal.Goal goal_id)
    {
        _goal_popup.setPopup(goal_id);
    }

    public void closePopup()
    {
        _goal_popup.closePopup();
    }


}

namespace UI
{
    interface IGoalDropdown
    {
        void setup(UnityEvent<ConfigGoal.Goal> event_goal_dropdown_changed);
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

