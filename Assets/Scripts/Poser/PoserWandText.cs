using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoserWandText : MonoBehaviour
{
    Text text;
    void Start()
    {
        text = GetComponent<Text>();
    }

    void changeText(string new_text)
    {
        text.text = new_text;
    }

    void Update()
    {
        float h, s, v;
        Color.RGBToHSV(text.color, out h, out s, out v);

        // Use HSV values to increase H in HSVToRGB. It looks like putting a value greater than 1 will round % 1 it
        Color r = Color.HSVToRGB(h + Time.deltaTime, s, v);
        text.color = r;
    }

}
