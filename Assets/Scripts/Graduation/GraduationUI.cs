using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GraduationUI : MonoBehaviour
{
    public Canvas canvas;
    public Text text;
    public static GraduationUI current;
    
    void Start()
    {
        current = this;
    }

    public void showGraduation(string _text)
    {
        text.text = _text;
        canvas.enabled = true;
    }

    public void hideGraduation()
    {
        canvas.enabled = false;
    }

}
