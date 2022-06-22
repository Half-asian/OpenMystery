using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LocalData : MonoBehaviour
{
    public static string getLine(string line)
    {
        if (!Configs.config_local_data.LocalData.ContainsKey(line))
        {
            Debug.LogError("Could not find localdataline " + line);
            return line;
        }

        string text = Configs.config_local_data.LocalData[line].en_US;
        text = text.Replace("::FirstName::", Player.local_avatar_first_name);
        text = text.Replace("::LastName::", Player.local_avatar_last_name);
        text = text.Replace("::FullName::", Player.local_avatar_full_name);
        text = text.Replace("::Date::", Configs.config_companion.Companion[Player.companionId].speakerId);
        text = text.Replace("::pointsOfHouse(\"ravenclaw\")::", "9999");
        text = text.Replace("::pointsOfHouse(\"gryffindor\")::", "9999");
        text = text.Replace("::pointsOfHouse(\"hufflepuff\")::", "9999");
        text = text.Replace("::pointsOfHouse(\"slytherin\")::", "9999");


        //Technically this stuff should be found with the "House" config.
        switch (Player.local_avatar_house) //House
        {
            case "hufflepuff":
                text = text.Replace("::House::", "Hufflepuff");
                text = text.Replace("::PrefectSpeakerName::", "Jane");
                text = text.Replace("::firstPlaceHouse()::", "Hufflepuff");
                break;
            case "ravenclaw":
                text = text.Replace("::House::", "Ravenclaw");
                text = text.Replace("::PrefectSpeakerName::", "Chester");
                text = text.Replace("::firstPlaceHouse()::", "Ravenclaw");
                break;
            case "slytherin":
                text = text.Replace("::House::", "Slytherin");
                text = text.Replace("::PrefectSpeakerName::", "Felix");
                text = text.Replace("::firstPlaceHouse()::", "Slytherin");
                break;
            case "gryffindor":
                text = text.Replace("::House::", "Gryffindor");
                text = text.Replace("::PrefectSpeakerName::", "Angelica");
                text = text.Replace("::firstPlaceHouse()::", "Gryffindor");
                break;
            default:
                throw new System.Exception("Unknown Player House");
        }

        switch (Player.local_avatar_opponent_house) //Opp House
        {
            case "hufflepuff":
                text = text.Replace("::opponentHouse::", "Hufflepuff");
                break;
            case "ravenclaw":
                text = text.Replace("::opponentHouse::", "Ravenclaw");
                break;
            case "slytherin":
                text = text.Replace("::opponentHouse::", "Slytherin");
                break;
            case "gryffindor":
                text = text.Replace("::opponentHouse::", "Gryffindor");
                break;
            default:
                text.Replace("::opponentHouse::", "Hufflepuff");
                break;
        }

        if (Player.local_avatar_gender == "male") //Gender
        {
            text = text.Replace("::Mx::", "Mr.");
            text = text.Replace("::HeadKid::", "Head Boy");

        }
        else
        {
            text = text.Replace("::HeadKid::", "Head Girl");
            text = text.Replace("::Mx::", "Ms.");
        }

        text = replaceTextTags(text);


        return text;

    }

    public static string replaceTextTags(string line)
    {
        //line = line.Replace("{bold}", "<b>");
        //line = line.Replace("{italic}", "<i>");


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
}
