using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;


public class ConfigInteraction : Config<ConfigInteraction>
{
    [System.Serializable]
    public class Interaction
    {
        public string id;
        public string dialogId;
        public string filterPredicate;
        public string groupId;
        public string spot;
        public string projectId;
        public string encounterId;
        public string scenarioId;
        public string hudDialogSpeaker;
        public string endHudDialog;
        public string matchId;
        public string successReward;
        public int maxToShow;
        public int minToShow;
        public int maxProgress;
        public int groupProgress;
        public int projectProgress;
        public int progressRequired = 9999;
        public bool autoSelect;
        public string type;
        public string[] groupMembers;
        public string[] leadsTo;
        public string[] leadsToPredicate;
        public string[] loctags;
        public string[] enterEvents;
        public string[] exitEvents;
        public string[] qteSuccessEvents;
        public string[] qteFailEvents;
        public string[] successEvents;
        public string[] failEvents; //unknown how these are triggered. possibly a mistake
    }

    public Dictionary<string, Interaction> Interactions;

    public override ConfigInteraction combine(List<ConfigInteraction> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Interactions.Keys)
            {
                Interactions[key] = other_list[i].Interactions[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_interaction = getJObjectsConfigsListST("Interactions");
    }

    public static async Task getConfigAsyncv2()
    {
        Configs.config_interaction = await getJObjectsConfigsListAsync("Interactions");
    }

    public static async Task getConfigAsync()
    {
        await Task.Run(() => {
            Configs.config_interaction = getJObjectsConfigsListST("Interactions");
        }
        );
    }
    public static async Task loadJ()
    {
        Configs.config_interaction = await loadConfigType();
    }
}
