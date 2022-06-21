using System.Collections.Generic;
using System.Diagnostics;

public class ConfigHPDialogueLine : Config<ConfigHPDialogueLine>
{
    [System.Serializable]
    public class HPDialogueLine
    {
        public string cameraShot;
        public float? cameraTransitionTime;
        public string dialogue;
        public string[] enterEvents;
        public string[] exitEvents;
        public string id;
        public bool? initialTurn;
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

        public float CameraTransitionTime => cameraTransitionTime ?? 0.0f;
    }

    public Dictionary<string, HPDialogueLine> HPDialogueLines;
    HPDialogueLine combineDialogueLine(HPDialogueLine a, HPDialogueLine b)
    {
        a.cameraShot = b.cameraShot ?? a.cameraShot;
        a.cameraTransitionTime = b.cameraTransitionTime ?? a.cameraTransitionTime;
        a.dialogue = b.dialogue ?? a.dialogue;
        a.enterEvents = b.enterEvents ?? a.enterEvents;
        a.exitEvents = b.exitEvents ?? a.exitEvents;
        a.id = b.id ?? a.id;
        a.initialTurn = b.initialTurn ?? a.initialTurn;
        a.barkPlaylistIds = b.barkPlaylistIds ?? a.barkPlaylistIds;
        a.barkPredicates = b.barkPredicates ?? a.barkPredicates;
        a.dialogueChoiceIds = b.dialogueChoiceIds ?? a.dialogueChoiceIds;
        a.nextTurnIds = b.nextTurnIds ?? a.nextTurnIds;
        a.nextTurnPredicates = b.nextTurnPredicates ?? a.nextTurnPredicates;
        a.emoteEvents = b.emoteEvents ?? a.emoteEvents;
        a.emoteResetEvents = b.emoteResetEvents ?? a.emoteResetEvents;
        a.speakerId = b.speakerId ?? a.speakerId;
        a.token = b.token ?? a.token;
        a.lookAt = b.lookAt ?? a.lookAt;
        a.headOnly = b.headOnly ?? a.headOnly;

        return a;
    }
    public override ConfigHPDialogueLine combine(List<ConfigHPDialogueLine> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].HPDialogueLines.Keys)
            {
                if (HPDialogueLines.ContainsKey(key))
                    HPDialogueLines[key] = combineDialogueLine(HPDialogueLines[key], other_list[i].HPDialogueLines[key]);
                else
                    HPDialogueLines[key] = other_list[i].HPDialogueLines[key];
            }
        }
        return this;
    }

    public static ConfigHPDialogueLine getConfig()
    {
        string type = "HPDialogueLines";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigHPDialogueLine> configs = getConfigList(type);
        configs[0].combine(configs);
        GameStart.logWrite(type + ": " + stopwatch.Elapsed);
        return configs[0];
    }
}

public class ConfigDialogueChoice : Config<ConfigDialogueChoice>
{
    [System.Serializable]
    public class _DialogueChoice
    {
        public string availablePredicate; //todo
        public string choiceId;
        public string choiceToken;
        public bool persistChoice; //unused
        public string className; //unused
        public string noQteTurnId;
        public string preQteTurnId;
        public string failTurnId; //unused
        public string qteGrade; //unused
        public string qteId; //unused
        public string successTurnId;
        public string reward; //todo
    }

    public Dictionary<string, _DialogueChoice> DialogueChoice;

    _DialogueChoice combineDialogueChoice(_DialogueChoice a, _DialogueChoice b) {
        a.availablePredicate = b.availablePredicate ?? a.availablePredicate;
        a.choiceId = b.choiceId ?? a.choiceId;
        a.choiceToken = b.choiceToken ?? a.choiceToken;
        a.noQteTurnId = b.noQteTurnId ?? a.noQteTurnId;
        a.preQteTurnId = b.preQteTurnId ?? a.preQteTurnId;
        a.successTurnId = b.successTurnId ?? a.successTurnId;
        a.reward = b.reward ?? a.reward;
        return a;
    }
    public override ConfigDialogueChoice combine(List<ConfigDialogueChoice> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueChoice.Keys)
            {
                if (DialogueChoice.ContainsKey(key))
                    DialogueChoice[key] = combineDialogueChoice(DialogueChoice[key], other_list[i].DialogueChoice[key]);
                else
                    DialogueChoice[key] = other_list[i].DialogueChoice[key];
            }
        }
        return this;
    }

    public static ConfigDialogueChoice getConfig()
    {
        string type = "DialogueChoice";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigDialogueChoice> configs = getConfigList(type);
        configs[0].combine(configs);
        GameStart.logWrite(type + ": " + stopwatch.Elapsed);
        return configs[0];
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

    public override ConfigHPDialogueOverride combine(List<ConfigHPDialogueOverride> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].HPDialogueOverride.Keys)
            {
                HPDialogueOverride[key] = other_list[i].HPDialogueOverride[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_hp_dialogue_override = getJObjectsConfigsListST("HPDialogueOverride");
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

    public override ConfigDialogueChoiceOverride combine(List<ConfigDialogueChoiceOverride> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueChoiceOverride.Keys)
            {
                DialogueChoiceOverride[key] = other_list[i].DialogueChoiceOverride[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_dialogue_choice_override = getJObjectsConfigsListST("DialogueChoiceOverride");
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


    public override ConfigDialogueSpeakers combine(List<ConfigDialogueSpeakers> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].DialogueSpeaker.Keys)
            {
                DialogueSpeaker[key] = other_list[i].DialogueSpeaker[key];
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_dialogue_speakers = getJObjectsConfigsListST("DialogueSpeaker");
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
    
    public override ConfigDialogueSpeakerMapping combine(List<ConfigDialogueSpeakerMapping> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
             DialogueSpeakerMapping.AddRange(other_list[i].DialogueSpeakerMapping);
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_dialogue_speaker_mapping = getJObjectsConfigsListST("DialogueSpeakerMapping");
    }
}



