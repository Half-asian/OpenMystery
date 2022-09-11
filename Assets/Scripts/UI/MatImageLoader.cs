using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatImageLoader : ImageLoader
{
    [SerializeField]
    private MeshRenderer mesh_renderer;
    [SerializeField]
    private string image_name;

    [SerializeField]
    private bool is_transparent;

    [SerializeField]
    private string h_name;
    [SerializeField]
    private string g_name;
    [SerializeField]
    private string r_name;
    [SerializeField]
    private string s_name;

    void Awake()
    {
        if (is_transparent)
            mesh_renderer.material = Resources.Load("UI/transparent") as Material;

        Texture2D texture = null;
        if (!string.IsNullOrEmpty(h_name))
        {
            switch (Player.local_avatar_house)
            {
                case "hufflepuff":
                    texture = loadTextureFromApk(h_name);
                    break;
                case "gryffindor":
                    texture = loadTextureFromApk(g_name);
                    break;
                case "ravenclaw":
                    texture = loadTextureFromApk(r_name);
                    break;
                case "slytherin":
                    texture = loadTextureFromApk(s_name);
                    break;
            }
        }
        else
            texture = loadTextureFromApk(image_name);

        mesh_renderer.material.SetTexture("_BaseColorMap", texture);


    }
}
