using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ModLoader : MonoBehaviour
{
    public static void addModConfigsToContents(ConfigContents configContents)
    {
        if (GlobalEngineVariables.active_mods != null)
        {
            foreach(string mod in GlobalEngineVariables.active_mods)
            {
                Debug.Log("Loading mod: " + mod);
                foreach(string file in Directory.EnumerateFiles(Path.Combine(GlobalEngineVariables.mods_folder, mod))){
                    if (file.EndsWith(".json"))
                    {
                        Debug.Log("Loading mod content " + file);
                        string config_type = Path.GetFileNameWithoutExtension(file);

                        if (configContents.Contents.ContainsKey(config_type))
                        {
                            configContents.Contents[config_type].Add(Path.Combine(new string[] { GlobalEngineVariables.mods_folder, mod, file }));
                        }
                        else
                        {
                            throw new System.Exception("Unknown mod config type: " + config_type);
                        }
                    }
                }
            }
        }
    }
}
