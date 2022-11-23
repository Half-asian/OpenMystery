using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public abstract partial class Node : MonoBehaviour
{
    public event Action<string> onAnimationFinished = delegate { };

    public Model model;

    public Animation animation_component;

    IEnumerator currentAnimationAlerter;
    public HPAnimation idle;

    Dictionary<string, SkinnedMeshRenderer> mesh_renderers;

    bool anim_flip_flop = false;

    public HPAnimation queued_anim = null;
    public float queued_anim_fade_time = 0.4f;

    public bool reset_animation = true;

    private void Update()
    {
        if (queued_anim != null && reset_animation == false)
            playAnimationOnComponent();
        reset_animation = false;

        queued_anim = null;
        queued_anim_fade_time = 0.4f;
    }

    public void setup(Model _model)
    {
        idle = new HPAnimation(Resources.Load("default") as AnimationClip);
        model = _model;
        if (model.game_object.GetComponent<Animation>() == null)
            model.game_object.AddComponent<Animation>();
        animation_component = model.game_object.GetComponent<Animation>();

        mesh_renderers = new Dictionary<string, SkinnedMeshRenderer>();
        for (int i = 0; i < _model.game_object.transform.childCount; i++)
        {
            SkinnedMeshRenderer smr = _model.game_object.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                mesh_renderers[smr.name] = smr;
        }
    }

    protected void raiseOnAnimationFinished(string animation_name)
    {
        onAnimationFinished.Invoke(animation_name);
    }

    public void OnDestroy()
    {
        destroyProps();
        Destroy(model.game_object);
    }

    public void queueAnimationOnComponent(HPAnimation animation, float fade_time = 0.4f)
    {
        Debug.Log("queueAnimationOnComponent on " + name + " " + animation.anim_clip.name);
        queued_anim = animation;
        queued_anim_fade_time = MathF.Min(fade_time, 0.4f);

        string new_name = "flop";
        if (anim_flip_flop == true)
        {
            new_name = "flip";
        }
        animation_component.AddClip(queued_anim.anim_clip, new_name);

        if (reset_animation)
        {
            Debug.Log("Playing immediately on " + name);
            animation_component.Play(new_name);
            queued_anim = null;
            queued_anim_fade_time = 0.4f;
        }
        anim_flip_flop = !anim_flip_flop;
    }

    private void playAnimationOnComponent()
    {
        if (currentAnimationAlerter != null)
            StopCoroutine(currentAnimationAlerter);

        string new_name = "flip";
        if (anim_flip_flop == true)
        {
            new_name = "flop";
        }
        Debug.Log("Playing " + queued_anim.anim_clip.name + " on " + name);
        Debug.Log("crossfading on " + name + " with time of " + queued_anim_fade_time);


        animation_component.CrossFade(new_name, queued_anim_fade_time);

        currentAnimationAlerter = animationAlert(queued_anim.anim_clip);
        StartCoroutine(currentAnimationAlerter);

        if (queued_anim.shaderAnimations != null)
        {
            StartCoroutine(shaderAnimPlayer(queued_anim.shaderAnimations, queued_anim.anim_clip));
        }
    }


    protected abstract IEnumerator animationAlert(AnimationClip clip);

    IEnumerator shaderAnimPlayer(List<ShaderAnimation> shaderAnimations, AnimationClip anim_clip)
    {
        float start_time = Time.realtimeSinceStartup;
        float delta_time = 0.0f;

        while (delta_time < anim_clip.length)
        {
            delta_time = (Time.realtimeSinceStartup - start_time);

            foreach (var anim in shaderAnimations)
            {
                if (!mesh_renderers.ContainsKey(anim.mesh_id))
                {
                    continue;
                }
                SkinnedMeshRenderer smr = mesh_renderers[anim.mesh_id];

                if (anim is ShaderAnimationFloat)
                {
                    float divider = delta_time / anim_clip.length;
                    ShaderAnimationFloat animf = (ShaderAnimationFloat)anim;

                    if (divider < animf.start || divider > animf.end)
                        continue;

                    divider += animf.start;
                    divider += 1.0f - animf.end;
                    divider = Mathf.Min(1.0f, divider);
                    float val = Mathf.Lerp(animf.start_value, animf.end_value, divider);

                    smr.material.SetFloat(animf.value_id, val);
                }
            }


            yield return null;
        }
    }

}
