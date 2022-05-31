using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
public class AnimationTest : MonoBehaviour
{
    ConfigAnimation animation_config;

    //RuntimeAnimatorController rac;

    protected Animator animator;
    protected AnimatorOverrideController animatorOverrideController;

    protected int weaponIndex;

    protected AnimationClipOverrides clipOverrides;

    public AnimationClip animation_to_replace;
    ConfigTexture texture_config;
    Config3DModel character_model_config;

    /*void Awake()
    {
        animation_config = AnimationManager.ConfigAnimation.CreateFromJSON("configs//3dmodel//3DAnimationConfig.json");
        texture_config = TextureManager.ConfigTexture.CreateFromJSON("TextureConfig.json");
        character_model_config = ModelManager.Config3DModel.CreateFromJSON("configs//3dmodel//3DModelConfig_Characters.json");


        //float start_time = Time.realtimeSinceStartup;
        //Debug.Log(start_time);
        AnimationClip anim = AnimationManager.loadAnimationClip("c_Stu_DialogueFlirty01", "c3t", animation_config);
        //Debug.Log(Time.realtimeSinceStartup);

        //Debug.Log(Time.realtimeSinceStartup - start_time);

         //skye_example_c3t.meshes[0].parts[0].indices = skye_c3t.meshes[0].parts[0].indices;




        //ModelManager.loadModel("c_SkyeParkins_skin", ref skye_c3t, ref texture_config, ref character_model_config);

        animator = GetComponent<Animator>();

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animatorOverrideController;

        clipOverrides = new AnimationClipOverrides(animatorOverrideController.overridesCount);
        animatorOverrideController.GetOverrides(clipOverrides);

        clipOverrides["c_Stu_DialoguePensive01_v2"] = anim;
        animatorOverrideController.ApplyOverrides(clipOverrides);

        Debug.Log("Finished");
        //Debug.Log(Quaternion.Euler(0, 0, 180));

    }*/

}
