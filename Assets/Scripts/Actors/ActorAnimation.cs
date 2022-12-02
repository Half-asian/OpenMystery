using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class ActorController : Node
{
    public HPAnimation animation_current_intro;
    public HPAnimation animation_current_loop;
    public HPAnimation animation_current_exit;
    public string animation_current_name = null;
    public ActorState animation_current_state;

    public HPAnimation animation_previous_intro;
    public HPAnimation animation_previous_loop;
    public HPAnimation animation_previous_exit;
    public string animation_previous_name = null;


    private string idle_queued_animate = null;
    public string idle_animation = null;
    public string idle_animation_sequence = null;

    private string walk_animation = null;
    private string walk_animation_sequence = null;

    HPAnimation default_anim;


    private string anim_state = "loop";

    public string delayed_anim;

    public Dictionary<string, AnimationManager.BoneMod> bone_mods;

    private IEnumerator waitForAnimation;

    public void initializeAnimations()
    {
        default_anim = new HPAnimation(Resources.Load("default") as AnimationClip);
        default_anim.anim_clip.legacy = true;
        if (config_hpactor is not null)
        {
            walk_animation = config_hpactor.animId_walk;
        }
    }

    protected override IEnumerator animationAlert(AnimationClip clip)
    {
        while (true)
        {
            yield return new WaitForSeconds(clip.length);
            raiseOnAnimationFinished(clip.name);

            GameStart.event_manager.notifyCharacterAnimationComplete(name, clip.name);
            if (clip.wrapMode != WrapMode.Loop)
            {
                yield break;
            }
        }
    }

    public void playIdleAnimation(bool force_loop = false)
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
            loadAnimationSet();
            if (anim_state == "loop" ) //If we are still introducing a new animation, don't bother playing outro
                anim_state = "outro";
            else
                anim_state = "intro";

            if (reset_animation)
                force_loop = true;

            updateAndPlayAnimationState(force_loop);
        }
    }

    public void playWalkAnimation()
    {
        cleanupState();

        loadAnimationSet();
        //if (anim_state == "loop") //If we are still introducing a new animation, don't bother playing outro
        //    anim_state = "outro";
        //else
        anim_state = "loop";
        updateAndPlayAnimationState();
    }


    public void replaceCharacterIdle(string anim_name)
    {
        if (idle_animation == anim_name && idle_animation_sequence == null)
            return;
        bool force_loop = false;
        if (idle_animation_sequence != null)
            force_loop = true;
        if (idle_animation == anim_name)
            anim_state = "loop";
        idle_animation = anim_name;
        idle_animation_sequence = null;
        idle_queued_animate = null;
        if (actor_state == ActorState.Idle)
        {
            playIdleAnimation(force_loop);
        }
    }

    public void replaceCharacterIdleStaggered(string anim_name)
    {
        if (idle_animation == anim_name && idle_animation_sequence == null)
            return;
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
        if (walk_animation == anim_name)
            return;
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
            animation_previous_intro = null;
            animation_previous_loop = null;
            animation_previous_exit = null;
            animation_current_intro = null;
            animation_current_loop = null;
            animation_current_exit = null;
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
        walk_animation = null;
        walk_animation_sequence = sequence_id;
        if (GetComponent<ActorAnimSequence>() != null) //Get rid of this shit with something cleaner
            DestroyImmediate(GetComponent<ActorAnimSequence>());
        var seq_component = gameObject.AddComponent<ActorAnimSequence>();
        seq_component.initAnimSequence(sequence_id, true);
    }


    public void customAnimationCharacter(string anim_name)
    {
        var new_anim = AnimationManager.loadAnimationClip(anim_name, model, config_hpactor, null, bone_mods: bone_mods);
        queueAnimationOnComponent(new_anim);
    }

    //Animate character plays once actor is idle
    //If the animation is clamped, it is discarded at end and returns to regular idle anim
    public void animateCharacter(string anim_name)
    {
        cleanupState();
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

        var new_anim = AnimationManager.loadAnimationClip(idle_queued_animate, model, config_hpactor, null, bone_mods: bone_mods);
        idle_queued_animate = null;
        queueAnimationOnComponent(new_anim);

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
        }
        else //Otherwise just wipe
        {
            animation_previous_intro = null;
            animation_previous_loop = null;
            animation_previous_exit = null;
            animation_previous_name = null;
        }

        animation_current_intro = null;
        animation_current_loop = null;
        animation_current_exit = null;

        animation_current_state = actor_state;
        animation_current_name = animation_id;

        var animation = Configs.config_animation.Animation3D[animation_id];

        animation_current_loop = AnimationManager.loadAnimationClip(animation_id, model, config_hpactor, null, bone_mods:bone_mods);
        //animation_current_loop.anim_clip.wrapMode = WrapMode.Loop; //Always loop even if the animation config says clamp
        //Need some animation clips to clamp to crossfade properly. E.g. Y6C6 HOM Tonks to binss
        //Possible alternate code if some animations freeze
        //if (animation_current_intro != null || animation_current_exit != null)
        //  animation_current_loop.anim_clip.wrapMode = WrapMode.Loop;



        if (animation.introAnim != null)
            animation_current_intro = AnimationManager.loadAnimationClip(animation.introAnim, model, config_hpactor, null, bone_mods: bone_mods);
        if (animation.outroAnim != null)
            animation_current_exit = AnimationManager.loadAnimationClip(animation.outroAnim, model, config_hpactor, null, bone_mods: bone_mods);

    }

    //Animation state is intro, loop and outro
    //Plays the actual animations
    public void updateAndPlayAnimationState(bool force_loop = false)
    {
        if (animation_previous_name != null && Configs.config_animation.Animation3D[animation_previous_name].wrapMode == "clamp")
        {
            force_loop = true;
            //reset_animation = true;
        }

        if (force_loop)
            anim_state = "loop";


        if (waitForAnimation != null)
            StopCoroutine(waitForAnimation);
        if (GetComponent<ActorAnimSequence>() != null)
        {
            if (GetComponent<ActorAnimSequence>().enabled == true && GetComponent<ActorAnimSequence>().walk == false)
            {
                return;
            }
        }

        if (anim_state == "intro" && animation_current_intro != null && animation_previous_exit != null)
        {
            queueAnimationOnComponent(animation_current_intro, animation_current_intro.anim_clip.length);
            waitForAnimation = WaitForAnimation(animation_current_intro, "intro");
            StartCoroutine(waitForAnimation);
        }
        else if (anim_state == "outro" && animation_current_intro != null && animation_previous_exit != null)
        {
            queueAnimationOnComponent(animation_previous_exit, animation_previous_exit.anim_clip.length);
            waitForAnimation = WaitForAnimation(animation_previous_exit, "outro");
            StartCoroutine(waitForAnimation);
        }
        else
        {
            //if (force_loop )
            //    queueAnimationOnComponent(animation_current_loop, 0.0f); //Play the loop without any crossfade
            //else
            queueAnimationOnComponent(animation_current_loop);
            anim_state = "loop";
            waitForAnimation = WaitForAnimation(animation_current_loop, "loop");
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
        yield return new WaitForSeconds(animation.anim_clip.length);
        //while (Time.realtimeSinceStartup <= animation.anim_clip.length + start_time - 0.01f) 
        //   yield return new WaitForEndOfFrame();

        GameStart.event_manager.notifyCharacterAnimationComplete(name, animation.anim_clip.name);


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
        if (animation.anim_clip.wrapMode != WrapMode.Loop)
        {
            playIdleAnimation(); //This should trigger the regular idle to play
        }

    }
    

}
