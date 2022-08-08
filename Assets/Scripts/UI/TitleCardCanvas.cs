using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TitleCardCanvas : MonoBehaviour
{
    [SerializeField]
    private Image divider1;
    [SerializeField]
    private Image divider2;
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private Image bg;

    private void Awake()
    {
        //divider1.gameObject.SetActive(false);
        //divider2.gameObject.SetActive(false);
        //text.gameObject.SetActive(false);
        //bg.gameObject.SetActive(false);
        divider1.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        divider2.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0.0f);
        InteractionTitleCard.onShowTitleCard += showTitleCard;
    }

    private void showTitleCard(string _text)
    {
        text.text = _text;
        StartCoroutine(showTitleCardRoutine());
    }

    IEnumerator showTitleCardRoutine()
    {
        const float fadetime = 0.5f;

        yield return new WaitForSeconds(1f);


        divider1.gameObject.SetActive(true);
        divider2.gameObject.SetActive(true);
        text.gameObject.SetActive(true);
        bg.gameObject.SetActive(true);
        float startime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startime < fadetime)
        {
            float difference = (Time.realtimeSinceStartup - startime) / fadetime;
            Debug.Log(difference);
            divider1.color = new Color(1.0f, 1.0f, 1.0f, difference);
            divider2.color = new Color(1.0f, 1.0f, 1.0f, difference);
            bg.color = new Color(1.0f, 1.0f, 1.0f, difference);
            text.color = new Color(text.color.r, text.color.g, text.color.b, difference);
            yield return null;
        }

        divider1.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        divider2.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1.0f);

        yield return new WaitForSeconds(2f);

        startime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startime < fadetime)
        {
            float difference = 1.0f - (Time.realtimeSinceStartup - startime) / fadetime;
            divider1.color = new Color(1.0f, 1.0f, 1.0f, difference);
            divider2.color = new Color(1.0f, 1.0f, 1.0f, difference);
            bg.color = new Color(1.0f, 1.0f, 1.0f, difference);
            text.color = new Color(text.color.r, text.color.g, text.color.b, difference);
            yield return null;
        }

        divider1.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        divider2.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0.0f);
        divider1.gameObject.SetActive(false);
        divider2.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        bg.gameObject.SetActive(false);


    }


}
