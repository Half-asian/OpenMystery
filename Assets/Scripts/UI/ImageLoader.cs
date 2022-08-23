using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
public class ImageLoader : MonoBehaviour
{
    protected static Texture2D loadTextureFromApk(string filename)
    {
        string filepath;
        string filepath_assets = Path.Combine(GlobalEngineVariables.apk_folder, "assets", filename);
        string filepath_apktex = Path.Combine(GlobalEngineVariables.assets_folder, "apktex", filename);
        if (File.Exists(filepath_assets))
        {
            filepath = filepath_assets;
        }
        else if (File.Exists(filepath_apktex))
        {
            filepath = filepath_apktex;
        }
        else
        {
            throw new System.Exception("Could not find apk asset " + filename);
        }

        using (FileStream fs = File.Open(filepath, FileMode.Open))
        {
            byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            tex.name = filename;
            return tex;
        }

    }

    protected static Sprite loadSpriteFromApk(string filename)
    {
        string filepath;
        string filepath_assets = Path.Combine(GlobalEngineVariables.apk_folder, "assets", filename);
        string filepath_apktex = Path.Combine(GlobalEngineVariables.assets_folder, "apktex", filename);
        if (File.Exists(filepath_assets))
        {
            filepath = filepath_assets;
        }
        else if (File.Exists(filepath_apktex))
        {
            filepath = filepath_apktex;
        }
        else
        {
            throw new System.Exception("Could not find apk asset " + filename + " " + filepath_apktex);
        }

        using (FileStream fs = File.Open(filepath, FileMode.Open))
        {
            byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(data);
            tex.name = filename;
            Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f, 0, SpriteMeshType.FullRect);
            return sprite;
        }

    }
}

