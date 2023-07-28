using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
public class ConfigLocalData : Config<ConfigLocalData>
{
    [System.Serializable]
    public class _LocalData
    {
        public string en_US;
        public string es;
        public string pt;
    }

    public Dictionary<string, _LocalData> LocalData;

    //Hard overwrite
    public override ConfigLocalData combine(List<ConfigLocalData> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].LocalData.Keys)
            {
                LocalData[key] = other_list[i].LocalData[key];
            }
        }
        return this;
    }

    public static ConfigLocalData getConfig()
    {
        string type = "LocalData";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigLocalData> configs = getConfigList(type);
        configs[0].combine(configs);
        return configs[0];
    }

}

public class ConfigPredicateAlias : Config<ConfigPredicateAlias>
{
    [System.Serializable]
    public class _PredicateAlias
    {
        public string aliasId;
        public string aliasedPredicate;
    }
    public _PredicateAlias[] PredicateAlias;

    public override ConfigPredicateAlias combine(List<ConfigPredicateAlias> other_list)
    {
        throw new Exception();
    }
    public static void getConfig()
    {
        Configs.config_predicate_alias = getJObjectsConfigsListST("PredicateAlias", Newtonsoft.Json.Linq.MergeArrayHandling.Concat);
    }
}

