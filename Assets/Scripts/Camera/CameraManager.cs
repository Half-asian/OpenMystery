using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private enum CameraState
    {
        StateStatic,
        StateLocked,
        StateAnimation,
        StateLerp,
        StatePanCamOnTrack,
    }

    private struct CameraTransform
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    public static CameraManager current;

    [SerializeField]
    private Transform main_camera;
    [SerializeField]
    private Transform main_camera_holder;
    [SerializeField]
    private Transform main_camera_jt_all_bind;
    [SerializeField]
    public Transform main_camera_jt_cam_bind;
    [SerializeField]
    private Transform main_camera_jt_anim_bind;
    [SerializeField]

    private CameraState camera_state;
    private Model camera_model;
    private IEnumerator lerpCoroutine;
    private Animation camera_animation;

    private IEnumerator wait_camera_coroutine;

    /*----------        Public        ----------*/
    public void initialise()
    {
        camera_animation = main_camera_holder.GetComponent<Animation>();
        camera_model = new Model();
        camera_model.game_object = main_camera_holder.gameObject;
        camera_model.jt_all_bind = main_camera_jt_all_bind;
        setCameraState(CameraState.StateStatic);
        main_camera.transform.localPosition = Vector3.zero;
        main_camera.transform.localEulerAngles = Vector3.zero;
    }

    public bool getCameraControllable()
    {
        return camera_state == CameraState.StateStatic;
    }

    public void setMainLockedCamera(Vector3 position, Vector3 rotation)
    {
        setCameraState(CameraState.StateLocked);
        resetCamera();
        main_camera.localPosition = position;
        main_camera.localEulerAngles = rotation;
    }
    public void setMainLockedCamera(Vector3 position, Quaternion rotation)
    {
        setCameraState(CameraState.StateLocked);
        resetCamera();
        main_camera.localPosition = position;
        main_camera.localRotation = rotation;
    }

    public void setFOV(float fov)
    {
        main_camera.GetComponent<Camera>().fieldOfView = fov;
    }

    public void cleanup()
    {
        camera_animation.Stop();
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

    public ConfigScene._Scene.Camera focusCam(ref string[] action_params)
    {
        if (camera_state == CameraState.StateLerp)
        {
            main_camera.localPosition = Vector3.zero;
            main_camera.localEulerAngles = new Vector3(0, 180, 0);
        }

        setCameraState(CameraState.StateStatic);

        ConfigScene._Scene.Camera last_camera = null;

        if (action_params.Length < 1)
            return last_camera;


        if (action_params.Length < 2)
        {
            last_camera = staticFocusCam(action_params[0]);
            return last_camera;
        }

        if (action_params.Length > 2)
        {
            last_camera = staticFocusCam(action_params[0]);
            Debug.LogError("Camera what the fuck? why do you have more than 2 params?");
        }
        if (action_params[1] == "0")
        {
            last_camera = staticFocusCam(action_params[0]);
        }
        else
        {
            if (float.TryParse(action_params[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float time))
                last_camera = lerpFocusCam(action_params[0], time);
            else
                Debug.LogError("Unknown focuscam string, string");
        }
        return last_camera;
    }

    public void playCameraAnimation(string animation, bool disable_jt_cam_bind)
    {
        if (wait_camera_coroutine != null) StopCoroutine(wait_camera_coroutine);
        setCameraState(CameraState.StateAnimation);

        Dictionary<string, AnimationManager.BoneMod> bone_mods = new Dictionary<string, AnimationManager.BoneMod>();

        if (disable_jt_cam_bind)
        {
            bone_mods["jt_cam_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, Vector3.one);
            bone_mods["jt_cam_bind"].CameraHack = true;
        }

        AnimationClip anim_clip = AnimationManager.loadAnimationClip(animation, camera_model, null, null, bone_mods: bone_mods, is_camera: true).anim_clip;

        camera_animation.AddClip(anim_clip, "default");
        camera_animation.Play("default");

        wait_camera_coroutine = waitCameraAnimation(anim_clip.length, animation);
        StartCoroutine(wait_camera_coroutine);
    }

    //Plays a camera lerp between two scene cameras
    public void playLerpCamera(Vector3 start_position, Quaternion start_rotation, Vector3 end_position, Quaternion end_rotation, float length)
    {
        setCameraState(CameraState.StateLerp);
        lerpCoroutine = waitLerpCamera(start_position, start_rotation, end_position, end_rotation, length);
        StartCoroutine(lerpCoroutine);
    }

    //The camera pans over a track specified as an animation
    //Camera can still be moved by the player
    public void panCamOnTrack(string animation)
    {
        if (animation == null)
            return;

        setCameraState(CameraState.StatePanCamOnTrack);

        AnimationClip anim_clip_pancam = AnimationManager.loadAnimationClip(animation, camera_model, null, null, null, is_camera: true).anim_clip;
        anim_clip_pancam.wrapMode = WrapMode.Once;
        camera_animation.AddClip(anim_clip_pancam, "default");
        camera_animation.Play("default");
        StartCoroutine(waitPanCam(anim_clip_pancam.length));
    }

    /*----------        Private        ----------*/
    private void Awake()
    {
        Scenario.onScenarioCallClear += cleanup;
        current = this;
    }

    private void setCameraState(CameraState state)
    {
        if (lerpCoroutine != null) StopCoroutine(lerpCoroutine);
        camera_animation.Stop();
        camera_state = state;
    }

    //Gets the position and rotation from a scene camera
    private CameraTransform getCameraTransform(ConfigScene._Scene.Camera camera)
    {
        CameraTransform camera_transform = new CameraTransform();

        //Set the fullroom transform
        if (camera.fullRoomCam_position != null)
        {
            camera_transform.position = new Vector3(
                    camera.fullRoomCam_position[0] * 0.01f,
                    camera.fullRoomCam_position[1] * 0.01f,
                    camera.fullRoomCam_position[2] * 0.01f
                    );
        }

        if (camera.fullRoomCam_rotation != null)
        {
            camera_transform.rotation = Quaternion.identity;
            camera_transform.rotation *= Quaternion.Euler(new Vector3(0, 0, camera.fullRoomCam_rotation[2]));
            camera_transform.rotation *= Quaternion.Euler(new Vector3(0, camera.fullRoomCam_rotation[1], 0));
            camera_transform.rotation *= Quaternion.Euler(new Vector3(camera.fullRoomCam_rotation[0], 0, 0));
        }

        //Set the regular transform
        if (camera.position != null && camera.position.Length != 0)
        {
            camera_transform.position = new Vector3(
                camera.position[0] * 0.01f,
                camera.position[1] * 0.01f,
                camera.position[2] * 0.01f);
        }

        if (camera.rotation != null && camera.rotation.Length != 0)
        {
            camera_transform.rotation = Quaternion.identity;
            camera_transform.rotation *= Quaternion.Euler(new Vector3(0, 0, camera.rotation[2]));
            camera_transform.rotation *= Quaternion.Euler(new Vector3(0, camera.rotation[1], 0));
            camera_transform.rotation *= Quaternion.Euler(new Vector3(camera.rotation[0], 0, 0));
        }
        return camera_transform;
    }


    //Sets the camera to a static position
    //Camera can still be moved by the player
    private ConfigScene._Scene.Camera staticFocusCam(string camera_name)
    {
        Debug.Log("Static FocusCam " + camera_name);

        setCameraState(CameraState.StateStatic);

        //Get the scene camera
        if (!Scene.current.camera_dict.ContainsKey(camera_name))
        {
            Debug.LogError("No camera " + camera_name + " in scene.");
            return null;
        }
        ConfigScene._Scene.Camera camera = Scene.current.camera_dict[camera_name];

        resetCamera();

        CameraTransform camera_transform = getCameraTransform(camera);
        main_camera_jt_cam_bind.transform.localPosition = camera_transform.position;
        main_camera_jt_cam_bind.transform.localRotation = camera_transform.rotation;

        //Set the sampled animation
        if (camera.animation != null)
        {
            AnimationClip anim_clip = AnimationManager.loadAnimationClip(
                camera.animation, camera_model, null, null, null, is_camera: true).anim_clip;
            anim_clip.SampleAnimation(main_camera_holder.gameObject, 0.0f);
        }

        //Game defined Field of View is often pretty bad. Usually looks better if its constant.
        if (camera.verticalAOV != 0.0f)
            main_camera.GetComponent<Camera>().fieldOfView = camera.verticalAOV * 1.35f;

        return camera;
    }

    //Lerps the camera between two scene cameras over a time in seconds
    //Player cannot be moved by the player
    private ConfigScene._Scene.Camera lerpFocusCam(string camera_name, float time)
    {
        Debug.Log("Lerp cam");

        setCameraState(CameraState.StateLerp);

        //Get Camera
        if (!Scene.current.camera_dict.ContainsKey(camera_name))
            return null;
        ConfigScene._Scene.Camera camera = Scene.current.camera_dict[camera_name];

        CameraTransform camera_transform = getCameraTransform(camera);
        Vector3 end_position = camera_transform.position;
        Quaternion end_rotation = camera_transform.rotation;

        playLerpCamera(main_camera_jt_cam_bind.transform.localPosition, main_camera_jt_cam_bind.transform.localRotation, end_position, end_rotation, time);
        return camera;
    }

    /*----------        Coroutines      ----------*/

    private IEnumerator waitLerpCamera(Vector3 start_position, Quaternion start_rotation, Vector3 end_position, Quaternion end_rotation, float length)
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
    }

    private IEnumerator waitCameraAnimation(float length, string animation)
    {
        GameStart.event_manager.setLastCamAnim(""); //Not entirely sure this is necessary

        yield return new WaitForSeconds(length + 0.1f);

        setCameraState(CameraState.StateStatic);
        GameStart.event_manager.notifyCamAnimFinished(animation);
    }

    private IEnumerator waitPanCam(float length)
    {
        yield return new WaitForSeconds(length);
        setCameraState(CameraState.StateStatic);
    }
}
