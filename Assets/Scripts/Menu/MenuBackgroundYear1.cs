using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelLoading;

public class MenuBackgroundYear1 : MonoBehaviour, IMenuBackground
{
    // Start is called before the first frame update
    public GameObject PostProcessYear1;
    Model background_model;
    Model sky_model;
    ActorController Merula;
    public void spawnMenuBackground()
    {

        PostProcessYear1.SetActive(true);
        GameStart.post_process_manager.PostProcessDefault.SetActive(false);

        CameraManager.current.setMainLockedCamera(new Vector3(0.930000007f, 0.75f, 4.80999994f), new Quaternion(-0.0118420422f, -0.98382771f, -0.0710394531f, 0.164000839f));
        CameraManager.current.setFOV(58.3f);

        Merula = Actor.spawnActor("c_Merula_Normal_skin", null, "Merula");
        Merula.replaceCharacterIdle("", "c_Stu_DialogueSerious01sitting");
        Merula.model.game_object.transform.position = new Vector3(2.32399988f, 0.119999997f, 0.550000012f);
        Merula.model.game_object.transform.rotation = new Quaternion(0, 0.980171442f, 0, 0.198151663f);
        background_model = ModelManager.loadModel("b_Lakeshore_skin");
        sky_model = ModelManager.loadModel("p_LakeshoreSkyDome_skin");
    }

    public void destroy()
    {
        GameStart.post_process_manager.PostProcessDefault.SetActive(true);
        PostProcessYear1.SetActive(false);

        Merula.destroy();
        GameObject.Destroy(background_model.game_object);
        GameObject.Destroy(sky_model.game_object);
        
    }

}
