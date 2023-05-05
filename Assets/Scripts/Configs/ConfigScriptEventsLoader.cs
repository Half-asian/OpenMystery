using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public float? duration;
        public string shouldRun;
        public string[] sequenceIds;
        //Y6C2P3_borfRunSequence is an example of a crazy complicated coroutine
        public bool looping; //unimplemented
        public bool persisting;//unimplemented

        public float Duration => duration ?? 20.0f; //Tested
    }
    public Dictionary<string, ScriptEvent> ScriptEvents;
    ScriptEvent combineScriptEvents(ScriptEvent a, ScriptEvent b)
    {
        a.eventId = b.eventId ?? a.eventId;
        a.type = b.type ?? a.type;
        a.action = b.action ?? a.action;
        a.param = b.param ?? a.param;
        a.messageAndKeys = b.messageAndKeys ?? a.messageAndKeys;
        a.duration = b.duration ?? a.duration;
        a.shouldRun = b.shouldRun ?? a.shouldRun;
        a.sequenceIds = b.sequenceIds ?? a.sequenceIds;
        return a;
    }
    public override ConfigScriptEvents combine(List<ConfigScriptEvents> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].ScriptEvents.Keys)
            {
                if (ScriptEvents.ContainsKey(key))
                    ScriptEvents[key] = combineScriptEvents(ScriptEvents[key], other_list[i].ScriptEvents[key]);
                else
                    ScriptEvents[key] = other_list[i].ScriptEvents[key];
            }
        }
        return this;
    }
    public static ConfigScriptEvents getConfig()
    {
        string type = "ScriptEvents";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigScriptEvents> configs = getConfigList(type);
        configs[0].combine(configs);
        return configs[0];
    }

    public static void getAllReferences(string script_event_id, ref ReferenceTree reference_tree)
    {
        if (Configs.config_script_events.ScriptEvents.ContainsKey(VariantManager.getVariantForId(script_event_id)))
            script_event_id = VariantManager.getVariantForId(script_event_id);

        if (!reference_tree.script_events.Contains(script_event_id))
            reference_tree.script_events.Add(script_event_id);
        else
            return;
    }
}

