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
        if (File.Exists(GlobalEngineVariables.apk_folder + "\\assets\\" + filename))
        {
            filepath = GlobalEngineVariables.apk_folder + "\\assets\\" + filename;
        }
        else if (File.Exists(GlobalEngineVariables.assets_folder + "\\apktex\\" + filename))
        {
            filepath = GlobalEngineVariables.assets_folder + "\\apktex\\" + filename;
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
        if (File.Exists(GlobalEngineVariables.apk_folder + "\\assets\\" + filename))
        {
            filepath = GlobalEngineVariables.apk_folder + "\\assets\\" + filename;
        }
        else if (File.Exists(GlobalEngineVariables.assets_folder + "\\apktex\\" + filename))
        {
            filepath = GlobalEngineVariables.assets_folder + "\\apktex\\" + filename;
        }
        else
        {
            throw new System.Exception("Could not find apk asset " + filename);
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
