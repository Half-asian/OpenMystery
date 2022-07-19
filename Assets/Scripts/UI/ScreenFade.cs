using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScreenFade : MonoBehaviour
{

    private static ScreenFade current;
    [SerializeField]
    private GameObject fade_object;
    [SerializeField]
    private Animation animation_component;
    [SerializeField]
    private Image image;

    public void Awake()
    {
        current = this;
        fade_object.SetActive(true);
    }

    public static void fadeFrom(float time, Color color)
    {
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
    }

    public static void fadeTo(float time, Color color)
    {
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
    }

}
