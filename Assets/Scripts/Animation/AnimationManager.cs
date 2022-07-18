using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
public class AnimationManager : MonoBehaviour
{
	public static void processAnimationNode(string name, Model model, Dictionary<string, string> bone_fullname_dict, float animation_length, CocosModel.Node node, float animation_scale, float head_animation_scale, ref Dictionary<string, CocosModel.Animation.Bone> bone_dict, ref AnimationClip anim_clip, ref List<string> bonemods_activated, ActorController ac = null, Dictionary<string, BoneMod> bone_mods = null, bool is_camera = false, float jt_all_bind_scale_x = 1.0f, float jt_all_bind_scale_y = 1.0f, float jt_all_bind_scale_z = 1.0f)
	{
		name = name + "/" + node.id;

		anim_clip.legacy = true;

		string new_name = node.id;

		foreach (string old in bone_fixes.Keys)
		{
			new_name = new_name.Replace(old, bone_fixes[old]);
		}

		//if (name != "Armature/jt_all_bind")
		if (bone_fullname_dict.ContainsKey(new_name))
		{
			new_name = bone_fullname_dict[new_name];

			bool first = true;
			Quaternion previous_quaternion = Quaternion.identity;

			List<Keyframe> key_frames_pos_x = new List<Keyframe>();
			List<Keyframe> key_frames_pos_y = new List<Keyframe>();
			List<Keyframe> key_frames_pos_z = new List<Keyframe>();

			List<Keyframe> key_frames_rot_x = new List<Keyframe>();
			List<Keyframe> key_frames_rot_y = new List<Keyframe>();
			List<Keyframe> key_frames_rot_z = new List<Keyframe>();
			List<Keyframe> key_frames_rot_w = new List<Keyframe>();

			List<Keyframe> key_frames_scale_x = new List<Keyframe>();
			List<Keyframe> key_frames_scale_y = new List<Keyframe>();
			List<Keyframe> key_frames_scale_z = new List<Keyframe>();

			//List<Keyframe> key_frames_aov = new List<Keyframe>();

			string[] split_name = name.Split('/');
			string bone_name = split_name[split_name.Length - 1];
			bool use_bone_mod = false;

			if (bone_mods != null)
			{
				if (bone_mods.ContainsKey(bone_name))
				{
					use_bone_mod = true;
				}
			}

			foreach (CocosModel.Animation.Bone.Keyframe keyframe in bone_dict[node.id].keyframes)
			{
				/*if (first == false) //Spazzing fix
				{
					if (keyframe.rotation != null)
					{
						float rotation_difference = Mathf.Abs(prev_x - keyframe.rotation[0]) + Mathf.Abs(prev_y - keyframe.rotation[1]) + Mathf.Abs(prev_z - keyframe.rotation[2]) + Mathf.Abs(prev_w - keyframe.rotation[3]);
						if (rotation_difference > 2.5f)
						{
							keyframe.keytime = prev_kt + 0.00001f;
						}
					}
				}*/

				if (keyframe.translation != null)
				{
					if (use_bone_mod == true)
					{
						if (bone_mods[bone_name].enabled)
						{

							if (!is_camera)
							{
								key_frames_pos_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * -1 + bone_mods[bone_name].translation.x));
								key_frames_pos_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] + bone_mods[bone_name].translation.y));
								key_frames_pos_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] + bone_mods[bone_name].translation.z));
							}
							else
							{
								key_frames_pos_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * -0.01f + bone_mods[bone_name].translation.x));
								key_frames_pos_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] * 0.01f + bone_mods[bone_name].translation.y));
								key_frames_pos_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] * 0.01f + bone_mods[bone_name].translation.z));
							}
						}
						/*else //jt fucking cam bind
                        {
							Debug.LogError("AAA");
							key_frames_pos_x.Add(new Keyframe(keyframe.keytime * animation_length, CameraManager.main_camera_jt_cam_bind.transform.localPosition.x));
							key_frames_pos_y.Add(new Keyframe(keyframe.keytime * animation_length, CameraManager.main_camera_jt_cam_bind.transform.localPosition.y));
							key_frames_pos_z.Add(new Keyframe(keyframe.keytime * animation_length, CameraManager.main_camera_jt_cam_bind.transform.localPosition.z));
						}*/
					}
					else
					{
						if (!is_camera)
						{
							key_frames_pos_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * -1));
							key_frames_pos_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1]));
							key_frames_pos_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2]));
						}
						else //jt_anim bind
						{
							key_frames_pos_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[0] * 0.01f));
							key_frames_pos_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[1] * 0.01f));
							key_frames_pos_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.translation[2] * 0.01f));
						}
					}
				}


				if (keyframe.rotation != null)
				{
					if (use_bone_mod == true)
					{
						if (bone_mods[bone_name].enabled)
						{
							Quaternion bone_quaternion_swizzle = Quaternion.Euler(new Vector3(bone_mods[bone_name].rotation.eulerAngles.y, bone_mods[bone_name].rotation.eulerAngles.x * -1, bone_mods[bone_name].rotation.eulerAngles.z));       //Swizzle X and Y for some reason and -y
							Quaternion current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1] * -1, keyframe.rotation[2] * -1, keyframe.rotation[3]) * bone_quaternion_swizzle;

							if (first == false)
							{
								if (Quaternion.Dot(current_quat, previous_quaternion) < 0.0f)
									current_quat = new Quaternion(-current_quat.x, -current_quat.y, -current_quat.z, -current_quat.w);
							}

							key_frames_rot_x.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.y));
							key_frames_rot_y.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.y));
							key_frames_rot_z.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.z));
							key_frames_rot_w.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.w));

							previous_quaternion = current_quat;
							first = false;
						}

						else
						{
							Vector3 current_rotation = CameraManager.current.main_camera_jt_cam_bind.transform.localEulerAngles;
							Quaternion cam_bind_anim_quaternion = new Quaternion(keyframe.rotation[0], keyframe.rotation[1], keyframe.rotation[2], keyframe.rotation[3]);
							Vector3 cam_bind_anim_euler = cam_bind_anim_quaternion.eulerAngles;
							Vector3 combined_euler = new Vector3(cam_bind_anim_euler.x, current_rotation.y, cam_bind_anim_euler.z);
							Quaternion current_quat = Quaternion.Euler(combined_euler);

							if (first == false)
							{
								if (Quaternion.Dot(current_quat, previous_quaternion) < 0.0f)
									current_quat = new Quaternion(-current_quat.x, -current_quat.y, -current_quat.z, -current_quat.w);
							}

							key_frames_rot_x.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.x));
							key_frames_rot_y.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.y));
							key_frames_rot_z.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.z));
							key_frames_rot_w.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.w));

							previous_quaternion = current_quat;

							first = false;

						}

					}

					else
					{
						Quaternion current_quat;
						if (!is_camera)
							current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1] * -1, keyframe.rotation[2] * -1, keyframe.rotation[3]);
						else
							current_quat = new Quaternion(keyframe.rotation[0], keyframe.rotation[1], keyframe.rotation[2], keyframe.rotation[3]);

						if (first == false)
						{
							if (Quaternion.Dot(current_quat, previous_quaternion) < 0.0f)
								current_quat = new Quaternion(-current_quat.x, -current_quat.y, -current_quat.z, -current_quat.w);
						}

						key_frames_rot_x.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.x));
						key_frames_rot_y.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.y));
						key_frames_rot_z.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.z));
						key_frames_rot_w.Add(new Keyframe(keyframe.keytime * animation_length, current_quat.w));

						first = false;
						previous_quaternion = current_quat;

					}
				}
				if (keyframe.scale != null)
				{


					if (Path.GetFileName(new_name) == "head1_neck_bind")
					{
						if (head_animation_scale != 0.0f)
						{
							keyframe.scale[0] *= head_animation_scale;
							keyframe.scale[1] *= head_animation_scale;
							keyframe.scale[2] *= head_animation_scale;
						}
					}
					if (use_bone_mod == true)
					{
						if (bone_mods[bone_name].enabled)
						{
							keyframe.scale[0] *= bone_mods[bone_name].scale.x;
							keyframe.scale[2] *= bone_mods[bone_name].scale.y;
							keyframe.scale[1] *= bone_mods[bone_name].scale.z;
						}
					}

					if (Path.GetFileName(new_name) == "jt_all_bind") //AnimationScale takes the place of jt_all_bind
					{
						/*if (animationScale != 0.0f)
						{
							key_frames_scale_x.Add(new Keyframe(keyframe.keytime * animation_length, animationScale));
							key_frames_scale_y.Add(new Keyframe(keyframe.keytime * animation_length, animationScale));
							key_frames_scale_z.Add(new Keyframe(keyframe.keytime * animation_length, animationScale));
						}*/
						//else
						{
							key_frames_scale_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[0] * animation_scale));
							key_frames_scale_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[1] * animation_scale));
							key_frames_scale_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[2] * animation_scale));
						}
						jt_all_bind_scale_x = keyframe.scale[0];
						jt_all_bind_scale_y = keyframe.scale[1];
						jt_all_bind_scale_z = keyframe.scale[2];
					}
					/*else if (name == "Armature/jt_all_bind/jt_prop_bind")
					{
						key_frames_scale_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[0] * jt_all_bind_scale_x)); //why were these switched?
						key_frames_scale_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[1] * jt_all_bind_scale_y));
						key_frames_scale_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[2] * jt_all_bind_scale_z));
					}*/
					else
					{
						{
							key_frames_scale_x.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[0])); //why were these switched?
							key_frames_scale_y.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[1]));
							key_frames_scale_z.Add(new Keyframe(keyframe.keytime * animation_length, keyframe.scale[2]));
						}
					}
				}
			}

			//string new_name = name;





			/*if (bone_mods != null)
			{
				if (standard_bone_to_avatar_bone.ContainsKey(name))
				{
					new_name = standard_bone_to_avatar_bone[name];
				}
			}*/



			if (Path.GetFileName(new_name) != "jt_all_bind")
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

			else
			{
				//Used for root motion
				/*if (ac != null) {
					Vector3 previous_movement = Vector3.zero;
					float previous_time = 0;
					for (int key_frame = 0; key_frame < key_frames_pos_x.Count; key_frame++)
					{
						AnimationEvent ae = new AnimationEvent();
						ae.time = key_frames_pos_x[key_frame].time;
						ae.functionName = "rootMotionMove";

						Vector3 new_movement = new Vector3(key_frames_pos_x[key_frame].value,
								key_frames_pos_y[key_frame].value,
								key_frames_pos_z[key_frame].value);

						Vector3 result = new_movement - previous_movement;
						float result_time = key_frames_pos_x[key_frame].time - previous_time;
						ae.stringParameter = result.x + "," + result.y + "," + result.z + "," + result_time;
						previous_movement = new_movement;
						previous_time = key_frames_pos_x[key_frame].time;

						ae.objectReferenceParameter = ac;
						anim_clip.AddEvent(ae);
					}
				}*/

				/*AnimationCurve curve_pos_x = new AnimationCurve(key_frames_pos_x.ToArray());
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
				anim_clip.SetCurve(new_name, typeof(Transform), "localRotation.x", curve_rot_x);*/

				AnimationCurve curve_scale_x = new AnimationCurve(key_frames_scale_x.ToArray());
				anim_clip.SetCurve(new_name, typeof(Transform), "localScale.x", curve_scale_x);

				AnimationCurve curve_scale_y = new AnimationCurve(key_frames_scale_y.ToArray());
				anim_clip.SetCurve(new_name, typeof(Transform), "localScale.y", curve_scale_y);

				AnimationCurve curve_scale_z = new AnimationCurve(key_frames_scale_z.ToArray());
				anim_clip.SetCurve(new_name, typeof(Transform), "localScale.z", curve_scale_z);
			}
		}


		if (node.children != null)
		{
			foreach (CocosModel.Node child_node in node.children)
			{
				processAnimationNode(name, model, bone_fullname_dict, animation_length, child_node, head_animation_scale, animation_scale, ref bone_dict, ref anim_clip, ref bonemods_activated, ac, bone_mods, is_camera, jt_all_bind_scale_x, jt_all_bind_scale_y, jt_all_bind_scale_z);
			}
		}
	}

	static Dictionary<string, string> boneMODName_to_skeleton = new Dictionary<string, string>
	{
		["chin_MOD_Joint_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jaw1_Joint_bind/chin_MOD_Joint_bind",
		["jawCorners_MOD_Joint_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jaw1_Joint_bind/jawCorners_MOD_Joint_bind",
		["jt_lowLipParent_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jaw1_Joint_bind/jawCorners_MOD_Joint_bind/jt_lowLipParent_MOD_bind",
		["jt_nose_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_nose_MOD_bind",
		["jt_noseParent_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_nose_MOD_bind/jt_noseParent_MOD_bind",
		["jt_noseBridge_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_noseBridge_MOD_bind",
		["jt_L_eye_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_L_eye_MOD_bind",
		["jt_L_eyeParent_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_L_eye_MOD_bind/jt_L_eyeParent_MOD_bind",
		["jt_R_eye_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_R_eye_MOD_bind",
		["jt_R_eyeParent_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_R_eye_MOD_bind/jt_R_eyeParent_MOD_bind",
		["jt_mouth_MOD_bind"] = "Armature/jt_all_bind/jt_hips_bind/spine1_loResSpine2_bind/spine1_loResSpine3_bind/head1_neck_bind/jt_head_bind/jt_mouth_MOD_bind",
	};

	static Dictionary<string, Vector3> boneMODDefault_position = new Dictionary<string, Vector3>
	{
		["chin_MOD_Joint_bind"] = new Vector3(1.400244e-16f, -3.916735f, 2.928557f),
		["jawCorners_MOD_Joint_bind"] = new Vector3(0.00242094f, -1.961419f, -0.2943561f),
		["jt_lowLipParent_MOD_bind"] = new Vector3(-1.387779e-16f, 1.961419f, 0.294366f),
		["jt_nose_MOD_bind"] = new Vector3(-0.4869505f, 3.511287e-14f, 4.476315f),
		["jt_noseParent_MOD_bind"] = new Vector3(-3.489662e-14f, -0.4869505f, -4.476315f),
		["jt_noseBridge_MOD_bind"] = new Vector3(-2.376049f, 3.61496e-14f, 4.345019f),
		["jt_L_eye_MOD_bind"] = new Vector3(-2.574324f, -1.844644f, 3.032392f),
		["jt_L_eyeParent_MOD_bind"] = new Vector3(1.844644f, -2.574573f, -3.023612f),
		["jt_R_eye_MOD_bind"] = new Vector3(-2.574324f, 1.844644f, 3.032392f),
		["jt_R_eyeParent_MOD_bind"] = new Vector3(-1.844644f, 2.574573f, 3.023612f),
		["jt_mouth_MOD_bind"] = new Vector3(0.9160487f, -2.145447e-15f, 4.235064f),
	};

	static Dictionary<string, Quaternion> boneMODDefault_rotation = new Dictionary<string, Quaternion>
	{
		["chin_MOD_Joint_bind"] = Quaternion.Euler(new Vector3(0, 0, 0)),
		["jawCorners_MOD_Joint_bind"] = Quaternion.Euler(new Vector3(0, -0.471f, 0)),
		["jt_lowLipParent_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0.471f, 0)),
		["jt_nose_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, 90)),
		["jt_noseParent_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, -90)),
		["jt_noseBridge_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, 90)),
		["jt_L_eye_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, 90)),
		["jt_L_eyeParent_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, -90)), //0, 0, -90
		["jt_R_eye_MOD_bind"] = Quaternion.Euler(new Vector3(0, 180, 90)), //Should be 0, 180, 90
		["jt_R_eyeParent_MOD_bind"] = Quaternion.Euler(new Vector3(0, 180, 90)), //Should be 0, 180, 90
		["jt_mouth_MOD_bind"] = Quaternion.Euler(new Vector3(0, 0, 0)),
	};

	static Dictionary<string, string> bone_fixes = new Dictionary<string, string>
	{
		["jt_all_bind1"] = "jt_all_bind", //Just why
		["jt_all_bind2"] = "jt_all_bind",
		["jt_all_bind3"] = "jt_all_bind",
		["jt_all_bind4"] = "jt_all_bind",
		["jt_all_bind5"] = "jt_all_bind",
	};
	public static void setBoneMODMods(ref AnimationClip anim_clip, ref Dictionary<string, BoneMod> bone_mods)
	{
		foreach (string key in bone_mods.Keys)
		{
			if (key.Contains("MOD"))
			{
				List<Keyframe> key_frames_pos_x = new List<Keyframe>();
				List<Keyframe> key_frames_pos_y = new List<Keyframe>();
				List<Keyframe> key_frames_pos_z = new List<Keyframe>();

				List<Keyframe> key_frames_rot_x = new List<Keyframe>();
				List<Keyframe> key_frames_rot_y = new List<Keyframe>();
				List<Keyframe> key_frames_rot_z = new List<Keyframe>();
				List<Keyframe> key_frames_rot_w = new List<Keyframe>();

				List<Keyframe> key_frames_scale_x = new List<Keyframe>();
				List<Keyframe> key_frames_scale_y = new List<Keyframe>();
				List<Keyframe> key_frames_scale_z = new List<Keyframe>();

				key_frames_pos_x.Add(new Keyframe(0, bone_mods[key].translation[0] + boneMODDefault_position[key].x));
				key_frames_pos_y.Add(new Keyframe(0, bone_mods[key].translation[1] + boneMODDefault_position[key].y));
				key_frames_pos_z.Add(new Keyframe(0, bone_mods[key].translation[2] + boneMODDefault_position[key].z));



				Quaternion resulting_quaternion = bone_mods[key].rotation * boneMODDefault_rotation[key];

				key_frames_rot_x.Add(new Keyframe(0, resulting_quaternion.x));
				key_frames_rot_y.Add(new Keyframe(0, resulting_quaternion.y));
				key_frames_rot_z.Add(new Keyframe(0, resulting_quaternion.z));
				key_frames_rot_w.Add(new Keyframe(0, resulting_quaternion.w));



				key_frames_scale_x.Add(new Keyframe(0, bone_mods[key].scale[0]));
				key_frames_scale_y.Add(new Keyframe(0, bone_mods[key].scale[1]));
				key_frames_scale_z.Add(new Keyframe(0, bone_mods[key].scale[2]));



				string bone_full_name = boneMODName_to_skeleton[key];

				AnimationCurve curve_pos_x = new AnimationCurve(key_frames_pos_x.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localPosition.x", curve_pos_x);

				AnimationCurve curve_pos_y = new AnimationCurve(key_frames_pos_y.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localPosition.y", curve_pos_y);

				AnimationCurve curve_pos_z = new AnimationCurve(key_frames_pos_z.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localPosition.z", curve_pos_z);

				AnimationCurve curve_rot_w = new AnimationCurve(key_frames_rot_w.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localRotation.w", curve_rot_w);

				AnimationCurve curve_rot_y = new AnimationCurve(key_frames_rot_y.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localRotation.y", curve_rot_y);

				AnimationCurve curve_rot_z = new AnimationCurve(key_frames_rot_z.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localRotation.z", curve_rot_z);

				AnimationCurve curve_rot_x = new AnimationCurve(key_frames_rot_x.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localRotation.x", curve_rot_x);

				AnimationCurve curve_scale_x = new AnimationCurve(key_frames_scale_x.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localScale.x", curve_scale_x);

				AnimationCurve curve_scale_y = new AnimationCurve(key_frames_scale_y.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localScale.y", curve_scale_y);

				AnimationCurve curve_scale_z = new AnimationCurve(key_frames_scale_z.ToArray());
				anim_clip.SetCurve(bone_full_name, typeof(Transform), "localScale.z", curve_scale_z);
			}
		}


	}

	public class BoneMod
	{
		public BoneMod(Vector3 _p, Quaternion _r, Vector3 _s)
		{
			translation = _p;
			rotation = _r;
			scale = _s;
			enabled = true;
		}

		public BoneMod(bool _enabled)
		{
			enabled = _enabled;
		}

		public bool enabled;
		public Vector3 translation;
		public Quaternion rotation;
		public Vector3 scale;
	}

	static void activateTriggerMap(Dictionary<string, string> triggerReplacement, ConfigAnimation._Animation3D.TriggerMap trigger, AnimationClip anim_clip, float offset)
    {
		AnimationEvent animationEvent = new AnimationEvent();
		switch (trigger.id)
        {
			case "playSound":
				if (triggerReplacement != null && triggerReplacement.ContainsKey(trigger.parameters[0]))
					animationEvent.stringParameter = triggerReplacement[trigger.parameters[0]];
				else
					animationEvent.stringParameter = trigger.parameters[0];
				animationEvent.functionName = "PlaySound";
				animationEvent.time = trigger.time + offset;
				anim_clip.AddEvent(animationEvent);
				break;

			case "AttachProp":
				if (triggerReplacement != null && triggerReplacement.ContainsKey(trigger.parameters[0]))
					animationEvent.stringParameter = 
						triggerReplacement[trigger.parameters[0]] + ":" + triggerReplacement[trigger.parameters[0]] + ":" + trigger.parameters[1];
				else
					animationEvent.stringParameter = trigger.parameters[0] + ":" + trigger.parameters[0] + ":" + trigger.parameters[1];
				animationEvent.functionName = "AttachProp";
				animationEvent.time = trigger.time + offset;
				anim_clip.AddEvent(animationEvent);
				break;
			case "RemoveProp":
				if (triggerReplacement != null && triggerReplacement.ContainsKey(trigger.parameters[0]))
					animationEvent.stringParameter =
						triggerReplacement[trigger.parameters[0]];
				else
					animationEvent.stringParameter = trigger.parameters[0];
				animationEvent.functionName = "removeProp";
				animationEvent.time = trigger.time + offset;
				anim_clip.AddEvent(animationEvent);
				break;
			case "PlayPropAnim":

				if (triggerReplacement != null && triggerReplacement.ContainsKey(trigger.parameters[0]))
				{
					animationEvent.stringParameter = triggerReplacement[trigger.parameters[0]] + ":" + trigger.parameters[1];
				}
				else
					animationEvent.stringParameter = trigger.parameters[0] + ":" + trigger.parameters[1];
				animationEvent.functionName = "PlayPropAnim";
				animationEvent.time = trigger.time + 0.01f + offset;
				anim_clip.AddEvent(animationEvent);
				break;
			case "ScriptTrigger":
				animationEvent.functionName = "ScriptTrigger";
				animationEvent.stringParameter = trigger.parameters[0];
				animationEvent.time = trigger.time + 0.01f + offset;
				anim_clip.AddEvent(animationEvent);
				break;

			case "AttachParticleSystem":

				if (trigger.parameters.Length == 2)
				{

					if (triggerReplacement != null && triggerReplacement.ContainsKey(trigger.parameters[0]))
						animationEvent.stringParameter = triggerReplacement[trigger.parameters[0]] + ":" + trigger.parameters[1];
					else
						animationEvent.stringParameter = trigger.parameters[0] + ":" + trigger.parameters[1];
				}
                else
                {
					string parameters = "";
					if (triggerReplacement != null)
					{
						if (triggerReplacement.ContainsKey(trigger.parameters[0]))
							parameters += triggerReplacement[trigger.parameters[0]] + ":";
						else
							parameters += trigger.parameters[0] + ":";

						if (triggerReplacement.ContainsKey(trigger.parameters[1]))
							parameters += triggerReplacement[trigger.parameters[1]] + ":";
						else
							parameters += trigger.parameters[1] + ":";

						if (triggerReplacement.ContainsKey(trigger.parameters[2]))
							parameters += triggerReplacement[trigger.parameters[2]];
						else
							parameters += trigger.parameters[2];
					}

					else
						parameters = trigger.parameters[0] + ":" + trigger.parameters[1] + ":" + trigger.parameters[2];
					animationEvent.stringParameter = parameters;
				}

				animationEvent.functionName = "AttachParticleSystem";
				animationEvent.time = trigger.time + offset;
				anim_clip.AddEvent(animationEvent);
				break;

			default:
				 Debug.LogError("Unknown trigger id " + trigger.id + " in animation " + anim_clip.name);
				return;
		}



	}

	public static AnimationClip loadAnimationClip(string name, Model model, ConfigHPActorInfo._HPActorInfo character, Dictionary<string, string> triggerReplacement = null, ActorController ac = null, Dictionary<string, BoneMod> bone_mods = null, bool is_camera = false)
	{
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogError("loadAnimatonClip name is null or empty.");
			return null;
		}

		Dictionary<string, string> bone_fullname_dict = Common.GetTransformFullNameDict(model.jt_all_bind);

		float animationScale = 1.0f;
		float headAnimationScale = 1.0f;
		if (character != null)
		{
			animationScale = character.animationScale;
			headAnimationScale = character.headAnimationScale;
		}

		if (!Configs.config_animation.Animation3D.ContainsKey(name)){
			Debug.LogError("Animation " + name + " does not exist.");
			return null;
        }

		ConfigAnimation._Animation3D animation_config = Configs.config_animation.Animation3D[name];

		string filename = animation_config.fileName;

		CocosModel animation_c3t = C3B.loadC3B(filename, GlobalEngineVariables.animations_folder);

		if (animation_c3t == null)
		{
			Debug.Log("Couldn't find " + "animations\\" + name + ". Skipping.");
			return null;
		}

		animation_c3t.animations[0].bone_dict = new Dictionary<string, CocosModel.Animation.Bone>();

		AnimationClip anim_clip = new AnimationClip();

		foreach (CocosModel.Animation.Bone bone in animation_c3t.animations[0].bones)
		{
			animation_c3t.animations[0].bone_dict[bone.boneId] = bone;
		}

		foreach (CocosModel.Node node in animation_c3t.nodes)
		{
			List<string> bone_mods_activated = new List<string>();
			processAnimationNode("Armature", model, bone_fullname_dict, animation_c3t.animations[0].length, node, animationScale, headAnimationScale, ref animation_c3t.animations[0].bone_dict, ref anim_clip, ref bone_mods_activated, ac, bone_mods, is_camera);
		}

		if (bone_mods != null)
		{
			setBoneMODMods(ref anim_clip, ref bone_mods);
		}
		anim_clip.name = name;

		if (animation_config.triggerMap != null)
        {
			float offset = 0.0f; //Ensures events activate in the right order
			foreach (ConfigAnimation._Animation3D.TriggerMap trigger in animation_config.triggerMap)
            {
				activateTriggerMap(triggerReplacement, trigger, anim_clip, offset);
				offset += 0.001f;
            }
        }

		if (animation_config.wrapMode == "loop")
			anim_clip.wrapMode = WrapMode.Loop;
		else
			anim_clip.wrapMode = WrapMode.ClampForever;


		return anim_clip;
	}

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
}