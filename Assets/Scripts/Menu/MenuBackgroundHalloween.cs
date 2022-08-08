using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;

public class MenuBackgroundHalloween : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject postProcessDefault;
    public GameObject postProcessHalloweenMenu;

    Model background_model;
    Model sky_model;
    GameObject merula;
    GameObject ismelda;

    public void spawnMenuBackground()
    {
        postProcessDefault.SetActive(false);
        postProcessHalloweenMenu.SetActive(true);

        CameraManager.current.main_camera.transform.position = new Vector3(-11.816085815429688f, 0.7540035247802734f, 24.68346405029297f);
        CameraManager.current.main_camera.transform.rotation = new Quaternion(0.002704386366531253f, -0.9186273813247681f, -0.004992018453776836f, -0.39508432149887087f);
        CameraManager.current.main_camera.GetComponent<Camera>().fieldOfView = 43.50319f;

        background_model = ModelManager.loadModel("b_GodricsHollow_Graveyard_skin");
        Texture2D new_lightmap = Resources.Load("menulightmap") as Texture2D;
        background_model.game_object.transform.Find("Foilage_A_01").GetComponent<MeshRenderer>().material.SetTexture("u_lightmapMap", new_lightmap);
        background_model.game_object.transform.Find("Foilage_B").GetComponent<MeshRenderer>().material.SetTexture("u_lightmapMap", new_lightmap);

        background_model.game_object.transform.Find("RoofIvy").GetComponent<MeshRenderer>().material.SetTexture("u_lightmapMap", new_lightmap);

        Texture2D new_lightmap2 = Resources.Load("menulightmap2") as Texture2D;
        background_model.game_object.transform.Find("ChurchWalls").GetComponent<MeshRenderer>().material.SetTexture("u_lightmapMap", new_lightmap2);
        background_model.game_object.transform.Find("ChurchWallsCorners").GetComponent<MeshRenderer>().material.SetTexture("u_lightmapMap", new_lightmap2);

        sky_model = ModelManager.loadModel("p_NightBG01_skin");

        ActorController merula_manager = Actor.spawnActor("c_Merula_Normal_skin", null, "merula_menu");
        ActorController ismelda_manager = Actor.spawnActor("c_Ismelda_Normal_skin", null, "ismelda_menu");

        merula.transform.position = new Vector3(-9.11499977f, 0, 23.0330009f);
        merula.transform.eulerAngles = new Vector3(0, 285.151855f, 0);

        ismelda.transform.position = new Vector3(-9.3739996f, 0, 20.6919994f);
        ismelda.transform.eulerAngles = new Vector3(0, 38.4808159f, 0);

        merula_manager.actor_animation.replaceCharacterIdle("c_Stu_SpellCastingStanding01_lumosLoop");
        merula_manager.actor_animation.updateAnimationState();

        GameObject merula_wand_light_gameobject = Instantiate(Resources.Load("merula_wand_light") as GameObject);
        Transform wand_tip = merula.gameObject.transform.Find("Armature/jt_all_bind/jt_wandTip_bind");
        if (wand_tip == null)
        {
            Debug.LogError("wand tip null");
        }
        merula_wand_light_gameobject.transform.parent = wand_tip;
        merula_wand_light_gameobject.transform.localPosition = new Vector3(-0.0799999982f, 0.0500000007f, -2.22000003f);
        merula_wand_light_gameobject.transform.localEulerAngles = Vector3.zero;

        ismelda_manager.actor_animation.replaceCharacterIdle("c_Stu_Searching01_Idle01");
        ismelda_manager.actor_animation.updateAnimationState();
        GameObject ismelda_wand_light_gameobject = Instantiate(Resources.Load("merula_wand_light") as GameObject);
        Transform ismelda_wand_tip = ismelda.gameObject.transform.Find("Armature/jt_all_bind/jt_wandTip_bind");
        if (ismelda_wand_tip == null)
        {
            Debug.LogError("wand tip null");
        }
        ismelda_wand_light_gameobject.transform.parent = ismelda_wand_tip;
        ismelda_wand_light_gameobject.transform.localPosition = new Vector3(-0.0799999982f, 0.0500000007f, -2.22000003f);
        ismelda_wand_light_gameobject.transform.localEulerAngles = Vector3.zero;




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
        GameObject.Destroy(sky_model.game_object);
        GameObject.Destroy(merula);
        GameObject.Destroy(ismelda);
    }

}
