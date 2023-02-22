using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour
{
    public Canvas canvas;

    public Text debug_text;

    void Update()
    {
        string text = "";

        if (Input.GetKeyDown("f3"))
        {
            canvas.enabled = !canvas.enabled; 
        }


        if (GoalChain.active_goalchains != null)
        {
            foreach (GoalChain goal_chain in GoalChain.active_goalchains)
                text += "\nGoalchain: " + goal_chain.goal_chain_config.id;

            foreach (Goal goal in Goal.active_goals)
                text += "\nGoal: " + goal.goal_config.goal_id;

            foreach (Objective objective in Objective.active_objectives)
            {
                text += "\n Objective ID: " + objective.objective_config.objective_id + "\n          message: " + objective.objective_config.message + "\n          keys: ";
                if (objective.objective_config.keys != null)
                {
                    foreach (string key in objective.objective_config.keys)
                    {
                        text += key + ", ";
                    }
                    text += "\n";
                }
            }                    
        }

        if (Scenario.current != null)
        {
            text += "\nScenario: " + Scenario.current.scenario_config.scenarioId;
            if (Scenario.current.objective != null)
            {
                text += "        Objective: " + Scenario.current.objective.objective_config.objective_id;
            }

            if (InteractionManager.active_interactions != null)
            {
                foreach (Interaction i in InteractionManager.active_interactions)
                {
                    text += "\n Interaction: " + i.name + " is_active: " + i.is_active;
                    if (i is InteractionGroup || i is InteractionAutotuneGroup)
                    {
                        text += " " + i.group_progress + "/" + i.config_interaction.ProgressRequired;
                    }
                }
            }

        }

        text += "\nDialogue Status: " + GameStart.dialogue_manager.dialogue_status.ToString();

        if (Project.project_config != null)
        {
            text += "\nProject: " + Project.project_config.projectId + " Progress: " + Project.current_progress + "/" + Project.getProgressNeeded();
        }

        if (Scene.current != null)
        {
            text += "\nScene: " + Scene.current.layoutId;
            if (Location.current != null)
                text += "\n Location: " + Location.current.locationId;
            else
                text += "\n Location: None";
            if (LocationHub.current != null)
                text += "\n Location Hub: " + LocationHub.current.hubId;
            else
                text += "\n Location Hub: None";

        }

        if (Location.activeScenarios != null && Location.activeScenarios.Count != 0)
        {
            text += "\n\nActive Scenarios: ";

            foreach (ConfigLocation._Location location in Location.activeScenarios.Keys)
            {
                foreach (Scenario scenario in Location.activeScenarios[location])
                {
                    text += "\n LocationId: " + location.locationId + " ScenarioId: " + scenario.scenario_config.scenarioId;
                }
            }
        }

        if (HubNPC.activeHubNPCs != null && HubNPC.activeHubNPCs.Count != 0)
        {
            text += "\n\nActive HUBNPCS: ";

            foreach(HubNPC hubnpc in HubNPC.activeHubNPCs.Values)
            {
                text += "\n HUBNPC " + hubnpc.config_hubnpc.hubNpcId + " Hub: " + hubnpc.config_hubnpc.hubId;
            }
        }

        debug_text.text = text;
    }
}
