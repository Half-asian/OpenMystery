using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;

public class MenuBackgroundChristmas : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject postProcessChristmasMenu;
    ActorController merula_manager;
    Model background_model;
    GameObject sky_model;
    GameObject _light;
    public void spawnMenuBackground()
    {
        postProcessChristmasMenu.SetActive(true);
        GameStart.post_process_manager.PostProcessDefault.SetActive(false);

        //CameraManager.current.main_camera.transform.localPosition = new Vector3(5.94999981f, 2.93000007f, 6.42000008f);
        //CameraManager.current.main_camera.transform.localRotation = new Quaternion(-0.02014447f, 0.906656086f, -0.0440791287f, -0.419077516f);
        //CameraManager.current.main_camera.GetComponent<Camera>().fieldOfView = 58.3f;

        background_model = ModelManager.loadModel("b_WinterBallroomStudent_skin");

        merula_manager = Actor.spawnActor("c_Merula_Normal_skin", null, "merula_menu");

        merula_manager.transform.position = new Vector3(1.028f, 0, -0.0379999988f);
        merula_manager.transform.eulerAngles = new Vector3(0, 285.151855f, 0);


        merula_manager.replaceCharacterIdle("", "c_Penny_DanceWithAvatar01_loop");

        _light = new GameObject();
        _light.AddComponent<Light>();
        _light.GetComponent<Light>().intensity = 5000;
        _light.transform.position = new Vector3(0.537f, 1.09000003f, 1.03499997f);


        Prop prop;
        if (Prop.spawned_props.ContainsKey(name))
        {
            prop = Prop.spawned_props[name];
        }
        else
        {
            Model model = ModelManager.loadModel("p_MerulasMagicalBlindfold_skin");
            prop = model.game_object.AddComponent<Prop>();
            prop.setup(name, model, Prop.spawner.Event, "");
            Prop.spawned_props[name] = prop;
        }

        prop.model.game_object.transform.parent =  merula_manager.model.pose_bones["jt_head_bind"];
        prop.model.game_object.transform.localPosition = Vector3.zero;
        prop.model.game_object.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0)); //Works for "merula:jt_head_bind:p_MerulasMagicalBlindfold_skin".
        

        /*ModelManager.loadModel(ref quidditch_tent, "b_QuidditchTent_01_skin", "c3b");

        ModelManager.loadModel(ref skye, "c_SkyeParkins_skin", "c3b");
        ModelManager.loadModel(ref erika, "c_ErikaRath_skin", "c3b");
        ModelManager.loadModel(ref orion, "c_OrionAmari_skin", "c3b");
        ModelManager.loadModel(ref murphy, "c_MurphyMcNully_skin", "c3b");
        ModelManager.loadModel(ref wheelchair, "p_MurphyMcNullyWheelchair_skin", "c3b");

        skye.transform.position = new Vector3(-1.37f, 0, 1999.513f);
        skye.transform.eulerAngles = new Vector3(0, 53.48f, 0);
        skye.AddComponent<Animation>();
        skye.GetComponent<Animation>().AddClip(AnimationManager.loadAnimationClip("c_Stu_DialogueLaughing01", "c3b", null), "a");
        skye.GetComponent<Animation>().wrapMode = WrapMode.Loop;
        skye.GetComponent<Animation>().Play("a");

        erika.transform.position = new Vector3(0.54f, 0, 2001.41f);
        erika.transform.eulerAngles = new Vector3(0, 175.112f, 0);
        erika.AddComponent<Animation>();
        erika.GetComponent<Animation>().AddClip(AnimationManager.loadAnimationClip("c_Stu_DialogueDetermined01", "c3b", null), "a");
        erika.GetComponent<Animation>().wrapMode = WrapMode.Loop;
        erika.GetComponent<Animation>().Play("a");

        orion.transform.position = new Vector3(-1.135f, 0, 2000.599f);
        orion.transform.eulerAngles = new Vector3(0, 147.372f, 0);
        orion.AddComponent<Animation>();
        orion.GetComponent<Animation>().AddClip(AnimationManager.loadAnimationClip("c_Stu_DialoguePensive01", "c3b", null), "a");
        orion.GetComponent<Animation>().wrapMode = WrapMode.Loop;
        orion.GetComponent<Animation>().Play("a");

        murphy.transform.position = new Vector3(-0.606f, 0, 1999.209f);
        murphy.transform.eulerAngles = new Vector3(0, 10.38f, 0);
        murphy.AddComponent<Animation>();
        murphy.GetComponent<Animation>().AddClip(AnimationManager.loadAnimationClip("c_MurphyMcNully_DialogueCommentatingIdle01", "c3b", null), "a");
        murphy.GetComponent<Animation>().wrapMode = WrapMode.Loop;
        murphy.GetComponent<Animation>().Play("a");

        wheelchair.transform.position = new Vector3(-0.606f, 0, 1999.209f);
        wheelchair.transform.eulerAngles = new Vector3(0, 10.38f, 0);

        quidditch_tent.transform.position = new Vector3(0, 0, 2000);*/
    }

    public void destroy()
    {
        GameStart.post_process_manager.PostProcessDefault.SetActive(true);

        postProcessChristmasMenu.SetActive(false);
        GameObject.Destroy(background_model.game_object);
        GameObject.Destroy(sky_model);
        if (merula_manager != null)
            GameObject.Destroy(merula_manager.gameObject);
        GameObject.Destroy(_light);
    }

}
