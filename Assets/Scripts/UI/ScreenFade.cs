using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScreenFade : MonoBehaviour
{
    public enum FadeState {
        faded,
        unfaded,
    }


    private static ScreenFade current;
    [SerializeField]
    private GameObject fade_object;
    [SerializeField]
    private Animation animation_component;
    [SerializeField]
    private Image image;

    private FadeState fade_state = FadeState.faded;

    static Coroutine fallback;
    static Coroutine waitfade;



    public void Awake()
    {
        current = this;
        fade_object.SetActive(true);
    }

    static IEnumerator fadeFallback()
    {
        yield return new WaitForSeconds(5);
        fadeFrom(0.01f, Color.black, true);
        current.StopCoroutine(waitfade);
        GameStart.event_manager.notifyScreenFadeComplete();
    }

    static IEnumerator waitFade(float time)
    {
        yield return new WaitForSeconds(time);
        GameStart.event_manager.notifyScreenFadeComplete();
    }

    public static void fadeFrom(float time, Color color, bool force = false)
    {
        if (force == false && current.fade_state == FadeState.unfaded)
        {
            waitfade = current.StartCoroutine(waitFade(0.01f));
            return;
        }
        current.fade_state = FadeState.unfaded;

        if (fallback != null)
            current.StopCoroutine(fallback);
        current.image.color = color;
        AnimationClip anim_clip = new AnimationClip();
        anim_clip.legacy = true;
        List<Keyframe> keyframes = new List<Keyframe>();
        keyframes.Add(new Keyframe(0.0f, 1.0f));
        keyframes.Add(new Keyframe(time, 0.0f));
        AnimationCurve curve = new AnimationCurve(keyframes.ToArray());
        anim_clip.SetCurve("", typeof(Image), "m_Color.a", curve);
        anim_clip.wrapMode = WrapMode.ClampForever;
        current.animation_component.AddClip(anim_clip, "default");
        current.animation_component.Play("default");
        waitfade = current.StartCoroutine(waitFade(time));
    }

    public static void fadeTo(float time, Color color, bool force = false)
    {
        if (force == false && current.fade_state == FadeState.faded)
        {
            waitfade = current.StartCoroutine(waitFade(0.01f));
            return;
        }

        current.fade_state = FadeState.faded;

        current.image.color = color;
        AnimationClip anim_clip = new AnimationClip();
        anim_clip.legacy = true;
        List<Keyframe> keyframes = new List<Keyframe>();
        keyframes.Add(new Keyframe(0.0f, 0.0f));
        keyframes.Add(new Keyframe(time, 1.0f));
        AnimationCurve curve = new AnimationCurve(keyframes.ToArray());
        anim_clip.SetCurve("", typeof(Image), "m_Color.a", curve);
        anim_clip.wrapMode = WrapMode.ClampForever;
        current.animation_component.AddClip(anim_clip, "default");
        current.animation_component.Play("default");
        fallback = current.StartCoroutine(fadeFallback());
        waitfade = current.StartCoroutine(waitFade(time));
    }

}
