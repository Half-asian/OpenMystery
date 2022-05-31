using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuBackgroundQuidditch : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject postProcessDefault;
    public GameObject postProcessHalloweenMenu;

    Model background_model;
    GameObject quidditch_tent;
    GameObject skye;
    GameObject erika;
    GameObject orion;
    GameObject murphy;
    GameObject wheelchair;

    public void spawnMenuBackground()
    {
        postProcessDefault.SetActive(false);
        postProcessHalloweenMenu.SetActive(true);
        CameraManager.current.main_camera.transform.localPosition = new Vector3(-14.7869997f, -18.1630001f, 82.9499969f);
        CameraManager.current.main_camera.transform.localRotation = Quaternion.Euler(new Vector3(1.24213919e-08f, 3.91365266f, 2.71203842e-08f));
        background_model = ModelManager.loadModel("b_GodricsHollow_Graveyard_skin");


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
        postProcessDefault.SetActive(true);
        postProcessHalloweenMenu.SetActive(false);
        GameObject.Destroy(background_model.game_object);
        GameObject.Destroy(quidditch_tent);
        GameObject.Destroy(skye);
        GameObject.Destroy(erika);
        GameObject.Destroy(orion);
        GameObject.Destroy(murphy);
        GameObject.Destroy(wheelchair);
    }

}
