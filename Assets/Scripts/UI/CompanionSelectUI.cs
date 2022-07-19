using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CompanionSelectUI : MonoBehaviour
{
    [SerializeField]
    private GameObject display;
    [SerializeField]
    private TMPro.TMP_InputField input_field;


    void Awake()
    {
        EncounterDate.toggleCompanionCanvas += toggleCompanionCanvas;
    }

    private void toggleCompanionCanvas(bool toggle)
    {
        display.SetActive(toggle);
    }

    public void onButtonClicked(string companion)
    {
        EncounterDate.setCompanion(companion);
    }

    public void onButtonClickedCustom()
    {
        EncounterDate.setCompanion(input_field.text);
    }

}
