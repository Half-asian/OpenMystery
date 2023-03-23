using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker
{
    private static Dictionary<string, string> aliases = new Dictionary<string, string>()
    {
    };

    public static void addAlias(string alias, string speaker)
    {
        aliases[alias] = speaker;
    }

    public static void removeAlias(string alias)
    {
        aliases.Remove(alias);
    }

    public static string mapSpeakerName(string speaker)
    {
        if (speaker == "Avatar") //I'm not putting this in the aliases, because then something could theoretically remove it.
            return LocalData.getLine("You");

        if (speaker == null) return "";
        if (EncounterDate.companion != null)
            speaker = speaker.Replace("::Date::", Configs.config_companion.Companion[EncounterDate.companion].speakerId);

        if (aliases.ContainsKey(speaker))
        {
            speaker = aliases[speaker];
        }

        foreach (var mapping in Configs.config_dialogue_speaker_mapping.DialogueSpeakerMapping)
        {
            if (mapping.mapId == speaker)
            {
                if (Predicate.parsePredicate(mapping.predicate))
                    speaker = mapping.speakerId;
            }
        }

        if (!Configs.config_dialogue_speakers.DialogueSpeaker.ContainsKey(speaker))
        {
            Debug.LogError("Could not find speaker from id " + speaker);
            return speaker;
        }

        return LocalData.getLine(Configs.config_dialogue_speakers.DialogueSpeaker[speaker].name);
    }
}
