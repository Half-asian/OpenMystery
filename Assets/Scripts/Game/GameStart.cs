using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using ModelLoading;
using UnityEngine.EventSystems;

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
    public AudioClip music;

    public Transform actors_holder;
    public Transform props_holder;

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
        var actor = Actor.getActor(character_name);
        if (actor == null)
            return;

        foreach (Model patch in actor.patches) {
            Destroy(patch.game_object);
        }
        actor.patches.Clear();
    }

    public static void Log(string message)
    {
        Debug.Log(message);
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
            patch.game_object.transform.parent = Actor.getActor(character_name).gameObject.gameObject.transform;
            patch.game_object.transform.localPosition = Vector3.zero;
            patch.game_object.transform.localRotation = Quaternion.Euler(Vector3.zero);
            Actor.getActor(character_name).patches.Add(patch);
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
        CameraManager.current.cleanup();
        quidditch_manager.cleanup();
        GraduationUI.current.hideGraduation();

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("hud_dialog"))
        {
            GameObject.Destroy(g);
        }
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("hud_important"))
        {
            GameObject.Destroy(g);
        }
        Resources.UnloadUnusedAssets();
        onReturnToMenu.Invoke();
        menu_background.spawnMenuBackground();
    }

    public void Awake()
    {
        string engine_variables_json = Path.Combine("..", "engine_variables.json");
        //Are we in model view or game mode?
        if (!File.Exists(engine_variables_json))
            throw new System.Exception("Please launch the game using the launcher.");
        GlobalEngineVariables.CreateFromJSON(engine_variables_json);
        if (!GlobalEngineVariables.checkIntegrity())
            throw new System.Exception("Please launch the game using the launcher.");
        PlayerManager.initialize();
    }

    public async void Start()
    {
        onReturnToMenu = delegate { };

        Debug.Log("GameStart Start");
        Sound.current.playCustom("theblueghost.mp3");

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
        TextureManager.Initialize();
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
            File.Delete(Path.Combine("..", "engine_variables.json"));

        main_menu.state = MainMenu.State.stateLoadingScreenLoading;

        ScreenFade.fadeFrom(1.0f, Color.black, true);

        System.IO.FileStream oFileStream = null;

        string screenshots_folder = Path.Combine(GlobalEngineVariables.player_folder, "..", "..", "screenshots");
        string choices_made_txt = Path.Combine(GlobalEngineVariables.player_folder, "choices_made.txt");
        string goals_complete_txt = Path.Combine(GlobalEngineVariables.player_folder, "goals_complete.txt");
        string goalchains_complete_txt = Path.Combine(GlobalEngineVariables.player_folder, "goalchains_complete.txt");
        string matches_won_txt = Path.Combine(GlobalEngineVariables.player_folder, "matches_won.txt");
        string skills_unlocked_txt = Path.Combine(GlobalEngineVariables.player_folder, "skills_unlocked.txt");
        string content_vars_txt = Path.Combine(GlobalEngineVariables.player_folder, "content_vars.txt");

        if (!Directory.Exists(screenshots_folder))
        {
            Directory.CreateDirectory(screenshots_folder);
        }

        if (!File.Exists(choices_made_txt))
        {
            oFileStream = new System.IO.FileStream(choices_made_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(goals_complete_txt))
        {
            oFileStream = new System.IO.FileStream(goals_complete_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(goalchains_complete_txt))
        {
            oFileStream = new System.IO.FileStream(goalchains_complete_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(matches_won_txt))
        {
            oFileStream = new System.IO.FileStream(matches_won_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(skills_unlocked_txt))
        {
            oFileStream = new System.IO.FileStream(skills_unlocked_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        if (!File.Exists(content_vars_txt))
        {
            oFileStream = new System.IO.FileStream(content_vars_txt, System.IO.FileMode.Create);
            oFileStream.Close();
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;

        ui_manager.setup();
        Debug.Log("StartLoading");
        Configs.preload();
        List<Task> tasks = new List<Task>();

        if (GlobalEngineVariables.launch_mode == "character")
            await Task.Run(() => Configs.loadConfigModelInspector());
        else if (GlobalEngineVariables.launch_mode == "model_inspector" || model_inspector)
            await Task.Run(() => Configs.loadConfigModelInspector());
        else
            await Task.Run(() => Configs.loadConfigAll());

        Configs.postload();
        onConfigsLoaded.Invoke();
        CameraManager.current.initialise();

        if (GlobalEngineVariables.launch_mode == "model_inspector" || model_inspector)
        {
            SceneManager.LoadScene("ModelInspector");
        }
        else if (GlobalEngineVariables.launch_mode == "character")
        {
            SceneManager.LoadScene("CharacterCreator");
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
        //Configs.predicate_alias_dict["rowanY6Look"].aliasedPredicate = "false";
    }
}