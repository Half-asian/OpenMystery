using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

public abstract class Objective
{
    public static event Action<string> onObjectiveCompleted = delegate { };
    public static List<Objective> active_objectives = new List<Objective>();

    public ConfigObjective.Objective objective_config;

    protected int keys_completed = 0;
    protected List<string> keys;

    public static void Initialize()
    {
        GameStart.onReturnToMenu += cleanup;
    }

    public static void cleanup()
    {
        active_objectives = new List<Objective>();
    }


    public static Objective startObjective(string objective_id)
    {
        if (!Configs.config_objective.Objectives.ContainsKey(objective_id))
            throw new Exception("Tried to start invalid objective with id " + objective_id);

        ConfigObjective.Objective objective_config = Configs.config_objective.Objectives[objective_id];
        return startObjective(objective_config);
    }

    public static Objective startObjective(ConfigObjective.Objective objective)
    {
        Debug.Log("Activating objective: " + objective.objective_id);
        Objective new_objective;
        switch (objective.message)
        {
            case "interactionComplete":
                new_objective = new ObjectiveInteractionComplete(objective);
                break;
            case "dialogueComplete":
                new_objective = new ObjectiveDialogueComplete(objective);
                break;
            case "matchComplete":
                new_objective = new ObjectiveMatchComplete(objective);
                break;
            case "projectSuccess":
                new_objective = new ObjectiveProjectSuccess(objective);
                break;
            case "repeatableMatchComplete":
                new_objective = new ObjectiveRepeatableMatchComplete(objective);
                break;
            case "Y1C1P6Complete":
                new_objective = new ObjectiveY1C1P6Complete(objective);
                break;
            case "outfitChanged":
                new_objective = new ObjectiveOutfitChanged(objective);
                break;
            case "friendTutComplete":
                new_objective = new ObjectiveFriendTutComplete(objective);
                break;
            case "visitLocation":
                new_objective = new ObjectiveVisitLocation(objective);
                break;
            case "socialEncounterComplete":
                new_objective = new ObjectiveSocialEncounterComplete(objective);
                break;
            case "earnedAttributeXp":
                new_objective = new ObjectiveEarnedAttributeXp(objective);
                break;
            case "creatureFed":
                new_objective = new ObjectiveCreatureFed(objective);
                break;
            case "earnParentProjectStars":
                new_objective = new ObjectiveEarnParentProjectStars(objective);
                break;
            case null:
                new_objective = new ObjectiveBlank(objective);
                break;
            default:
                throw new System.Exception("Objective type not implemented " + objective.message);
        }

        if (new_objective.objective_config.objectiveHubNpcs != null)
        {
            foreach (string hubNpcId in new_objective.objective_config.objectiveHubNpcs)
            {
                HubNPC.addHubNPC(hubNpcId);
            }
        }

        if (new_objective.objective_config.total_function != null)
        {
            GameStart.onUpdate += new_objective.UpdateTotalFunction;
        }

        Assert.IsNotNull(new_objective.objective_config);
        active_objectives.Add(new_objective);
        new_objective.activateScenarioIfValid();
        return new_objective;
    }

    //Total functions are another way of completing an objective.
    //It can be attached to any objective type.
    public void UpdateTotalFunction()
    {
        if (NewPredicate.parsePredicate(objective_config.total_function))
        {
            GameStart.onUpdate -= UpdateTotalFunction;
            objectiveCompleted();
        }
    }

    public void objectiveCompleted()
    {
        Debug.Log("Objective completed: " + objective_config.objective_id);
        active_objectives.Remove(this);

        if (objective_config.objectiveHubNpcs != null)
        {
            foreach (string hubNpcId in objective_config.objectiveHubNpcs)
            {
                HubNPC.removeHubNPC(hubNpcId);
            }
        }

        onObjectiveCompleted.Invoke(objective_config.objective_id);

        if (objective_config.restartScenarioOnComplete)
        {
            Scenario.restartScenario();
        }

    }

    public virtual void activateScenarioIfValid()
    {
        if (objective_config.objectiveScenario != null)
        {
            Scenario.Activate(objective_config.objectiveScenario, this);
        }
    }

    public virtual void LoadScenarioIfValid()
    {
        if (objective_config.objectiveScenario != null)
        {
            Scenario.Load(objective_config.objectiveScenario);
        }
        else if (objective_config.objectiveHubNpcs != null)
        {
            ConfigHubNPC._HubNPC first_npc = Configs.config_hub_npc.HubNPC[objective_config.objectiveHubNpcs[0]];
            LocationHub.loadLocationHub(first_npc.hubId);
        }
        else
        {
            throw new Exception("Couldn't find a scenario to start from objective " + objective_config.objective_id);
        }
    }
}

