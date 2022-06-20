using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class ConfigSound : Config<ConfigSound>
{

    [System.Serializable]
    public class _BGMusic //What is the point of this? Seems to do nothing.
    {
        public string playlistId;
    }

    [System.Serializable]
    public class _Ambient
    {
        public string contentPackId;
        public string soundname; //filename
        public string trigger;
    }

    [System.Serializable]
    public class _Playlist
    {
        public string contentPackId;
        public string[] files; //filenames
        public string frequencyID;
        public int neededCount;
        public string playlistId;
    }

    [System.Serializable]
    public class _Sound
    {
        public string contentPackId;
        public string filename;
        public bool loops;
        public string soundId;
        public int weight;
    }

    [System.Serializable]
    public class _SFX //Only used for UI
    {
        [JsonProperty(PropertyName = "event")]
        public string _event;
        public string file;
        public bool needed;
        public string objectId;
    }

    public List<_BGMusic> BGMusic;
    public List<_Ambient> Ambient;
    public List<_Playlist> Playlist;
    public List<_Sound> Sounds;
    public List<_SFX> SFX;

    public override ConfigSound combine(List<ConfigSound> other_list)
    {
        for (int i = 1; i < other_list.Count; i++)
        {
            foreach (_BGMusic key in other_list[i].BGMusic)
            {
                if (BGMusic == null)
                    BGMusic = new List<_BGMusic>();
                BGMusic.Add(key);
            }
            foreach (_Ambient key in other_list[i].Ambient)
            {
                if (Ambient == null)
                    Ambient = new List<_Ambient>();
                Ambient.Add(key);
            }
            foreach (_Playlist key in other_list[i].Playlist)
            {
                if (Playlist == null)
                    Playlist = new List<_Playlist>();
                Playlist.Add(key);
            }
            foreach (_Sound key in other_list[i].Sounds)
            {
                if (Sounds == null)
                    Sounds = new List<_Sound>();
                Sounds.Add(key);
            }
            foreach (_SFX key in other_list[i].SFX)
            {
                if (SFX == null)
                    SFX = new List<_SFX>();
                SFX.Add(key);
            }
        }
        return this;
    }
    public static void getConfig()
    {
        Configs.config_sound = getJObjectsConfigsListST("Playlist");
    }
}
