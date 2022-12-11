using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
public partial class NewPredicate
{


    static Dictionary<string, Delegate> functions = new Dictionary<string, Delegate>()
    {
        {"multiplyStuff"                , new Func<SymbolConstantInteger, SymbolConstantInteger, SymbolConstantInteger>(multiplyStuff) },
        {"skillUnlocked"                , new Func<SymbolConstantString, SymbolConstantBool>(skillUnlocked) },
        {"interactionComplete"          , new Func<SymbolConstantString, SymbolConstantBool>(interactionComplete) },
        {"isGoalComplete"               , new Func<SymbolConstantString, SymbolConstantBool>(isGoalComplete) },
        {"goalChainComplete"            , new Func<SymbolConstantString, SymbolConstantBool>(goalChainComplete) },
        {"isTLSQComplete"               , new Func<SymbolConstantString, SymbolConstantBool>(isTLSQComplete) },
        {"isTLSQFailed"                 , new Func<SymbolConstantString, SymbolConstantBool>(isTLSQFailed) },
        {"isTimedPromoExpired"          , new Func<SymbolConstantString, SymbolConstantBool>(isTimedPromoExpired) },
        {"featuredModalCompleteCount"   , new Func<SymbolConstantString, SymbolConstantInteger>(featuredModalCompleteCount) },
        {"matchWon"                     , new Func<SymbolConstantString, SymbolConstantBool>(matchWon) },
        {"quidditchPosition"            , new Func<SymbolConstantString>(quidditchPosition) },
        {"madeChoice"                   , new Func<SymbolConstantString, SymbolConstantBool>(madeChoice) },
        {"isInHouse"                    , new Func<SymbolConstantString, SymbolConstantBool>(isInHouse) },
        {"isAvatarFemale"               , new Func<SymbolConstantBool>(isAvatarFemale) },
        {"isAvatarMale"                 , new Func<SymbolConstantBool>(isAvatarMale) },
        {"playerYear"                   , new Func<SymbolConstantInteger>(playerYear) },
        {"currentScenario"              , new Func<SymbolConstantString>(currentScenario) },
        {"tutorialStepComplete"         , new Func<SymbolConstantString, SymbolConstantString, SymbolConstantBool>(tutorialStepComplete) },
        {"lastPivotalPlaySuccessful"    , new Func<SymbolConstantBool>(lastPivotalPlaySuccessful) },
        {"lastPivotalPlayFailed"        , new Func<SymbolConstantBool>(lastPivotalPlayFailed) },
        {"lastPivotalPlayHasQTEPhase"   , new Func<SymbolConstantBool>(lastPivotalPlayHasQTEPhase) },
        {"winningCurrentMatch"          , new Func<SymbolConstantBool>(winningCurrentMatch) },
        {"opponentHouse"                , new Func<SymbolConstantString>(opponentHouse) },
        {"isTLSQActive"                 , new Func<SymbolConstantString, SymbolConstantBool>(isTLSQActive) },
        {"isTimedPromoActive"           , new Func<SymbolConstantString, SymbolConstantBool>(isTimedPromoActive) },
        {"isGoalInProgress"             , new Func<SymbolConstantString, SymbolConstantBool>(isGoalInProgress) },
        {"isMemoryActiveWithTag"        , new Func<SymbolConstantString, SymbolConstantBool>(isMemoryActiveWithTag) },
        {"inScenarioForTlsqWithTag"     , new Func<SymbolConstantString, SymbolConstantBool>(inScenarioForTlsqWithTag) },
        {"getContentVar"                , new Func<SymbolConstantString, SymbolConstantString, SymbolConstantInteger>(getContentVar) },
        {"placeOfHouse"                 , new Func<SymbolConstantString, SymbolConstantInteger>(placeOfHouse) },
        {"placeOfPlayer"                , new Func<SymbolConstantInteger>(placeOfPlayer) },
        {"currentLocation"              , new Func<SymbolConstantString>(currentLocation) },
        {"isRomanticallyBusy"           , new Func<SymbolConstantString, SymbolConstantBool>(isRomanticallyBusy) },
        {"romanceLevelWithPartner"      , new Func<SymbolConstantString, SymbolConstantInteger>(romanceLevelWithPartner) },
        {"isExclusiveEmpty"             , new Func<SymbolConstantBool>(isExclusiveEmpty) },
        {"isExclusive"                  , new Func<SymbolConstantString, SymbolConstantBool>(isExclusive) },
        {"isPetOwned"                   , new Func<SymbolConstantString, SymbolConstantBool>(isPetOwned) },
        {"isCreatureUnlocked"           , new Func<SymbolConstantString, SymbolConstantBool>(isCreatureUnlocked) },
        {"goalViewed"                   , new Func<SymbolConstantString, SymbolConstantBool>(goalViewed) },
        {"creatureAffinityLevel"        , new Func<SymbolConstantString, SymbolConstantInteger>(creatureAffinityLevel) },
        {"random"                       , new Func<SymbolConstantFloat>(random) },
        {"sessionRandom"                , new Func<SymbolConstantFloat>(random) },
        {"isVersionAtLeast"             , new Func<SymbolConstantString, SymbolConstantBool>(isVersionAtLeast) },
        {"companionLevel"               , new Func<SymbolConstantString, SymbolConstantInteger >(companionLevel) },
        {"numCompletedDatesWithPartner" , new Func<SymbolConstantString, SymbolConstantInteger>(numCompletedDatesWithPartner) },
        {"attributeLevel"               , new Func<SymbolConstantString, SymbolConstantInteger>(attributeLevel) },
        {"hasCompletedNux"              , new Func<SymbolConstantBool>(hasCompletedNux) },
    };
    static SymbolConstantInteger multiplyStuff(SymbolConstantInteger i, SymbolConstantInteger i2)
    {
        return new SymbolConstantInteger(i.value * i2.value);
    }

    static SymbolConstantBool skillUnlocked(SymbolConstantString skill_id)
    {
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\skills_unlocked.txt").Contains("skillUnlocked(\"" + skill_id.value + "\")"))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool interactionComplete(SymbolConstantString interaction_id)
    {
        if (Scenario.getInteractionsCompleted().Contains(interaction_id.value))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool isGoalComplete(SymbolConstantString goal_id)
    {
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\goals_complete.txt").Contains("isGoalComplete(\"" + goal_id.value + "\")"))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool goalChainComplete(SymbolConstantString goalchain_id)
    {
        //There was a bug where goalchains were being saved to the wrong txt. This just helps.
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\goals_complete.txt").Contains("goalChainComplete(\"" + goalchain_id.value + "\")"))
            return new SymbolConstantBool(true);
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\goalchains_complete.txt").Contains("goalChainComplete(\"" + goalchain_id.value + "\")"))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool isTLSQComplete(SymbolConstantString tlsq_id) //Affects two quidditch dialogues
    {
        return new SymbolConstantBool(true);
    }

    static SymbolConstantBool isTLSQFailed(SymbolConstantString tlsq_id) //Affects nothing
    {
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool isTimedPromoExpired(SymbolConstantString timed_promo_id)
    {
        return new SymbolConstantBool(true); //Always return true for now
    }

    static SymbolConstantInteger featuredModalCompleteCount(SymbolConstantString featured_modal)
    {
        return new SymbolConstantInteger(0);
    }

    static SymbolConstantBool matchWon(SymbolConstantString match_id)
    {
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\matches_won.txt").Contains("matchWon(\"" + match_id.value + "\")"))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantString quidditchPosition()
    {
        return new SymbolConstantString(Player.local_avatar_quidditch_position);
    }

    static SymbolConstantBool madeChoice(SymbolConstantString choice_id)
    {
        if (File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + choice_id.value + "\")"))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool isInHouse(SymbolConstantString house)
    {
        return new SymbolConstantBool(Player.local_avatar_house == house.value);
    }

    static SymbolConstantBool isAvatarFemale()
    {
        return new SymbolConstantBool(Player.local_avatar_gender == "female");
    }

    static SymbolConstantBool isAvatarMale()
    {
        return new SymbolConstantBool(Player.local_avatar_gender == "male");
    }

    static SymbolConstantInteger playerYear()
    {
        return new SymbolConstantInteger(Player.local_avatar_year);
    }

    static SymbolConstantString currentScenario()
    {
        return new SymbolConstantString(Scenario.getCurrentScenarioConfig().scenarioId);
    }

    static SymbolConstantBool tutorialStepComplete(SymbolConstantString a, SymbolConstantString b)
    {
        if (a.value == "diagonP5" && b.value == "ollivanderStep19") //TODO
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool lastPivotalPlaySuccessful()
    {
        return new SymbolConstantBool(true);
    }

    static SymbolConstantBool lastPivotalPlayFailed()
    {
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool lastPivotalPlayHasQTEPhase()
    {
        return new SymbolConstantBool(true);
    }

    static SymbolConstantBool winningCurrentMatch()
    {
        return new SymbolConstantBool(true);
    }

    static SymbolConstantString opponentHouse()
    {
        return new SymbolConstantString(Player.local_avatar_opponent_house);
    }

    static SymbolConstantBool isTLSQActive(SymbolConstantString tlsq_id)
    {
        return new SymbolConstantBool(true); //TODO
    }

    static SymbolConstantBool isTimedPromoActive(SymbolConstantString promo_id)
    {
        return new SymbolConstantBool(TimedPromo.isTimedPromoActive(promo_id.value));
    }

    static SymbolConstantBool isGoalInProgress(SymbolConstantString goal_id)
    {
        if (Goal.isGoalInProgress(goal_id.value))
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }

    static SymbolConstantBool isMemoryActiveWithTag(SymbolConstantString tag)
    {
        return new SymbolConstantBool(false);
    }
    static SymbolConstantBool inScenarioForTlsqWithTag(SymbolConstantString tag)
    {
        return new SymbolConstantBool(false);
    }
    static SymbolConstantInteger getContentVar(SymbolConstantString a, SymbolConstantString b) //Content vars are like scenario variables from what I gather
    {
        int value = Scenario.getContentVar(a.value, b.value);
        return new SymbolConstantInteger(value);
    }


    static SymbolConstantInteger placeOfHouse(SymbolConstantString a)
    {
        if (Player.local_avatar_house == "ravenclaw")
        {
            if (a.value == "ravenclaw")
                return new SymbolConstantInteger(0);
            if (a.value == "hufflepuff")
                return new SymbolConstantInteger(1);
            if (a.value == "slytherin")
                return new SymbolConstantInteger(2);
            return new SymbolConstantInteger(3);
        }
        if (Player.local_avatar_house == "gryffindor")
        {
            if (a.value == "gryffindor")
                return new SymbolConstantInteger(0);
            if (a.value == "slytherin")
                return new SymbolConstantInteger(1);
            if (a.value == "hufflepuff")
                return new SymbolConstantInteger(2);
            return new SymbolConstantInteger(3);
        }
        if (Player.local_avatar_house == "slytherin")
        {
            if (a.value == "slytherin")
                return new SymbolConstantInteger(0);
            if (a.value == "ravenclaw")
                return new SymbolConstantInteger(1);
            if (a.value == "gryffindor")
                return new SymbolConstantInteger(2);
            return new SymbolConstantInteger(3);
        }
        if (a.value == "hufflepuff")
            return new SymbolConstantInteger(0);
        if (a.value == "gryffindor")
            return new SymbolConstantInteger(1);
        if (a.value == "ravenclaw")
            return new SymbolConstantInteger(2);
        return new SymbolConstantInteger(3);
    }

    static SymbolConstantInteger placeOfPlayer()
    {
        return new SymbolConstantInteger(0);
    }

    static SymbolConstantString currentLocation()
    {
        return new SymbolConstantString(Location.currentLocation());
    }
    static SymbolConstantBool isRomanticallyBusy(SymbolConstantString friend)
    {
        if (friend.toString().ToLower() == GlobalEngineVariables.exclusively_dating.ToLower())
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }
    static SymbolConstantInteger romanceLevelWithPartner(SymbolConstantString friend)
    {
        if (friend.toString().ToLower() == GlobalEngineVariables.exclusively_dating.ToLower())
            return new SymbolConstantInteger(999);
        return new SymbolConstantInteger(0);
    }
    static SymbolConstantBool isExclusiveEmpty()
    {
        if (GlobalEngineVariables.exclusively_dating.ToLower() == "none")
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }
    static SymbolConstantBool isExclusive(SymbolConstantString friend)
    {
        if (friend.toString().ToLower() == GlobalEngineVariables.exclusively_dating.ToLower())
            return new SymbolConstantBool(true);
        return new SymbolConstantBool(false);
    }
    static SymbolConstantBool isPetOwned(SymbolConstantString pet)
    {
        return new SymbolConstantBool(true);
    }
    static SymbolConstantBool isCreatureUnlocked(SymbolConstantString creature)
    {
        return new SymbolConstantBool(true);
    }
    static SymbolConstantBool goalViewed(SymbolConstantString goal)
    {
        return new SymbolConstantBool(true);
    }
    static SymbolConstantInteger creatureAffinityLevel(SymbolConstantString creature)
    {
        return new SymbolConstantInteger(10);
    }
    static SymbolConstantFloat random()
    {
        System.Random r = new System.Random();
        //return new SymbolConstantFloat((float)r.NextDouble());
        return new SymbolConstantFloat(0.0f);
    }
    static SymbolConstantBool isVersionAtLeast(SymbolConstantString version)
    {
        return new SymbolConstantBool(true);
    }
    static SymbolConstantInteger companionLevel(SymbolConstantString companion)
    {
        return new SymbolConstantInteger(10);
    }
    static SymbolConstantInteger numCompletedDatesWithPartner(SymbolConstantString partner)
    {
        return new SymbolConstantInteger(100);
    }
    static SymbolConstantInteger attributeLevel(SymbolConstantString attribute)
    {
        return new SymbolConstantInteger(100);
    }

    static SymbolConstantBool hasCompletedNux()
    {
        return new SymbolConstantBool(true);
    }
}
