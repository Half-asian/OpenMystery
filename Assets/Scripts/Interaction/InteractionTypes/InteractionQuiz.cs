using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using static ConfigInteraction;

public class InteractionQuiz : Interaction
{
    ConfigQuizGroup._QuizGroup quizGroup;
    ConfigQuiz._Quiz quiz;

    public override Interaction setup(ref ConfigInteraction.Interaction _interaction)
    {
        base.setup(ref _interaction);

        if (Configs.config_quiz_group.QuizGroup.ContainsKey(_interaction.quizOrGroupId))
            quizGroup = Configs.config_quiz_group.QuizGroup[_interaction.quizOrGroupId];
        else if (Configs.config_quiz.Quiz.ContainsKey(_interaction.quizOrGroupId))
            quiz = Configs.config_quiz.Quiz[_interaction.quizOrGroupId];
        else
            throw new Exception("Couldn't find quiz group or quiz " + _interaction.quizOrGroupId);

        activate();
        return this;
    }

    public override void onFinishedEnterEvents()
    {
        base.onFinishedEnterEvents();

        if (quizGroup != null)
        {
            System.Random random = new System.Random();
            int index = random.Next(quizGroup.quizIds.Length);
            string quizid = quizGroup.quizIds[index];
            if (!Configs.config_quiz.Quiz.ContainsKey(quizid))
                throw new Exception("Couldn't find quiz " + quizid);
            quiz = Configs.config_quiz.Quiz[quizid];
        }

        QuizUI.onQuizGameFinished += onQuizGameFinished;
        QuizUI.startQuizGame(quiz.question, quiz.correctAnswer, quiz.wrongAnswers.ToList());
    }

    public void onQuizGameFinished(bool quiz_success)
    {
        QuizUI.onQuizGameFinished -= onQuizGameFinished;
        interactionComplete(quiz_success);
    }

}
