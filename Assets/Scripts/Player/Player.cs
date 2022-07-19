using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    static Model _outfit;
    static Model _outfit_bottom;

    public static string local_avatar_first_name;
    public static string local_avatar_last_name;
    public static string local_avatar_full_name;
    public static string local_avatar_house;
    public static string local_avatar_gender;
    public static string local_avatar_quidditch_position = "chaser";
    public static string local_avatar_clothing_type;
    public static string local_avatar_secondary_clothing_option;
    public static string local_avatar_opponent_house;
    public static string local_avatar_onscreen_name;
    public static int local_avatar_year => GlobalEngineVariables.player_year;








    public static void changeClothes(string clothing_type, string secondary_clothing_option) //This is broken and shit.
    {
        return;
        Debug.Log("CHANGE CLOTHES MUTHAFUCKER");
        switch (clothing_type)
        {
            case "QuidditchRobesWalk":
                Debug.Log("CHANGE CLOTHES MUTHAFUCKER WALK");

                GameStart.current.removePatchFromCharacter(local_avatar_onscreen_name, _outfit);
                GameStart.current.removePatchFromCharacter(local_avatar_onscreen_name, _outfit_bottom);

                if (secondary_clothing_option == "houseCup")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHouseOfficialRobes_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHouseOfficialRobes_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                else if (secondary_clothing_option == "friendly")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHousePracticeRobesHome_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHousePracticeRobesHome_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                else if (secondary_clothing_option == "preTryout")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHouseTryoutRobesHome_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHouseTryoutRobesHome_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                _outfit = _outfit_bottom;
                break;


            case "QuidditchRobesFly":
                GameStart.current.removePatchFromCharacter("Avatar", _outfit);
                GameStart.current.removePatchFromCharacter("Avatar", _outfit_bottom);
                Debug.Log("CHANGE CLOTHES MUTHAFUCKER FLY");
                if (secondary_clothing_option == "houseCup")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHouseOfficialRobes_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHouseOfficialRobes_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                else if (secondary_clothing_option == "friendly")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHousePracticeRobesHome_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHousePracticeRobesHome_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                else if (secondary_clothing_option == "preTryout")
                {
                    if (local_avatar_gender == "male")
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_male_QuidditchHouseTryoutRobesHome_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                    else
                        _outfit = GameStart.current.addPatchToCharacter(local_avatar_onscreen_name, "o_female_QuidditchHouseTryoutRobesHome_flying_FULL_skin", Actor.actor_controllers["Avatar"].model.pose_bones);
                }
                _outfit = _outfit_bottom;
                break;
        }

        /*public void setQuidditchHelmet()
        {


            GetComponent<GameStart>().removePatchFromCharacter("Avatar", GameStart.current.GetComponent<Player>()._hair);

            if (DialogueManager.local_avatar_gender == "male")
                GetComponent<Player>()._hair = GameStart.current.GetComponent<GameStart>().addPatchToCharacter(DialogueManager.local_avatar_onscreen_name, "c_male_QuidditchKeeperHelmet_skin", Actor.actor_managers["Avatar"].model.parent_bones);
            else
                GetComponent<Player>()._hair = GameStart.current.GetComponent<GameStart>().addPatchToCharacter(DialogueManager.local_avatar_onscreen_name, "c_female_QuidditchKeeperHelmet_skin", Actor.actor_managers["Avatar"].model.parent_bones);
        }*/
    }
}
