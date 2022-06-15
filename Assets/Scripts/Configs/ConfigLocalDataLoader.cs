using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

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

    public override void combine(List<ConfigLocalData> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].LocalData.Keys)
            {
                LocalData[key] = other_list[i].LocalData[key];
            }
        }
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

    public override void combine(List<ConfigPredicateAlias> other_list)
    {
    }
}

class ConfigLocalDataLoader
{
    public static async Task loadConfigsAsync()
    {
        Configs.config_local_data = await ConfigLocalData.getJObjectsConfigsList("LocalData");

        foreach (ConfigLocalData._LocalData l in Configs.config_local_data.LocalData.Values)
        {
            if (l.es != null)
                l.en_US = l.es;
            else if (l.pt != null)
                l.en_US = l.pt;
        }

        List<ConfigPredicateAlias> list_predicate_alias = await ConfigPredicateAlias.getDeserializedConfigsList("PredicateAlias");
        Configs.config_predicate_alias = list_predicate_alias[0];
        Configs.predicate_alias_dict = new Dictionary<string, ConfigPredicateAlias._PredicateAlias>();
        foreach(ConfigPredicateAlias._PredicateAlias p in Configs.config_predicate_alias.PredicateAlias)
        {
            Configs.predicate_alias_dict[p.aliasId] = p;
        }


    }
}
