using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assignment
{
    public static List<Assignment> active_assignments = new List<Assignment>();

    ConfigAssignment._Assignment assignment_config;
    int active_objective_index = 0;
    public Objective active_objective;

    public static void startAssignment(string assignment_id, int starting_index = 0)
    {
        if (!Configs.config_assignment.Assignment.ContainsKey(assignment_id))
            throw new Exception("Tried to start invalid assignment with id " + assignment_id);

        ConfigAssignment._Assignment assignment_config = Configs.config_assignment.Assignment[assignment_id];
        startAssignment(assignment_config);
    }

    public static void startAssignment(ConfigAssignment._Assignment _assignment_config)
    {
        Debug.Log("Starting assignment: " + _assignment_config.id);
        Assignment assignment = new Assignment();
        assignment.assignment_config = _assignment_config;
        assignment.active_objective_index = 0;
        active_assignments.Add(assignment);
    }

    public void startIntroScenario()
    {
        Scenario.Activate(assignment_config.introScenario);
        Scenario.Load(assignment_config.introScenario);
        DialogueManager.onDialogueFinishedEventSecondary += onDialogueFinishedIntro;
    }

    public void onDialogueFinishedIntro(string dialogue_id)
    {
        if (dialogue_id != assignment_config.startDialogTrigger)
            return;
        DialogueManager.onDialogueFinishedEventSecondary -= onDialogueFinishedIntro;
        active_objective = Objective.startObjective(assignment_config.objectives[active_objective_index]);
        Objective.onObjectiveCompleted += onObjectiveComplete;
        GameStart.ui_manager.showNextButton();
    }

    public void loadCurrentObjectiveScenario()
    {
        var objective_config = Configs.config_objective.Objectives[assignment_config.objectives[active_objective_index]];
        Scenario.Load(objective_config.objectiveScenario);
    }

    public void onObjectiveComplete(string objective_id)
    {
        if (objective_id != assignment_config.objectives[active_objective_index])
            return;
        active_objective_index++;
        if (active_objective_index >= assignment_config.objectives.Length)
        {
            Objective.onObjectiveCompleted -= onObjectiveComplete;
            startOutroScenario();
            return;
        }
        active_objective = Objective.startObjective(assignment_config.objectives[active_objective_index]);
        GameStart.ui_manager.showNextButton();
    }

    public void startOutroScenario()
    {
        Scenario.Activate(assignment_config.outroScenario);
        Scenario.Load(assignment_config.outroScenario);
        DialogueManager.onDialogueFinishedEventSecondary += onDialogueFinishedOutro;
    }

    public void onDialogueFinishedOutro(string dialogue_id)
    {
        if (dialogue_id != assignment_config.finalizeDialogTrigger)
            return;
        DialogueManager.onDialogueFinishedEventSecondary -= onDialogueFinishedOutro;
        GameStart.ui_manager.showExitMenuButton();
    }
    
}
