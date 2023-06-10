using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public static string local_avatar_first_name;
    public static string local_avatar_last_name;
    public static string local_avatar_full_name;
    public static string local_avatar_house;
    public static string local_avatar_gender;
    public static string local_avatar_quidditch_position;
    public static string local_avatar_opponent_house = "gryffindor";
    public static string local_avatar_onscreen_name;
    public static int local_avatar_year => GlobalEngineVariables.player_year;
}
