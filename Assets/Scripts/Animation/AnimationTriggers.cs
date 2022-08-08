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


}


