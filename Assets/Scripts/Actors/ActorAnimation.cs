using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActorAnimation : MonoBehaviour
{
    public ActorController actor_controller;

    public bool blocked = false;

    HPAnimation default_anim;

    private event Action onBlockedFinish;

    public HPAnimation animation1_intro;
    public HPAnimation animation1_loop;
    public HPAnimation animation1_exit;
    public string animation1_name;

    public HPAnimation animation2_intro;
    public HPAnimation animation2_loop;
    public HPAnimation animation2_exit;
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

    private ConfigHPActorInfo._HPActorInfo actor_info { get { return actor_controller.actor_info; } }
    private ActorState actor_state { 
        get { return actor_controller.actor_state; } 
        set { actor_controller.actor_state = value; }
    }


    public void replaceCharacterIdle(string anim_name)
    {
        if (animation1_loop != null && animId_idle == anim_name) //The same animation
            return;

        anim_state = "outro";

        animId_idle = anim_name;
        if (actor_state == ActorState.Idle)
        {
            loadAnimationSet();
            if (blocked == false)
            {
                if (actor_controller.GetComponent<ActorAnimSequence>() != null)
                    DestroyImmediate(actor_controller.GetComponent<ActorAnimSequence>());
                updateAnimationState();
                actor_controller.destroyProps();
                foreach (GameObject particle in actor_controller.particles)
                {
                    GameObject.Destroy(particle);
                }
                actor_controller.particles = new List<GameObject>();
            }
            else
                onBlockedFinish += setCharacterIdle;
        }
        actor_controller.idle = animation1_loop;
    }

    public void setCharacterIdle()
    {
        actor_controller.destroyProps();
        //actor_controller.actor_head.clearLookat();
        //actor_controller.actor_head.clearTurnHeadAt();
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
        actor_controller.playAnimationOnComponent(animation1_loop);
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
        animation1_loop = AnimationManager.loadAnimationClip(anim_name, actor_controller.model, actor_info, null, bone_mods:bone_mods);
        blocked = true;

        if (actor_controller.actor_state == ActorState.Walk)
        {
            return;
        }

        GameObject.DestroyImmediate(actor_controller.GetComponent<ActorAnimSequence>());
        actor_controller.playAnimationOnComponent(animation1_loop);
        StartCoroutine(WaitForAnimateCharacterFinished(animation1_loop));
    }

    public void loadAnimationSet(string _animation = "")
    {
        if (default_anim == null)
        {
            default_anim = new HPAnimation(Resources.Load("default") as AnimationClip);
            default_anim.anim_clip.legacy = true;
        }

        animation2_intro = animation1_intro;
        animation2_loop = animation1_loop;
        animation2_exit = animation1_exit;
        animation2_name = animation1_name;

        if (animation2_intro == null)
            animation2_intro = new HPAnimation(Resources.Load("default") as AnimationClip);
        if (animation2_loop == null)
            animation2_loop = new HPAnimation(Resources.Load("default") as AnimationClip);
        if (animation2_exit == null)
            animation2_exit = new HPAnimation(Resources.Load("default") as AnimationClip);

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

        animation1_loop = AnimationManager.loadAnimationClip(anim_name, actor_controller.model, actor_info, null, bone_mods:bone_mods);
        if (animation1_loop == null)
        {
            Debug.LogError("Animation1_Loop was null somehow");
            return;
        }
        animation1_loop.anim_clip.wrapMode = WrapMode.Loop; //Always loop even if the animation config says clamp

        if (animation.introAnim != null)
            animation1_intro = AnimationManager.loadAnimationClip(animation.introAnim, actor_controller.model, actor_info, null, bone_mods: bone_mods);
        if (animation.outroAnim != null)
            animation1_exit = AnimationManager.loadAnimationClip(animation.outroAnim, actor_controller.model, actor_info, null, bone_mods: bone_mods);

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

        if (anim_state == "intro")
            actor_controller.playAnimationOnComponent(animation1_intro);
        else if (anim_state == "loop")
            actor_controller.playAnimationOnComponent(animation1_loop);
        else if (anim_state == "outro")
            actor_controller.playAnimationOnComponent(animation2_exit);

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

    private IEnumerator WaitForAnimation(HPAnimation animation, string current)
    {
        //The 0.01f fixes a T-pose
        //I have no idea why
        //There seems to be no other way around this
        float start_time = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup <= animation.anim_clip.length + start_time - 0.01f) 
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
            if (animation.anim_clip.wrapMode == WrapMode.ClampForever)
                updateAnimationState();
        }
    }

    public IEnumerator WaitForAnimateCharacterFinished(HPAnimation animation)
    {
        yield return new WaitForSeconds(animation.anim_clip.length);
        if (animation.anim_clip.wrapMode == WrapMode.Clamp)
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
