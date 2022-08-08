using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreenCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject image;

    public static LoadingScreenCanvas current;

    private void Awake()
    {
        current = this;
        image.SetActive(false);
        Scenario.onScenarioLoading += showImage;
        Scenario.onScenarioLoaded += hideImage;
    }

    public void showImage()
    {
        Debug.LogError("SHOWIMAGE");
        image.SetActive(true);
    }

    private void hideImage()
    {
        image.SetActive(false);
    }

}
