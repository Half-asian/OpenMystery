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
        Scenario.onScenarioLoaded += refreshBgSound;
        Scenario.onScenarioLoaded += refreshMusic;
    }

    #region static methods

    public static void playBGMusic(string BGMusic_id)
    {
        current.playAudioFile(Configs.playlist_dict[BGMusic_id].files[0], "bgaudio");
    }

    public static void playSoundEffect(string sound_effect)
    {
        if (Configs.playlist_dict.ContainsKey(sound_effect))
        {
            current.playAudioFile(Configs.playlist_dict[sound_effect].files[0], "soundeffect");
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
            if (Configs.playlist_dict.ContainsKey(Scenario.current.scenario_config.bgSoundPlaylistId))
            {
                if (Configs.playlist_dict[Scenario.current.scenario_config.bgSoundPlaylistId].files != null)
                {
                    playAudioFile(Configs.playlist_dict[Scenario.current.scenario_config.bgSoundPlaylistId].files[0], "bgaudio");
                }
            }
        }
    }

    private void refreshMusic()
    {
        if (Scenario.current.scenario_config.musicPlaylistId != null)
        {
            if (Configs.playlist_dict.ContainsKey(Scenario.current.scenario_config.musicPlaylistId))
            {
                if (Configs.playlist_dict[Scenario.current.scenario_config.musicPlaylistId].files != null)
                {
                    playAudioFile(Configs.playlist_dict[Scenario.current.scenario_config.musicPlaylistId].files[0], "bgaudio");
                }
            }
        }
    }

    private void playAudioFile(string filename, string type)
    {
        if (SoundEffectPlayer == null) return;
        if (File.Exists(GlobalEngineVariables.assets_folder + "\\sounds\\" + filename) || filename == "none")
        {
            StartCoroutine(PlayAudioFile(GlobalEngineVariables.assets_folder + "\\sounds\\" + filename, type));
        }
        else
        {
            Debug.Log("Could not find sound file " + filename);
        }
    }

    public void playCustom(AudioClip audioClip)
    {
        if (BGMusicPlayer.GetComponent<AudioSource>().clip == audioClip)
            return;

        BGMusicPlayer.GetComponent<AudioSource>().clip = audioClip;
        BGMusicPlayer.GetComponent<AudioSource>().Play();
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
            else if (type == "bgaudio")
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

