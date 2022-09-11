using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public partial class ModelHolder : MonoBehaviour
{
    public Model model;

    public Animation animation_component;

    IEnumerator currentAnimationAlerter;
    public float creation_time;
    HPAnimation lastclip;
    public HPAnimation idle;

    Dictionary<string, SkinnedMeshRenderer> mesh_renderers;

    public void setup(Model _model)
    {
        idle = new HPAnimation(Resources.Load("default") as AnimationClip);
        model = _model;
        if (model.game_object.GetComponent<Animation>() == null)
            model.game_object.AddComponent<Animation>();
        animation_component = model.game_object.GetComponent<Animation>();
        creation_time = Time.realtimeSinceStartup;

        mesh_renderers = new Dictionary<string, SkinnedMeshRenderer>();
        for (int i = 0; i < _model.game_object.transform.childCount; i++)
        {
            SkinnedMeshRenderer smr = _model.game_object.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                mesh_renderers[smr.name] = smr;
        }
    }

    public void playAnimationOnComponent(HPAnimation animation)
    {
        if (currentAnimationAlerter != null)
            StopCoroutine(currentAnimationAlerter);

        animation_component.AddClip(animation.anim_clip, "default");

        if (Time.realtimeSinceStartup - creation_time < 0.5f)
        {//First frame we can't let them T-pose
            animation_component.Play("default");
        }
        else
        {
            animation_component.Play("default");

            //This check prevents a small spaz in lookats for looping anims
            /*if (lastclip != null && lastclip == animation)
            {
                animation_component.Play("default");
            }
            else
            {
                animation_component.CrossFade("default", 0.4f);
            }*/
        }
        lastclip = animation;
        currentAnimationAlerter = animationAlert(animation.anim_clip);
        StartCoroutine(currentAnimationAlerter);

        if (animation.shaderAnimations != null)
        {
            StartCoroutine(shaderAnimPlayer(animation.shaderAnimations, animation.anim_clip));
        }
    }

    public void changeOpaqueMaterialToTransparent()
    {

    }

    IEnumerator animationAlert(AnimationClip clip)
    {
        while (true)
        {
            yield return new WaitForSeconds(clip.length);
            GameStart.event_manager.notifyCharacterAnimationComplete(name, clip.name);
            if (clip.wrapMode != WrapMode.Loop)
            {
                //yield return null;
                //yield return null;
                //playAnimationOnComponent(idle);
                yield break;
            }
        }
    }

    IEnumerator shaderAnimPlayer(List<ShaderAnimation> shaderAnimations, AnimationClip anim_clip)
    {
        float start_time = Time.realtimeSinceStartup;
        float delta_time = 0.0f;

        while (delta_time < anim_clip.length)
        {
            delta_time = (Time.realtimeSinceStartup - start_time);

            foreach (var anim in shaderAnimations)
            {
                if (mesh_renderers.ContainsKey(anim.mesh_id))
                {
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
                else
                {
                    Debug.LogError("Could not find mesh renderer " + anim.mesh_id + " in model");
                }

            }


            yield return null;
        }
    }

}
