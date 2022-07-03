using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasImageLoader : ImageLoader
{
    [SerializeField]
    private Image image_target;
    [SerializeField]
    private string image_name;

    void Start()
    {
        if (!string.IsNullOrEmpty(image_name))
        {
            image_target.sprite = loadSpriteFromApk(image_name);
        }
    }
}
