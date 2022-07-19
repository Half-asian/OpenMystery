using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graduation
{

    static Model env;
    static ActorController avatar;
    public static void Graduate()
    {
        GameStart.current.StartCoroutine(GraduateCoroutine());

    }

    static void Cleanup()
    {
        GameObject.Destroy(env.game_object);
        avatar.destroy();
    }

    static IEnumerator GraduateCoroutine()
    {
        ScreenFade.fadeTo(1.0f, Color.black);

        yield return new WaitForSeconds(1);

        GameStart.fakeCleanup();
        GraduationUI.current.showGraduation("YEAR " + Player.local_avatar_year + " COMPLETE");
        env = ModelManager.loadModel("b_GreatHall_Subdued_skin");
        avatar = Actor.spawnActor("Avatar", null, "Avatar");
        avatar.model.game_object.transform.position = new Vector3(-5.558f, 0.228f, 4.538f);
        CameraManager.current.simple_camera_controller.enabled = false;
        //GameStart.post_process_manager.PostProcessDefaultLight.transform.eulerAngles = new Vector3(14.1f, 0, 0);
        CameraManager.current.main_camera.localPosition = new Vector3(0, 0, 0);
        CameraManager.current.main_camera.localEulerAngles = new Vector3(0, -180, 0);
        GameStart.onReturnToMenu += Cleanup;

        CameraManager.current.main_camera_jt_cam_bind.localPosition = new Vector3(5.593f, 1.905f, 7.171f);
        CameraManager.current.main_camera_jt_cam_bind.localRotation = Quaternion.Euler(new Vector3(35.327f, 0, 0));

        yield return new WaitForSeconds(1);

        ScreenFade.fadeFrom(1.0f, Color.black);

        yield return new WaitForSeconds(2);

        CameraManager.current.startLerpCamera(
        new Vector3(5.593f, 1.905f, 7.171f),
        Quaternion.Euler(new Vector3(35.327f, 0, 0)),
        new Vector3(5.593f, 0.808f, 7.171f),
        Quaternion.Euler(new Vector3(0, 0, 0)),
        3);
        yield return new WaitForSeconds(3);

        if (Player.local_avatar_gender == "male")
            avatar.actor_animation.replaceCharacterIdle("c_Stu_LevelUpMale01_maleLevelUp");
        else
            avatar.actor_animation.replaceCharacterIdle("c_Stu_LevelUpFemale01_femaleLevelUp");
        yield return new WaitForSeconds(2);
        GameStart.ui_manager.exitPopup();

    }



}
