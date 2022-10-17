using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizUIButton : MonoBehaviour
{
    public string choice_id;
    public bool is_social_quiz;

    public void buttonClicked()
    {
        if (is_social_quiz)
        {
            SocialQuizUI.onButtonClicked(choice_id);
        }
        else
        { 
            QuizUI.onButtonClicked(choice_id);
        }
    }

}
