using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SocialQuizUI : MonoBehaviour
{
    static SocialQuizUI current;

    public static event Action<bool> onSocialQuizGameFinished = delegate { };

    [SerializeField]
    private TMPro.TMP_Text question_text;

    private static System.Random rng = new System.Random();

    private static List<GameObject> active_buttons = new List<GameObject>();
    private static List<GameObject> active_results = new List<GameObject>();

    private static bool waiting_for_button_press = false;
    private static string selected_choice_id = "";
    private static List<string> correct_choice_ids = new List<string>();
    private static List<string> okay_choice_ids = new List<string>();
    private static List<string> active_choices;

    [SerializeField]
    private GameObject SuccessText;
    [SerializeField]
    private GameObject FailText;

    const float result_scale_time = 0.5f;

    public void Start()
    {
        current = this;
        question_text.gameObject.SetActive(false);
        waiting_for_button_press = true;
        selected_choice_id = "";
    }

    public static void Shuffle(List<string> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void startSocialQuiz(string question, List<string> correct_choices, List<string> okay_choices, List<string> incorrect_choices)
    {
        active_choices = new List<string>();
        active_choices.AddRange(correct_choices);
        active_choices.AddRange(okay_choices);
        active_choices.AddRange(incorrect_choices);
        correct_choice_ids = correct_choices;
        okay_choice_ids = okay_choices;
        Shuffle(active_choices);

        current.question_text.gameObject.SetActive(true);

        current.question_text.text = LocalData.getLine(question);

        Vector3 position = new Vector3(210, 50, 0);
        foreach(var choice_id in active_choices)
        {
            var choice = Instantiate(Resources.Load<GameObject>("UI/SocialQuiz/Choice"));
            choice.name = "Choice: " + choice_id;
            choice.GetComponentInChildren<TMP_Text>().text = LocalData.getLine(choice_id);
            var quiz_ui_button = choice.GetComponent<QuizUIButton>();
            quiz_ui_button.choice_id = choice_id;
            choice.transform.SetParent(current.transform);
            choice.transform.localPosition = position;
            choice.transform.localScale = new Vector3(1, 1, 1);
            active_buttons.Add(choice);

            position.y -= 35;
        }

        waiting_for_button_press = true;
    }

    public static void onButtonClicked(string choice_id)
    {
        if (!waiting_for_button_press)
            return;
        waiting_for_button_press = false;

        selected_choice_id = choice_id;
        current.StartCoroutine(showResults());
    }

    private static IEnumerator showResults()
    {
        Sound.playSFX("Button");
        yield return new WaitForSeconds(0.5f);

        var position = new Vector3(90, 50, 0);
        foreach (var choice_id in active_choices)
        {
            GameObject result;
            if (correct_choice_ids.Contains(choice_id))
            {
                result = Instantiate(Resources.Load<GameObject>("UI/SocialQuiz/CorrectChoiceResult"));
            }
            else if (okay_choice_ids.Contains(choice_id))
            {
                result = Instantiate(Resources.Load<GameObject>("UI/SocialQuiz/OkayChoiceResult"));
            }
            else
            {
                result = Instantiate(Resources.Load<GameObject>("UI/SocialQuiz/IncorrectChoiceResult"));
            }

            result.transform.SetParent(current.transform);
            result.transform.localPosition = position;
            active_results.Add(result);
            position.y -= 35;
        }

        yield return new WaitForSeconds(3);

        foreach(var g in active_buttons)
        {
            Destroy(g);
        }
        foreach (var g in active_results)
        {
            Destroy(g);
        }

        current.question_text.gameObject.SetActive(false);

        if (correct_choice_ids.Contains(selected_choice_id)  || okay_choice_ids.Contains(selected_choice_id))
            current.StartCoroutine(current.showSuccessText());
        else
            current.StartCoroutine(current.showFailText());

    }

    IEnumerator showSuccessText()
    {
        SuccessText.SetActive(true);
        Sound.playSFX("QuizSuccess");
        float elapsedTime = 0.0f;
        while (elapsedTime < result_scale_time)
        {
            SuccessText.SetActive(true);

            elapsedTime += Time.deltaTime;

            SuccessText.transform.localScale = Vector3.Lerp(new Vector3(2f, 2f, 2f), new Vector3(1, 1, 1), elapsedTime / result_scale_time);
            yield return null;
        }
        SuccessText.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(3f);
        SuccessText.SetActive(false);
        onSocialQuizGameFinished.Invoke(true);
    }

    IEnumerator showFailText()
    {
        FailText.SetActive(true);
        Sound.playSFX("QuizFail");
        float elapsedTime = 0.0f;
        while (elapsedTime < result_scale_time)
        {
            FailText.SetActive(true);

            elapsedTime += Time.deltaTime;

            FailText.transform.localScale = Vector3.Lerp(new Vector3(2f, 2f, 2f), new Vector3(1, 1, 1), elapsedTime / result_scale_time);
            yield return null;
        }
        FailText.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(3f);
        FailText.SetActive(false);
        onSocialQuizGameFinished.Invoke(false);
    }

}
