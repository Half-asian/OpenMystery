using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class GlobalEngineVariables
{
    public static string configs_content_file = "contents.json";


    [JsonProperty("configs_folder")]
    public static string configs_folder;
    [JsonProperty("assets_folder")]
    public static string assets_folder;
    [JsonProperty("apk_folder")]
    public static string apk_folder;
    [JsonProperty("player_folder")]
    public static string player_folder;
    [JsonProperty("tlsq_name")]
    public static string tlsq_name;
    [JsonProperty("launch_mode")]
    public static string launch_mode;
    [JsonProperty("player_year")]
    public static int player_year;
    [JsonProperty("mods_folder")]
    public static string mods_folder;
    [JsonProperty("active_mods")]
    public static string[] active_mods;
    [JsonProperty("goal_chains")]
    public static Dictionary<string, string> goal_chains;
    [JsonProperty("exclusively_dating")]
    public static string exclusively_dating;
    public static string models_folder => Path.Combine(assets_folder, "models");

    public static string animations_folder => Path.Combine(assets_folder, "animations");


    public static void CreateFromJSON(string file)
    {
        StreamReader reader = new StreamReader(file);
        string content = reader.ReadToEnd();
        reader.Close();
        
        JsonConvert.DeserializeObject<GlobalEngineVariables>(content);
    }

    public static bool checkIntegrity()
    {
        if (assets_folder == null || configs_folder == null || apk_folder == null || player_folder == null)
            return false;
        if (!Directory.Exists(assets_folder) || !Directory.Exists(configs_folder) || !Directory.Exists(apk_folder) || !Directory.Exists(player_folder))
            return false;
        return true;
    }
    
    public static string getTLSQName()
    {
        if (tlsq_name == null)
            return "";
        return tlsq_name;
    }
}
