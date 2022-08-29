using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizUIButton : MonoBehaviour
{
    public string choice_id;

    public void buttonClicked()
    {
        QuizUI.onButtonClicked(choice_id);
    }

}
