using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigAnimation : Config<ConfigAnimation>
{
    [System.Serializable]
    public class _Animation3D
    {
        public string animName;
        public string name;
        public string fileName;
        public string introAnim;
        public string outroAnim;
        public int rigVersion;
        public string wrapMode;

        [System.Serializable]
        public class TriggerMap
        {
            public int prob; //unimplemented
            [JsonProperty(PropertyName = "params")]
            public string[] parameters;
            public string id;
            public float time;
        }
        public TriggerMap[] triggerMap;

    }
    public Dictionary<string, _Animation3D> Animation3D;

    public override ConfigAnimation combine(List<ConfigAnimation> other_list)
    {
        throw new NotImplementedException();
    }
}

public class ConfigCharAnimSequence : Config<ConfigCharAnimSequence>
{
    [System.Serializable]
    public class _CharAnimSequence
    {
        [System.Serializable]
        public class _data
        {
            [System.Serializable]
            public class action
            {
                public string type;
                public string id;
                public string target;
                public bool unowned;
                public string alias;
            }
            [System.Serializable]
            public class _startEdge
            {
                public string destinationId;
                public int weight;
                public action[] actions;
            }
            public _startEdge startEdge;

            [System.Serializable]
            public class _endEdge
            {
                public action[] actions;
            }
            [System.Serializable]
            public class node
            {
                [System.Serializable]
                public class edge
                {
                    public string destinationId;
                    public int weight;
                }
                public edge[] edges;
                public action[] entryActions;
                public string animName;
                public string walkAnimName;
                public string type;
                public string nodeId;
                public bool blocking;
                public bool allowRootTransforms;
                public int minLoops; //unimplemented
                public int maxLoops; //unimplemented
            }
            public node[] nodes;

            public Dictionary<string, string> triggerReplacement; //TODO


        }
        public _data data;
        public string sequenceId;
        public string type;
        public bool useStagger;
        public bool isOneShot;
    }

    public Dictionary<string, _CharAnimSequence> CharAnimSequence;

    public override ConfigCharAnimSequence combine(List<ConfigCharAnimSequence> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].CharAnimSequence.Keys)
            {
                CharAnimSequence[key] = other_list[i].CharAnimSequence[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_char_anim_sequence = getJObjectsConfigsListST("CharAnimSequence");
    }
}

