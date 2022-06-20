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
    public static async Task getConfig()
    {
        Configs.config_local_data = await getJObjectsConfigsListAsync("LocalData");
    }

    public static async Task getConfigAsync()
    {
        await Task.Run(() => {
            Configs.config_local_data = getJObjectsConfigsListST("LocalData");
        }
        );
    }
    public static async Task getConfigAsyncv2()
    {
        Configs.config_local_data = await getJObjectsConfigsListAsync("LocalData");
    }
    public static async Task loadJ()
    {
        Configs.config_local_data = await loadConfigType();
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
        Configs.config_predicate_alias = getJObjectsConfigsListST("PredicateAlias");
    }
}

