using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class ConfigHPDialogueLine : Config<ConfigHPDialogueLine>
{
    [System.Serializable]
    public class HPDialogueLine
    {
        public string cameraShot;
        public float cameraTransitionTime;
        public string dialogue;
        public string[] enterEvents;
        public string[] exitEvents;
        public string id;
        public bool initialTurn; //No clue
        public string[] barkPlaylistIds;
        public string[] barkPredicates;
        public string[] dialogueChoiceIds;
        public string[] nextTurnIds;
        public string[] nextTurnPredicates;
        public string[] emoteEvents;
        public string[] emoteResetEvents;
        public string speakerId;
        public string token;
        public string[] lookAt;
        public string[] _headOnly;
        public dynamic headOnly; //some fucker entered an integer
    }

    public Dictionary<string, HPDialogueLine> HPDialogueLines;

    public override void combine(List<ConfigHPDialogueLine> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].HPDialogueLines.Keys)
            {
                HPDialogueLines[key] = other_list[i].HPDialogueLines[key];
            }
        }
    }
}

public class ConfigDialogueChoice : Config<ConfigDialogueChoice>
{
    [System.Serializable]
    public class _DialogueChoice
    {
        public string availabalePredicate;
        public string choiceId;
        public string choiceToken;
        public bool persistChoice;
        public string className;
        public string noQteTurnId;
        public string preQteTurnId;
        public string failTurnId;
        public string qteGrade;
        public string qteId;
        public string successTurnId;
        public string reward;
    }

    public Dictionary<string, _DialogueChoice> DialogueChoice;

    public override void combine(List<ConfigDialogueChoice> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueChoice.Keys)
            {
                DialogueChoice[key] = other_list[i].DialogueChoice[key];
            }
        }
    }
}

public class ConfigHPDialogueOverride : Config<ConfigHPDialogueOverride>         //Only used in dating
{
    [System.Serializable]
    public class _HPDialogueOverride
    {
        public string id;
        public string[] barkPlaylistIds;
        public string[] nextTurnIds;
        public string[] nextTurnPredicates;
        public string overridesId;
        public string token;

        public string companionId;
        public string[] emoteEvents;
        public string[] emoteResetEvents;

        public void overrideLine(ConfigHPDialogueLine.HPDialogueLine line)
        {
            line.id = id;
            line.barkPlaylistIds = barkPlaylistIds;
            if (nextTurnIds != null)
            {
                line.nextTurnIds = nextTurnIds;
                line.nextTurnPredicates = nextTurnPredicates;
            }
            line.token = token;
            line.emoteEvents = emoteEvents;
            line.emoteResetEvents = emoteResetEvents;
        }
    }

    public Dictionary<string, _HPDialogueOverride> HPDialogueOverride;

    public override void combine(List<ConfigHPDialogueOverride> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].HPDialogueOverride.Keys)
            {
                HPDialogueOverride[key] = other_list[i].HPDialogueOverride[key];
            }
        }
    }
}

public class ConfigDialogueChoiceOverride : Config<ConfigDialogueChoiceOverride>         //Only used in dating
{
    [System.Serializable]
    public class _DialogueChoiceOverride
    {
        public string choiceToken;
        public string companionId;
        public string id;
        public string overridesId;
        public string resultCategory;
        public string reward;
        public void overrideChoice(ConfigDialogueChoice._DialogueChoice choice)
        {
            choice.choiceToken = choiceToken;
        }
    }

    public Dictionary<string, _DialogueChoiceOverride> DialogueChoiceOverride;

    public override void combine(List<ConfigDialogueChoiceOverride> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueChoiceOverride.Keys)
            {
                DialogueChoiceOverride[key] = other_list[i].DialogueChoiceOverride[key];
            }
        }
    }
}

public class ConfigDialogueSpeakers : Config<ConfigDialogueSpeakers>
{
    [System.Serializable]
    public class _DialogueSpeaker
    {
        public string animId;
        public string houseId;
        public string icon;
        public string id;
        public string[] modalModelOffset;
        public float modalModelScale;
        public string name;
        public string[] portraitModelOffset;
    }

    public Dictionary<string, _DialogueSpeaker> DialogueSpeaker;


    public override void combine(List<ConfigDialogueSpeakers> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueSpeaker.Keys)
            {
                DialogueSpeaker[key] = other_list[i].DialogueSpeaker[key];
            }
        }
    }
}

public class ConfigDialogueSpeakerMapping : Config<ConfigDialogueSpeakerMapping> {
    [System.Serializable]
    public class _DialogueSpeakerMapping
    {
        public string mapId;
        public string predicate;
        public int priority;
        public string speakerId;
    }

    public List<_DialogueSpeakerMapping> DialogueSpeakerMapping;
    
    public override void combine(List<ConfigDialogueSpeakerMapping> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
             DialogueSpeakerMapping.AddRange(other_list[i].DialogueSpeakerMapping);
        }
    }
}


public class ConfigDialoguesLoader{

    public static async Task loadConfigsAsync()
    {
        List<ConfigHPDialogueLine> list_config_hp_dialogue_line = await ConfigHPDialogueLine.getDeserializedConfigsList("HPDialogueLines");
        Configs.config_hp_dialogue_line = list_config_hp_dialogue_line[0];
        Configs.config_hp_dialogue_line.combine(list_config_hp_dialogue_line);

        List<ConfigDialogueChoice> list_config_dialogue_choice = await ConfigDialogueChoice.getDeserializedConfigsList("DialogueChoice");
        Configs.config_dialogue_choices = list_config_dialogue_choice[0];
        Configs.config_dialogue_choices.combine(list_config_dialogue_choice);

        List<ConfigHPDialogueOverride> dialogue_line_override_dict = await ConfigHPDialogueOverride.getDeserializedConfigsList("HPDialogueOverride");
        Configs.config_hp_dialogue_override = dialogue_line_override_dict[0];
        Configs.config_hp_dialogue_override.combine(dialogue_line_override_dict);

        List<ConfigDialogueChoiceOverride> dialogue_choice_override_dict = await ConfigDialogueChoiceOverride.getDeserializedConfigsList("DialogueChoiceOverride");
        Configs.config_dialogue_choice_override = dialogue_choice_override_dict[0];
        Configs.config_dialogue_choice_override.combine(dialogue_choice_override_dict);

        List<ConfigDialogueSpeakers> list_config_dialogue_speaker = await ConfigDialogueSpeakers.getDeserializedConfigsList("DialogueSpeaker");
        Configs.config_dialogue_speakers = list_config_dialogue_speaker[0];
        Configs.config_dialogue_speakers.combine(list_config_dialogue_speaker);

        List<ConfigDialogueSpeakerMapping> list_config_dialogue_speaker_mapping = await ConfigDialogueSpeakerMapping.getDeserializedConfigsList("DialogueSpeakerMapping");
        Configs.config_dialogue_speaker_mapping = list_config_dialogue_speaker_mapping[0];
        Configs.config_dialogue_speaker_mapping.combine(list_config_dialogue_speaker_mapping);

        await Task.Run(
        () =>
        {
            Configs.dialogue_dict = new Dictionary<string, List<ConfigHPDialogueLine.HPDialogueLine>>();
            foreach (ConfigHPDialogueLine.HPDialogueLine dialogue_line in Configs.config_hp_dialogue_line.HPDialogueLines.Values)
            {
                if (dialogue_line.dialogue != null)
                {
                    if (!Configs.dialogue_dict.ContainsKey(dialogue_line.dialogue))
                    {
                        Configs.dialogue_dict[dialogue_line.dialogue] = new List<ConfigHPDialogueLine.HPDialogueLine>();
                    }
                    Configs.dialogue_dict[dialogue_line.dialogue].Add(dialogue_line);
                }
            }

            foreach(ConfigHPDialogueLine.HPDialogueLine hpdl in Configs.config_hp_dialogue_line.HPDialogueLines.Values)
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
        );
    }
}



