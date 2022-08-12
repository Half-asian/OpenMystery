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

}

public class PointLight : SceneLight
{

}