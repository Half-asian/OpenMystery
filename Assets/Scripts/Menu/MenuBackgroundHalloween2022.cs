using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;
using System;
using System.Security.Cryptography;

public class MenuBackgroundHalloween2022 : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject PostProcessHalloween2022;
    Model background_model;
    ActorController Merula;
    ActorController MC;
    ActorController Jane;
    ActorController SnareC;

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

    Coroutine bobBargeCoroutine = null;

    public void spawnMenuBackground()
    {

        PostProcessHalloween2022.SetActive(true);
        GameStart.post_process_manager.PostProcessDefault.SetActive(false);

        CameraManager.current.setMainLockedCamera(new Vector3(51.08075f, -0.9422843f, 14.49538f), new Quaternion(-0.0830284283f, -0.532737613f, 0.0189941451f, 0.841983438f));
        CameraManager.current.setFOV(95);

        Scene.current = new ConfigScene._Scene();
        Scene.current.Lighting = new ConfigScene._Scene._Lighting();
        Scene.current.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();
        Scene.current.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();

        ConfigScene._Scene._Lighting.Layer env_layer = new ConfigScene._Scene._Lighting.Layer();
        env_layer.name = "CHARACTER";
        env_layer.lights = new string[] { "amb", "dir", "dir2", "dir3"};
        Scene.current.Lighting.layers["CHARACTER"] = env_layer;

        ConfigScene._Scene._Lighting.Light amb_light = new ConfigScene._Scene._Lighting.Light();
        amb_light.color = new string[] { "180.0", "190.0", "230.0" };
        amb_light.intensity = 0.7f;
        amb_light.name = "amb";
        amb_light.type = "ambientLight";

        ConfigScene._Scene._Lighting.Light dir_light = new ConfigScene._Scene._Lighting.Light();
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
        Scene.current.Lighting.lights["dir3"] = dir_light3;


        Scene.current.Lighting.lights["amb"] = amb_light;
        Scene.spawnLights();

        background_model = ModelManager.loadModel("b_MinistryArchive_skin");
        Merula = Actor.spawnActor("c_Merula_Normal_skin", null, "Merula");
        Merula.customAnimationCharacter("Animations/MerulaHalloween2022");
        Merula.transform.position = new Vector3(-50.33944f, -1.629f, 14.60226f);
        Merula.transform.eulerAngles = new Vector3(0, -138.104f, 0);

        MC = Actor.spawnActor("Avatar", null, "MC");
        MC.customAnimationCharacter("Animations/MCHalloween2022");
        MC.transform.localPosition = new Vector3(-50.28798f, -1.625f, 15.213f);
        MC.transform.localEulerAngles = new Vector3(0, -157.526f, 0);

        Jane = Actor.spawnActor("c_Jane_Normal_skin", null, "Jane");
        //Jane.customAnimationCharacter("Animations/JaneHalloween2022");

        Jane.transform.localPosition = new Vector3(-47.102f, 0.744f, 22.997f);
        Jane.transform.localEulerAngles = new Vector3(0, -167.871f, 0);
        Jane.transform.localScale = new Vector3(1, 1.5f, 1.5f);


        //SnareC = Actor.spawnActor("c_DevilsSnare_C_skin", null, "SnareC");
        //SnareC.transform.localPosition = new Vector3(-47.872f, 0.744f, 22.091f);
        //SnareC.transform.localEulerAngles = new Vector3(0, -167.871f, 0);

        barge1 = GameObject.Find(barge1name);
        barge2 = GameObject.Find(barge2name);
        barge3 = GameObject.Find(barge3name);
        barge4 = GameObject.Find(barge4name);
        barge5 = GameObject.Find(barge5name);
        barge6 = GameObject.Find(barge6name);

        bobBargeCoroutine = StartCoroutine(bobBarge());

    }
    IEnumerator bobBarge()
    {
        while (true)
        {
            float newY = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

            barge1.transform.localPosition = new Vector3(barge1.transform.localPosition.x, newY, barge1.transform.localPosition.z);
            barge2.transform.localPosition = new Vector3(barge2.transform.localPosition.x, newY, barge2.transform.localPosition.z);
            barge3.transform.localPosition = new Vector3(barge3.transform.localPosition.x, newY, barge3.transform.localPosition.z);
            barge4.transform.localPosition = new Vector3(barge4.transform.localPosition.x, newY, barge4.transform.localPosition.z);
            barge5.transform.localPosition = new Vector3(barge5.transform.localPosition.x, newY, barge5.transform.localPosition.z);
            barge6.transform.localPosition = new Vector3(barge6.transform.localPosition.x, newY, barge6.transform.localPosition.z);
            //SnareC.transform.localPosition = new Vector3(SnareC.transform.localPosition.x, newY + snarestartHeight, SnareC.transform.localPosition.z);
            Jane.transform.localPosition = new Vector3(Jane.transform.localPosition.x, newY + janestartHeight, Jane.transform.localPosition.z);

            float newY2 = Mathf.Sin(Time.time + 0.3f * cameraBobSpeed) * cameraBobAmplitude;

            CameraManager.current.setMainLockedCamera(new Vector3(51.08075f, newY2 - 0.9422843f, 14.49538f), new Quaternion(-0.0830284283f, -0.532737613f, 0.0189941451f, 0.841983438f));


            yield return null;
        }
    }

    public void destroy()
    {
        GameStart.post_process_manager.PostProcessDefault.SetActive(true);
        PostProcessHalloween2022.SetActive(false);

        StopCoroutine(bobBargeCoroutine);

        MC.destroy();
        Jane.destroy();
        Merula.destroy();
        //SnareC.destroy();

        Destroy(MC);
        Destroy(Jane);
        Destroy(Merula);
        Destroy(SnareC);

        Jane = null;
        //SnareC = null;

        Destroy(background_model.game_object);        
    }

}
