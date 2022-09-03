using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.XR;

public abstract class ShaderAnimation
{
    public ShaderAnimation(string _mesh_id, string _value_id)
    {
        mesh_id = _mesh_id;
        value_id = _value_id;
    }
    public readonly string mesh_id;
    public readonly string value_id;
}

public class ShaderAnimationFloat : ShaderAnimation
{
    public ShaderAnimationFloat(
        string _mesh_id,
        string _value_id,
        float _start,
        float _start_value,
        float _end,
        float _end_value) : base(_mesh_id, _value_id)
    {
        Debug.Log("Adding new shaderanimationfloat start: " + _start + " end: " + _end);
        start = _start;
        start_value = _start_value;
        end = _end;
        end_value = _end_value;
    }

    public readonly float start;
    public readonly float start_value;
    public readonly float end;
    public readonly float end_value;
}

public static partial class AnimationManager
{
    static List<ShaderAnimation> processShaderAnimation(string shader_animation_id)
    {
        List<ShaderAnimation> list = new List<ShaderAnimation>();
        if (!Configs.config_shader_animation.ShaderAnimation.ContainsKey(shader_animation_id))
        {
            Debug.LogError("Shader animation " + shader_animation_id + " not found");
        }

        var shader_animation_config = Configs.config_shader_animation.ShaderAnimation[shader_animation_id];

        foreach(var mesh_id in shader_animation_config.data.Keys)
        {
            foreach (dynamic x in shader_animation_config.data[mesh_id].fltdata)
            {
                JProperty property = (JProperty)x;
                var obj = property.Value.ToObject<ConfigShaderAnimation._ShaderAnimation.TimeData[]>();

                foreach(var thingy in obj)
                {
                    ShaderAnimation shaderAnimation = new ShaderAnimationFloat(mesh_id, property.Name, thingy.start, thingy.startVal, thingy.end, thingy.endVal);
                    list.Add(shaderAnimation);
                }


            }
        }
        return list;
    }

    static List<ShaderAnimation> processEffects(ConfigAnimation._Animation3D.EffectInfo[] effectInfo)
    {
        List<ShaderAnimation> list = new List<ShaderAnimation>();

        foreach (var effect in effectInfo)
        {
            Debug.Log("ADDING EFFECT for " + effect.name);
            if (effect.fades == null)
            {
                Debug.LogError("Unknown effect");
                return new List<ShaderAnimation>();
            }

            foreach (var fade in effect.fades)
            {
                Debug.Log("ADDING EFFECT FADE");
                ShaderAnimation shaderAnimation = new ShaderAnimationFloat(effect.name, "alpha", fade.start, fade.startVal, fade.end, fade.endVal);
                list.Add(shaderAnimation);
            }
        }
        return list;
    }
}

