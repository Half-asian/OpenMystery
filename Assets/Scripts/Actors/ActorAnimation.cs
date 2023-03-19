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

    public string current_anim_state = "loop";
    public string next_anim_state = "loop";

    [Serializable]
    public struct ActorAnim
    {
        public enum AnimType
        {
            Regular,
            Staggered,
            Sequence,
            Finished
        }
        public AnimType anim_type;
        public string id;

        public ActorAnim(AnimType _anim_type, string _id)
        {
            anim_type = _anim_type;
            id = _id;
        }
    }

    public Dictionary<string, AnimationManager.BoneMod> bone_mods;

    [SerializeField]
    private ActorAnim idle_actor_anim;
    [SerializeField]
    private ActorAnim current_actor_anim;
    [SerializeField] 
    private Common.FixedSizedQueue<string> modifying_events = new Common.FixedSizedQueue<string>(5);
    private HPAnimation default_anim;
    private IEnumerator waitForAnimation;
    private IEnumerator waitForAnimateCharacterSequenceFinished;
    private IEnumerator waitForAnimateCharacterFinished;

    private static AnimationManager.BoneMod frozen_bonemod = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1), true);



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

    private void playActorAnimation(ActorAnim actor_anim)
    {
        if (current_actor_anim.id == actor_anim.id && current_actor_anim.anim_type == actor_anim.anim_type)
            return;
        cleanupState();
        if (actor_anim.anim_type == ActorAnim.AnimType.Sequence)
        {
            playSequence(actor_anim);
        }
        else
        {
            loadAnimationSet(actor_anim);
            if (current_anim_state == "loop") //If we are still introducing a new animation, don't bother playing outro
                next_anim_state = "intro";

            if (actor_anim.anim_type == ActorAnim.AnimType.Staggered)
            {
                next_anim_state = "loop";
                cancel_crossfade = true;
            }
            updateAndPlayAnimationState();
        }
        current_actor_anim = actor_anim;
    }

    private void playSequence(ActorAnim actorAnim)
    {
        current_anim_state = "loop";
        next_anim_state = "loop";
        animation_previous_intro = null;
        animation_previous_loop = null;
        animation_previous_exit = null;
        animation_current_intro = null;
        animation_current_loop = null;
        animation_current_exit = null;
        if (GetComponent<ActorAnimSequence>() != null) //Get rid of this shit with something cleaner
            DestroyImmediate(GetComponent<ActorAnimSequence>());

        var seq_component = gameObject.AddComponent<ActorAnimSequence>();
        seq_component.initAnimSequence(actorAnim.id, false);
    }

    /*------ Public Functions -----*/

    public void markCurrentAnimationFinished()
    {
        current_actor_anim =  new ActorAnim(ActorAnim.AnimType.Finished, "");
    }

    //Regular animation, will play when actor switches to idle state
    public void replaceCharacterIdle(string event_id, string animation_id)
    {
        modifying_events.Enqueue(event_id);
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Regular, animation_id);
        if (actor_state == ActorState.Idle)
        {
            playActorAnimation(new_anim);
        }
        idle_actor_anim = new_anim;
    }

    //Staggered animation, will play when actor switches to idle state
    //Staggered animation skips the intro and has no crossfade

    public void replaceCharacterIdleStaggered(string event_id, string animation_id)
    {
        modifying_events.Enqueue(event_id);
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Staggered, animation_id);
        if (actor_state == ActorState.Idle)
        {
            playActorAnimation(new_anim);
        }
        idle_actor_anim = new_anim;
    }


    //Animation Sequence, will play when actor switches to idle state
    public void replaceCharacterIdleSequence(string event_id, string sequence_id)
    {
        modifying_events.Enqueue(event_id);
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Sequence, sequence_id);
        if (actor_state == ActorState.Idle)
        {
            playActorAnimation(new_anim);
        }
        idle_actor_anim = new_anim;
    }

    //Regular animation, will play only if actor is in idle state.
    //Actor will no longer be in idle, therefore replace idles will not overwrite
    public void animateCharacter(string event_id, string anim_name, int max_loops)
    {
        modifying_events.Enqueue(event_id);
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Regular, anim_name);

        if (actor_state == ActorState.Idle) //Ignored if not idle
        {
            setCharacterAnimate();
            if (waitForAnimation != null)
                StopCoroutine(waitForAnimation);
            current_anim_state = "loop";
            playActorAnimation(new_anim);

            var new_anim_clip = AnimationManager.loadAnimationClip(anim_name, model, config_hpactor, null, bone_mods: bone_mods);
            waitForAnimateCharacterFinished = WaitForAnimateCharacterFinished(new_anim_clip, max_loops);
            StartCoroutine(waitForAnimateCharacterFinished);
        }
    }


    //Animation Sequence, will play only if actor is in idle state.
    //Actor will no longer be in idle, therefore replace idles will not overwrite
    public void playCharacterAnimSequence(string event_id, string sequence_id)
    {
        modifying_events.Enqueue(event_id);
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Sequence, sequence_id);
        if (actor_state == ActorState.Idle) //If the character is not idle, do nothing
        {
            setCharacterAnimate();
            playActorAnimation(new_anim);
            waitForAnimateCharacterSequenceFinished = WaitForAnimateCharacterSequenceFinished(GetComponent<AnimationSequence>());
            StartCoroutine(waitForAnimateCharacterSequenceFinished);
        }
    }

    //Only used for rare cases. Not part of the main game.
    public void customAnimationCharacter(string anim_name)
    {
        cancel_crossfade = true;
        var new_anim = AnimationManager.loadAnimationClip(anim_name, model, config_hpactor, null, bone_mods: bone_mods);
        queueAnimationOnComponent(new_anim);
    }

    public void advanceAnimSequence()
    {
        if (gameObject.GetComponent<AnimationSequence>() != null) {
            gameObject.GetComponent<AnimationSequence>().advanceAnimSequence();
        }
    }


    /*----- Private -----*/
    private void replaceCharacterWalkSequence(string sequence_id)
    {
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Sequence, sequence_id);
        if (actor_state == ActorState.Walk)
        {
            playActorAnimation(new_anim);
        }
    }

    private void replaceCharacterWalk(string anim_name)
    {
        ActorAnim new_anim = new ActorAnim(ActorAnim.AnimType.Regular, anim_name);
        if (actor_state == ActorState.Walk)
        {
            playActorAnimation(new_anim);
        }
    }

    private void initializeAnimations()
    {
        //Freezes these bones in place when no animations are playing to stop infinite spin on lookats
        bone_mods = new Dictionary<string, AnimationManager.BoneMod>()
        {
            ["jt_hips_bind"] = frozen_bonemod,
            ["spine1_loResSpine2_bind"] = frozen_bonemod,
            ["spine1_loResSpine3_bind"] = frozen_bonemod,
            ["jt_head_bind"] = frozen_bonemod,
        };

        default_anim = new HPAnimation(Resources.Load("default") as AnimationClip);
        default_anim.anim_clip.legacy = true;
    }

    private void loadAnimationSet(ActorAnim actor_anim)
    {
        string animation_id = actor_anim.id;

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
            current_anim_state = "loop";
            next_anim_state = "loop";
        }

        animation_current_intro = null;
        animation_current_loop = null;
        animation_current_exit = null;

        animation_current_state = actor_state;
        animation_current_name = animation_id;

        var animation = Configs.config_animation.Animation3D[animation_id];

        animation_current_loop = AnimationManager.loadAnimationClip(animation_id, model, config_hpactor, null, bone_mods:bone_mods);
        //Need some animation clips to clamp to crossfade properly. E.g. Y6C5 HOM Tonks to binss

        if (animation.introAnim != null)
            animation_current_intro = AnimationManager.loadAnimationClip(animation.introAnim, model, config_hpactor, null, bone_mods: bone_mods);

    }

    //Animation state is intro, loop and outro
    //Plays the actual animations
    public void updateAndPlayAnimationState()
    {

        //If we go from intro to loop, very short crossfade
        //If we go from outro to intro, very short crossfade
        if (animation_previous_name != null && Configs.config_animation.Animation3D[animation_previous_name].wrapMode == "clamp")
        {
            next_anim_state = "loop";
        }

        if (waitForAnimation != null)
            StopCoroutine(waitForAnimation);
        if (GetComponent<ActorAnimSequence>() != null)
        {
            if (GetComponent<ActorAnimSequence>().enabled == true && GetComponent<ActorAnimSequence>().walk == false)
            {
                return;
            }
        }

        if (next_anim_state == "intro" && animation_current_intro != null)
        {
            queueAnimationOnComponent(animation_current_intro);
            waitForAnimation = WaitForAnimation(animation_current_intro, "intro", false, false);
            StartCoroutine(waitForAnimation);
        }
        else
        {
            //Intro to loop doesn't need crossfade
            if (current_anim_state == "intro")
                queueAnimationOnComponent(animation_current_loop);
            else
                queueAnimationOnComponent(animation_current_loop);
            next_anim_state = "loop";
            waitForAnimation = WaitForAnimation(animation_current_loop, "loop");
            StartCoroutine(waitForAnimation);
        }
        current_anim_state = next_anim_state;

    }


    /*----------        Coroutines      ----------*/

    private IEnumerator WaitForAnimation(HPAnimation animation, string current, bool half = false, bool skip = false)
    {
        if (!half && skip == false)
            yield return new WaitForSeconds(animation.anim_clip.length);
        else if (half & skip == false)
            yield return new WaitForSeconds(animation.anim_clip.length / 2);

        GameStart.event_manager.notifyCharacterAnimationComplete(name, animation.anim_clip.name);

        if (current == "intro")
        {
            next_anim_state = "loop";
            updateAndPlayAnimationState();
        }
        else if (current == "outro")
        {
            next_anim_state = "intro";
            updateAndPlayAnimationState();
        }
        else
        {
            if (animation.anim_clip.wrapMode == WrapMode.ClampForever && actor_state != ActorState.Animate)
            {
                markCurrentAnimationFinished();
                setCharacterIdle();
            }
        }
    }

    public IEnumerator WaitForAnimateCharacterSequenceFinished(AnimationSequence animSequence)
    {
        while(animSequence != null)
            yield return null;
        waitForAnimateCharacterSequenceFinished = null;
        setCharacterIdle();
    }

    public IEnumerator WaitForAnimateCharacterFinished(HPAnimation animation, int max_loops)
    {
        int loop_count = 0;
        while (true)
        {
            yield return new WaitForSeconds(animation.anim_clip.length);
            loop_count++;
            if (animation.anim_clip.wrapMode != WrapMode.Loop)
                break;
            if (max_loops > 0 && loop_count >= max_loops)
                break;
        }
        waitForAnimateCharacterFinished = null;
        setCharacterIdle();
    }

}
