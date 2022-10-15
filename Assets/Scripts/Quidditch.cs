using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Quidditch
{
    public event Action<string> match_finished_event = delegate { };

    int match_pivotal_play_index;
    int match_pivotal_play_outro_index;

    bool has_played_pivotal_outro;
    public enum State
    {
        state_not_waiting,
        state_waiting_enter_events,
        state_waiting_dialogue,
        state_waiting_exit_events,
    };
    public State state;

    string phase;

    ConfigPivotalPlay._PivotalPlay current_pivotal_play;

    
    ConfigMatch._Match current_match;
    string play_phase_name;

    List<string> moves_already_played;

    public void startMatch(string match_name)
    {
        Debug.Log("Starting match " + match_name);
        moves_already_played = new List<string>();
        state = State.state_not_waiting;
        match_pivotal_play_index = 0;
        match_pivotal_play_outro_index = 0;
        has_played_pivotal_outro = false;
        CameraManager.current.resetCamera();


        if (!Configs.config_match.Match.ContainsKey(match_name))
        {
            throw new System.Exception("Quidditch:startMatch - Config empty for id " + match_name + "in Match");
        }

        current_match = Configs.config_match.Match[match_name];
        if (Player.local_avatar_quidditch_position == null)
        {
            if (current_match.practisePosition != null)
            {
                Player.local_avatar_quidditch_position = current_match.practisePosition;
            }
            else
            {
                Player.local_avatar_quidditch_position = "chaser";
            }
        }


        Scenario.Activate(current_match.scenarioId, Scenario.current.objective);
        Scenario.Load(current_match.scenarioId);

        /*while (GameStart.game_state.state != GameState.State.StateFinishedLoading)
        {
            yield return null;
        }*/

        string team_player = null;
        string opponent_player = null;

        switch (Player.local_avatar_house)
        {
            case "gryffindor":
                team_player = current_match.teamId_gPlayer;
                opponent_player = current_match.teamId_gOpponent;
                break;
            case "ravenclaw":
                team_player = current_match.teamId_rPlayer;
                opponent_player = current_match.teamId_rOpponent;
                break;
            case "hufflepuff":
                team_player = current_match.teamId_hPlayer;
                opponent_player = current_match.teamId_hOpponent;
                break;
            case "slytherin":
                team_player = current_match.teamId_sPlayer;
                opponent_player = current_match.teamId_sOpponent;
                break;
        }
        switch (Player.local_avatar_quidditch_position)
        {
            case "chaser":
                Actor.spawnActor("Avatar", "way_player_chaser1", "player_chaser1");
                Actor.spawnActor(teamIdToActorId(team_player, "beater1"), "way_player_beater1", "player_beater1");
                Actor.spawnActor(teamIdToActorId(team_player, "keeper"), "way_player_keeper", "player_keeper");
                Actor.spawnActor(teamIdToActorId(team_player, "seeker"), "way_player_seeker", "player_seeker");
                break;
            case "beater":
                Actor.spawnActor(teamIdToActorId(team_player, "chaser1"), "way_player_chaser1", "player_chaser1");
                Actor.spawnActor("Avatar", "way_player_beater1", "player_beater1");
                Actor.spawnActor(teamIdToActorId(team_player, "keeper"), "way_player_keeper", "player_keeper");
                Actor.spawnActor(teamIdToActorId(team_player, "seeker"), "way_player_seeker", "player_seeker");
                break;
            case "keeper":
                Actor.spawnActor(teamIdToActorId(team_player, "chaser1"), "way_player_chaser1", "player_chaser1");
                Actor.spawnActor(teamIdToActorId(team_player, "beater1"), "way_player_beater1", "player_beater1");
                Actor.spawnActor("Avatar", "way_player_keeper", "player_keeper");
                Actor.spawnActor(teamIdToActorId(team_player, "seeker"), "way_player_seeker", "player_seeker");
                break;
            case "seeker":
                Actor.spawnActor(teamIdToActorId(team_player, "chaser1"), "way_player_chaser1", "player_chaser1");
                Actor.spawnActor(teamIdToActorId(team_player, "beater1"), "way_player_beater1", "player_beater1");
                Actor.spawnActor(teamIdToActorId(team_player, "keeper"), "way_player_keeper", "player_keeper");
                Actor.spawnActor("Avatar", "way_player_seeker", "player_seeker");
                break;
        }

        Actor.spawnActor(teamIdToActorId(team_player, "chaser2"), "way_player_chaser2", "player_chaser2");
        Actor.spawnActor(teamIdToActorId(team_player, "chaser3"), "way_player_chaser3", "player_chaser3");
        Actor.spawnActor(teamIdToActorId(team_player, "beater2"), "way_player_beater2", "player_beater2");
        Actor.spawnActor(teamIdToActorId(opponent_player, "seeker"), "way_opponent_seeker", "opponent_seeker");
        Actor.spawnActor(teamIdToActorId(opponent_player, "keeper"), "way_opponent_keeper", "opponent_keeper");
        Actor.spawnActor(teamIdToActorId(opponent_player, "chaser1"), "way_opponent_chaser1", "opponent_chaser1");
        Actor.spawnActor(teamIdToActorId(opponent_player, "chaser2"), "way_opponent_chaser2", "opponent_chaser2");
        Actor.spawnActor(teamIdToActorId(opponent_player, "chaser3"), "way_opponent_chaser3", "opponent_chaser3");
        Actor.spawnActor(teamIdToActorId(opponent_player, "beater1"), "way_opponent_beater1", "opponent_beater1");
        Actor.spawnActor(teamIdToActorId(opponent_player, "beater2"), "way_opponent_beater2", "opponent_beater2");
        nextMatchPivotalPlay();
    }

    public ConfigPivotalPlay._PivotalPlay getPivotalPlayFromBucket(string pivotal_play_name)
    {
        Debug.Log("getPivotalPlayFromBucket");
        List<string> keyList = new List<string>(Configs.config_pivotal_play_bucket.PivotalPlayBucket[pivotal_play_name].pivotalPlays.Keys);

        List<string> real_keys = new List<string>();

        foreach (string pivotal_play in keyList)
        {
            if (!moves_already_played.Contains(pivotal_play))  //Don't play the same move again.
            {
                if (Configs.config_pivotal_play.PivotalPlay[pivotal_play].availablePredicate != null)
                {
                    if (Predicate.parsePredicate(Configs.config_pivotal_play.PivotalPlay[pivotal_play].availablePredicate))
                    {
                        real_keys.Add(pivotal_play);
                    }
                }
                else
                {
                    real_keys.Add(pivotal_play);
                }
            }
        }

        string randomKey = real_keys[UnityEngine.Random.Range(0, real_keys.Count)];

        moves_already_played.Add(randomKey);

        return Configs.config_pivotal_play.PivotalPlay[randomKey];
    }


    public string teamIdToActorId(string teamId, string position)
    {
        if (!Configs.config_quidditch_team.QuidditchTeam.ContainsKey(teamId))
        {
            Debug.LogError("Invalid TeamId " + teamId + " in config quidditch team.");
            return "";
        }

        ConfigQuidditchTeam._QuidditchTeam team = Configs.config_quidditch_team.QuidditchTeam[teamId];

        switch (position)
        {
            case "beater1":
                if (team.beater1Predicates != null)
                {
                    for (int i = 0; i < team.beater1Predicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.beater1Predicates[i]))
                            return team.beater1ActorIds[i];
                    }
                }
                else if (team.beater1ActorIds != null)
                    return team.beater1ActorIds[0];
                return team.beater1DefaultActorId;
            case "beater2":
                if (team.beater2Predicates != null)
                {
                    for (int i = 0; i < team.beater2Predicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.beater2Predicates[i]))
                            return team.beater2ActorIds[i];
                    }
                }
                else if (team.beater2ActorIds != null)
                    return team.beater2ActorIds[0];
                return team.beater2DefaultActorId;
            case "chaser1":
                if (team.chaser1Predicates != null)
                {
                    for (int i = 0; i < team.chaser1Predicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.chaser1Predicates[i]))
                            return team.chaser1ActorIds[i];
                    }
                }
                else if (team.chaser1ActorIds != null)
                    return team.chaser1ActorIds[0];
                return team.chaser1DefaultActorId;
            case "chaser2":
                if (team.chaser2Predicates != null)
                {
                    for (int i = 0; i < team.chaser2Predicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.chaser2Predicates[i]))
                            return team.chaser2ActorIds[i];
                    }
                }
                else if (team.chaser2ActorIds != null)
                    return team.chaser2ActorIds[0];
                return team.chaser2DefaultActorId;
            case "chaser3":
                if (team.chaser3Predicates != null)
                {
                    for (int i = 0; i < team.chaser3Predicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.chaser3Predicates[i]))
                            return team.chaser3ActorIds[i];
                    }
                }
                else if (team.chaser3ActorIds != null)
                    return team.chaser3ActorIds[0];
                return team.chaser3DefaultActorId;
            case "keeper":
                if (team.keeperPredicates != null)
                {
                    for (int i = 0; i < team.keeperPredicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.keeperPredicates[i]))
                            return team.keeperActorIds[i];
                    }
                }
                else if (team.keeperActorIds != null)
                    return team.keeperActorIds[0];
                return team.keeperDefaultActorId;
            case "seeker":
                if (team.seekerPredicates != null)
                {
                    for (int i = 0; i < team.seekerPredicates.Length; i++)
                    {
                        if (Predicate.parsePredicate(team.seekerPredicates[i]))
                            return team.seekerActorIds[i];
                    }
                }
                else if (team.seekerActorIds != null)
                    return team.seekerActorIds[0];
                return team.seekerDefaultActorId;
            default:
                Debug.LogError("Invalid quidditch position " + position + " in config quidditch team.");
                return "";
        }
    }

    public void nextMatchPivotalPlay()
    {
        phase = "intro";
        if (match_pivotal_play_index < current_match.pivotalPlaySlots.Length)
        {
            activatePivotalPlayIntroPhases(current_match.pivotalPlaySlots[match_pivotal_play_index]);
            match_pivotal_play_index += 1;
        }
        else if (has_played_pivotal_outro == false && match_pivotal_play_outro_index < current_match.outroPlays.Length)
        {
            activatePivotalPlayOutroPhases(current_match.outroPlays[match_pivotal_play_outro_index]);
            match_pivotal_play_outro_index += 1;
        }
        else
        {
            finishMatch();

            Scenario.Activate(current_match.scenarioId, Scenario.current.objective);
            Scenario.Load(current_match.outroScenarioId);
        }
    }

    public void activatePivotalPlayIntroPhases(string pivotal_play_name)
    {
        if (!Configs.config_pivotal_play.PivotalPlay.ContainsKey(pivotal_play_name))
        {
            if (!Configs.config_pivotal_play_bucket.PivotalPlayBucket.ContainsKey(pivotal_play_name))
            {
                Debug.LogError("Quidditch:activatePlayPhase - Config empty for id " + pivotal_play_name + "in PivotalPlay/PivotalPlayBucket");
                return;
            }
            else
            {
                current_pivotal_play = getPivotalPlayFromBucket(pivotal_play_name);
            }
        }
        else
        {
            current_pivotal_play = Configs.config_pivotal_play.PivotalPlay[pivotal_play_name];
            moves_already_played.Add(pivotal_play_name);
        }

        if (current_pivotal_play.availablePredicate != null && !NewPredicate.parsePredicate(current_pivotal_play.availablePredicate)) //Failed the predicate, skip
        {
            nextMatchPivotalPlay();
            return;
        }

        play_phase_name = current_pivotal_play.introPhases[0];
        activatePlayPhase();
    }

    public void activatePivotalPlayOutroPhases(string pivotal_play_name)
    {
        if (!Configs.config_pivotal_play.PivotalPlay.ContainsKey(pivotal_play_name))
        {
            if (!Configs.config_pivotal_play_bucket.PivotalPlayBucket.ContainsKey(pivotal_play_name))
            {
                Debug.LogError("Quidditch:activatePlayPhase - Config empty for id " + pivotal_play_name + "in PivotalPlay/PivotalPlayBucket");
                return;
            }
            else
            {
                current_pivotal_play = getPivotalPlayFromBucket(pivotal_play_name);
            }
        }
        else
        {
            current_pivotal_play = Configs.config_pivotal_play.PivotalPlay[pivotal_play_name];
            moves_already_played.Add(pivotal_play_name);
        }

        if (current_pivotal_play.availablePredicate != null && !NewPredicate.parsePredicate(current_pivotal_play.availablePredicate)) //Failed the predicate, skip
        {
            nextMatchPivotalPlay();
            return;
        }

        has_played_pivotal_outro = true;
        play_phase_name = current_pivotal_play.introPhases[0];
        activatePlayPhase();
    }


    public void activatePlayPhase()
    {
        Debug.Log("activatePlayPhase " + play_phase_name);
        if (!Configs.config_play_phase.PlayPhase.ContainsKey(play_phase_name))
        {
            throw new System.Exception("Quidditch:activatePlayPhase - Config empty for id " + play_phase_name + " in PlayPhase");
        }
        if (Configs.config_play_phase.PlayPhase[play_phase_name].enterEvents != null)
        {
            Debug.Log("Adding playphase " + play_phase_name + " enter events.");
            GameStart.event_manager.main_event_player.addEvents(Configs.config_play_phase.PlayPhase[play_phase_name].enterEvents);
        }
        state = State.state_waiting_enter_events;
        EventManager.all_script_events_finished_event += enterEventsCallback;
    }

    public void enterEventsCallback()
    {
        EventManager.all_script_events_finished_event -= enterEventsCallback;

        Debug.Log("enter events callback");
        if (state == State.state_waiting_enter_events)
        {
            if (play_phase_name == null)
            {
                Debug.LogError("play_phase_name was null");
                return;
            }
            state = State.state_waiting_dialogue;
            if (Configs.config_play_phase.PlayPhase[play_phase_name].dialogueId == null)
            {
                //Debug.LogError("play_phase_name " + play_phase_name + " dialogue_id was null");
                dialogueCallback("");
                return;
            }

            Debug.Log("Activating dialogue " + Configs.config_play_phase.PlayPhase[play_phase_name].dialogueId);
            DialogueManager.onDialogueFinishedEventPrimary += dialogueCallback;
            GameStart.dialogue_manager.activateDialogue(Configs.config_play_phase.PlayPhase[play_phase_name].dialogueId);
        }
    }

    public void dialogueCallback(string dialogue_id)
    {
        DialogueManager.onDialogueFinishedEventPrimary -= dialogueCallback;

        Debug.Log("DialogueCallback");
        if (Configs.config_play_phase.PlayPhase[play_phase_name].exitEvents != null)
        {
            GameStart.event_manager.main_event_player.addEvents(Configs.config_play_phase.PlayPhase[play_phase_name].exitEvents);
            state = State.state_waiting_exit_events;

            EventManager.all_script_events_finished_event += exitEventsCallback;

        }
        else
        {
            if (phase == "intro")
            {
                if (current_pivotal_play.successPhases != null)
                {
                    Debug.Log("GOT TO THE SUCCESS PHASE");
                    phase = "success";
                    play_phase_name = current_pivotal_play.successPhases[0];
                    activatePlayPhase();
                }
                else if (current_pivotal_play.outroPhases != null)
                {
                    Debug.Log("GOT TO THE OUTRO PHASE");
                    phase = "outro";
                    play_phase_name = current_pivotal_play.outroPhases[0];
                    activatePlayPhase();
                }
                else
                {
                    nextMatchPivotalPlay();
                }
            }
            else if (phase == "success")
            {
                phase = "outro";
                play_phase_name = current_pivotal_play.outroPhases[0];
                activatePlayPhase();
            }
            else
            {
                nextMatchPivotalPlay();
            }
            /*finishMatch();

            GameStart.game_state.state = GameStart.GameState.State.StateGame;
            GameStart.current.GetComponent<DungeonMaster>().checkObjective(current_match.matchId, "matchComplete");

            game_start.activateScenario(current_match.outroScenarioId);*/
        }
    }

    public void exitEventsCallback()
    {
        EventManager.all_script_events_finished_event -= exitEventsCallback;

        if (state == State.state_waiting_exit_events)
        {
            if (phase == "intro")
            {
                if (current_pivotal_play.successPhases != null)
                {
                    Debug.Log("GOT TO THE SUCCESS PHASE");
                    phase = "success";
                    play_phase_name = current_pivotal_play.successPhases[0];
                    activatePlayPhase();
                }
                else if (current_pivotal_play.outroPhases != null)
                {
                    Debug.Log("GOT TO THE OUTRO PHASE");
                    phase = "outro";
                    play_phase_name = current_pivotal_play.outroPhases[0];
                    activatePlayPhase();
                }
                else
                {
                    nextMatchPivotalPlay();
                }
            }
            else if (phase == "success")
            {
                Debug.Log("GOT TO THE OUTRO PHASE");
                phase = "outro";
                play_phase_name = current_pivotal_play.outroPhases[0];
                activatePlayPhase();
            }
            else
            {
                nextMatchPivotalPlay();
            }
        }
    }

    public void finishMatch()
    {
        Debug.Log("Finished match");

        if (current_match.perfectRewardId != null) //Lets assume we did it perfectly.
        {
            Reward.getReward(current_match.perfectRewardId);
        }
        string matches_won_txt = Path.Combine(GlobalEngineVariables.player_folder, "matches_won.txt");
        if (!File.ReadAllText(matches_won_txt).Contains("matchWon(\"" + current_match.matchId + "\")"))
        {
            StreamWriter writer = new StreamWriter(matches_won_txt, true);
            writer.WriteLine("matchWon(\"" + current_match.matchId + "\")");
            writer.Close();
        }

        match_finished_event.Invoke(current_match.matchId);
    }

}
