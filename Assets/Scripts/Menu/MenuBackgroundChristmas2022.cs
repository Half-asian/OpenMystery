using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System;
using System.Security.Cryptography;

public class MenuBackgroundChristmas2022 : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject PostProcessChristmas2022;
    Model background_model;
    Model sky;
    Model snow1;
    Model car;

    ActorController Merula;
    ActorController Bill;
    ActorController Charlie;

    string barge1name = "BargeA_Railing";
    string barge2name = "Barge_LODS";
    string barge3name = "Ropes";
    string barge4name = "RailingCards";
    string barge5name = "Barge_Panels";
    string barge6name = "BargeLamps";

    GameObject barge1;
    GameObject barge2;
    GameObject barge3;
    GameObject barge4; 
    GameObject barge5;
    GameObject barge6;


    public float bobSpeed;
    public float bobAmplitude;
    public float cameraBobSpeed;
    public float cameraBobAmplitude;
    float janestartHeight = 0.744f;
    float snarestartHeight = 1.944f;

    Coroutine bobBargeCoroutine = null;

    public void spawnMenuBackground()
    {

        PostProcessChristmas2022.SetActive(true);
        GameStart.post_process_manager.PostProcessDefault.SetActive(false);

        CameraManager.current.setMainLockedCamera(new Vector3(105.480003f, 75.200034f, -31.4599991f), new Quaternion(0.0622748733f, -0.920771956f, -0.181320518f, -0.339740783f));
        CameraManager.current.setFOV(81);

        Scene.current = new ConfigScene._Scene();
        Scene.current.Lighting = new ConfigScene._Scene._Lighting();
        Scene.current.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();
        Scene.current.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();

        ConfigScene._Scene._Lighting.Layer env_layer = new ConfigScene._Scene._Lighting.Layer();
        env_layer.name = "CHARACTER";
        env_layer.lights = new string[] { "amb" };// "dir", "dir2", "dir3"};
        Scene.current.Lighting.layers["CHARACTER"] = env_layer;

        ConfigScene._Scene._Lighting.Light amb_light = new ConfigScene._Scene._Lighting.Light();
        amb_light.color = new string[] { "255.0", "255.0", "255.0" };
        amb_light.intensity = 1.0f;
        amb_light.name = "amb";
        amb_light.type = "ambientLight";

        /*ConfigScene._Scene._Lighting.Light dir_light = new ConfigScene._Scene._Lighting.Light();
        dir_light.color = new string[] { "200.0", "160.0", "120.0" };
        dir_light.rotation = new string[] { "-130.331181", "20.980526", "-121.478907" };
        dir_light.intensity = 1.258741f;
        dir_light.name = "dir";
        dir_light.type = "directionalLight";

        ConfigScene._Scene._Lighting.Light dir_light2 = new ConfigScene._Scene._Lighting.Light();
        dir_light2.color = new string[] { "180.0", "180.0", "255.0" };
        dir_light2.rotation = new string[] { "-187.316957", "38.045635", "-96.195167" };
        dir_light2.intensity = 0.559441f;
        dir_light2.name = "dir2";
        dir_light2.type = "directionalLight";

        ConfigScene._Scene._Lighting.Light dir_light3 = new ConfigScene._Scene._Lighting.Light();
        dir_light3.color = new string[] { "255.0", "202.0", "132.0" };
        dir_light3.rotation = new string[] { "-71.287748", "-9.978722", "53.942136" };
        dir_light3.intensity = 1.328671f;
        dir_light3.name = "dir3";
        dir_light3.type = "directionalLight";

        Scene.current.Lighting.lights["dir"] = dir_light;
        Scene.current.Lighting.lights["dir2"] = dir_light2;
        Scene.current.Lighting.lights["dir3"] = dir_light3;*/


        Scene.current.Lighting.lights["amb"] = amb_light;
        Scene.spawnLights();

        background_model = ModelManager.loadModel("b_HubLakeshoreWinter_skin");
        sky = ModelManager.loadModel("p_HagridsHutExtSnowyDTSky_skin");
        snow1 = ModelManager.loadModel("p_HGSM_Snowfall_skin");
        snow1.game_object.transform.position = new Vector3(-105.176003f, 74.731369f, -32.8370018f);
        snow1.game_object.transform.eulerAngles = new Vector3(1.84856915f, 25.2137585f, 0.717001915f);
        snow1.game_object.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

        car = ModelManager.loadModel("p_FordAnglia_skin");
        car.game_object.transform.position = new Vector3(-107.319412f, 75.4508972f, -32.4608879f);
        car.game_object.transform.eulerAngles = new Vector3(340.684998f, 302.123871f, 342.968506f);
        car.game_object.transform.localScale = new Vector3(0.629999995f, 0.899999976f, 0.899999976f);

        Merula = Actor.spawnActor("c_Merula_skin", null, "Merula");
        Merula.replaceCharacterIdle("c_Stu_DialogueConfident01sitting");
        Merula.transform.position = new Vector3(-107.416f, 75.4120026f, -32.2410011f);
        Merula.transform.eulerAngles = new Vector3(27.3902473f, 61.3430138f, 353.793396f);

        Bill = Actor.spawnActor("c_BillPrefect_skin", null, "Bill");
        Bill.replaceCharacterIdle("c_Stu_DialogueExitedSitting01");
        Bill.transform.position = new Vector3(-107.463997f, 75.3720016f, -32.4589043f);
        Bill.transform.eulerAngles = new Vector3(16.4332237f, 56.1122284f, 351.505005f);

        Charlie = Actor.spawnActor("c_Charlie_skin", null, "Charlie");
        Charlie.replaceCharacterIdle("c_Stu_DialogueHappy01sitting");
        Charlie.transform.position = new Vector3(-107.057999f, 75.2939987f, -32.4630013f);
        Charlie.transform.eulerAngles = new Vector3(19.1390762f, 53.4420357f, 350.998901f);

        Destroy(car.game_object.transform.Find("cast_mesh").gameObject);
        Destroy(car.game_object.transform.Find("CarGlass_mesh").gameObject);
        Destroy(background_model.game_object.transform.Find("BG").gameObject);
        Destroy(background_model.game_object.transform.Find("frozenLake").gameObject);

        
        //MC = Actor.spawnActor("Avatar", null, "MC");
        //MC.customAnimationCharacter("Animations/MCHalloween2022");
        //MC.transform.localPosition = new Vector3(-50.28798f, -1.625f, 15.213f);
        //MC.transform.localEulerAngles = new Vector3(0, -157.526f, 0);


        //SnareC = Actor.spawnActor("c_DevilsSnare_C_skin", null, "SnareC");
        //SnareC.transform.localPosition = new Vector3(-47.872f, 0.744f, 22.091f);
        //SnareC.transform.localEulerAngles = new Vector3(0, -167.871f, 0);

        //barge1 = GameObject.Find(barge1name);
        //barge2 = GameObject.Find(barge2name);
        // barge3 = GameObject.Find(barge3name);
        //barge4 = GameObject.Find(barge4name);
        //barge5 = GameObject.Find(barge5name);
        // barge6 = GameObject.Find(barge6name);

        bobBargeCoroutine = StartCoroutine(bobBarge());

    }
    IEnumerator bobBarge()
    {
        while (true)
        {
            float newY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

            /*barge1.transform.localPosition = new Vector3(barge1.transform.localPosition.x, newY, barge1.transform.localPosition.z);
            barge2.transform.localPosition = new Vector3(barge2.transform.localPosition.x, newY, barge2.transform.localPosition.z);
            barge3.transform.localPosition = new Vector3(barge3.transform.localPosition.x, newY, barge3.transform.localPosition.z);
            barge4.transform.localPosition = new Vector3(barge4.transform.localPosition.x, newY, barge4.transform.localPosition.z);
            barge5.transform.localPosition = new Vector3(barge5.transform.localPosition.x, newY, barge5.transform.localPosition.z);
            barge6.transform.localPosition = new Vector3(barge6.transform.localPosition.x, newY, barge6.transform.localPosition.z);*/
            //SnareC.transform.localPosition = new Vector3(SnareC.transform.localPosition.x, newY + snarestartHeight, SnareC.transform.localPosition.z);

            float newY2 = Mathf.Sin(Time.time + 0.3f * cameraBobSpeed) * cameraBobAmplitude;

            CameraManager.current.setMainLockedCamera(new Vector3(105.480003f, 75.200034f + newY2, -31.4599991f), new Quaternion(0.0622748733f, -0.920771956f, -0.181320518f, -0.339740783f));
            CameraManager.current.setFOV(81);

            yield return null;
        }
    }

    public void destroy()
    {
        GameStart.post_process_manager.PostProcessDefault.SetActive(true);
        PostProcessChristmas2022.SetActive(false);

        StopCoroutine(bobBargeCoroutine);

        Merula.destroy();
        Bill.destroy();
        Charlie.destroy();

        Destroy(Merula);
        Destroy(Bill);
        Destroy(Charlie);


        Destroy(background_model.game_object);
        Destroy(sky.game_object);
        Destroy(snow1.game_object);
        Destroy(car.game_object);
    }

}
