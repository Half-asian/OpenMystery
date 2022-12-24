using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.TextCore.Text;
using static AnimationManager;

public class HPAnimation {
	public AnimationClip anim_clip;
	public List<ShaderAnimation> shaderAnimations;
	public List<VerticalAOV> verticalAOVs;
	public HPAnimation(AnimationClip _anim_clip = null)
    {
		anim_clip = _anim_clip;
    }

}

public static partial class AnimationManager
{
	static private string						animation_name;
	static private Dictionary<string, string>	bone_fullname_dict;
	static private float						animation_scale;
	static private float						head_animation_scale;
	static private Model						model;
	static private Dictionary<string, string>	triggerReplacement;
	static private Dictionary<string, BoneMod>	bone_mods;
	static private bool							is_camera;
	static private float						animation_length;
	static private AnimationClip				anim_clip;
	static private List<Keyframe>				key_frames_pos_x;
	static private List<Keyframe>				key_frames_pos_y;
	static private List<Keyframe>				key_frames_pos_z;

	static private List<Keyframe>				key_frames_rot_x;
	static private List<Keyframe>				key_frames_rot_y;
	static private List<Keyframe>				key_frames_rot_z;
	static private List<Keyframe>				key_frames_rot_w;

	static private List<Keyframe>				key_frames_scale_x;
	static private List<Keyframe>				key_frames_scale_y;
	static private List<Keyframe>				key_frames_scale_z;

	public static HPAnimation loadAnimationClip(
		string							_animation_name,
		Model							_model,
		ConfigHPActorInfo._HPActorInfo	_character,
		Dictionary<string, string>		_triggerReplacement = null,
		Dictionary<string, BoneMod>		bone_mods			= null,
		bool							is_camera			= false)
	{
		animation_name = _animation_name;
		model = _model;
		triggerReplacement = _triggerReplacement;
		AnimationManager.bone_mods = bone_mods;
		AnimationManager.is_camera = is_camera;

		if (_character != null)
		{
			animation_scale = _character.animationScale;
			head_animation_scale = _character.headAnimationScale;
		}
		else
		{
			animation_scale = 1.0f;
			head_animation_scale = 1.0f;
		}
		return _loadAnimationClip();
	}

	public static HPAnimation _loadAnimationClip()
	{
		if (string.IsNullOrEmpty(animation_name))
		{
			Debug.LogError("loadAnimatonClip name is null or empty.");
			return null;
		}

		ConfigAnimation._Animation3D animation_config;
		CocosModel animation_c3t;

        if (Resources.Load(animation_name) != null)
		{
			animation_config = new ConfigAnimation._Animation3D();
			animation_config.wrapMode = "loop";
            UnityEngine.TextAsset asset = Resources.Load(animation_name) as UnityEngine.TextAsset;
            animation_c3t = C3B.loadC3B(asset.bytes);
		}
		else if (!Configs.config_animation.Animation3D.ContainsKey(animation_name))
		{
			Debug.LogError("Animation " + animation_name + " does not exist.");
			return null;
		}
		else
		{
            animation_config = Configs.config_animation.Animation3D[animation_name];
            //Load anim file
            string filename = animation_config.fileName;
            animation_c3t = C3B.loadC3B(filename, GlobalEngineVariables.animations_folder);
        }

        if (animation_c3t == null)
        {
            Debug.Log("Couldn't find " + "animations\\" + animation_name + ". Skipping.");
            return null;
        }
        //Generate Animations
        animation_length = animation_c3t.animations[0].length;

        anim_clip = new AnimationClip();
		anim_clip.legacy = true;
		bone_fullname_dict = Common.GetTransformFullNameDict(model.jt_all_bind);

		List<string> animated_bones = new List<string>();

        foreach (CocosModel.Animation.Bone node in animation_c3t.animations[0].bones)
        {
			animated_bones.Add(node.boneId);
            processAnimationBone(node);
        }

		/*foreach(var bone_mod in bone_mods.Keys)
		{
			if (bone_mods[bone_mod].freezehack == true)
			{
                CocosModel.Animation.Bone node = new CocosModel.Animation.Bone();
                node.boneId = bone_mod;
                var keyframe1 = new CocosModel.Animation.Bone.Keyframe();
                keyframe1.keytime = 0.0f;
				/*keyframe1.translation = new float[] {
					model.pose_bones[bone_mod].transform.localPosition.x,
					model.pose_bones[bone_mod].transform.localPosition.y,
					model.pose_bones[bone_mod].transform.localPosition.z};*/
                /*keyframe1.rotation = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
                node.keyframes = new CocosModel.Animation.Bone.Keyframe[] { keyframe1 };
                processAnimationBone(node);
            }
		}*/

        //Apply bone mods
        if (bone_mods != null)
		{
			foreach (string key in bone_mods.Keys)
			{
				if (bone_mods[key].freezehack == false)
					setBoneMod(ref anim_clip, ref bone_mods, key);
				else if (!animated_bones.Contains(key))
				{
					if (!model.pose_bones.ContainsKey(key))
						continue;
					CocosModel.Animation.Bone node = new CocosModel.Animation.Bone();
					node.boneId = key;
					var keyframe1 = new CocosModel.Animation.Bone.Keyframe();
					keyframe1.keytime = 0.0f;
					Vector3 rotation = model.pose_bones[key].transform.localEulerAngles;
					Quaternion final = Quaternion.Euler(new Vector3(rotation.x, -rotation.y, -rotation.z));
                    keyframe1.rotation = new float[] {
                        final.x,
                        final.y,
                        final.z,
                        final.w
                    };
                    keyframe1.scale = new float[] {
                        model.pose_bones[key].transform.localScale.x,
                        model.pose_bones[key].transform.localScale.y,
                        model.pose_bones[key].transform.localScale.z};
                    node.keyframes = new CocosModel.Animation.Bone.Keyframe[] { keyframe1 };
					processAnimationBone(node);
				}

			}
		}

		//Add trigger map
		if (animation_config.triggerMap != null)
		{
			float offset = 0.0f; //Ensures events activate in the right order
			foreach (ConfigAnimation._Animation3D.TriggerMap trigger in animation_config.triggerMap)
			{
				processTriggerMap(triggerReplacement, trigger, offset);
				offset += 0.001f;
			}
		}
		HPAnimation hpanimation = new HPAnimation();

		//Add shader animation
		if (animation_config.shaderAnimationId != null)
			hpanimation.shaderAnimations = processShaderAnimation(animation_config.shaderAnimationId);
		else
			hpanimation.shaderAnimations = null;

		if (animation_config.effectInfo != null)
		{
			if (hpanimation.shaderAnimations != null) {
				hpanimation.shaderAnimations.AddRange(processEffects(animation_config.effectInfo));
			}
			else
			{
				hpanimation.shaderAnimations = new List<ShaderAnimation>();
                hpanimation.shaderAnimations.AddRange(processEffects(animation_config.effectInfo));
            }
        }

		if (animation_config.camerainfo != null)
		{
			if (animation_config.camerainfo.verticalAOV != null)
			{
				hpanimation.verticalAOVs = new List<VerticalAOV>();
				hpanimation.verticalAOVs.AddRange(VerticalAOV.processVerticalAOV(animation_config.camerainfo));
			}
        }

        //Final settings
        anim_clip.legacy = true;
		anim_clip.name = animation_name;
		if (animation_config.wrapMode == "loop")
			anim_clip.wrapMode = WrapMode.Loop;
		else
			anim_clip.wrapMode = WrapMode.ClampForever;

		hpanimation.anim_clip = anim_clip;

		return hpanimation;
	}
	public static void processAnimationBone(CocosModel.Animation.Bone bone)
	{
		string new_name = bone.boneId;
		foreach (string old in bone_fixes.Keys)
		{
			new_name = new_name.Replace(old, bone_fixes[old]);
		}

		if (!bone_fullname_dict.ContainsKey(new_name))
			return;
		new_name = bone_fullname_dict[new_name];

		key_frames_pos_x = new List<Keyframe>();
		key_frames_pos_y = new List<Keyframe>();
		key_frames_pos_z = new List<Keyframe>();

		key_frames_rot_x = new List<Keyframe>();
		key_frames_rot_y = new List<Keyframe>();
		key_frames_rot_z = new List<Keyframe>();
		key_frames_rot_w = new List<Keyframe>();

		key_frames_scale_x = new List<Keyframe>();
		key_frames_scale_y = new List<Keyframe>();
		key_frames_scale_z = new List<Keyframe>();

		//Possibly could add this later
		//List<Keyframe> key_frames_aov = new List<Keyframe>();

		bool use_bone_mod = false;
		if (bone_mods != null && bone_mods.ContainsKey(bone.boneId))
			use_bone_mod = true;

		Quaternion previous_quaternion = Quaternion.identity;
		bool first_rotation = true;
		foreach (CocosModel.Animation.Bone.Keyframe keyframe in bone.keyframes)
		{
			if (keyframe.translation is not null)
				processAnimationTranslation(bone.boneId, use_bone_mod, keyframe);
			if (keyframe.rotation is not null)
				processAnimationRotation(bone.boneId, use_bone_mod, keyframe, first_rotation, ref previous_quaternion);
			if (keyframe.scale is not null)
				processAnimationScale(bone.boneId, use_bone_mod, keyframe);
			first_rotation = false;
		}

		if (bone.boneId != "jt_all_bind")
			setCurves(new_name);
		else
			setCurvesJtAllBind(new_name);
	}

	public static void processAnimationTranslation(
		string								bone_name,
		bool								use_bone_mod,
		CocosModel.Animation.Bone.Keyframe	keyframe
		)
	{
		Keyframe keyframe_pos_x;
		Keyframe keyframe_pos_y;
		Keyframe keyframe_pos_z;

		if (use_bone_mod == true) //Not even used?
		{
			if (!bone_mods[bone_name].CameraHack)
			{
				keyframe_pos_x = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * -0.01f + bone_mods[bone_name].translation.x * 0.01f);
				keyframe_pos_y = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] * 0.01f + bone_mods[bone_name].translation.y * 0.01f);
				keyframe_pos_z = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] * 0.01f + bone_mods[bone_name].translation.z * 0.01f);
			}
			else
				return;
		}
		else
		{
			if (!is_camera)
			{
				keyframe_pos_x = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * -0.01f);
				keyframe_pos_y = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] * 0.01f);
				keyframe_pos_z = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] * 0.01f);
			}
			else
			{
				keyframe_pos_x = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * 0.01f);
				keyframe_pos_y = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] * 0.01f);
				keyframe_pos_z = new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] * 0.01f);
			}
		}
		keyframe_pos_x.weightedMode = WeightedMode.Both;
		keyframe_pos_y.weightedMode = WeightedMode.Both;
		keyframe_pos_z.weightedMode = WeightedMode.Both;

		key_frames_pos_x.Add(keyframe_pos_x);
		key_frames_pos_y.Add(keyframe_pos_y);
		key_frames_pos_z.Add(keyframe_pos_z);
	}

	public static void processAnimationRotation(
		string								bone_name,
		bool								use_bone_mod,
		CocosModel.Animation.Bone.Keyframe	keyframe,
		bool first_rotation,
		ref Quaternion previous_quaternion
		)
	{
		Keyframe keyframe_rot_x;
		Keyframe keyframe_rot_y;
		Keyframe keyframe_rot_z;
		Keyframe keyframe_rot_w;
		Quaternion current_quat;

		if (use_bone_mod == true)
		{
			if (!bone_mods[bone_name].CameraHack)
			{
				Quaternion bone_quaternion_swizzle = Quaternion.Euler(new Vector3(bone_mods[bone_name].rotation.eulerAngles.y, bone_mods[bone_name].rotation.eulerAngles.x * -1, bone_mods[bone_name].rotation.eulerAngles.z));       //Swizzle X and Y for some reason and -y
				current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1] * -1, keyframe.rotation[2] * -1, keyframe.rotation[3]) * bone_quaternion_swizzle;
			}
			else
			{
				Vector3 current_rotation = CameraManager.current.camera_jt_cam_bind_transform.localEulerAngles;
				Quaternion cam_bind_anim_quaternion = new Quaternion(keyframe.rotation[0], keyframe.rotation[1], keyframe.rotation[2], keyframe.rotation[3]);
				Vector3 cam_bind_anim_euler = cam_bind_anim_quaternion.eulerAngles;
				Vector3 combined_euler = new Vector3(cam_bind_anim_euler.x, current_rotation.y, cam_bind_anim_euler.z);
				current_quat = Quaternion.Euler(combined_euler);
			}
		}
		else
		{
			if (!is_camera)
				current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1] * -1, keyframe.rotation[2] * -1, keyframe.rotation[3]);
			else
				current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1], keyframe.rotation[2], keyframe.rotation[3]);
		}

		if (first_rotation == false && Quaternion.Dot(current_quat, previous_quaternion) < 0.0f)
			current_quat = new Quaternion(-current_quat.x, -current_quat.y, -current_quat.z, -current_quat.w);

		if (use_bone_mod == true && bone_mods[bone_name].CameraHack)
			return;

		keyframe_rot_x = new Keyframe(keyframe.keytime * animation_length, current_quat.x);
		keyframe_rot_y = new Keyframe(keyframe.keytime * animation_length, current_quat.y);
		keyframe_rot_z = new Keyframe(keyframe.keytime * animation_length, current_quat.z);
		keyframe_rot_w = new Keyframe(keyframe.keytime * animation_length, current_quat.w);

		keyframe_rot_x.weightedMode = WeightedMode.Both;
		keyframe_rot_y.weightedMode = WeightedMode.Both;
		keyframe_rot_z.weightedMode = WeightedMode.Both;
		keyframe_rot_w.weightedMode = WeightedMode.Both;

		key_frames_rot_x.Add(keyframe_rot_x);
		key_frames_rot_y.Add(keyframe_rot_y);
		key_frames_rot_z.Add(keyframe_rot_z);
		key_frames_rot_w.Add(keyframe_rot_w);

		previous_quaternion = current_quat;
	}

	public static void processAnimationScale(
		string								bone_name,
		bool								use_bone_mod,
		CocosModel.Animation.Bone.Keyframe	keyframe
		)
	{
		bool is_jt_all_bind = false;
		if (Path.GetFileName(bone_name) == "jt_all_bind")
			is_jt_all_bind = true;

		if (Path.GetFileName(bone_name) == "head1_neck_bind" && head_animation_scale != 0.0f)
		{
			keyframe.scale[0] *= head_animation_scale;
			keyframe.scale[1] *= head_animation_scale;
			keyframe.scale[2] *= head_animation_scale;
		}

		if (use_bone_mod && bone_mods[bone_name].CameraHack)
		{
			keyframe.scale[0] *= bone_mods[bone_name].scale.x;
			keyframe.scale[2] *= bone_mods[bone_name].scale.y;
			keyframe.scale[1] *= bone_mods[bone_name].scale.z;
		}

		//AnimationScale takes the place of jt_all_bind

		Keyframe keyframe_sca_x = new Keyframe(keyframe.keytime * animation_length, is_jt_all_bind ? keyframe.scale[0] * animation_scale : keyframe.scale[0]);
		Keyframe keyframe_sca_y = new Keyframe(keyframe.keytime * animation_length, is_jt_all_bind ? keyframe.scale[1] * animation_scale : keyframe.scale[1]);
		Keyframe keyframe_sca_z = new Keyframe(keyframe.keytime * animation_length, is_jt_all_bind ? keyframe.scale[2] * animation_scale : keyframe.scale[2]);

		keyframe_sca_x.weightedMode = WeightedMode.Both;
		keyframe_sca_y.weightedMode = WeightedMode.Both;
		keyframe_sca_z.weightedMode = WeightedMode.Both;

		key_frames_scale_x.Add(keyframe_sca_x);
		key_frames_scale_y.Add(keyframe_sca_y);
		key_frames_scale_z.Add(keyframe_sca_z);
	}

	public static void setCurves(string new_name)
	{
		AnimationCurve curve_pos_x = new AnimationCurve(key_frames_pos_x.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localPosition.x", curve_pos_x);

		AnimationCurve curve_pos_y = new AnimationCurve(key_frames_pos_y.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localPosition.y", curve_pos_y);

		AnimationCurve curve_pos_z = new AnimationCurve(key_frames_pos_z.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localPosition.z", curve_pos_z);

		AnimationCurve curve_rot_w = new AnimationCurve(key_frames_rot_w.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localRotation.w", curve_rot_w);

		AnimationCurve curve_rot_y = new AnimationCurve(key_frames_rot_y.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localRotation.y", curve_rot_y);

		AnimationCurve curve_rot_z = new AnimationCurve(key_frames_rot_z.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localRotation.z", curve_rot_z);

		AnimationCurve curve_rot_x = new AnimationCurve(key_frames_rot_x.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localRotation.x", curve_rot_x);

		AnimationCurve curve_scale_x = new AnimationCurve(key_frames_scale_x.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.x", curve_scale_x);

		AnimationCurve curve_scale_y = new AnimationCurve(key_frames_scale_y.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.y", curve_scale_y);

		AnimationCurve curve_scale_z = new AnimationCurve(key_frames_scale_z.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.z", curve_scale_z);
	}

	public static void setCurvesJtAllBind(string new_name)
	{
		AnimationCurve curve_scale_x = new AnimationCurve(key_frames_scale_x.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.x", curve_scale_x);

		AnimationCurve curve_scale_y = new AnimationCurve(key_frames_scale_y.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.y", curve_scale_y);

		AnimationCurve curve_scale_z = new AnimationCurve(key_frames_scale_z.ToArray());
		anim_clip.SetCurve(new_name, typeof(Transform), "localScale.z", curve_scale_z);
	}
}