using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class Common
{
    public static bool arrayContains<T>(T[] data, T value)
    {
        foreach (T t in data)
        {
            if (EqualityComparer<T>.Default.Equals(t, value))
            {
                return true;
            }
        }
        return false;
    }

    public static int arrayIndex<T>(T[] data, T value)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (EqualityComparer<T>.Default.Equals(data[i], value))
            {
                return i;
            }
        }
        return -1;
    }

    public static void setWaypointTransform(ref GameObject prop, ConfigScene._Scene.PropLocator prop_locator)
    {
        if (prop_locator.position != null)
        {
            prop.transform.position = new Vector3(prop_locator.position[0] * -0.01f, prop_locator.position[1] * 0.01f, prop_locator.position[2] * 0.01f);
        }
        if (prop_locator.rotation != null)
        {
            prop.transform.rotation = Quaternion.identity;
            prop.transform.Rotate(new Vector3(0, 0, -prop_locator.rotation[2]));
            prop.transform.Rotate(new Vector3(0, -prop_locator.rotation[1], 0));
            prop.transform.Rotate(new Vector3(prop_locator.rotation[0], 0, 0));
        }
        if (prop_locator.scale != null)
        {
            prop.transform.localScale = new Vector3(prop_locator.scale[0] * 0.01f, prop_locator.scale[1] * 0.01f, prop_locator.scale[2] * 0.01f);
        }
    }

    public static void setWaypointTransform(ref GameObject prop, ConfigScene._Scene.WayPoint prop_locator)
    {
        if (prop_locator.position != null)
        {
            prop.transform.position = new Vector3(prop_locator.position[0] * -0.01f, prop_locator.position[1] * 0.01f, prop_locator.position[2] * 0.01f);
        }
        if (prop_locator.rotation != null)
        {
            prop.transform.rotation = Quaternion.identity;
            prop.transform.Rotate(new Vector3(0, 0, -prop_locator.rotation[2]));
            prop.transform.Rotate(new Vector3(0, -prop_locator.rotation[1], 0));
            prop.transform.Rotate(new Vector3(prop_locator.rotation[0], 0, 0));
        }
        if (prop_locator.scale != null)
        {
            prop.transform.localScale = new Vector3(prop_locator.scale[0], prop_locator.scale[1], prop_locator.scale[2]);
        }
    }

    public static void setSceneTransform(ref GameObject obj, Vector3 position, Vector3 rotation)
    {
        obj.transform.position = new Vector3(position[0] * -0.01f, position[1] * 0.01f, position[2] * 0.01f);
        obj.transform.eulerAngles = new Vector3(rotation[0], -rotation[1], -rotation[2]);
    }

    public static string getConfigPath(string config_name)
    {
        if (!string.IsNullOrWhiteSpace(GlobalEngineVariables.configs_folder))
        {
            string[] file_paths = Directory.GetFiles(GlobalEngineVariables.configs_folder);
            foreach (string file_path in file_paths)
            {
                string file_name = Path.GetFileName(file_path);

                if (file_name.StartsWith(config_name))
                {
                    return file_path;
                }
            }
        }
        /*else
        {
            throw new System.Exception("Tried to load a config but config path not set.");
        }
        throw new System.Exception("Config was missing " + config_name);*/
        //Debug.Log("Could not find config " + config_name);
        //Log.writeFull("Could not find config " + config_name);
        return null;
    }

    public static Transform recursiveFindChild(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            Transform found = recursiveFindChild(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    public static Dictionary<string, string> GetTransformFullNameDict(Transform parent, string path = "Armature/", Dictionary<string, string> dict = null)
    {
        if (dict == null)
            dict = new Dictionary<string, string>();

        path += parent.name;
        dict[parent.name] = path;

        foreach (Transform child in parent)
        {
            GetTransformFullNameDict(child, path + "/", dict);
        }
        return dict;
    }

    public static string stringReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static int substringCount(string orig, string find)
    {
        var s2 = orig.Replace(find, "");
        return (orig.Length - s2.Length) / find.Length;
    }

    static Bounds getRenderBounds(GameObject objeto)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Renderer render = objeto.GetComponent<Renderer>();
        if (render != null)
        {
            return render.bounds;
        }
        return bounds;
    }

    public static Bounds getBounds(GameObject objeto)
    {
        Bounds bounds;
        Renderer childRender;
        bounds = getRenderBounds(objeto);
        if (bounds.extents.x == 0)
        {
            bounds = new Bounds(objeto.transform.position, Vector3.zero);
            foreach (Transform child in objeto.transform)
            {
                childRender = child.GetComponent<Renderer>();
                if (childRender)
                {
                    bounds.Encapsulate(childRender.bounds);
                }
                else
                {
                    bounds.Encapsulate(getBounds(child.gameObject));
                }
            }
        }
        return bounds;
    }
}
