using System.Collections.Generic;
using System.Diagnostics;
using static ConfigInteraction;
using static ConfigQuizGroup;

public class ConfigInteraction : Config<ConfigInteraction>
{
    [System.Serializable]
    public class Interaction
    {
        public string id;
        public string dialogId;
        public string filterPredicate;
        public string groupId;
        public string spot;
        public string projectId;
        public string encounterId;
        public string scenarioId;
        public string hudDialogSpeaker;
        public string endHudDialog;
        public string matchId;
        public string successReward;
        public string titleCardTitle;
        public string quizOrGroupId;
        public int? maxToShow;//unused
        public int? minToShow;//unused
        public int? maxProgress;//unused
        public int? groupProgress;
        public int? projectProgress;
        public int? progressRequired;
        public bool? autoSelect;
        public string type;
        public string[] groupMembers;
        public string[] leadsTo;
        public string[] leadsToPredicate;
        public string[] loctags; //unused
        public string[] enterEvents;
        public string[] exitEvents;
        public string[] qteSuccessEvents;
        public string[] qteFailEvents;
        public string[] successEvents;
        public string[] failEvents; //unknown how these are triggered. possibly a mistake

        public int MaxToShow => (maxToShow ?? 0); //unused
        public int MinToShow => (minToShow ?? 0); //unused
        public int MaxProgress => (maxProgress ?? 0); //unused
        public int GroupProgress => (groupProgress ?? 0);
        public int ProjectProgress => (projectProgress ?? 0);
        public int ProgressRequired => (progressRequired ?? 9999);
        public bool AutoSelect => (autoSelect ?? false);


    }

    public Dictionary<string, Interaction> Interactions;

    Interaction combineInteraction(Interaction a, Interaction b)
    {
        a.id = b.id ?? a.id;
        a.dialogId = b.dialogId ?? a.dialogId;
        a.filterPredicate = b.filterPredicate ?? a.filterPredicate;
        a.groupId = b.groupId ?? a.groupId;
        a.spot = b.spot ?? a.spot;
        a.projectId = b.projectId ?? a.projectId;
        a.encounterId = b.encounterId ?? a.encounterId;
        a.scenarioId = b.scenarioId ?? a.scenarioId;
        a.hudDialogSpeaker = b.hudDialogSpeaker ?? a.hudDialogSpeaker;
        a.endHudDialog = b.endHudDialog ?? a.endHudDialog;
        a.matchId = b.matchId ?? a.matchId;
        a.successReward = b.successReward ?? a.successReward;
        a.groupProgress = b.groupProgress ?? a.groupProgress;
        a.projectProgress = b.projectProgress ?? a.projectProgress;
        a.progressRequired = b.progressRequired ?? a.progressRequired;
        a.autoSelect = b.autoSelect ?? a.autoSelect;
        a.type = b.type ?? a.type;
        a.groupMembers = b.groupMembers ?? a.groupMembers;
        a.leadsTo = b.leadsTo ?? a.leadsTo;
        a.leadsToPredicate = b.leadsToPredicate ?? a.leadsToPredicate;
        a.enterEvents = b.enterEvents ?? a.enterEvents;
        a.exitEvents = b.exitEvents ?? a.exitEvents;
        a.qteSuccessEvents = b.qteSuccessEvents ?? a.qteSuccessEvents;
        a.qteFailEvents = b.qteFailEvents ?? a.qteFailEvents;
        a.successEvents = b.successEvents ?? a.successEvents;
        a.failEvents = b.failEvents ?? a.failEvents;
        return a;
    }

    public override ConfigInteraction combine(List<ConfigInteraction> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Interactions.Keys)
            {
                if (Interactions.ContainsKey(key))
                {
                    Interactions[key] = combineInteraction(Interactions[key], other_list[i].Interactions[key]);
                }
                else
                {
                    Interactions[key] = other_list[i].Interactions[key];
                }
            }
        }
        return this;
    }

    public static ConfigInteraction getConfig()
    {
        string type = "Interactions";
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<ConfigInteraction> configs = getConfigList(type);
        configs[0].combine(configs);
        return configs[0];
    }

}

public class ConfigQuizGroup : Config<ConfigQuizGroup>
{
    [System.Serializable]
    public class _QuizGroup
    {
        public string id;
        public string[] quizIds;
    }
    public Dictionary<string, _QuizGroup> QuizGroup;

    public override ConfigQuizGroup combine(List<ConfigQuizGroup> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].QuizGroup.Keys)
            {
                if (!QuizGroup.ContainsKey(key))
                {
                    QuizGroup[key] = other_list[i].QuizGroup[key];
                }
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_quiz_group = getJObjectsConfigsListST("QuizGroup");
    }
}

public class ConfigQuiz : Config<ConfigQuiz>
{
    [System.Serializable]
    public class _Quiz
    {
        public string id;
        public string question;
        public string correctAnswer;
        public string[] wrongAnswers;
    }
    public Dictionary<string, _Quiz> Quiz;

    public override ConfigQuiz combine(List<ConfigQuiz> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (string key in other_list[i].Quiz.Keys)
            {
                if (!Quiz.ContainsKey(key))
                {
                    Quiz[key] = other_list[i].Quiz[key];
                }
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_quiz = getJObjectsConfigsListST("Quiz");
    }
}

