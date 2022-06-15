using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
public class ConfigScriptEvents : Config<ConfigScriptEvents>
{
    [System.Serializable]
    public class ScriptEvent
    {
        public string eventId;
        public string type;
        public string[] action;
        public string[] param;
        public string[][] messageAndKeys;
        public float duration;
        public string shouldRun;
        public string[] sequenceIds;
    }
    public Dictionary<string, ScriptEvent> ScriptEvents;

    public override void combine(List<ConfigScriptEvents> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].ScriptEvents.Keys)
            {
                ScriptEvents[key] = other_list[i].ScriptEvents[key];
            }
        }
    }
}


class ConfigScriptEventsLoader
{

    public static async Task loadConfigsAsync()
    {
        Configs.config_script_events = await ConfigScriptEvents.getJObjectsConfigsList("ScriptEvents");
    }

}

