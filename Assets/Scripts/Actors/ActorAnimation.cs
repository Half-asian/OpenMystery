using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class ActorController : Node
{
    public HPAnimation animation_current_intro;
    public HPAnimation animation_current_loop;
    public HPAnimation animation_current_exit;
    public string animation_current_name;
    public ActorState animation_current_state;

    public HPAnimation animation_previous_intro;
    public HPAnimation animation_previous_loop;
    public HPAnimation animation_previous_exit;
    public string animation_previous_name;


    private string idle_queued_animate = null;
    public string idle_animation = null;
    public string idle_animation_sequence = null;

    private string walk_animation = null;


    HPAnimation default_anim;


    private string anim_state = "loop";

    public string delayed_anim;

    public Dictionary<string, AnimationManager.BoneMod> bone_mods;

    private IEnumerator waitForAnimation;

    public void initializeAnimations()
    {
        default_anim = new HPAnimation(Resources.Load("default") as AnimationClip);
        default_anim.anim_clip.legacy = true;
        idle_animation = actor_info.animId_idle;
        walk_animation = actor_info.animId_walk;
    }



    public void playIdleAnimation()
    {
        cleanupState();
        if (idle_queued_animate != null)
        {
            playAnimateCharacter();
        }
        else if (idle_animation_sequence != null)
        {
            replaceCharacterIdleSequence(idle_animation_sequence);
        }
        else
        {
            Debug.Log("Returning to normal idle");
            loadAnimationSet();
            if (anim_state == "loop") //If we are still introducing a new animation, don't bother playing outro
                anim_state = "outro";
            else
                anim_state = "intro";

            Debug.Log(animation_current_loop != null);

            updateAndPlayAnimationState();
        }
    }

    public void playWalkAnimation()
    {
        Debug.Log("playWalkAnimation");
        cleanupState();

        loadAnimationSet();
        if (anim_state == "loop") //If we are still introducing a new animation, don't bother playing outro
            anim_state = "outro";
        else
            anim_state = "intro";
        updateAndPlayAnimationState();
    }


    public void replaceCharacterIdle(string anim_name)
    {
        idle_animation = anim_name;
        idle_animation_sequence = null;
        idle_queued_animate = null;
        if (actor_state == ActorState.Idle)
        {
            playIdleAnimation();
        }
    }

    public void replaceCharacterIdleStaggered(string anim_name)
    {
        idle_animation = anim_name;
        idle_animation_sequence = null;
        idle_queued_animate = null;
        if (actor_state == ActorState.Idle)
        {
            playIdleAnimation();
            anim_state = "loop";
            updateAndPlayAnimationState();
        }
    }

    public void replaceCharacterWalk(string anim_name)
    {
        walk_animation = anim_name;
        if (actor_state == ActorState.Walk)
        {
            playWalkAnimation();
        }
    }


    public void replaceCharacterIdleSequence(string sequence_id)
    {
        idle_animation_sequence = sequence_id;
        if (actor_state == ActorState.Idle)
        {
            if (GetComponent<ActorAnimSequence>() != null) //Get rid of this shit with something cleaner
                DestroyImmediate(GetComponent<ActorAnimSequence>());

            var seq_component = gameObject.AddComponent<ActorAnimSequence>();
            seq_component.initAnimSequence(idle_animation_sequence, false);
        }
    }

    //We cannot save the walk sequence
    public void replaceCharacterWalkSequence(string sequence_id)
    {
        if (actor_state != ActorState.Walk)
        {
            throw new Exception("Tried to replace a characters walk sequence while not walking");
        }
        if (GetComponent<ActorAnimSequence>() != null) //Get rid of this shit with something cleaner
            DestroyImmediate(GetComponent<ActorAnimSequence>());
        var seq_component = gameObject.AddComponent<ActorAnimSequence>();
        seq_component.initAnimSequence(sequence_id, true);
    }


    //Animate character plays once actor is idle
    //If the animation is clamped, it is discarded at end and returns to regular idle anim
    public void animateCharacter(string anim_name)
    {
        cleanupState();
        Debug.Log("animateCharacter " + gameObject.name + " with anim " + anim_name);
        if (!Configs.config_animation.Animation3D.ContainsKey(anim_name))
        {
            Debug.LogError("Couldn't find animation " + anim_name);
            return;
        }
        //It seems we can't animate a character while they're walking
        //Instead it gets queued
        idle_queued_animate = anim_name;
        if (actor_state == ActorState.Idle)
        {
            playAnimateCharacter();
        }
    }

    private void playAnimateCharacter()
    {
        if (waitForAnimation != null)
            StopCoroutine(waitForAnimation);
        anim_state = "loop";

        var new_anim = AnimationManager.loadAnimationClip(idle_queued_animate, model, actor_info, null, bone_mods: bone_mods);
        idle_queued_animate = null;
        playAnimationOnComponent(new_anim);
        Debug.Log("Starting WaitForAnimateCharacterFinished");

        StartCoroutine(WaitForAnimateCharacterFinished(new_anim));
    }

    private void loadAnimationSet()
    {
        string animation_id;
        switch (actor_state)
        {
            case ActorState.Walk:
                animation_id = walk_animation;
                break;
            case ActorState.Idle:
                animation_id = idle_animation;
                break;
            default:
                throw new Exception("Unknown actor state.");
        }

        Debug.Log("loadAnimationSet " + animation_id);

        if (!Configs.config_animation.Animation3D.ContainsKey(animation_id))
        {
            Debug.LogError("Couldn't find animation " + animation_id);
            return;
        }
        //Not sure if we needs to save animation states of old actor states
        //Same actor state, so mesh between old and new anims
        if (animation_current_state == actor_state)
        {
            animation_previous_intro = animation_current_intro;
            animation_previous_loop = animation_current_loop;
            animation_previous_exit = animation_current_exit;
            animation_previous_name = animation_current_name;
            //Switch all the old anims

            if (animation_previous_intro == null) animation_previous_intro = default_anim;
            if (animation_previous_loop == null) animation_previous_loop = default_anim;
            if (animation_previous_exit == null) animation_previous_exit = default_anim;
        }
        else //Otherwise just wipe
        {
            animation_previous_intro = default_anim;
            animation_previous_loop = default_anim;
            animation_previous_exit = default_anim;
            animation_previous_name = "";
        }

        animation_current_intro = default_anim;
        animation_current_loop = default_anim;
        animation_current_exit = default_anim;

        animation_current_state = actor_state;
        animation_current_name = animation_id;

        var animation = Configs.config_animation.Animation3D[animation_id];

        animation_current_loop = AnimationManager.loadAnimationClip(animation_id, model, actor_info, null, bone_mods:bone_mods);
        animation_current_loop.anim_clip.wrapMode = WrapMode.Loop; //Always loop even if the animation config says clamp

        if (animation.introAnim != null)
            animation_current_intro = AnimationManager.loadAnimationClip(animation.introAnim, model, actor_info, null, bone_mods: bone_mods);
        if (animation.outroAnim != null)
            animation_current_exit = AnimationManager.loadAnimationClip(animation.outroAnim, model, actor_info, null, bone_mods: bone_mods);

    }

    //Animation state is intro, loop and outro
    //Plays the actual animations
    public void updateAndPlayAnimationState()
    {
        if (waitForAnimation != null)
            StopCoroutine(waitForAnimation);
        if (GetComponent<ActorAnimSequence>() != null)
        {
            if (GetComponent<ActorAnimSequence>().enabled == true && GetComponent<ActorAnimSequence>().walk == false)
            {
                return;
            }
        }

        if (anim_state == "intro" && animation_current_intro != null)
            playAnimationOnComponent(animation_current_intro);
        else if (anim_state == "outro" && animation_previous_exit != null)
            playAnimationOnComponent(animation_previous_exit);
        else
        {
            playAnimationOnComponent(animation_current_loop);
            anim_state = "loop";
        }

        if (anim_state == "intro")
        {
            waitForAnimation = WaitForAnimation(animation_current_intro, "intro");
            StartCoroutine(waitForAnimation);
        }
        else if (anim_state == "outro")
        {
            waitForAnimation = WaitForAnimation(animation_previous_exit, "outro");
            StartCoroutine(waitForAnimation);
        }
    }


    /*----------        Coroutines      ----------*/

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
            updateAndPlayAnimationState();
        }
        else if (current == "outro")
        {
            anim_state = "intro";
            updateAndPlayAnimationState();
        }
        else
        {
            if (animation.anim_clip.wrapMode == WrapMode.ClampForever)
                updateAndPlayAnimationState();
        }
    }

    public IEnumerator WaitForAnimateCharacterFinished(HPAnimation animation)
    {
        yield return new WaitForSeconds(animation.anim_clip.length);
        Debug.Log("Finished WaitForAnimateCharacterFinished");
        if (animation.anim_clip.wrapMode != WrapMode.Loop)
        {
            playIdleAnimation(); //This should trigger the regular idle to play
        }

    }
    

}
