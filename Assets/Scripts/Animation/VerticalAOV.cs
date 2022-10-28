using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalAOV
{
    public VerticalAOV(float _start, float _startVal, float _end, float _endVal)
    {
        start = _start;
        startVal = _startVal;
        end = _end;
        endVal = _endVal;
    }

    public float start;
    public float startVal;
    public float end;
    public float endVal;


    public static List<VerticalAOV> processVerticalAOV(ConfigAnimation._Animation3D.Camerainfo camera_info)
    {
        List<VerticalAOV> list = new List<VerticalAOV>();

        foreach (var aov in camera_info.verticalAOV)
        {
            VerticalAOV shaderAnimation = new VerticalAOV(aov.start, aov.startVal, aov.end, aov.endVal);
            list.Add(shaderAnimation);
        }
        return list;
    }
}
