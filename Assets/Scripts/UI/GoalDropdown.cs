using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UI
{
    class GoalDropdown : MonoBehaviour, IGoalDropdown
    {
        [SerializeField]
        protected Dropdown _goal_chain_dropdown;
        [SerializeField]
        protected Dropdown _goal_dropdown;
        public IQuestBox _quest_box;
        UnityEvent<string> _event_goal_dropdown_changed;
        [SerializeField]
        protected GameObject warning_popup;
        [SerializeField]
        protected TMPro.TMP_Text warning_text;

        Dictionary<string, string> dropdown_name_to_goalchainid = new Dictionary<string, string>();

        public void Awake()
        {
            _quest_box = GetComponent<IQuestBox>();
        }


        public void setup(UnityEvent<string> event_goal_dropdown_changed)
        {
            setGoalChainDropdown();
            _quest_box.setQuest(getCurrentGoalChainId());
            _event_goal_dropdown_changed = event_goal_dropdown_changed;
        }

        public void setGoalChainDropdown()
        {
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            if (GlobalEngineVariables.launch_mode == "tlsq")
            {
                var goal_chain_ids = Configs.config_master_tlsq.MasterTLSQ[GlobalEngineVariables.getTLSQName()].goalChainIds;
                var title_key = Configs.config_master_tlsq.MasterTLSQ[GlobalEngineVariables.getTLSQName()].title;
                var tlsq_name = Configs.config_local_data.LocalData[title_key].en_US;
                for (int i = 0; i < goal_chain_ids.Length; i++)
                {
                    var dropdown_name = tlsq_name + " Chapter " + (i + 1).ToString();
                    options.Add(new Dropdown.OptionData(dropdown_name));
                    dropdown_name_to_goalchainid.Add(dropdown_name, goal_chain_ids[i]);
                }
            }
            else
            {
                foreach (string goal_chain in GlobalEngineVariables.goal_chains.Keys)
                {
                    options.Add(new Dropdown.OptionData(goal_chain));

                }
            }
            _goal_chain_dropdown.AddOptions(options);
        }

        public void goalChainSelected()
        {
            _goal_dropdown.value = 0;
            setGoalDropdown();
            _quest_box.setQuest(getCurrentGoalChainId());
            _event_goal_dropdown_changed.Invoke(getCurrentGoalId());
        }

        public void goalSelected()
        {
            _event_goal_dropdown_changed.Invoke(getCurrentGoalId());
        }

        public void setGoalDropdown()
        {
            string goal_chain_name = getCurrentGoalChainId();
            if (goal_chain_name != null) 
            {
                _goal_dropdown.options.Clear();
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                for(int i = 0; i < Configs.config_goal_chain.GoalChain[goal_chain_name].goalIds.Count; i++)
                {
                    options.Add(new Dropdown.OptionData("Part " + (i + 1)));
                }

                if (Configs.config_goal_chain.GoalChain[goal_chain_name].classGoalIds != null)
                {
                    for (int i = 0; i < Configs.config_goal_chain.GoalChain[goal_chain_name].classGoalIds.Count; i++)
                        options.Add(new Dropdown.OptionData("Class " + (i + 1)));
                }

                if (Configs.config_goal_chain.GoalChain[goal_chain_name].assignments != null)
                {
                    for (int i = 0; i < Configs.config_goal_chain.GoalChain[goal_chain_name].assignments.Count; i++)
                        options.Add(new Dropdown.OptionData("Assignment " + (i + 1)));
                }

                _goal_dropdown.AddOptions(options);
            }
        }

        public string getCurrentGoalChainId()
        {
            if (_goal_chain_dropdown.options[_goal_chain_dropdown.value].text == "Select Chapter")
                return null;
            if (GlobalEngineVariables.launch_mode == "tlsq")
            {
                return dropdown_name_to_goalchainid[_goal_chain_dropdown.options[_goal_chain_dropdown.value].text];
            }
            string current_goal_chain = _goal_chain_dropdown.options[_goal_chain_dropdown.value].text;
            if (GlobalEngineVariables.goal_chains.ContainsKey(current_goal_chain))
            {
                return GlobalEngineVariables.goal_chains[current_goal_chain];
            }
            return null;
        }

        public string getCurrentGoalId()
        {
            string current_goal_chain_id = getCurrentGoalChainId();
            if (current_goal_chain_id == null)
                return null;

            var current_goal_chain = Configs.config_goal_chain.GoalChain[current_goal_chain_id];
            int goal_count = current_goal_chain.goalIds != null ? current_goal_chain.goalIds.Count : 0;
            int class_count = current_goal_chain.classGoalIds != null ? current_goal_chain.classGoalIds.Count : 0;
            //int assignment_count = current_goal_chain.assignments != null ? current_goal_chain.assignments.Count : 0;

            string goal;
            bool is_assignment = false;

            if (_goal_dropdown.value < goal_count)
            {
                goal = current_goal_chain.goalIds[_goal_dropdown.value][0];
            }
            else if (_goal_dropdown.value < goal_count + class_count)
            {
                goal = current_goal_chain.classGoalIds[_goal_dropdown.value - goal_count];
            }
            else
            {
                goal = current_goal_chain.assignments[_goal_dropdown.value - goal_count - class_count];
                is_assignment = true;
            }

            Configs.reference_tree = new ReferenceTree();
            if (!is_assignment)
            {
                ConfigGoal.getAllReferences(goal, ref Configs.reference_tree);
                List<string> missing_actions = new List<string>();
                foreach (var script_event in Configs.reference_tree.script_events)
                {
                    foreach (var action in Configs.config_script_events.ScriptEvents[script_event].action ?? Enumerable.Empty<string>())
                    {
                        if (action.Contains(":"))
                            continue;
                        if (EventActions.blacklisted_actions.Contains(action))
                            continue;
                        if (!EventActions.implemented_actions.Contains(action) && !missing_actions.Contains(action))
                        {
                            missing_actions.Add(action);
                        }
                    }
                }
                if (missing_actions.Count > 0)
                {
                    warning_popup.SetActive(true);
                    string text = "Warning: the selected goal uses the following event actions that are currently unimplemented: ";
                    foreach (var action in missing_actions)
                    {
                        text += action + "\n";
                    }
                    text += "There is a possibility that this may impact your experience.";
                    warning_text.text = text;
                    return goal;
                }

            }
            warning_popup.SetActive(false);
            return goal;
        }

        public int getCurrentGoalIndex(GoalChainType goalChainType)
        {
            ConfigGoalChain._GoalChain goal_chain = Configs.config_goal_chain.GoalChain[getCurrentGoalChainId()];

            switch (goalChainType)
            {
                case GoalChainType.Main:
                    return _goal_dropdown.value;
                case GoalChainType.Class:
                    if (goal_chain.goalIds != null)
                        return _goal_dropdown.value - goal_chain.goalIds.Count;
                    else
                        return _goal_dropdown.value;
                case GoalChainType.Assignment:
                    if (goal_chain.goalIds != null)
                    {
                        if (goal_chain.classGoalIds != null)
                        {
                            return _goal_dropdown.value - goal_chain.goalIds.Count - goal_chain.classGoalIds.Count;
                        }
                        else
                        {
                            return _goal_dropdown.value - goal_chain.goalIds.Count;
                        }
                    }
                    else if (goal_chain.classGoalIds != null)
                    {
                        return _goal_dropdown.value - goal_chain.classGoalIds.Count;
                    }
                    else
                    {
                        return _goal_dropdown.value;
                    }
            }

            throw new Exception("Unknown goal chain type");
        }

        public GoalChainType getCurrentGoalChainType()
        {
            ConfigGoalChain._GoalChain goal_chain = Configs.config_goal_chain.GoalChain[getCurrentGoalChainId()];
            int goal_index = _goal_dropdown.value;

            if (goal_chain.goalIds != null)
            {
                if (goal_index < goal_chain.goalIds.Count)
                {
                    return GoalChainType.Main;
                }
                if (goal_chain.classGoalIds != null)
                {
                    if (goal_index < (goal_chain.goalIds.Count + goal_chain.classGoalIds.Count))
                        return GoalChainType.Class;
                }
            }
            else if (goal_chain.classGoalIds != null)
            {
                if (goal_index < goal_chain.classGoalIds.Count)
                {
                    return GoalChainType.Class;
                }
            }
            return GoalChainType.Assignment;
        } 
    }
}
