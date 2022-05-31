using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SplashText : MonoBehaviour
{

    List<string> splash_texts = new List<string>()
    {
        "Woo, /hpgg/!",
        "Merula a cute!",
        "Merula Snyde!",
        "Merula!",
        "Wurgh!",
        "Give me Skye romance!",
        "\"...\" - Erika Rath",
        "tfw no witch gf",
        "Did you know:\n Merula and Skye are cousins?",
        "Did you know:\n Chiara is Borf's mother?",
        "Hufflepuff will rise!",
        "Oh shit, I'm sorry.",
        "Drain Gang!",
        "Scum Gang!",
        "BOOBA!",
        "Hey Ron, can we say \nfuck in this game?",
        "White Boy Summer!",
        "Come to Brazil!",
        "We're all gonna make it!",
        "Drink Monster Energy",
        "Made with Unity? Shocker!",
        "Sneed's Feed and Seed.",
        "Khazar milkers.",
        "RIP Robyn Thistlethwaite",
        "Benny Harvey R.I.P",
        "The game was rigged\n from the start.",
        "Nowadays, anything can be art.\n This splash is art.",
        "My demented fantasy.",
        "My sick fantasy world.",
        "My deep, dark fantasy.",
        "I want Rath to crush\n my head with her thighs.",
        "A bean!",
        "Legalize heroin!",
        "I'm a targeted individual.",
        "Young Jennifer Connelly.",
        "Need it or keep it?",
        "Liz Tuttle fucks horses.",
        "The weak should\n fear the strong.",
        "Hail to the burger king.",
        "Epstein didn't kill himself.",
        "Jam City, hire me!",
        "Chrrp!",
        "Shoutout to Zero!",
        "Don't Stop 'Til You Get Enough.",
        "Go Braidless.",
        "I'm an Island Boy.",
        "sposnored by Saudi oil interests",
        "Take Anti-Depressants\nTo Fix Problems",
        "You hate this game\nbecause you hate yourself.",
        "Get to know\nthe name, C----Sucker",
        "$100 Meru Lover",

    };

    void Start()
    {
        int random_number = Random.Range(0, splash_texts.Count);

        GetComponent<Text>().text = splash_texts[random_number];
    }

    /*public float max_size;
    public float min_size;
    public float speed;
    public bool get_bigger;
    float scale;
    void Update()
    {
        if (get_bigger)
        {
            if (scale < max_size)
            {
                scale += speed * Time.deltaTime;
            }
            else
            {
                get_bigger = false;
            }
        }
        else
        {
            if (scale > min_size)
            {
                scale -= speed * Time.deltaTime;
            }
            else
            {
                get_bigger = true;
            }
        }

        transform.localScale = new Vector3(scale, scale, scale);
    }*/

    public float scale_add;
    public float scale_multiplier;
    public float sin_speed;
    private void Update()
    {
        float scale = Mathf.Abs(Mathf.Sin(Time.realtimeSinceStartup * sin_speed));
        scale += scale_add;
        scale *= scale_multiplier;

        transform.localScale = new Vector3(scale, scale, scale);
    
    }

}
