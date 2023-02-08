using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoadingScreenCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject image;

    public static LoadingScreenCanvas current;

    private IEnumerator waitHideImage;

    public static bool is_loading = false;

    private void Awake()
    {
        current = this;
        image.SetActive(false);
        Scenario.onScenarioLoading += showImage;
        Scenario.onScenarioLoaded += hideImage;
    }

    public void showImage()
    {
        if (waitHideImage != null)
            StopCoroutine(waitHideImage);
        image.SetActive(true);
        is_loading = true;
    }

    private void hideImage()
    {
        waitHideImage = waitHideImageCoroutine();
        StartCoroutine(waitHideImage);
    }

    //Hides the ugly parts of loading
    IEnumerator waitHideImageCoroutine()
    {
        yield return new WaitForSeconds(0.05f);
        image.SetActive(false);
        waitHideImage = null;
        is_loading = false;
    }

}
