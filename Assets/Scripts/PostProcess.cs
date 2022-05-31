using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PostProcess : MonoBehaviour
{
    public GameObject PostProcessDefault;
    public GameObject PostProcessDefaultLight;
    public Volume volumeProfile;
    ColorAdjustments colorAdjustments;
    GradientSky gradientsky;
    private void Awake()
    {
        volumeProfile.profile.TryGet(out colorAdjustments);
        volumeProfile.profile.TryGet(out gradientsky);
    }
    public void changeFilter(Color filterColor)
    {        
        if (gradientsky != null)
        {
            Debug.Log("GradientSky");
            gradientsky.top.value = filterColor;
            gradientsky.middle.value = filterColor;
            gradientsky.bottom.value = filterColor;


            //gradientsky.exposure.value = filterColor.maxColorComponent * 8;

            //Adjust your lens here. ie

            //colorAdjustments.colorFilter.value = new Color(filterColor.r / 255f, filterColor.g / 255f, filterColor.b / 255f);
        }
        else
        {
            Debug.Log("Gradientsky failed");
        }
    }

}
