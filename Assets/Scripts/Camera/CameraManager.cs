using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager current;
    public enum CameraState
    {
        StateFree,
        StateAnimation,
        StateLerp,
        StatePanCamOnTrack,
    }
    


    public Transform main_camera;
    public Transform main_camera_holder;
    public Transform main_camera_jt_all_bind;
    public Transform main_camera_jt_cam_bind;
    public Transform main_camera_jt_anim_bind;
    public SimpleCameraController simple_camera_controller;
    public CameraState camera_state;
    public Model camera_model;
    private IEnumerator lerpCoroutine;


    private void Awake()
    {       
        Scenario.onScenarioLoaded += cleanup;
        current = this;
    }
    private void cleanup()
    {
        if (main_camera_holder.GetComponent<Animator>() != null)
        {
            Destroy(main_camera_holder.GetComponent<Animator>());
        }
    }

    public void initialise()
    {
        simple_camera_controller = main_camera.GetComponent<SimpleCameraController>();
        camera_model = new Model();
        camera_model.game_object = main_camera_holder.gameObject;
        camera_model.jt_all_bind = main_camera_jt_all_bind;
        camera_state = CameraState.StateFree;
        main_camera.transform.localPosition = Vector3.zero;
        main_camera.transform.localEulerAngles = Vector3.zero;
    }

    public void resetCamera()
    {
        main_camera_holder.transform.localScale = new Vector3(-1, 1, 1);
        main_camera_jt_anim_bind.transform.localPosition = Vector3.zero;
        main_camera_jt_anim_bind.transform.localEulerAngles = Vector3.zero;
        main_camera_jt_cam_bind.transform.localPosition = Vector3.zero;
        main_camera_jt_cam_bind.transform.localEulerAngles = Vector3.zero;
        main_camera.transform.localEulerAngles = new Vector3(0, 180, 0);
        main_camera.transform.position = Vector3.zero;//new Vector3(0, 0.05f, 0); //idk?
    }

    public void freeCamera()
    {
        camera_state = CameraState.StateFree;
        //simple_camera_controller.enabled = true;
    }

    public void playCameraAnimation(string animation, bool disable_jt_cam_bind)
    {
        simple_camera_controller.enabled = false;

        camera_state = CameraState.StateAnimation;
        Dictionary<string, AnimationManager.BoneMod> bone_mods = new Dictionary<string, AnimationManager.BoneMod>();

        if (disable_jt_cam_bind)
        {
            bone_mods["jt_cam_bind"] = new AnimationManager.BoneMod(false);
        }

        AnimationClip anim_clip = AnimationManager.loadAnimationClip(animation, camera_model, null, null, bone_mods, true);


        if (main_camera_holder.GetComponent<Animation>() == null)
        {
            main_camera_holder.gameObject.AddComponent<Animation>();
        }
        main_camera_holder.GetComponent<Animation>().AddClip(anim_clip, "default");
        main_camera_holder.GetComponent<Animation>().Play("default");
        main_camera_holder.GetComponent<Animation>().wrapMode = WrapMode.Once;

        GameStart.event_manager.StartCoroutine(GameStart.event_manager.waitCameraAnimation(Time.realtimeSinceStartup, anim_clip.length, animation));
    }

    public void panCamOnTrack(string animation)
    {
        if (animation == null)
        {
            return;
        }

        simple_camera_controller.enabled = false;

        camera_state = CameraState.StatePanCamOnTrack;

        
        AnimationClip anim_clip_pancam = AnimationManager.loadAnimationClip(animation, camera_model, null, null, null, true);

        if (main_camera_holder.GetComponent<Animation>() == null)
        {
            main_camera_holder.gameObject.AddComponent<Animation>();
        }
        main_camera_holder.GetComponent<Animation>().AddClip(anim_clip_pancam, "default");
        main_camera_holder.GetComponent<Animation>().Play("default");
        main_camera_holder.GetComponent<Animation>().wrapMode = WrapMode.Once;
        GameStart.event_manager.StartCoroutine(GameStart.event_manager.waitCameraAnimation(Time.realtimeSinceStartup, anim_clip_pancam.length, animation));
    }

    public ConfigScene._Scene.Camera focusCam(string camera_name)
    {
        if (!Scene.current.camera_dict.ContainsKey(camera_name))
            return null;
        ConfigScene._Scene.Camera camera = Scene.current.camera_dict[camera_name];

        resetCamera();
        if (camera.fullRoomCam_position != null)
            main_camera_jt_cam_bind.transform.localPosition = new Vector3(camera.fullRoomCam_position[0] * 0.01f, camera.fullRoomCam_position[1] * 0.01f, camera.fullRoomCam_position[2] * 0.01f);
        if (camera.fullRoomCam_rotation != null)
        {

            main_camera_jt_cam_bind.transform.rotation = Quaternion.identity;
            main_camera_jt_cam_bind.transform.Rotate(new Vector3(0, 0, camera.fullRoomCam_rotation[2]));
            main_camera_jt_cam_bind.transform.Rotate(new Vector3(0, camera.fullRoomCam_rotation[1], 0));
            main_camera_jt_cam_bind.transform.Rotate(new Vector3(camera.fullRoomCam_rotation[0], 0, 0));
        }

        if (camera.animation != null)
        {
            AnimationClip anim_clip = AnimationManager.loadAnimationClip(camera.animation, camera_model, null, null, null, true);
            anim_clip.SampleAnimation(main_camera_holder.gameObject, 0.0f);
        }

        if (camera.position != null)
        {
            if (camera.position.Length != 0)
                main_camera_jt_cam_bind.transform.localPosition = new Vector3(camera.position[0] * 0.01f, camera.position[1] * 0.01f, camera.position[2] * 0.01f);
        }
        if (camera.rotation != null)
        {
            if (camera.rotation.Length != 0)
            {
                main_camera_jt_cam_bind.transform.rotation = Quaternion.identity;
                main_camera_jt_cam_bind.transform.Rotate(new Vector3(0, 0, camera.rotation[2]));
                main_camera_jt_cam_bind.transform.Rotate(new Vector3(0, camera.rotation[1], 0));
                main_camera_jt_cam_bind.transform.Rotate(new Vector3(camera.rotation[0], 0, 0));
            }
        }

        //Game defined Field of View is often pretty bad. Usually looks better if its constant.
        if (camera.verticalAOV != 0.0f)
            main_camera.GetComponent<Camera>().fieldOfView = camera.verticalAOV * 1.35f;

        simple_camera_controller.enabled = true;
        return camera;
    }

    public ConfigScene._Scene.Camera focusCam(string camera_name, float time)
    {
        if (!Scene.current.camera_dict.ContainsKey(camera_name))
            return null;
        ConfigScene._Scene.Camera camera = Scene.current.camera_dict[camera_name];

        Vector3 end_position = Vector3.zero;
        Quaternion end_rotation = Quaternion.identity;

        if (camera.rotation != null)
        {
            end_rotation *= Quaternion.Euler(new Vector3(0, 0, camera.rotation[2]));
            end_rotation *= Quaternion.Euler(new Vector3(0, camera.rotation[1], 0));
            end_rotation *= Quaternion.Euler(new Vector3(camera.rotation[0], 0, 0));
        }
        if (camera.fullRoomCam_rotation != null)
        {
            end_rotation *= Quaternion.Euler(new Vector3(0, 0, camera.fullRoomCam_rotation[2]));
            end_rotation *= Quaternion.Euler(new Vector3(0, camera.fullRoomCam_rotation[1], 0));
            end_rotation *= Quaternion.Euler(new Vector3(camera.fullRoomCam_rotation[0], 0, 0));
        }

        if (camera.position != null)
            end_position = new Vector3(camera.position[0] * 0.01f, camera.position[1] * 0.01f, camera.position[2] * 0.01f);

        if (camera.fullRoomCam_position != null)
            end_position = new Vector3(camera.fullRoomCam_position[0] * 0.01f, camera.fullRoomCam_position[1] * 0.01f, camera.fullRoomCam_position[2] * 0.01f);

        startLerpCamera(main_camera_jt_cam_bind.transform.localPosition, main_camera_jt_cam_bind.transform.localRotation, end_position, end_rotation, time);
        return camera;
    }

    public ConfigScene._Scene.Camera focusCam(ref string[] action_params) { //action param 1 is probably lerp transition time
        if (camera_state == CameraState.StateLerp)
        {
            main_camera.localPosition = Vector3.zero;
            main_camera.localEulerAngles = new Vector3(0, 180, 0);
        }     
        
        if (camera_state == CameraState.StatePanCamOnTrack)
            main_camera_holder.GetComponent<Animation>().Stop();

        camera_state = CameraState.StateFree;

        ConfigScene._Scene.Camera last_camera = null;
        
        simple_camera_controller.enabled = false;
        if (action_params.Length > 1)
        {
            if (action_params.Length > 2)
                Debug.Log("Camera what the fuck? why do you have more than 2 params?");

            if (action_params[1] == "0")
                last_camera = focusCam(action_params[0]);
            else
            {
                if (float.TryParse(action_params[1], out float time))
                    last_camera = focusCam(action_params[0], time);
                else
                    Debug.LogError("Unknown focuscam string, string");
            }
        }
        return last_camera;
    }

    private IEnumerator LerpCamera(Vector3 start_position, Quaternion start_rotation, Vector3 end_position, Quaternion end_rotation, float length)
    {
        float start_time = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < length + start_time)
        {
            if (camera_state != CameraState.StateLerp)
                yield break;
            float elapsed_time = Time.realtimeSinceStartup - start_time;
            float progress = elapsed_time / length;

            main_camera_jt_cam_bind.transform.localPosition = Vector3.Lerp(start_position, end_position, progress);
            main_camera_jt_cam_bind.transform.localRotation = Quaternion.Lerp(start_rotation, end_rotation, progress);
            yield return null;

        }
        main_camera_jt_cam_bind.transform.localPosition = end_position;
        main_camera_jt_cam_bind.transform.localRotation = end_rotation;
        main_camera.GetComponent<SimpleCameraController>().enabled = true;
    }

    public void startLerpCamera(Vector3 start_position, Quaternion start_rotation, Vector3 end_position, Quaternion end_rotation, float length)
    {
        camera_state = CameraState.StateLerp;
        if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
        lerpCoroutine = LerpCamera(start_position, start_rotation, end_position, end_rotation, length);
        StartCoroutine(lerpCoroutine);
    }
}
