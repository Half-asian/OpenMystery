using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;

public class GameStart : MonoBehaviour
{
    public static GameStart current;

    public static event Action onReturnToMenu = delegate { };
    public static event Action<string> onChoiceMade = delegate { };
    public static event Action onUpdate = delegate { };

    public InteractionProject project_callback;

    MainMenu main_menu;
    public static IMenuBackground menu_background;

    public static InteractionManager interaction_manager;
    public static EventManager event_manager;
    public static DialogueManager dialogue_manager;
    public static Quidditch quidditch_manager;
    public static EncounterManager encounter_manager;
    public static Project project_manager;
    public static UiManager ui_manager;
    public static PostProcess post_process_manager;

    public bool model_inspector;
    public string model_inspector_model;
    public string model_inspector_actor;
    public static string _model_inspector_model;
    public static string _model_inspector_actor;
    public AudioClip music;

    void Update()
    {
        onUpdate.Invoke();
    }

    public static void fakeCleanup()
    {
        onReturnToMenu.Invoke();
    }

    public void removePatchesFromCharacter(string character_name)
    {
        if (Actor.actor_controllers.ContainsKey(character_name))
        {
            foreach (Model patch in Actor.actor_controllers[character_name].patches) {
                Destroy(patch.game_object);
            }
            Actor.actor_controllers[character_name].patches.Clear();
        }
    }


    public void removePatchFromCharacter(string character_name, Model patch) //DONT USE THIS
    {
        return;
        if (patch == null) {
            return;
        }
        if (Actor.actor_controllers.ContainsKey(character_name)){

            if (Actor.actor_controllers[character_name].patches.Contains(patch))
            {

            }
            else
            {
                Debug.LogWarning("Could not find patch " + patch.game_object.name);
            }
        }
    }

    public Model addPatchToCharacter(string character_name, string patch_name, Dictionary<string, Transform> parent_bones)
    {
        Debug.Log("add patch " + patch_name + " to " + character_name );
        if (patch_name == "o_Male_ForestFormal_FULL3_skin")
        {
            patch_name = "o_Male_ForestFormal_FULL_skin"; //WHY THE FUCK IS THIS NAMED WRONG
        }
        if (patch_name == "o_Male_ForestFormal_FULL1_skin")
        {
            patch_name = "o_Male_ForestFormal_FULL_skin"; //WHY THE FUCK IS THIS NAMED WRONG
        }
        
        if (parent_bones == null)
        {
            Debug.LogError("FUCK");
        }

        Model patch = ModelManager.loadModel(patch_name, parent_bones);
        if (patch.game_object != null)
        {
            patch.game_object.AddComponent<Animation>();
            patch.game_object.transform.parent = Actor.actor_controllers[character_name].gameObject.gameObject.transform;
            patch.game_object.transform.localPosition = Vector3.zero;
            patch.game_object.transform.localRotation = Quaternion.Euler(Vector3.zero);
            Actor.actor_controllers[character_name].patches.Add(patch);
        }
        else
        {
            Debug.LogWarning("Failed to load patch " + patch_name);
        }
        return patch;
    }


    public void cleanUp() {
        interaction_manager.destroyAllInteractions();

        Scene.destroyScenePrefab();
        dialogue_manager.current_dialogue = null;
        dialogue_manager.dialogue_status = DialogueStatus.Finished;
        dialogue_manager.ui_dialogue.SetActive(false);

        CameraManager.current.resetCamera();
        CameraManager.current.simple_camera_controller.enabled = false;

        menu_background.spawnMenuBackground();
        GraduationUI.current.hideGraduation();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("hud_dialog"))
        {
            GameObject.Destroy(g);
        }
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("hud_important"))
        {
            GameObject.Destroy(g);
        }

        onReturnToMenu.Invoke();
    }

    public void dialogueChoice1Callback()
    {
        Debug.Log("dialogueChoice1Callback");
        dialogue_manager.ui_dialogue_choice_1.SetActive(false);
        dialogue_manager.ui_dialogue_choice_2.SetActive(false);
        dialogue_manager.ui_dialogue_choice_3.SetActive(false);
        dialogue_manager.next_dialogue = dialogue_manager.dialogue_choice_1_next_dialogue;

        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + dialogue_manager.choices[0] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + dialogue_manager.choices[0] + "\")");
            writer.Close();
        }

        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[1] + "\")", ""));
        if (dialogue_manager.choices.Count > 2)        
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[2] + "\")", ""));


        dialogue_manager.waiting_for_dialogue = true;
        dialogue_manager.activateDialogue(dialogue_manager.next_dialogue);
    }
    public void dialogueChoice2Callback()
    {
        Debug.Log("dialogueChoice2Callback");
        dialogue_manager.ui_dialogue_choice_1.SetActive(false);
        dialogue_manager.ui_dialogue_choice_2.SetActive(false);
        dialogue_manager.ui_dialogue_choice_3.SetActive(false);
        dialogue_manager.next_dialogue = dialogue_manager.dialogue_choice_2_next_dialogue;
        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + dialogue_manager.choices[1] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + dialogue_manager.choices[1] + "\")");
            writer.Close();
        }
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[0] + "\")", ""));
        if (dialogue_manager.choices.Count > 2)
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[2] + "\")", ""));

        dialogue_manager.waiting_for_dialogue = true;
        dialogue_manager.activateDialogue(dialogue_manager.next_dialogue);
    }
    public void dialogueChoice3Callback()
    {
        Debug.Log("dialogueChoice3Callback");
        dialogue_manager.ui_dialogue_choice_1.SetActive(false);
        dialogue_manager.ui_dialogue_choice_2.SetActive(false);
        dialogue_manager.ui_dialogue_choice_3.SetActive(false); 
        dialogue_manager.next_dialogue = dialogue_manager.dialogue_choice_3_next_dialogue;
        if (!File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Contains("madeChoice(\"" + dialogue_manager.choices[2] + "\")"))
        {
            StreamWriter writer = new StreamWriter(GlobalEngineVariables.player_folder + "\\choices_made.txt", true);
            writer.WriteLine("madeChoice(\"" + dialogue_manager.choices[2] + "\")");
            writer.Close();
        }
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[0] + "\")", ""));
        File.WriteAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt", File.ReadAllText(GlobalEngineVariables.player_folder + "\\choices_made.txt").Replace("madeChoice(\"" + dialogue_manager.choices[1] + "\")", ""));
        dialogue_manager.waiting_for_dialogue = true;
        dialogue_manager.activateDialogue(dialogue_manager.next_dialogue);
    }




    public async Task StartLoading()
    {
        //anything thats a getcomponent, makes use of coroutines or needs references to game objects

        dialogue_manager = GetComponent<DialogueManager>();
        event_manager = GetComponent<EventManager>();
        interaction_manager = GetComponent<InteractionManager>();
        quidditch_manager = new Quidditch();
        encounter_manager = GetComponent<EncounterManager>();
        ui_manager = GameObject.Find("UI Handler").GetComponent<UiManager>();
        post_process_manager = GetComponent<PostProcess>();
        dialogue_manager = GetComponent<DialogueManager>();

        DialogueManager.local_avatar_clothing_type = null;
        DialogueManager.local_avatar_secondary_clothing_option = null;

        System.IO.FileStream oFileStream = null;

        if (!Directory.Exists(GlobalEngineVariables.player_folder + "\\..\\..\\screenshots"))
        {
            Directory.CreateDirectory(GlobalEngineVariables.player_folder + "\\..\\..\\screenshots");
        }

        if (!File.Exists(GlobalEngineVariables.player_folder + "\\choices_made.txt"))
        {
            oFileStream = new System.IO.FileStream(GlobalEngineVariables.player_folder + "\\choices_made.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(GlobalEngineVariables.player_folder + "\\goals_complete.txt"))
        {
            oFileStream = new System.IO.FileStream(GlobalEngineVariables.player_folder + "\\goals_complete.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(GlobalEngineVariables.player_folder + "\\goalchains_complete.txt"))
        {
            oFileStream = new System.IO.FileStream(GlobalEngineVariables.player_folder + "\\goalchains_complete.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(GlobalEngineVariables.player_folder + "\\matches_won.txt"))
        {
            oFileStream = new System.IO.FileStream(GlobalEngineVariables.player_folder + "\\matches_won.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(GlobalEngineVariables.player_folder + "\\skills_unlocked.txt"))
        {
            oFileStream = new System.IO.FileStream(GlobalEngineVariables.player_folder + "\\skills_unlocked.txt", System.IO.FileMode.Create);
            oFileStream.Close();
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;

        ui_manager.setup();
        Debug.Log("StartLoading");
        //await Task.Run(() => Configs.loadConfigs());
        Configs.preload();
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        List<Task> tasks = new List<Task>();
        //tasks.Add(Task.Run(() => Configs.loadConfigsa()));
        //tasks.Add(Task.Run(() => Configs.loadConfigsb()));
        //tasks.Add(Task.Run(() => Configs.loadConfigsc()));
        //await Task.WhenAll(tasks);
        if (GlobalEngineVariables.launch_mode == "character")
        {
            await Task.Run(() => Configs.loadConfigModelInspector());
        }
        else if (!model_inspector)
        {
            await Task.Run(() => Configs.loadConfigAll());
            Configs.postload();
        }
        else
            await Task.Run(() => Configs.loadConfigModelInspector());
        CameraManager.current.initialise();
        stopwatch.Stop();
        Debug.Log("Time to load configs: " + stopwatch.Elapsed);
        //Sound.playBGMusic("BGM");

        Sound.current.playCustom("theblueghost.mp3");

        if (Application.isEditor && model_inspector)
        {
            _model_inspector_model = model_inspector_model;
            _model_inspector_actor = model_inspector_actor;
            UnityEngine.SceneManagement.SceneManager.LoadScene("ModelInspector");
        }
        else if (GlobalEngineVariables.launch_mode == "character")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterCreator");
        }

        else
        {
            ui_manager.setMenu();
            menu_background = GetComponent<IMenuBackground>();
            menu_background.spawnMenuBackground();
        }
        main_menu.loading_spinner.enabled = false;
        ui_manager.please_wait_text.enabled = false;
        ui_manager.press_space_text.enabled = true;
        main_menu.state = MainMenu.State.stateLoadingScreenAwait;
    }
}