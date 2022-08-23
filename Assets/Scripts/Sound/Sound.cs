using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class Sound : MonoBehaviour
{
    [SerializeField]
    private GameObject BGMusicPlayer;
    [SerializeField]
    private GameObject AMBAudioPlayer;
    [SerializeField]
    private GameObject SoundEffectPlayer;

    public static Sound current;

    void Awake()
    {
        current = this;
        Scenario.onScenarioCallClear += refreshBgSound;
        Scenario.onScenarioCallClear += refreshMusic;
    }

    #region static methods

    public static void playBGMusic(string BGMusic_id)
    {
        if (Configs.playlist_dict.ContainsKey(BGMusic_id))
            current.playAudioFile(Configs.playlist_dict[BGMusic_id].files[0], "bgaudio");
    }

    public static void playAmbient(string ambient_id)
    {
        if (Configs.ambient_dict.ContainsKey(ambient_id))
            current.playAudioFile(Configs.ambient_dict[ambient_id].soundname, "amb");
        else
            current.AMBAudioPlayer.GetComponent<AudioSource>().Stop();

    }

    public static void playSoundEffect(string sound_effect)
    {
        if (Configs.playlist_dict.ContainsKey(sound_effect))
        {
            current.playAudioFile(Configs.playlist_dict[sound_effect].files[0], "soundeffect");
        }
    }

    public static void playSFX(string sfx)
    {
        if (Configs.sfx_dict.ContainsKey(sfx))
        {
            current.playAudioFile(Configs.sfx_dict[sfx].file, "soundeffect");
        }
    }

    public static void playBark(string bark_id)
    {
        if (Configs.playlist_dict.ContainsKey(bark_id))
        {
            if (Configs.playlist_dict[bark_id].files == null)
            {
                Debug.LogError("bark " + bark_id + " defined no files.");
                return;
            }
            current.playAudioFile(Configs.playlist_dict[bark_id].files[0], "bark");
        }
        else
        {
            Debug.LogError("Couldn't find bark " + bark_id);
        }
    }

    public static void playSound(string sound_id)
    {
        if (Configs.sounds_dict.ContainsKey(sound_id))
        {
            current.playAudioFile(Configs.sounds_dict[sound_id].filename, "sound");
        }
        else if (Configs.playlist_dict.ContainsKey(sound_id))
        {
            current.playAudioFile(Configs.playlist_dict[sound_id].files[0], "sound");
        }
        else
        {
            Debug.LogError("Couldn't find sound " + sound_id);
        }
    }
    #endregion

    private void refreshBgSound()
    {
        if (Scenario.current.scenario_config.bgSoundPlaylistId != null)
        {
            if (Scenario.current.scenario_config.bgSoundPlaylistId != "noSoundPL")
            {
                playAmbient(Scenario.current.scenario_config.bgSoundPlaylistId);
            }
            else
            {
                playAmbient("none");
            }
        }
    }

    private void refreshMusic()
    {
        if (Scenario.current.scenario_config.musicPlaylistId != null)
        {
            playBGMusic(Scenario.current.scenario_config.musicPlaylistId);
        }
    }

    private void playAudioFile(string filename, string type)
    {
        if (SoundEffectPlayer == null) return;
        string path = Path.Combine(GlobalEngineVariables.assets_folder, "sounds", filename);
        if (File.Exists(path) || filename == "none")
        {
            StartCoroutine(PlayAudioFile(path, type));
        }
        else
        {
            Debug.Log("Could not find sound file " + filename);
        }
    }

    public void playCustom(string filename)
    {
        Debug.Log("load custom: " + Path.Combine(Path.GetDirectoryName(Application.dataPath), filename));
        StartCoroutine(PlayAudioFile(Path.Combine(Path.GetDirectoryName(Application.dataPath), filename), "custom"));
    }


    IEnumerator PlayAudioFile(string filename, string type)
    {
        if (filename == "none" && type == "amb")
        {
            AMBAudioPlayer.GetComponent<AudioSource>().Stop();
            yield break;
        }

        if (type == "bgaudio")
        {
            if (BGMusicPlayer.GetComponent<AudioSource>().clip != null)
            {
                if (BGMusicPlayer.GetComponent<AudioSource>().clip.name == filename)
                {
                    yield break;
                }
            }
        }

        if (type == "custom")
        {
            if (BGMusicPlayer.GetComponent<AudioSource>().clip != null)
            {
                if (BGMusicPlayer.GetComponent<AudioSource>().clip.name == filename)
                {
                    yield break;
                }
            }
        }

        UnityWebRequest www;
        if (filename.Contains(".wav"))
        {
            www = UnityWebRequestMultimedia.GetAudioClip("file://" + filename, AudioType.WAV);
        }
        else
        {
            www = UnityWebRequestMultimedia.GetAudioClip("file://" + filename, AudioType.MPEG);
        }
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(www.error);
            throw new System.Exception(www.error);
        }
        else
        {
            AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);

            if (type == "bark" || type == "sound") //TODO: sounds have the ability to loop
            {
                CameraManager.current.main_camera.GetComponent<AudioSource>().PlayOneShot(myClip);
            }
            else if (type == "soundeffect")
            {
                SoundEffectPlayer.GetComponent<AudioSource>().Stop();
                SoundEffectPlayer.GetComponent<AudioSource>().PlayOneShot(myClip);
            }
            else if (type == "bgaudio" || type == "custom")
            {
                myClip.name = filename;
                BGMusicPlayer.GetComponent<AudioSource>().clip = myClip;

                BGMusicPlayer.GetComponent<AudioSource>().Play();
            }
            else if (type == "amb")
            {
                AMBAudioPlayer.GetComponent<AudioSource>().clip = myClip;
                AMBAudioPlayer.GetComponent<AudioSource>().Play();
            }
            else
            {
                throw new System.Exception();
            }
        }
    }
}

