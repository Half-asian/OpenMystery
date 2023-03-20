using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public abstract partial class Node : MonoBehaviour
{
    public event Action<string> onAnimationFinished = delegate { };
    private Node parent_node = null;
    public Model model;

    public Animation animation_component;

    IEnumerator currentAnimationAlerter;
    IEnumerator shaderPlayer;

    public HPAnimation idle;

    Dictionary<string, SkinnedMeshRenderer> mesh_renderers;

    bool anim_flip_flop = false;

    public HPAnimation queued_anim = null;

    public bool cancel_crossfade = true;

    private void Update()
    {
        if (queued_anim != null)
            playAnimationOnComponent();
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

    public void queueAnimationOnComponent(HPAnimation animation)
    {
        queued_anim = animation;

        //Debug.Log("queueAnimationOnComponent on " + name + " " + animation.anim_clip.name + " as " + (anim_flip_flop ? "flop" : "flip"));
        animation_component.AddClip(queued_anim.anim_clip, anim_flip_flop ? "flop" : "flip");

        if (cancel_crossfade) //This needs to be on queue to prevent t-poses.
        {
            playAnimationOnComponent();
        }
    }

    private void playAnimationOnComponent()
    {
        string anim_name = anim_flip_flop ? "flop" : "flip";

        if (currentAnimationAlerter != null)
            StopCoroutine(currentAnimationAlerter);
        if (shaderPlayer != null)
            StopCoroutine(shaderPlayer);

        if (cancel_crossfade == false)
        {
            animation_component.CrossFade(anim_name, 0.3f);
        }
        else
        {
            animation_component.Play(anim_name);
            cancel_crossfade = false;
        }

        currentAnimationAlerter = animationAlert(queued_anim.anim_clip);
        StartCoroutine(currentAnimationAlerter);

        if (queued_anim.shaderAnimations != null)
        {
            shaderPlayer = shaderAnimPlayer(queued_anim.shaderAnimations, queued_anim.anim_clip);
            StartCoroutine(shaderPlayer);
        }

        anim_flip_flop = !anim_flip_flop;
        queued_anim = null;
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

    public void PlaySound(string sound) =>
    Sound.playSoundEffect(sound);
    public void ScriptTrigger(string trigger) =>
    GameStart.event_manager.notifyScriptTrigger(trigger);

}
