using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FocusUI : MonoBehaviour
{

    private static FocusUI current;
    public static event Action<bool> onFocusGameFinished = delegate { };

    [SerializeField]
    private GameObject FocusGameGroup;
    [SerializeField]
    private GameObject StartFocusButton;
    [SerializeField]
    private GameObject InnerRing;
    [SerializeField]
    private GameObject FocusBackgroundSuccess;
    [SerializeField]
    private GameObject FocusBackgroundFail;
    [SerializeField]
    private GameObject HelpText;
    [SerializeField]
    private GameObject PressText;
    [SerializeField]
    private GameObject SuccessText;
    [SerializeField]
    private GameObject FailText;


    const float result_scale_time = 0.5f;
    const float frequency = 0.2f; // in Hz
    Vector3 minScale = new Vector3(0.3f, 0.3f, 1.0f);
    Vector3 maxScale = new Vector3(2.0f, 2.0f, 1.0f);

    const float minSuccessScale = 0.78f;
    const float maxSuccessScale = 1.0f;

    void Start()
    {
        current = this;
        FocusGameGroup.SetActive(false);
        StartFocusButton.SetActive(false);
        FocusBackgroundSuccess.SetActive(false);
        FocusBackgroundFail.SetActive(false);
    }

    public static void startFocusGame()
    {
        current.StartFocusButton.SetActive(true);
        current.StartCoroutine(spaceStartGame());
    }

    public void onStartFocusButtonPressed()
    {
        FocusGameGroup.SetActive(true);
        StartFocusButton.SetActive(false);
        HelpText.SetActive(true);
        StartCoroutine(focusGame());
    }

    static IEnumerator spaceStartGame()
    {
        yield return null;
        while (true)
        {
            if (current.StartFocusButton.activeSelf)
            {
                if (Input.GetKeyDown("space"))
                {
                    current.onStartFocusButtonPressed();
                    yield break;
                }
            }
            else
                yield break;
            yield return null;
        }
    }

    IEnumerator focusGame()
    {
        bool space_pressed = false;
        float elapsedTime = 0.0f;
        yield return null;


        while (space_pressed == false)
        {
            elapsedTime += Time.deltaTime;
            float cosineValue = Mathf.Cos(2.0f * Mathf.PI * frequency * elapsedTime);
            InnerRing.transform.localScale = minScale + (maxScale - minScale) * 0.5f * (1 - cosineValue);

            if (InnerRing.transform.localScale.x <= maxSuccessScale
                && InnerRing.transform.localScale.x >= minSuccessScale)
                PressText.SetActive(true);
            else
                PressText.SetActive(false);

            if (Input.GetKeyDown(KeyCode.Space))
                space_pressed = true;
            yield return null;
        }
        Sound.playSFX("Button");
        PressText.SetActive(false);
        HelpText.SetActive(false);

        if (InnerRing.transform.localScale.x <= maxSuccessScale
            && InnerRing.transform.localScale.x >= minSuccessScale)
        {
            //Success
            FocusBackgroundSuccess.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            FocusGameGroup.SetActive(false);

            yield return showSuccessText();

            focusGameFinished(true);
            yield break;
        }
        else {
            //Fail
            FocusBackgroundFail.SetActive(true);
            yield return new WaitForSeconds(2.0f);

            FocusGameGroup.SetActive(false);
            yield return showFailText();

            focusGameFinished(false);
            yield break;
        }
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
    }


    private void focusGameFinished(bool success)
    {
        FocusGameGroup.SetActive(false);
        StartFocusButton.SetActive(false);
        FocusBackgroundSuccess.SetActive(false);
        FocusBackgroundFail.SetActive(false);
        onFocusGameFinished.Invoke(success);
    }
}
