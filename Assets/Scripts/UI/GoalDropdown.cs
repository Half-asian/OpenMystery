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
            foreach (string goal_chain in GlobalEngineVariables.goal_chains.Keys)
            {
                options.Add(new Dropdown.OptionData(goal_chain));

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

            if (_goal_dropdown.value < goal_count)
            {
                return current_goal_chain.goalIds[_goal_dropdown.value][0];
            }
            else if (_goal_dropdown.value < goal_count + class_count)
            {
                return current_goal_chain.classGoalIds[_goal_dropdown.value - goal_count];
            }
            else
            {
                return current_goal_chain.assignments[_goal_dropdown.value - goal_count - class_count];
            }
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
