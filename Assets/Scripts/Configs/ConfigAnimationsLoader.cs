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
        public string shaderAnimationId;
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

        [System.Serializable]
        public class Camerainfo
        {
            [System.Serializable]
            public class VerticalAOV
            {
                public float start;
                public float endVal;
                public float end;
                public float startVal;
            }
            public VerticalAOV[] verticalAOV;
        }
        public Camerainfo camerainfo;

        [System.Serializable]
        public class EffectInfo
        {
            [System.Serializable]
            public class FadeEffect
            {
                public float start;
                public float endVal;
                public float end;
                public float startVal;
            }
            public FadeEffect[] fades;
            //unknown what these do

            public float[][] fadeKeys;
            //public frames
            //public frameKeys
            public string name;
        }
        public EffectInfo[] effectInfo;
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
            public string exitAnim;
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
            public class _endEdge //Todo
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
                    public action[] actions;
                }
                public edge[] edges;
                public action[] entryActions;
                public action[] exitActions;
                public string animName;
                public string walkAnimName;
                public string type;
                public string nodeId;
                public bool blocking;
                public bool allowRootTransforms;
                public int minLoops; //unimplemented
                public int maxLoops;
            }
            public node[] nodes;

            public Dictionary<string, string> triggerReplacement; //TODO


        }
        public _data data;
        public string sequenceId;
        public string type;
        public bool useStagger; //If true, do not crossfade. Otherwise, char animation may not be synced up with prop animations
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

public class ConfigShaderAnimation : Config<ConfigShaderAnimation>
{
    [System.Serializable]
    public class _ShaderAnimation
    {
        [System.Serializable]
        public class TimeData
        {
            public float start;
            public float end;
            public float startVal;
            public float endVal;
        }


        [System.Serializable]
        public class FloatData
        {
            public TimeData[] u_opacity;
        }


        [System.Serializable]
        public class PartData
        {
            [JsonProperty(PropertyName = "float")]
            public dynamic fltdata;
        }

        public Dictionary<string, PartData> data;
        public float duration;
        public string name;
        public string wrapMode;

    }
    public Dictionary<string, _ShaderAnimation> ShaderAnimation;

    public override ConfigShaderAnimation combine(List<ConfigShaderAnimation> other_list)
    {
        throw new NotImplementedException();
    }
}