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
        private TMPro.TMP_Text _tlsq_description;

        public void setQuest(string goal_chain_id)
        {
            if (GlobalEngineVariables.launch_mode == "tlsq")
            {
                ConfigMasterTLSQ._MasterTLSQ tlsq = Configs.config_master_tlsq.MasterTLSQ[GlobalEngineVariables.tlsq_name];
                _tlsq_name.text = LocalData.getLine(tlsq.title);

                if (goal_chain_id == null)
                {
                    _tlsq_description.text = LocalData.getLine(tlsq.introDescription);
                    _tlsq_description.text = _tlsq_description.text.Replace("\\n", "\n");
                }
                else
                {
                    ConfigGoalChain._GoalChain goal_chain = Configs.config_goal_chain.GoalChain[goal_chain_id];
                    if (goal_chain.description != null)
                    {
                        _tlsq_description.text = LocalData.getLine(goal_chain.description);
                        _tlsq_description.text = _tlsq_description.text.Replace("\\n", "\n");
                    }
                }

            }
            else {
                _tlsq_name.text = "";
                _tlsq_description.text = "";
            }

        }
    }
}
