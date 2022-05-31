using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace UI
{
    class TlsqBox : MonoBehaviour, IQuestBox
    {
        [SerializeField]
        private Text _tlsq_name;
        [SerializeField]
        private Text _tlsq_description;

        public void setQuest(string goal_chain_id)
        {
            ConfigTimeLimitedSideQuest._TimeLimitedSideQuest tlsq = Configs.config_time_limited_side_quest.TimeLimitedSideQuest[GlobalEngineVariables.tlsq_name];
            _tlsq_name.text = LocalData.getLine(tlsq.title);
            if (goal_chain_id == null)
            {
                _tlsq_description.text = LocalData.getLine(tlsq.introDescription);
            }
            else
            {
                ConfigGoalChain._GoalChain goal_chain = Configs.config_goal_chain.GoalChain[goal_chain_id];
                if (goal_chain.description != null)
                {
                    _tlsq_description.text = LocalData.getLine(goal_chain.description);
                }
            }
        }
    }
}
