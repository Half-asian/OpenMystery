using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;


[System.Serializable]
public class PlayerManager
{

    public static PlayerFile current;
    public static void initialize()
    {
        current = PlayerFile.createFromJson(GlobalEngineVariables.player_folder + "\\avatar.json");
        string[] d_split = Path.GetFileName(GlobalEngineVariables.player_folder).Split('_');
        Player.local_avatar_first_name = d_split[0];
        Player.local_avatar_last_name = d_split[1];
        Player.local_avatar_full_name = d_split[0] + " " + d_split[1];
        Player.local_avatar_gender = d_split[2].ToLower();
        Player.local_avatar_house = d_split[3].ToLower();
    }

}