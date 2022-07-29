using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public static GameStart current;

    public static event Action onReturnToMenu = delegate { };
    public static event Action<string> onChoiceMade = delegate { };
    public static event Action onUpdate = delegate { };
    public static event Action onConfigsLoaded = delegate { };

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


    public static void logWrite(string message)
    {
        //StreamWriter writer = new StreamWriter("log.txt", true);
        Debug.Log(message);
        //writer.WriteLine(message);
        //writer.Close();
    }

    public void Awake()
    {
        //Are we in model view or game mode?
        if (!File.Exists("..\\engine_variables.json"))
            throw new System.Exception("Please launch the game using the launcher.");
        GlobalEngineVariables.CreateFromJSON("..\\engine_variables.json");
        if (!GlobalEngineVariables.checkIntegrity())
            throw new System.Exception("Please launch the game using the launcher.");
        PlayerManager.initialize();
    }

    public async void Start()
    {
        onReturnToMenu = delegate { };

        Debug.Log("GameStart Start");

        current = this;

        //Initialize Managers
        ModelManager.Initialize();
        Actor.Initialize();
        Prop.Initialize();
        Scenario.Initialize();
        Scene.Initialize();
        Location.Initialize();
        Goal.Initialize();
        GoalChain.Initialize();
        Objective.Initialize();
        HubNPC.Initialize();
        Tappie.Initialize();

        //anything thats a getcomponent, makes use of coroutines or needs references to game objects
        

        dialogue_manager = GetComponent<DialogueManager>();
        event_manager = GetComponent<EventManager>();
        interaction_manager = GetComponent<InteractionManager>();
        quidditch_manager = new Quidditch();
        encounter_manager = GetComponent<EncounterManager>();
        ui_manager = GameObject.Find("UI Handler").GetComponent<UiManager>();
        post_process_manager = GetComponent<PostProcess>();
        main_menu = GameObject.Find("MainMenuCanvas").GetComponent<MainMenu>();


        if (!Application.isEditor)
            File.Delete("..\\engine_variables.json");

        main_menu.state = MainMenu.State.stateLoadingScreenLoading;

        ScreenFade.fadeFrom(1.0f, Color.black);

        Player.local_avatar_clothing_type = null;
        Player.local_avatar_secondary_clothing_option = null;

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
        }
        else
            await Task.Run(() => Configs.loadConfigModelInspector());
        Configs.postload();
        onConfigsLoaded.Invoke();
        CameraManager.current.initialise();
        stopwatch.Stop();
        Debug.Log("Time to load configs: " + stopwatch.Elapsed);
        //Sound.playBGMusic("BGM");

        Sound.current.playCustom("theblueghost.mp3");

        if (model_inspector)
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