using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;

public abstract class Config<T>
{
    public static async Task<T> CreateFromJSONAsync(string file)
    {
        return await Task.Run(
            () =>
            {
                byte[] byte_array = File.ReadAllBytes(file);

                if ((char)byte_array[0] != '{')
                {
                    ConfigDecrypt.decrypt(byte_array, file);
                }

                string content = Encoding.UTF8.GetString(byte_array);

                return JsonConvert.DeserializeObject<T>(content);
            }
        );
    }

    public static async Task<List<T>> getDeserializedConfigsList(string type)
    {
        return await Task.Run(
            () =>
            {
                List<T> list_configs = new List<T>();
                byte[] byte_array;
                string content;
                foreach (string config_name in Configs.config_contents.Contents[type])
                {
                    string path = Common.getConfigPath(config_name);
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
                }
                return list_configs;
            }
        );
    }

    public static async Task<List<string>> getDecryptedConfigsList(string type)
    {
        return await Task.Run(
            () =>
            {
                List<string> list_configs = new List<string>();
                byte[] byte_array;
                string content;
                foreach (string config_name in Configs.config_contents.Contents[type])
                {

                    string path = Common.getConfigPath(config_name);
                    if (path != null)
                    {
                        byte_array = File.ReadAllBytes(path);

                        if ((char)byte_array[0] != '{')
                        {
                            ConfigDecrypt.decrypt(byte_array, Common.getConfigPath(config_name));
                        }
                        content = Encoding.UTF8.GetString(byte_array);
                        list_configs.Add(content);
                    }
                }
                return list_configs;
            }
        );
    }

public abstract void combine(List<T> other_list);
}

public class ConfigContents
{
    public Dictionary<string, string[]> Contents;
    public static ConfigContents loadFromJSON(string file)
    {
        return JsonConvert.DeserializeObject<ConfigContents>(File.ReadAllText(file));
    }
}

public class Configs{

    public static event Action onConfigsLoaded = delegate { };


    public static ConfigContents config_contents;

    public static ConfigGoalChain config_goal_chain;
    public static ConfigGoal config_goal;
    public static ConfigObjective config_objective;
    public static ConfigScenario config_scenario;
    public static ConfigTexture config_texture;
    public static Config3DModel config_fx;
    public static Config3DModel config_character_model;
    public static Config3DModel config_environment_model;
    public static Config3DModel config_prop_model;
    public static Config3DModel config_outfit_model;
    public static ConfigAnimation config_animation;
    public static ConfigCharAnimSequence config_char_anim_sequence;
    public static ConfigHPActorInfo config_hp_actor_info;
    public static ConfigEncounter config_encounter;
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
    public static ConfigReward config_reward;
    public static ConfigPredicateAlias config_predicate_alias;
    public static ConfigDialogueSpeakers config_dialogue_speakers;
    public static ConfigDialogueSpeakerMapping config_dialogue_speaker_mapping;

    public static Dictionary<string, ConfigSound._Ambient> ambient_dict;
    public static Dictionary<string, ConfigSound._Playlist> playlist_dict;
    public static Dictionary<string, ConfigSound._Sound> sounds_dict;

    public static Dictionary<string, List<ConfigHPDialogueLine.HPDialogueLine>> dialogue_dict;
    public static Dictionary<string, List<ConfigHPDialogueOverride._HPDialogueOverride>> dialogue_line_override_dict;
    public static Dictionary<string, List<ConfigDialogueChoiceOverride._DialogueChoiceOverride>> dialogue_choice_override_dict;
    public static Dictionary<string, ConfigPredicateAlias._PredicateAlias> predicate_alias_dict;
    public static async Task loadConfigsAsync()
    {
        config_contents = ConfigContents.loadFromJSON(GlobalEngineVariables.configs_content_file);

        var config_dialogues_task = ConfigDialoguesLoader.loadConfigsAsync();
        var config_encounters_task = ConfigEncounterLoader.loadConfigsAsync();
        var config_characters_task = ConfigCharactersLoader.loadConfigsAsync();
        var config_animations_task = ConfigAnimationsLoader.loadConfigsAsync();
        var config_3dmodels_task = Config3dModelsLoader.loadConfigsAsync();
        var config_sounds_task = ConfigSoundsLoader.loadConfigsAsync();
        var config_quidditch_task = ConfigQuidditchLoader.loadConfigsAsync();
        var config_goals_task = ConfigGoalsLoader.loadConfigsAsync();
        var config_locations_task = ConfigLocationsLoader.loadConfigsAsync();
        var config_quests_task = ConfigQuestsLoader.loadConfigsAsync();
        var config_localdata_task = ConfigLocalDataLoader.loadConfigsAsync();
        var config_scenario_task = ConfigScenarioLoader.loadConfigsAsync();
        var config_scene_task = ConfigSceneLoader.loadConfigsAsync();
        var config_interaction_task = ConfigInteractionLoader.loadConfigsAsync();
        var config_script_events_task = ConfigScriptEventsLoader.loadConfigsAsync();
        var config_project_task = ConfigProjectLoader.loadConfigsAsync();
        var config_avatar_task = ConfigsAvatarLoader.loadConfigsAsync();
        var config_rewards_task = ConfigRewardsLoader.loadConfigsAsync();

        await config_dialogues_task;
        await config_encounters_task;
        await config_characters_task;
        await config_animations_task;
        await config_3dmodels_task;
        await config_sounds_task;
        await config_quidditch_task;
        await config_goals_task;
        await config_locations_task;
        await config_quests_task;
        await config_localdata_task;
        await config_scenario_task;
        await config_scene_task;
        await config_interaction_task;
        await config_script_events_task;
        await config_project_task;
        await config_avatar_task;
        await config_rewards_task;

        onConfigsLoaded.Invoke();
    }
}
