using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class LocalData : MonoBehaviour
{

    static Dictionary<string, Delegate> functions = new Dictionary<string, Delegate>()
    {
        {"::firstname::"                        , new Func<string>(tokens.firstname) },
        {"::first-name::"                       , new Func<string>(tokens.firstname) }, //slight different spelling
        {"::playername::"                       , new Func<string>(tokens.PlayerName) }, //Seems to be same as firstname
        {"::lastname::"                         , new Func<string>(tokens.lastname) },
        {"::fullname::"                         , new Func<string>(tokens.fullname) },
        {"::house::"                            , new Func<string>(tokens.house) },
        {"::he/she::"                           , new Func<string>(tokens.heshe) },
        {"::his/her::"                          , new Func<string>(tokens.hisher) },
        {"::pronounpossessive::"                , new Func<string>(tokens.PronounPossessive) }, //Same as his/her
        {"::pointsofhouse(\"gryffindor\")::"    , new Func<string>(tokens.pointsofhouse) },
        {"::pointsofhouse(\"hufflepuff\")::"    , new Func<string>(tokens.pointsofhouse) },
        {"::pointsofhouse(\"slytherin\")::"     , new Func<string>(tokens.pointsofhouse) },
        {"::pointsofhouse(\"ravenclaw\")::"     , new Func<string>(tokens.pointsofhouse) },
        {"::firstplacehouse()::"                , new Func<string>(tokens.firstplacehouse) },
        {"::prefectspeakername::"               , new Func<string>(tokens.prefectspeakername) },
        {"::expeditioncreaturename::"           , new Func<string>(tokens.ExpeditionCreatureName) },
        {"::expeditionlocationname::"           , new Func<string>(tokens.ExpeditionLocationName) },
        {"::expeditiongiverfullname::"          , new Func<string>(tokens.ExpeditionGiverFullName) },
        {"::expeditiongiverfirstname::"         , new Func<string>(tokens.ExpeditionGiverFirstName) },
        {"::mx::"                               , new Func<string>(tokens.mx) },
        {"::mrx::"                              , new Func<string>(tokens.mx) }, //Seems to be a misspelling of mx
        {"::headkid::"                          , new Func<string>(tokens.HeadKid) },
        {"::student::"                          , new Func<string>(tokens.Student) },
        {"::opponenthouse::"                    , new Func<string>(tokens.opponentHouse) },
        {"::quidditchrivalhouse::"              , new Func<string>(tokens.quidditchRivalHouse) },
        {"::quidditchrivalhouseid::"            , new Func<string>(tokens.quidditchRivalHouseId) },
        {"::quidditchposition::"                , new Func<string>(tokens.quidditchPosition) },
        {"::househublocation::"                 , new Func<string>(tokens.HouseHubLocation) },
        {"::hufflepuff::"                       , new Func<string>(tokens.Hufflepuff) },
        {"::ravenclaw::"                        , new Func<string>(tokens.Ravenclaw) },
        {"::gryffindor::"                       , new Func<string>(tokens.Gryffindor) },
        {"::slytherin::"                        , new Func<string>(tokens.Slytherin) },
        {"::oppositeheadkid::"                  , new Func<string>(tokens.OppositeHeadKid) },
        {"::hayden::"                           , new Func<string>(tokens.Hayden) },

        {"::petname_pet_cat::"                  , new Func<string>(tokens.PetName_pet_cat) },
        {"::petname_pet_puppykrup::"            , new Func<string>(tokens.PetName_pet_puppykrup) },
        {"::creaturename_chinesefireball::"     , new Func<string>(tokens.CreatureName_Chinesefireball) },
        {"::petname_pet_rat::"                  , new Func<string>(tokens.PetName_pet_rat) },
        {"::petname_pet_puffskein::"            , new Func<string>(tokens.PetName_pet_puffskein) },
        {"::creaturename_mooncalf::"            , new Func<string>(tokens.CreatureName_Mooncalf) },
        {"::petname_pet_niffler::"              , new Func<string>(tokens.PetName_pet_niffler) },
        {"::creaturename_niffler::"             , new Func<string>(tokens.CreatureName_Niffler) },
        {"::creaturename_chupacabra::"          , new Func<string>(tokens.CreatureName_Chupacabra) },
        {"::creaturename_hodag::"               , new Func<string>(tokens.CreatureName_Hodag) },
        {"::creaturename_graphorn::"            , new Func<string>(tokens.CreatureName_Graphorn) },
        {"::creaturename_thunderbird::"         , new Func<string>(tokens.CreatureName_Thunderbird) },
        {"::creaturename_opaleye::"             , new Func<string>(tokens.CreatureName_Opaleye) },
        {"::creaturename_knarl::"               , new Func<string>(tokens.CreatureName_Knarl) },
        {"::creaturename_fairy::"               , new Func<string>(tokens.CreatureName_Fairy) },
        {"::creaturename_chimera::"             , new Func<string>(tokens.CreatureName_Chimera) },
        {"::creaturename_augurey::"             , new Func<string>(tokens.CreatureName_Augurey) },
        {"::creaturename_manticore::"           , new Func<string>(tokens.CreatureName_Manticore) },
        {"::creaturename_acromantula::"         , new Func<string>(tokens.CreatureName_Acromantula) },
        {"::creaturename_matagot::"             , new Func<string>(tokens.CreatureName_Matagot) },
        {"::petname_pet_owl::"                  , new Func<string>(tokens.PetName_pet_owl) },
        {"::creaturename_leucrotta::"           , new Func<string>(tokens.CreatureName_Leucrotta) },
        {"::creaturename_ashwinder::"           , new Func<string>(tokens.CreatureName_Ashwinder) },
        {"::creaturename_bowtruckle::"          , new Func<string>(tokens.CreatureName_Bowtruckle) },
        {"::creaturename_billywig::"            , new Func<string>(tokens.CreatureName_Billywig) },
        {"::creaturename_jackalope::"           , new Func<string>(tokens.CreatureName_Jackalope) },
        {"::petname_pet_jackalope::"            , new Func<string>(tokens.PetName_pet_jackalope) },
        {"::creaturename_thestral::"            , new Func<string>(tokens.CreatureName_Thestral) },

    };



    private static string replaceToken(string token)
    {
        if (functions.ContainsKey(token.ToLower()))
        {
            return (string)functions[token.ToLower()].DynamicInvoke();
        }
        token = token.Replace("::", "**");

        return token;
    }

    public static string getLine(string line_id)
    {
        if (!Configs.config_local_data.LocalData.ContainsKey(line_id))
        {
            Debug.LogError("Could not find localdataline " + line_id);
            return line_id;
        }

        string text = Configs.config_local_data.LocalData[line_id].en_US;

        int counter = 0;

        while (text.Contains("::"))
        {
            int firstidx = text.IndexOf("::");
            int secndidx = text.IndexOf("::", firstidx + 1) + 2;
            string word = text.Substring(firstidx, secndidx - firstidx);
            string replacementWord = replaceToken(word);
            text = text.Replace(word, replacementWord);
            Debug.Log(text);
            counter++;
            if (counter > 100)
            {
                Debug.LogError("Infinite loop in getline");
                break;
            }
        }
        text = replaceTextTags(text);
        return text;
    }

    public static string replaceTextTags(string line)
    {

        line = line.Replace("{italics}", "{italic}");
        line = line.Replace("{i}", "{italic}");
        line = line.Replace("{b}", "{bold}");
        line = line.Replace("{/italic}", "{/}");
        line = line.Replace("{/italics}", "{/}");
        line = line.Replace("{/bold}", "{/}");



        int bold_index = line.IndexOf("{bold}");
        int ital_index = line.IndexOf("{italic}");

        int counter = 0;

        while ((bold_index != -1 || ital_index != -1) && counter < 10)
        {
            counter++;
            if (bold_index != -1)
            {
                if (ital_index != -1)
                {
                    if (bold_index < ital_index)
                    {
                        line = Common.stringReplaceFirst(line, "{bold}", "<b>");
                        line = Common.stringReplaceFirst(line, "{/}", "</b>");
                    }
                    else
                    {
                        line = Common.stringReplaceFirst(line, "{italic}", "<i>");
                        line = Common.stringReplaceFirst(line, "{/}", "</i>");
                    }
                }
                else
                {
                    line = Common.stringReplaceFirst(line, "{bold}", "<b>");
                    line = Common.stringReplaceFirst(line, "{/}", "</b>");
                }
            }
            else
            {
                line = Common.stringReplaceFirst(line, "{italic}", "<i>");
                line = Common.stringReplaceFirst(line, "{/}", "</i>");
            }

            bold_index = line.IndexOf("{bold}");
            ital_index = line.IndexOf("{italic}");
        }
        if (counter == 10)
            Debug.LogError("infinite looped");

        return line;
    }

    /*----------        LocalData tokens        ----------*/
    public class tokens
    {
        public static string firstname() => Player.local_avatar_first_name;
        public static string lastname() => Player.local_avatar_last_name;
        public static string fullname() => Player.local_avatar_full_name;
        public static string house() => Common.titleCase(Player.local_avatar_house);
        public static string heshe() => Player.local_avatar_gender switch {"male" => "he", "female" => "she", _ => "he" };
        public static string hisher() => Player.local_avatar_gender switch { "male" => "his", "female" => "her", _ => "his" };
        public static string pointsofhouse() => "9999";
        public static string firstplacehouse() => house();
        public static string prefectspeakername() => getLine(Configs.config_house.House[Player.local_avatar_house].snippets_PrefectSpeakerName);
        public static string ExpeditionCreatureName() => throw new NotImplementedException("ExpeditionCreatureName not implemented");
        public static string ExpeditionLocationName() => throw new NotImplementedException("ExpeditionLocationName not implemented");
        public static string ExpeditionGiverFullName() => throw new NotImplementedException("ExpeditionGiverFullName not implemented");
        public static string ExpeditionGiverFirstName() => throw new NotImplementedException("ExpeditionGiverFirstName not implemented");
        public static string mx() => Player.local_avatar_gender switch { "male" => "Mr.", "female" => "Ms. ", _ => "Mr. " };
        public static string HeadKid() => Player.local_avatar_gender switch { "male" => "Head Boy", "female" => "Head Girl", _ => "Head Boy" };
        public static string Student() => "Student";
        public static string PronounPossessive() => hisher();
        public static string opponentHouse() => Common.titleCase(Player.local_avatar_opponent_house);
        public static string quidditchRivalHouse() => getLine(Configs.config_house.House[Player.local_avatar_house].snippets_quidditchRivalHouse);
        public static string quidditchRivalHouseId() => Common.titleCase(Configs.config_house.House[Player.local_avatar_house].snippets_quidditchRivalHouseId);
        public static string quidditchPosition() => Common.titleCase(Player.local_avatar_quidditch_position);
        public static string HouseHubLocation() => getLine(Configs.config_house.House[Player.local_avatar_house].snippets_HouseHubLocation);
        public static string PlayerName() => firstname();
        public static string Hufflepuff() => "Hufflepuff";
        public static string Ravenclaw() => "Ravenclaw";
        public static string Gryffindor() => "Gryffindor";
        public static string Slytherin() => "Slytherin";
        public static string OppositeHeadKid() => Player.local_avatar_gender switch { "male" => "Head Girl", "female" => "Head Boy", _ => "Head Girl" };
        public static string Hayden() => "Hayden";


        /*Creature and pets*/
        public static string PetName_pet_cat() => "Cat";
        public static string PetName_pet_puppykrup() => "Cruppy";
        public static string CreatureName_Chinesefireball() => "Chinese Fireball";
        public static string PetName_pet_rat() => "Rat";
        public static string PetName_pet_puffskein() => "Puffskein";
        public static string CreatureName_Mooncalf() => "Mooncalf";
        public static string PetName_pet_niffler() => "Niffler";
        public static string CreatureName_Niffler() => "Niffler";
        public static string CreatureName_Chupacabra() => "Chupacabra";
        public static string CreatureName_Hodag() => "Hodag";
        public static string CreatureName_Graphorn() => "Graphorn";
        public static string CreatureName_Thunderbird() => "Thunderbird";
        public static string CreatureName_Opaleye() => "Opaleye";
        public static string CreatureName_Knarl() => "Knarl";
        public static string CreatureName_Fairy() => "Fairy";
        public static string CreatureName_Chimera() => "Chimera";
        public static string CreatureName_Augurey() => "Augurey";
        public static string CreatureName_Manticore() => "Manticore";
        public static string CreatureName_Acromantula() => "Acromantula";
        public static string CreatureName_Matagot() => "Matagot";
        public static string PetName_pet_owl() => "Owl";
        public static string CreatureName_Leucrotta() => "Leucrotta";
        public static string CreatureName_Ashwinder() => "Ashwinder";
        public static string CreatureName_Bowtruckle() => "Bowtruckle";
        public static string CreatureName_Billywig() => "Billywig";
        public static string CreatureName_Jackalope() => "Jackalope";
        public static string PetName_pet_jackalope() => "Jackalope";
        public static string CreatureName_Thestral() => "Thestral";

    }


}
