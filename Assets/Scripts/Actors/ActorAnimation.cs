using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActorAnimation : MonoBehaviour
{
    public ActorController actor_controller;

    public bool blocked = false;

    AnimationClip default_anim;

    private event Action onBlockedFinish;

    public AnimationClip animation1_intro;
    public AnimationClip animation1_loop;
    public AnimationClip animation1_exit;
    public string animation1_name;

    public AnimationClip animation2_intro;
    public AnimationClip animation2_loop;
    public AnimationClip animation2_exit;
    public string animation2_name;

    public string anim_state = "loop";
    public string animId_idle = "";
    public string queued_anim = "";
    public int queued_anim_delay = 0;
    public string next_outro = "";
    public string anim_sequence_idle = "";

    public string delayed_anim;

    public Dictionary<string, AnimationManager.BoneMod> bone_mods;

    private IEnumerator waitForAnimation;

    private Animation animation_component => actor_controller.animation_component;

    private ConfigHPActorInfo._HPActorInfo actor_info { get { return actor_controller.actor_info; } }
    private ActorState actor_state { 
        get { return actor_controller.actor_state; } 
        set { actor_controller.actor_state = value; }
    }

    IEnumerator currentAnimationAlerter;


    public void playAnimationOnComponent(string id)
    {
        if (currentAnimationAlerter != null)
            StopCoroutine(currentAnimationAlerter);
        if (Time.realtimeSinceStartup - actor_controller.creation_time < 0.5f) //First frame we can't let them T-pose
            animation_component.Play(id);
        else
            animation_component.CrossFade(id, 0.4f);
        currentAnimationAlerter = animationAlert(animation_component.GetClip(id));
        StartCoroutine(currentAnimationAlerter);
    }

    IEnumerator animationAlert(AnimationClip clip)
    {
        while (true)
        {
            yield return new WaitForSeconds(clip.length);
            GameStart.event_manager.notifyCharacterAnimationComplete(name, clip.name);
            if (clip.wrapMode != WrapMode.Loop)
                yield break;
        }
    }

    public void replaceCharacterIdle(string anim_name)
    {
        if (animation1_name == actor_info.animId_idle || animation1_name == actor_info.animId_sitting_idle) //Default animation to another animation, play the intro
            anim_state = "intro";
        else if (anim_name == actor_info.animId_idle || anim_name == actor_info.animId_sitting_idle) //The next animation is the default animation, therefore we should play the outro
            anim_state = "outro";
        else
            anim_state = "loop";

        animId_idle = anim_name;

        if (actor_state == ActorState.Idle)
        {
            loadAnimationSet();
            if (blocked == false)
            {
                if (actor_controller.GetComponent<ActorAnimSequence>() != null)
                    DestroyImmediate(actor_controller.GetComponent<ActorAnimSequence>());
                updateAnimationState();

            }
            else
                onBlockedFinish += setCharacterIdle;
        }
    }

    public void setCharacterIdle()
    {
        actor_controller.destroyProps();
        actor_controller.actor_head.clearLookat();
        actor_controller.actor_head.clearTurnHeadAt();
        actor_controller.destroyProps();
        foreach(GameObject particle in actor_controller.particles)
        {
            GameObject.Destroy(particle);
        }
        actor_controller.particles = new List<GameObject>();

        updateAnimationState();
    }

    public void setCharacterWalk(string _animation)
    {
        actor_controller.destroyProps();
        if (actor_controller.GetComponent<ActorAnimSequence>() != null)
        {
            if (actor_controller.GetComponent<ActorAnimSequence>().walk != true)
            {
                actor_controller.GetComponent<ActorAnimSequence>().enabled = false;
                GameObject.DestroyImmediate(actor_controller.GetComponent<ActorAnimSequence>());
            }
            else
            {
                actor_state = ActorState.Walk;
                return;
            }
        }
        actor_state = ActorState.Walk;
        loadAnimationSet(_animation);
        anim_state = "loop";
        updateAnimationState();
        playAnimationOnComponent("loop");
    }

    public void animateCharacter(string anim_name)
    {
        if (waitForAnimation != null)
            actor_controller.StopCoroutine(waitForAnimation);
        anim_state = "loop";
        if (!Configs.config_animation.Animation3D.ContainsKey(anim_name))
        {
            Debug.LogError("Couldn't find animation " + anim_name);
            return;
        }
        animation1_loop = AnimationManager.loadAnimationClip(anim_name, actor_controller.model, actor_info, null, actor_controller, bone_mods);
        animation_component.AddClip(animation1_loop, "extra_animation");
        blocked = true;

        if (actor_controller.actor_state == ActorState.Walk)
        {
            return;
        }

        GameObject.DestroyImmediate(actor_controller.GetComponent<ActorAnimSequence>());
        playAnimationOnComponent("extra_animation");
        StartCoroutine(WaitForAnimateCharacterFinished(animation1_loop));
    }

    public void loadAnimationSet(string _animation = "")
    {
        if (default_anim == null)
        {
            default_anim = Resources.Load("default") as AnimationClip;
            default_anim.legacy = true;
        }

        animation2_intro = animation1_intro;
        animation2_loop = animation1_loop;
        animation2_exit = animation1_exit;
        animation2_name = animation1_name;

        if (animation2_intro == null)
            animation2_intro = Resources.Load("default") as AnimationClip;
        if (animation2_loop == null)
            animation2_loop = Resources.Load("default") as AnimationClip;
        if (animation2_exit == null)
            animation2_exit = Resources.Load("default") as AnimationClip;

        animation1_intro = default_anim;
        animation1_loop = default_anim;
        animation1_exit = default_anim;

        string anim_name = "";
        if (actor_state == ActorState.Idle)
            anim_name = animId_idle;
        else if (actor_state == ActorState.Walk)
        {
            if (_animation != "")
                anim_name = _animation;
            else
                anim_name = actor_info.animId_walk;
        }
        else if (actor_state == ActorState.Run)
            anim_name = actor_info.animId_run;
        else if (actor_state == ActorState.SittingIdle)
            anim_name = actor_info.animId_sitting_idle;
        else if (actor_state == ActorState.Success)
            anim_name = actor_info.animId_success;
        else if (actor_state == ActorState.RenownBoard)
            anim_name = actor_info.animId_renownboard;

        animation1_name = anim_name;

        if (!Configs.config_animation.Animation3D.ContainsKey(anim_name))
        {
            Debug.LogError("Couldn't find animation " + anim_name);
            return;
        }
        ConfigAnimation._Animation3D animation = Configs.config_animation.Animation3D[anim_name];

        animation1_loop = AnimationManager.loadAnimationClip(anim_name, actor_controller.model, actor_info, null, actor_controller, bone_mods);
        if (animation1_loop == null)
            Debug.LogError("Animation1_Loop was null somehow");

        animation1_loop.wrapMode = WrapMode.Loop; //Always loop even if the animation config says clamp

        if (animation.introAnim != null)
            animation1_intro = AnimationManager.loadAnimationClip(animation.introAnim, actor_controller.model, actor_info, null, actor_controller, bone_mods);
        if (animation.outroAnim != null)
            animation1_exit = AnimationManager.loadAnimationClip(animation.outroAnim, actor_controller.model, actor_info, null, actor_controller, bone_mods);

        animation_component.AddClip(animation1_intro, "intro");
        animation_component.AddClip(animation1_loop, "loop");
        animation_component.AddClip(animation2_exit, "outro");
    }

    public void updateAnimationState()
    {
        if (waitForAnimation != null)
            actor_controller.StopCoroutine(waitForAnimation);
        if (actor_controller.GetComponent<ActorAnimSequence>() != null)
        {
            if (actor_controller.GetComponent<ActorAnimSequence>().enabled == true && actor_controller.GetComponent<ActorAnimSequence>().walk == false)
            {
                Debug.Log("did not update animation state due to actor anim sequence");
                return;
            }
        }

        playAnimationOnComponent(anim_state);

        if (anim_state == "intro")
        {
            waitForAnimation = WaitForAnimation(animation1_intro, "intro");
            actor_controller.StartCoroutine(waitForAnimation);
        }
        else if (anim_state == "outro")
        {
            waitForAnimation = WaitForAnimation(animation2_exit, "outro");
            actor_controller.StartCoroutine(waitForAnimation);
        }
    }

    private IEnumerator WaitForAnimation(AnimationClip clip, string current)
    {
        //The 0.01f fixes a T-pose
        //I have no idea why
        //There seems to be no other way around this
        float start_time = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup <= clip.length + start_time - 0.01f) 
           yield return new WaitForEndOfFrame();

        if (current == "intro")
        {
            anim_state = "loop";
            updateAnimationState();
        }
        else if (current == "outro")
        {
            anim_state = "intro";
            updateAnimationState();
        }
        else
        {
            if (clip.wrapMode == WrapMode.ClampForever)
                updateAnimationState();
        }
    }

    public IEnumerator WaitForAnimateCharacterFinished(AnimationClip clip)
    {
        yield return new WaitForSeconds(clip.length);
        if (clip.wrapMode == WrapMode.Clamp)
        {
            onBlockedFinish -= setCharacterIdle;
            onBlockedFinish += setCharacterIdle;
        }

        unblock();

    }
    
    public void unblock()
    {
        blocked = false;
        if (onBlockedFinish == null)
            replaceCharacterIdle(actor_info.animId_idle);

        onBlockedFinish?.Invoke();
        onBlockedFinish = null;
    }
}
