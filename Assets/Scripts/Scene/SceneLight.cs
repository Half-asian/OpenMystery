using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLight
{
    public string name;
    public Color color;
}

public class DirLight : SceneLight
{
    public Vector3 direction;
    public Matrix4x4 preCameraMatrix;
}

public class AmbLight : SceneLight
{

}

public class SpotLight : SceneLight
{
    public float coneAngle;
    public float penumbraAngle;
    public float dropoff;
    public Vector3 position;
    public Vector3 direction;
    public float range = 5f;
}

public class PointLight : SceneLight
{
    public Vector3 position;
    public Vector3 rotation; //irrelevant
    public float range = 5f;
}