using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public static partial class AnimationManager
{
	static void processTriggerMap(Dictionary<string, string> triggerReplacement, ConfigAnimation._Animation3D.TriggerMap trigger, float offset)
	{
		if (triggerReplacement == null)
			triggerReplacement = new Dictionary<string, string>();
		AnimationEvent animationEvent = new AnimationEvent();
		switch (trigger.id.ToLower()) //Capitalization is not consistent
		{
			case "playsound":
				{
					string sound_id = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];

					animationEvent.stringParameter = sound_id;
					animationEvent.functionName = "PlaySound";
					animationEvent.time = trigger.time * anim_clip.length + offset;
					anim_clip.AddEvent(animationEvent);
					break;
				}
			case "attachprop":
				{
					string model_id = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];
					string bone_id = triggerReplacement.ContainsKey(trigger.parameters[1]) ? triggerReplacement[trigger.parameters[1]] : trigger.parameters[1]; ;
					if (trigger.parameters.Length < 3)
					{
                        animationEvent.stringParameter = model_id + ":" + bone_id;
                        Debug.Log("AttachProp: model_id: " + model_id + " bone_id: " + bone_id);
                    }
                    else
					{
                        string prop_id = triggerReplacement.ContainsKey(trigger.parameters[2]) ? triggerReplacement[trigger.parameters[2]] : trigger.parameters[2];
                        animationEvent.stringParameter = model_id + ":" + bone_id + ":" + prop_id;
                        Debug.Log("AttachProp: model_id: " + model_id + " bone_id: " + bone_id + " prop_id: " + prop_id);
                    }

                    animationEvent.functionName = "AttachProp";
					animationEvent.time = trigger.time * anim_clip.length + offset;
					anim_clip.AddEvent(animationEvent);
					break;
				}
			case "removeprop":
				{
					string prop_id = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];

					animationEvent.stringParameter = prop_id;
					animationEvent.functionName = "removeProp";
					animationEvent.time = trigger.time * anim_clip.length + offset;
					anim_clip.AddEvent(animationEvent);
					break;
				}
			case "playpropanim":
				{
					string prop_id = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];
                    string anim_id = triggerReplacement.ContainsKey(trigger.parameters[1]) ? triggerReplacement[trigger.parameters[1]] : trigger.parameters[1];

					animationEvent.stringParameter = prop_id + ":" + anim_id;
					animationEvent.functionName = "PlayPropAnim";
					animationEvent.time = trigger.time * anim_clip.length + 0.01f + offset;
					anim_clip.AddEvent(animationEvent);
                    Debug.Log("playpropanim: prop_id: " + prop_id + " anim_id: " + anim_id);
                    break;
				}
			case "scripttrigger":
				{
					string trigger_id = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];

                    animationEvent.functionName = "ScriptTrigger";
					animationEvent.stringParameter = trigger_id;
					animationEvent.time = trigger.time * anim_clip.length + 0.01f + offset;
					anim_clip.AddEvent(animationEvent);
					break;
				}
			case "attachparticlesystem":
				{
					string particle_name = triggerReplacement.ContainsKey(trigger.parameters[0]) ? triggerReplacement[trigger.parameters[0]] : trigger.parameters[0];
					string attach_bone = triggerReplacement.ContainsKey(trigger.parameters[1]) ? triggerReplacement[trigger.parameters[1]] : trigger.parameters[1];

					if (trigger.parameters.Length < 3)
					{
						animationEvent.stringParameter = particle_name + ":" + attach_bone;
					}
					else
					{
                        string attach_prop = triggerReplacement.ContainsKey(trigger.parameters[2]) ? triggerReplacement[trigger.parameters[2]] : trigger.parameters[2];
						animationEvent.stringParameter = particle_name + ":" + attach_bone + ":" + attach_prop;
					}

					animationEvent.functionName = "AttachParticleSystem";
					animationEvent.time = trigger.time * anim_clip.length + offset;
					anim_clip.AddEvent(animationEvent);
					break;
				}

			default:
				Debug.LogError("Unknown trigger id " + trigger.id + " in animation " + anim_clip.name);
				return;
		}
	}


}


