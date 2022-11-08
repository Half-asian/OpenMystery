using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Predicate : MonoBehaviour
{
    private static string predicateParseSymbols(string line)
    {
        if (line == null)
        {
            Debug.LogError("Predicate.predicateParseSymbols: the predicate was null.");
        }
        line = line.Replace("\\", "");
        int index = line.IndexOf("skillUnlocked(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("skillUnlocked(");
        }

        index = line.IndexOf("interactionComplete(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");


            if (Scenario.getInteractionsCompleted().Contains(line.Substring(index, closing_brace + 1))) //keeping it local
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("interactionComplete(");

        }

        index = line.IndexOf("isGoalComplete(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\goals_complete.txt").Contains(line.Substring(index, closing_brace + 1)))
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("isGoalComplete(");
        }

        index = line.IndexOf("goalChainComplete(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\goalchains_complete.txt").Contains(line.Substring(index, closing_brace + 1)))
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("goalChainComplete(");
        }

        index = line.IndexOf("isTimedPromoExpired("); //Always set false for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("isTimedPromoExpired(");
        }

        index = line.IndexOf("matchWon(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\matches_won.txt").Contains(line.Substring(index, closing_brace + 1)))
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("matchWon(");
        }

        index = line.IndexOf("quidditchPosition(");
        while (index != -1)
        {
            Debug.Log(line);
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            line = line.Substring(0, index) + "\"" + Player.local_avatar_quidditch_position + "\"" + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("quidditchPosition(");
            Debug.Log(line);
        }

        index = line.IndexOf("madeChoice(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains(line.Substring(index, closing_brace + 1)))
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("madeChoice(");
        }

        index = line.IndexOf("isInHouse(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if ((line.Substring(index, closing_brace + 1) == "isInHouse(\"ravenclaw\")" && Player.local_avatar_house == "ravenclaw") ||
                (line.Substring(index, closing_brace + 1) == "isInHouse(\"slytherin\")" && Player.local_avatar_house == "slytherin") ||
                (line.Substring(index, closing_brace + 1) == "isInHouse(\"hufflepuff\")" && Player.local_avatar_house == "hufflepuff") ||
                (line.Substring(index, closing_brace + 1) == "isInHouse(\"gryffindor\")" && Player.local_avatar_house == "gryffindor"))
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("isInHouse(");
        }

        index = line.IndexOf("isAvatarFemale(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (Player.local_avatar_gender == "female")
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("isAvatarFemale(");
        }

        index = line.IndexOf("isAvatarMale(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            if (Player.local_avatar_gender == "male")
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("isAvatarMale(");
        }

        index = line.IndexOf("playerYear(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            line = line.Substring(0, index) + Player.local_avatar_year + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("playerYear(");
        }

        index = line.IndexOf("currentScenario(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            line = line.Substring(0, index) + "\"" + Scenario.getCurrentScenarioConfig().scenarioId + "\"" + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("currentScenario(");
        }

        index = line.IndexOf("tutorialStepComplete("); //Always set false for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");


            if (line.Substring(index, closing_brace + 1) == "tutorialStepComplete(\"diagonP5\", \"ollivanderStep19\")")
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            index = line.IndexOf("tutorialStepComplete(");
        }

        index = line.IndexOf("lastPivotalPlaySuccessful("); //Always set true for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("lastPivotalPlaySuccessful(");
        }

        index = line.IndexOf("lastPivotalPlayFailed("); //Always set true for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("lastPivotalPlayFailed(");
        }

        index = line.IndexOf("lastPivotalPlayHasQTEPhase("); //Always set true for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("lastPivotalPlayHasQTEPhase(");
        }

        index = line.IndexOf("winningCurrentMatch("); //Always set true for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("winningCurrentMatch(");
        }

        index = line.IndexOf("opponentHouse(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " \"" + Player.local_avatar_opponent_house + "\" " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("opponentHouse(");
        }

        index = line.IndexOf("isTLSQActive("); //Always set true for now
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");

            line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);

            index = line.IndexOf("isTLSQActive(");
        }
        index = line.IndexOf("isGoalInProgress(");
        while (index != -1)
        {
            string substring = line.Substring(index);
            int closing_brace = substring.IndexOf(")");
            //Debug.Log(line.Substring(index + 16, closing_brace + 1));
            if (Goal.isGoalInProgress(line.Substring(index + 16, closing_brace + 1)))
            {
                line = line.Substring(0, index) + " true " + line.Substring(closing_brace + index + 1);
            }
            else
            {
                line = line.Substring(0, index) + " false " + line.Substring(closing_brace + index + 1);
            }
            index = line.IndexOf("isGoalInProgress(");
        }

        return line.Replace("  ", " ");
    }

    private static string predicateCrunchLogic(string line)
    {
        //Debug.Log(line);
        string beginning_line = (string)line.Clone();
        
        line = line.Replace("  ", " ");

        line = line.Replace("(\" ", "(\"");
        line = line.Replace("\" )", "\")");
        line = line.Replace("( ", "(");
        line = line.Replace(" )", ")");


        line = line.Replace(" ==\"", " == \"");
        line = line.Replace("\"== ", "\" == ");
        line = line.Replace("\"==\"", "\" == \"");

        line = line.Replace("==false", "== false");
        line = line.Replace("==true", "== true");

        line = line.Replace("false==", "false ==");
        line = line.Replace("true==", "true ==");

        line = line.Replace("true == true", "true");
        line = line.Replace("true == false", "false");
        line = line.Replace("false == true", "false");
        line = line.Replace("false == false", "true");

        line = line.Replace("true and true", "true");
        line = line.Replace("true and false", "false");
        line = line.Replace("false and true", "false");
        line = line.Replace("false and false", "false");

        line = line.Replace("true or true", "true");
        line = line.Replace("true or false", "true");
        line = line.Replace("false or true", "true");
        line = line.Replace("false or false", "false");

        line = line.Replace("not false", "true");
        line = line.Replace("not true", "true");
        

        line = line.Replace("(true)", "true");
        line = line.Replace("(false)", "false");
        

        if (line.Contains(" == ")) //only works for single digit numbers
        {
            int equals_index = line.IndexOf(" == ");

            int first_string_index = equals_index - 1;
            while (first_string_index > 0 && line[first_string_index] != ' ')
            {
                first_string_index--;
            }
            string first_string = line.Substring(first_string_index, equals_index - first_string_index);

            int second_string_index = equals_index + 4;
            while (second_string_index < line.Length && line[second_string_index] != ' ')
            {
                second_string_index++;
            }
            string second_string = line.Substring(equals_index + 4, second_string_index - equals_index - 4);

            //Debug.Log(first_string + ": :" + second_string);

            if (first_string.Replace(" ", "") == second_string.Replace(" ", ""))
            {
                //Debug.Log("string compare was true");
                line = line.Substring(0, first_string_index) + " true " + line.Substring(second_string_index);
            }
            else
            {
                //Debug.Log("string compare was false");
                line = line.Substring(0, first_string_index) + " false " + line.Substring(second_string_index);
            }

            /*if (char.IsDigit(line[equals_index - 1]) && char.IsDigit(line[equals_index + 4]))
            {
                if (line[equals_index - 1] == line[equals_index + 4])
                {
                    line = line.Substring(0, equals_index - 1) + "true" + line.Substring(equals_index + 5);
                }
                else
                {
                    line = line.Substring(0, equals_index - 1) + "false" + line.Substring(equals_index + 5);
                }
            }*/
        }

        if (line.Contains(" <= ")) //only works for single digit numbers
        {
            int equals_index = line.IndexOf(" <= ");
            if (line[equals_index - 1] <= line[equals_index + 4])
            {
                line = line.Substring(0, equals_index - 1) + "true" + line.Substring(equals_index + 5);
            }
            else
            {
                line = line.Substring(0, equals_index - 1) + "false" + line.Substring(equals_index + 5);
            }
        }

        if (line.Contains(" >= ")) //only works for single digit numbers
        {
            int equals_index = line.IndexOf(" >= ");
            if (line[equals_index - 1] >= line[equals_index + 4])
            {
                line = line.Substring(0, equals_index - 1) + "true" + line.Substring(equals_index + 5);
            }
            else
            {
                line = line.Substring(0, equals_index - 1) + "false" + line.Substring(equals_index + 5);
            }
        }

        if (line.Contains(" > ")) //only works for single digit numbers
        {
            int equals_index = line.IndexOf(" > ");
            if (line[equals_index - 1] > line[equals_index + 3])
            {
                line = line.Substring(0, equals_index - 1) + "true" + line.Substring(equals_index + 4);
            }
            else
            {
                line = line.Substring(0, equals_index - 1) + "false" + line.Substring(equals_index + 4);
            }
        }

        if (line.Contains(" < ")) //only works for single digit numbers
        {
            int equals_index = line.IndexOf(" < ");
            if (line[equals_index - 1] < line[equals_index + 3])
            {
                line = line.Substring(0, equals_index - 1) + "true" + line.Substring(equals_index + 4);
            }
            else
            {
                line = line.Substring(0, equals_index - 1) + "false" + line.Substring(equals_index + 4);
            }
        }

        line = line.Replace("TRUE", "true");
        line = line.Replace("FALSE", "false");

        if (line == " true ")
        {
            line = "true";
        }
        if (line == " false ")
        {
            line = "false";
        }

        /*int index = line.IndexOf("==");
        if (index != -1)
        {
            if (index + 1 + index < line.Length)
            {
                Debug.Log(line.Substring(0, index - 1) + " compare " + line.Substring(index + 3, index - 1));
                if (line.Substring(0, index - 1) == line.Substring(index + 3, index - 1))
                {
                    Debug.Log("true");
                    line = "true";
                }
                else
                {
                    Debug.Log("false");
                    line = "false";
                }
            }
        }*/



        if (beginning_line == line)
        {
            if (line != "true" && line != "false")
            {
                Debug.LogWarning("Could not crunch " + line + "!");
                return "false";
            }
            return line;
        }

        if (line != "false" && line != "true" && line != " true" && line != " false" && line != "true " && line != "false " && line != " true " && line != " false ")
        {
            line = predicateCrunchLogic(line);
        }
        return line.Replace(" ", "");
    }

    public static bool parsePredicate(string predicate)
    {
        Debug.Log("parse predicate " + predicate);
        bool result = NewPredicate.parsePredicate(predicate);
        Debug.Log("Result: " + result);
        //bool result = predicateCrunchLogic(predicateParseSymbols(predicate)).Contains("true");
        return result;
    }
}
