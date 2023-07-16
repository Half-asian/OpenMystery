using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Diagnostics;
public abstract class Config<T>
{
    public static T getJObjectsConfigsListST(string type, MergeArrayHandling mergeType = MergeArrayHandling.Replace)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        List<JObject> list_configs = new List<JObject>();
        byte[] byte_array;
        string content;
        foreach (string config_name in Configs.config_contents.Contents[type])
        {
            string path = null;
            if (config_name.Contains("\\mods\\") || config_name.Contains("/mods/"))
                path = config_name;
            else
                path = Common.getConfigPath(config_name);
            if (path != null)
            {
                byte_array = File.ReadAllBytes(path);

                if ((char)byte_array[0] != '{')
                {
                    ConfigDecrypt.decrypt(byte_array, Common.getConfigPath(config_name));
                }
                content = Encoding.UTF8.GetString(byte_array);
                list_configs.Add(JObject.Parse(content));
            }
            else
            {
                UnityEngine.Debug.Log("Could not find config " + config_name);
            }
        }
        //GameStart.logWrite(type + " loaded: " + st.Elapsed);
        for (int i = 1; i < list_configs.Count; i++)
        {
            list_configs[0].Merge(list_configs[i], new JsonMergeSettings
            {
                MergeArrayHandling = mergeType
            });
        }
        var settings = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
        var result = list_configs[0].ToObject<T>(JsonSerializer.Create(settings));

        stopwatch.Stop();
        return result;
    }

    public static List<T> getConfigList(string type)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        List<T> list_configs = new List<T>();
        byte[] byte_array;
        string content;
        foreach (string config_name in Configs.config_contents.Contents[type])
        {
            string path = null;
            if (config_name.Contains("\\mods\\") || config_name.Contains("/mods/"))
                path = config_name;
            else
                path = Common.getConfigPath(config_name);
            if (path != null)
            {
                byte_array = File.ReadAllBytes(path);

                if ((char)byte_array[0] != '{')
                {
                    ConfigDecrypt.decrypt(byte_array, Common.getConfigPath(config_name));
                }
                content = Encoding.UTF8.GetString(byte_array);
                
                list_configs.Add(JsonConvert.DeserializeObject<T>(content));
            }
            else
            {
                UnityEngine.Debug.Log("Could not find config " + config_name);
            }
        }
        return list_configs;
    }

    public abstract T combine(List<T> other_list);

}

public class ConfigContents
{
    public Dictionary<string, List<string>> Contents;
    public static ConfigContents loadFromJSON(string file)
    {
        return JsonConvert.DeserializeObject<ConfigContents>(File.ReadAllText(file));
    }
}

public class Configs{

    public static ConfigContents config_contents;

    public static ConfigGoalChain config_goal_chain;
    public static ConfigGoal config_goal;
    public static ConfigAssignment config_assignment;
    public static ConfigObjective config_objective;
    public static ConfigScenario config_scenario;
    public static ConfigTexture config_texture;
    public static Config3DModel config_3dmodel;
    public static ConfigAnimation config_animation;
    public static ConfigCharAnimSequence config_char_anim_sequence;
    public static ConfigHPActorInfo config_hp_actor_info;
    public static ConfigEncounter config_encounter;
    public static ConfigEncounterOpposition config_encounter_opposition;
    public static ConfigEncounterMood config_encounter_mood;
    public static ConfigEncounterChoice config_encounter_choice;
    public static ConfigHouse config_house;
    public static ConfigActorMapping config_actor_mapping;
    public static ConfigLocation config_location;
    public static ConfigLocationHub config_location_hub;
    public static ConfigHubNPC config_hub_npc;
    public static ConfigNpcWaypointSpawn config_npc_waypoint_spawn;
    public static ConfigLocalData config_local_data;
    public static ConfigScene config_scene;
    public static ConfigScriptEvents config_script_events;
    public static ConfigInteraction config_interaction;
    public static ConfigProject config_project;
    public static ConfigStation config_station;
    public static ConfigHPDialogueLine config_hp_dialogue_line;
    public static ConfigDialogueChoice config_dialogue_choices;
    public static ConfigTimeLimitedSideQuest config_time_limited_side_quest;
    public static ConfigYears config_years;
    public static ConfigMatch config_match;
    public static ConfigPlayPhase config_play_phase;
    public static ConfigPivotalPlay config_pivotal_play;
    public static ConfigPivotalPlayBucket config_pivotal_play_bucket;
    public static ConfigQuidditchTeam config_quidditch_team;
    public static ConfigQuidditchBroomInfo config_quidditch_broom_info;
    public static ConfigHPDialogueOverride config_hp_dialogue_override;
    public static ConfigDialogueChoiceOverride config_dialogue_choice_override;
    public static ConfigCompanion config_companion;
    public static ConfigDatePrompt config_date_prompt;
    public static ConfigSound config_sound;
    public static ConfigAvatarComponents config_avatar_components;
    public static ConfigAvatarAttributeColors config_avatar_attribute_colors;
    public static ConfigAvatarOutfitData config_avatar_outfit_data;
    public static ConfigAvatarPatchConfig config_avatar_patch_config;
    public static ConfigScriptedClothingSet config_scripted_clothing_set;
    public static ConfigReward config_reward;
    public static ConfigPredicateAlias config_predicate_alias;
    public static ConfigDialogueSpeakers config_dialogue_speakers;
    public static ConfigDialogueSpeakerMapping config_dialogue_speaker_mapping;
    public static ConfigTappie config_tappie;
    public static ConfigShaderAnimation config_shader_animation;
    public static ConfigQuizGroup config_quiz_group;
    public static ConfigQuiz config_quiz;
    public static ConfigParticleConfig config_particle_config;
    public static ConfigParticleInstance config_particle_instance;
    public static SceneEnvOverrides config_scene_env_override;

    public static Dictionary<string, ConfigSound._Ambient> ambient_dict;
    public static Dictionary<string, ConfigSound._Playlist> playlist_dict;
    public static Dictionary<string, ConfigSound._Sound> sounds_dict;
    public static Dictionary<string, ConfigSound._SFX> sfx_dict;

    public static Dictionary<string, List<ConfigHPDialogueLine.HPDialogueLine>> dialogue_dict;
    public static Dictionary<string, List<ConfigHPDialogueOverride._HPDialogueOverride>> dialogue_line_override_dict;
    public static Dictionary<string, List<ConfigDialogueChoiceOverride._DialogueChoiceOverride>> dialogue_choice_override_dict;
    public static Dictionary<string, ConfigPredicateAlias._PredicateAlias> predicate_alias_dict;
    
    public static void preload()
    {
        config_contents = ConfigContents.loadFromJSON(GlobalEngineVariables.configs_content_file);
        ModLoader.addModConfigsToContents(config_contents);
    }

    public static void postload()
    {

        //Avatar components
        if (config_avatar_components is not null)
        {

            AvatarComponents.avatar_components_hair = new List<string>();
            foreach (string key in config_avatar_components.AvatarComponents.Keys)
            {
                if (config_avatar_components.AvatarComponents[key].category == "hair")
                    AvatarComponents.avatar_components_hair.Add(key);
            }

            AvatarComponents.avatar_components_tops = new List<string>();
            foreach (string key in config_avatar_components.AvatarComponents.Keys)
            {
                if (config_avatar_components.AvatarComponents[key].category == "tops")
                    AvatarComponents.avatar_components_tops.Add(key);
            }

            AvatarComponents.avatar_components_one_piece = new List<string>();
            foreach (string key in config_avatar_components.AvatarComponents.Keys)
            {
                if (config_avatar_components.AvatarComponents[key].category == "one-piece")
                    AvatarComponents.avatar_components_one_piece.Add(key);
            }

            AvatarComponents.avatar_components_bottoms = new List<string>();
            foreach (string key in config_avatar_components.AvatarComponents.Keys)
            {
                if (config_avatar_components.AvatarComponents[key].category == "bottoms")
                    AvatarComponents.avatar_components_bottoms.Add(key);
            }

            AvatarComponents.avatar_components_facepaint = new List<string>();
            foreach (string key in config_avatar_components.AvatarComponents.Keys)
            {
                if (config_avatar_components.AvatarComponents[key].category == "facePaint")
                    AvatarComponents.avatar_components_facepaint.Add(key);
            }

            AvatarComponents.avatar_components_eyes = new List<string>();
            if (Player.local_avatar_gender == "male") {
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesM1a"].componentId);
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesM2a"].componentId);
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesM3a"].componentId);
            }
            else
            {
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesF1a"].componentId);
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesF2a"].componentId);
                AvatarComponents.avatar_components_eyes.Add(config_avatar_components.AvatarComponents["eyesF3a"].componentId);
            }

            AvatarComponents.avatar_components_lips = new List<string>();
            if (Player.local_avatar_gender == "male")
            {
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsM1"].componentId);
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsM2"].componentId);
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsM3"].componentId);
            }
            else
            {
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsF1"].componentId);
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsF2"].componentId);
                AvatarComponents.avatar_components_lips.Add(config_avatar_components.AvatarComponents["lipsF3"].componentId);
            }
        }

        //Dialogues
        if (config_hp_dialogue_line is not null)
        {
            dialogue_dict = new Dictionary<string, List<ConfigHPDialogueLine.HPDialogueLine>>();
            foreach (ConfigHPDialogueLine.HPDialogueLine dialogue_line in config_hp_dialogue_line.HPDialogueLines.Values)
            {
                if (dialogue_line.dialogue != null)
                {
                    if (!dialogue_dict.ContainsKey(dialogue_line.dialogue))
                    {
                        dialogue_dict[dialogue_line.dialogue] = new List<ConfigHPDialogueLine.HPDialogueLine>();
                    }
                    dialogue_dict[dialogue_line.dialogue].Add(dialogue_line);
                }
            }
            foreach (ConfigHPDialogueLine.HPDialogueLine hpdl in config_hp_dialogue_line.HPDialogueLines.Values)
            {
                if (hpdl.headOnly != null)
                {
                    try
                    {
                        hpdl._headOnly = (string[])hpdl.headOnly;
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            Configs.dialogue_line_override_dict = new Dictionary<string, List<ConfigHPDialogueOverride._HPDialogueOverride>>();
            foreach (ConfigHPDialogueOverride._HPDialogueOverride dialogue_override_line in Configs.config_hp_dialogue_override.HPDialogueOverride.Values)
            {
                if (!Configs.dialogue_line_override_dict.ContainsKey(dialogue_override_line.overridesId))
                {
                    Configs.dialogue_line_override_dict[dialogue_override_line.overridesId] = new List<ConfigHPDialogueOverride._HPDialogueOverride>();
                }
                Configs.dialogue_line_override_dict[dialogue_override_line.overridesId].Add(dialogue_override_line);
            }

            Configs.dialogue_choice_override_dict = new Dictionary<string, List<ConfigDialogueChoiceOverride._DialogueChoiceOverride>>();
            foreach (ConfigDialogueChoiceOverride._DialogueChoiceOverride dialogue_override_choice in Configs.config_dialogue_choice_override.DialogueChoiceOverride.Values)
            {
                if (!Configs.dialogue_choice_override_dict.ContainsKey(dialogue_override_choice.overridesId))
                {
                    Configs.dialogue_choice_override_dict[dialogue_override_choice.overridesId] = new List<ConfigDialogueChoiceOverride._DialogueChoiceOverride>();
                }
                Configs.dialogue_choice_override_dict[dialogue_override_choice.overridesId].Add(dialogue_override_choice);
            }

        }

        //Localdata
        if (config_local_data is not null)
        {
            foreach (ConfigLocalData._LocalData l in Configs.config_local_data.LocalData.Values)
            {
                if (l.es != null)
                    l.en_US = l.es;
                else if (l.pt != null)
                    l.en_US = l.pt;
            }
        }


        Configs.predicate_alias_dict = new Dictionary<string, ConfigPredicateAlias._PredicateAlias>();
        foreach (ConfigPredicateAlias._PredicateAlias p in Configs.config_predicate_alias.PredicateAlias)
        {
            Configs.predicate_alias_dict[p.aliasId] = p;
        }

        if (config_scenario is not null)
        {
            foreach (ConfigScenario._Scenario s in Configs.config_scenario.Scenario.Values)
            {
                if (s._mapLocationOverrides != null)
                {
                    try { s.mapLocationOverrides = (Dictionary<string, string>)s._mapLocationOverrides; }
                    catch { }
                }
            }
        }

        if (config_scene is not null)
        {
            foreach (ConfigScene._Scene scene in Configs.config_scene.Scene.Values)
            {
                if (scene.waypoints != null)
                {
                    scene.waypoint_dict = new Dictionary<string, ConfigScene._Scene.WayPoint>();
                    foreach (ConfigScene._Scene.WayPoint waypoint in scene.waypoints)
                    {
                        scene.waypoint_dict[waypoint.name] = waypoint;
                    }
                }
                if (scene.proplocators != null)
                {
                    scene.proplocator_dict = new Dictionary<string, ConfigScene._Scene.PropLocator>();
                    foreach (ConfigScene._Scene.PropLocator prop_locator in scene.proplocators)
                    {
                        scene.proplocator_dict[prop_locator.name] = prop_locator;
                        if (prop_locator.materials != null)
                        {
                            prop_locator.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();
                            foreach(var material in prop_locator.materials)
                            {
                                prop_locator.material_dict[material.nodeName] = material;
                            }
                        }
                    }
                }
                if (scene.cameras != null)
                {
                    scene.camera_dict = new Dictionary<string, ConfigScene._Scene.Camera>();
                    foreach (ConfigScene._Scene.Camera camera in scene.cameras)
                    {
                        scene.camera_dict[camera.name] = camera;
                    }
                }
                if (scene.hotspots != null)
                {
                    scene.hotspot_dict = new Dictionary<string, ConfigScene._Scene.HotSpot>();
                    foreach (ConfigScene._Scene.HotSpot hotspot in scene.hotspots)
                    {
                        scene.hotspot_dict[hotspot.name] = hotspot;
                    }
                }
                if (scene.envmaterials != null && scene.envmaterials.materials != null)
                {
                    scene.material_dict = new Dictionary<string, ConfigScene._Scene.Material>();
                    foreach(ConfigScene._Scene.Material mat in scene.envmaterials.materials)
                    {
                        if (scene.material_dict.ContainsKey(mat.nodeName)) //s_BroomClassroom_ChristmasDT_MS_rig has several material overides for the same nodes. The second ones seem to be erroneous.
                        {
                            continue;
                        }

                        scene.material_dict[mat.nodeName] = mat;
                    }
                }
            }
            Configs.config_scene.Scene["s_MQ5C5P1_rig"].hotspot_dict["hot_project"].position = new float[] { 507.067952f, 98.075569f, 428.769708f };

        }

        Configs.ambient_dict = new Dictionary<string, ConfigSound._Ambient>();
        Configs.playlist_dict = new Dictionary<string, ConfigSound._Playlist>();
        Configs.sounds_dict = new Dictionary<string, ConfigSound._Sound>();
        Configs.sfx_dict = new Dictionary<string, ConfigSound._SFX>();

        if (config_sound is not null)
        {
            foreach (ConfigSound._Ambient p in Configs.config_sound.Ambient)
            {
                Configs.ambient_dict[p.trigger] = p;
            }
            foreach (ConfigSound._Playlist p in Configs.config_sound.Playlist)
            {
                Configs.playlist_dict[p.playlistId] = p;
            }
            foreach (ConfigSound._Sound p in Configs.config_sound.Sounds)
            {
                Configs.sounds_dict[p.soundId] = p;
            }
            foreach (ConfigSound._SFX p in Configs.config_sound.SFX)
            {
                if (p.objectId != null)
                    Configs.sfx_dict[p.objectId] = p;
            }
        }

        if (config_goal is not null)
        {
            //This goal is for the avatar to change clothes. Not worth programming in the logic for this.
            //Configs.config_goal_chain.GoalChain["C3_v2"].goalIds.RemoveAt(3);
            //Configs.config_goal.Goals["Y1_C3_P6_v2"].dependencies = null;

            Configs.config_goal_chain.GoalChain["C2_v2"].classGoalIds.Insert(1, "Y1_C2_P4_hub"); //Tutorial triggers this class in between goals

            //Y1C3
            //Insert broom flying class within the rest of the goals. It needs to be done in order.
            Configs.config_goal_chain.GoalChain["C3_v2"].goalIds.Insert(5, new List<string> { "Y1_C3_SummonBroom_v2" });
            Configs.config_goal_chain.GoalChain["C3_v2"].classGoalIds = null;


            //Y2C2
            //Class must be completed in order
            Configs.config_goal_chain.GoalChain["Y2_C2"].goalIds[1] = new List<string>() { "Y2_C2_P2_huba", "Y2_C2_P2_hubb" };
            Configs.config_goal_chain.GoalChain["Y2_C2"].goalIds.Insert(2, new List<string>() { "Y2_C2_CG_1"});
            Configs.config_goal_chain.GoalChain["Y2_C2"].classGoalIds = null;
            Configs.config_goal.Goals["Y2_C2_P3"].dependencies = null;

            //QuidditchS1
            //Remove check to see if player has completed part of Y2
            Configs.config_goal.Goals["QuidditchS1C1_P1"].predicate = "true";
        }
        ConfigHPActorInfo._HPActorInfo chiara_adult = new ConfigHPActorInfo._HPActorInfo();
        chiara_adult.actorId = "chiara_adult";
        chiara_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        chiara_adult.modelId = "c_Chiara_adult_Casual_skin";
        Configs.config_hp_actor_info.HPActorInfo.Add("chiara_adult", chiara_adult);

        ConfigHPActorInfo._HPActorInfo chiara_mungos = new ConfigHPActorInfo._HPActorInfo();
        chiara_mungos.actorId = "chiara_mungos";
        chiara_mungos.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        chiara_mungos.modelId = "c_Chiara_adult_StMungosHealer_skin";
        Configs.config_hp_actor_info.HPActorInfo.Add("chiara_mungos", chiara_mungos);

        ConfigHPActorInfo._HPActorInfo merula_adult = new ConfigHPActorInfo._HPActorInfo();
        merula_adult.actorId = "merula_adult";
        merula_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        merula_adult.modelId = "c_MerulaHead_Y8_skin";
        merula_adult.modelPatches = new string[] { "c_MerulaBody_Y8_skin", "c_StudentHandsRosyPeach_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("merula_adult", merula_adult);

        ConfigHPActorInfo._HPActorInfo barnaby_adult = new ConfigHPActorInfo._HPActorInfo();
        barnaby_adult.actorId = "barnaby_adult";
        barnaby_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        barnaby_adult.modelId = "c_BarnabyHead_Y8_skin";
        barnaby_adult.modelPatches = new string[] { "c_BarnabyBody_Y8_skin", "c_StudentHandsWhite_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("barnaby_adult", barnaby_adult);

        ConfigHPActorInfo._HPActorInfo penny_adult = new ConfigHPActorInfo._HPActorInfo();
        penny_adult.actorId = "penny_adult";
        penny_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        penny_adult.modelId = "c_Penny_Y8_skin";
        Configs.config_hp_actor_info.HPActorInfo.Add("penny_adult", penny_adult);

        ConfigHPActorInfo._HPActorInfo jae_adult = new ConfigHPActorInfo._HPActorInfo();
        jae_adult.actorId = "jae_adult";
        jae_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        jae_adult.modelId = "c_JaeKimHead_Y8_skin";
        jae_adult.modelPatches = new string[] { "c_JaeKimBody_Y8_skin", "c_StudentHandsWhite_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("jae_adult", jae_adult);

        ConfigHPActorInfo._HPActorInfo tonks_adult = new ConfigHPActorInfo._HPActorInfo();
        tonks_adult.actorId = "tonks_adult";
        tonks_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        tonks_adult.modelId = "c_NymphadoraTonksHead_Y8_skin";
        tonks_adult.modelPatches = new string[] { "c_NymphadoraTonksBody_Y8_skin", "c_NymphadoraTonksHands_Y8_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("tonks_adult", tonks_adult);

        ConfigHPActorInfo._HPActorInfo tulip_adult = new ConfigHPActorInfo._HPActorInfo();
        tulip_adult.actorId = "tulip_adult";
        tulip_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        tulip_adult.modelId = "c_TulipHead_Y8_skin";
        tulip_adult.modelPatches = new string[] { "c_TulipBody_Y8_skin", "c_StudentHandsWhite_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("tulip_adult", tulip_adult);

        ConfigHPActorInfo._HPActorInfo murphy_adult = new ConfigHPActorInfo._HPActorInfo();
        murphy_adult.actorId = "murphy_adult";
        murphy_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        murphy_adult.modelId = "c_MurphyMcNullyHead_Y8_skin";
        murphy_adult.modelPatches = new string[] { "c_MurphyMcNullyArms_Y8_skin", "c_MurphyMcNullyBody_Y8_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("murphy_adult", murphy_adult);

        ConfigHPActorInfo._HPActorInfo ruby = new ConfigHPActorInfo._HPActorInfo();
        ruby.actorId = "ruby";
        ruby.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        ruby.modelId = "c_RubyHead_Y8_skin";
        ruby.modelPatches = new string[] { "c_RubyBody_Y8_skin", "c_StudentHandsWhite_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("ruby", ruby);

        ConfigHPActorInfo._HPActorInfo skye_adult = new ConfigHPActorInfo._HPActorInfo();
        skye_adult.actorId = "skye_adult";
        skye_adult.animId_idle = "c_Stu_IdleStandingNeutral01-Y8";
        skye_adult.modelId = "c_SkyeParkin_Head_Y8_skin";
        skye_adult.modelPatches = new string[] { "c_SkyeParkin_Body_Y8_skin" };
        Configs.config_hp_actor_info.HPActorInfo.Add("skye_adult", skye_adult);

        ConfigHPActorInfo._HPActorInfo harry = new ConfigHPActorInfo._HPActorInfo();
        harry.actorId = "harry";
        harry.animId_idle = "c_Stu_IdleStanding01";
        harry.modelId = "c_HarryPotter_skin";
        Configs.config_hp_actor_info.HPActorInfo.Add("harry", harry);

        Configs.config_hp_actor_info.HPActorInfo["c_TonksMasquerade_skin"].modelId = "c_hair_Tonks_skin";
        Configs.config_hp_actor_info.HPActorInfo["c_TonksMasquerade_skin"].modelPatches = new string[] { "c_NymphadoraTonksFaceClavicle_skin", "c_NPC_wand_generic_skin", "c_Female_MasqueradeBallOutfit_outfit_skin", "c_StudentHandsWhite_skin" };

        Configs.predicate_alias_dict["isAdultContext"] = new ConfigPredicateAlias._PredicateAlias();
        Configs.predicate_alias_dict["isAdultContext"].aliasId = "isAdultContext";
        Configs.predicate_alias_dict["isAdultContext"].aliasedPredicate = "false";
    }

    public static ReferenceTree reference_tree;

    public static void loadConfigModelInspector()
    {
        config_texture = ConfigTexture.getJObjectsConfigsListST("TextureConfig");
        config_3dmodel = Config3DModel.getJObjectsConfigsListST("3DModelConfig");
        config_animation = ConfigAnimation.getJObjectsConfigsListST("Animation3D");
        config_char_anim_sequence = ConfigCharAnimSequence.getJObjectsConfigsListST("CharAnimSequence");
        config_avatar_components = ConfigAvatarComponents.getJObjectsConfigsListST("AvatarComponents");
        config_avatar_attribute_colors = ConfigAvatarAttributeColors.getJObjectsConfigsListST("AvatarAttributeColors");
        config_avatar_outfit_data = ConfigAvatarOutfitData.getJObjectsConfigsListST("AvatarOutfitData");
        config_avatar_patch_config = ConfigAvatarPatchConfig.getJObjectsConfigsListST("AvatarPatchConfig");
        config_scripted_clothing_set = ConfigScriptedClothingSet.getJObjectsConfigsListST("ScriptedClothingSet");
        config_hp_actor_info = ConfigHPActorInfo.getJObjectsConfigsListST("HPActorInfo");
        config_actor_mapping = ConfigActorMapping.getJObjectsConfigsListST("ActorMapping");
        config_house = ConfigHouse.getJObjectsConfigsListST("House");
        config_predicate_alias = ConfigPredicateAlias.getJObjectsConfigsListST("PredicateAlias");
        config_shader_animation = ConfigShaderAnimation.getJObjectsConfigsListST("ShaderAnimation");
        config_quidditch_broom_info = ConfigQuidditchBroomInfo.getJObjectsConfigsListST("QuidditchBroomInfo");
        config_local_data = ConfigLocalData.getConfig();
        config_particle_instance = ConfigParticleInstance.getJObjectsConfigsListST("ParticleInstance");
        config_particle_config = ConfigParticleConfig.getJObjectsConfigsListST("ParticleConfig");
        Configs.config_3dmodel.createMaterialDict();
    }

    public static void loadConfigAll()
    {
        config_texture = ConfigTexture.getJObjectsConfigsListST("TextureConfig");
        config_3dmodel = Config3DModel.getJObjectsConfigsListST("3DModelConfig");
        config_animation = ConfigAnimation.getJObjectsConfigsListST("Animation3D");
        config_char_anim_sequence = ConfigCharAnimSequence.getJObjectsConfigsListST("CharAnimSequence");
        config_avatar_components = ConfigAvatarComponents.getJObjectsConfigsListST("AvatarComponents");
        config_avatar_attribute_colors = ConfigAvatarAttributeColors.getJObjectsConfigsListST("AvatarAttributeColors");
        config_avatar_outfit_data = ConfigAvatarOutfitData.getJObjectsConfigsListST("AvatarOutfitData");
        config_avatar_patch_config = ConfigAvatarPatchConfig.getJObjectsConfigsListST("AvatarPatchConfig");
        config_scripted_clothing_set = ConfigScriptedClothingSet.getJObjectsConfigsListST("ScriptedClothingSet");
        config_hp_actor_info = ConfigHPActorInfo.getJObjectsConfigsListST("HPActorInfo");
        config_actor_mapping = ConfigActorMapping.getJObjectsConfigsListST("ActorMapping");
        config_house = ConfigHouse.getJObjectsConfigsListST("House");
        config_hp_dialogue_line = ConfigHPDialogueLine.getConfig();
        config_dialogue_choices = ConfigDialogueChoice.getConfig();
        config_hp_dialogue_override = ConfigHPDialogueOverride.getJObjectsConfigsListST("HPDialogueOverride");
        config_dialogue_choice_override = ConfigDialogueChoiceOverride.getJObjectsConfigsListST("DialogueChoiceOverride");
        config_dialogue_speakers = ConfigDialogueSpeakers.getJObjectsConfigsListST("DialogueSpeaker");
        config_dialogue_speaker_mapping = ConfigDialogueSpeakerMapping.getJObjectsConfigsListST("DialogueSpeakerMapping");
        config_companion = ConfigCompanion.getJObjectsConfigsListST("Companion");
        config_date_prompt = ConfigDatePrompt.getJObjectsConfigsListST("DatePrompt");
        config_encounter = ConfigEncounter.getJObjectsConfigsListST("Encounter");
        config_encounter_opposition = ConfigEncounterOpposition.getJObjectsConfigsListST("EncounterOpposition");
        config_encounter_mood = ConfigEncounterMood.getJObjectsConfigsListST("EncounterMood");
        config_encounter_choice = ConfigEncounterChoice.getJObjectsConfigsListST("EncounterChoice");
        config_objective = ConfigObjective.getJObjectsConfigsListST("Objectives");
        config_goal_chain = ConfigGoalChain.getJObjectsConfigsListST("GoalChain");
        config_goal = ConfigGoal.getJObjectsConfigsListST("Goals");
        config_assignment = ConfigAssignment.getJObjectsConfigsListST("Assignment");
        config_interaction = ConfigInteraction.getConfig();
        config_local_data = ConfigLocalData.getConfig();
        config_predicate_alias = ConfigPredicateAlias.getJObjectsConfigsListST("PredicateAlias");
        config_location = ConfigLocation.getJObjectsConfigsListST("Location");
        config_location_hub = ConfigLocationHub.getJObjectsConfigsListST("LocationHub");
        config_hub_npc = ConfigHubNPC.getJObjectsConfigsListST("HubNpc");
        config_npc_waypoint_spawn = ConfigNpcWaypointSpawn.getJObjectsConfigsListST("NpcWaypointSpawn");
        config_project = ConfigProject.getJObjectsConfigsListST("Project");
        config_station = ConfigStation.getJObjectsConfigsListST("Station");
        config_years = ConfigYears.getJObjectsConfigsListST("Years");
        config_time_limited_side_quest = ConfigTimeLimitedSideQuest.getJObjectsConfigsListST("TimeLimitedSideQuest");
        config_quidditch_team = ConfigQuidditchTeam.getJObjectsConfigsListST("QuidditchTeam");
        config_quidditch_broom_info = ConfigQuidditchBroomInfo.getJObjectsConfigsListST("QuidditchBroomInfo");
        config_match = ConfigMatch.getJObjectsConfigsListST("Match");
        config_pivotal_play_bucket = ConfigPivotalPlayBucket.getJObjectsConfigsListST("PivotalPlayBucket");
        config_play_phase = ConfigPlayPhase.getJObjectsConfigsListST("PlayPhase");
        config_pivotal_play = ConfigPivotalPlay.getJObjectsConfigsListST("PivotalPlay");
        config_reward = ConfigReward.getJObjectsConfigsListST("Reward");
        config_scenario = ConfigScenario.getJObjectsConfigsListST("Scenario");
        config_scene = ConfigScene.getJObjectsConfigsListST("Scene");
        config_script_events = ConfigScriptEvents.getConfig();
        config_sound = ConfigSound.getJObjectsConfigsListST("Playlist", MergeArrayHandling.Union);
        config_tappie = ConfigTappie.getJObjectsConfigsListST("Tappie");
        config_shader_animation = ConfigShaderAnimation.getJObjectsConfigsListST("ShaderAnimation");
        config_quiz_group = ConfigQuizGroup.getJObjectsConfigsListST("QuizGroup");
        config_quiz = ConfigQuiz.getJObjectsConfigsListST("Quiz");
        config_particle_instance = ConfigParticleInstance.getJObjectsConfigsListST("ParticleInstance");
        config_particle_config = ConfigParticleConfig.getJObjectsConfigsListST("ParticleConfig");

        config_scene_env_override = SceneEnvOverrides.getJObjectsConfigsListST("SceneOverride");

        Configs.config_3dmodel.createMaterialDict();
    }
}
