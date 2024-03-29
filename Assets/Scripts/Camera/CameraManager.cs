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
    private Animation camera_holder_animation_component;
    [SerializeField]
    public Camera camera_component;
    [SerializeField]
    private Transform camera_transform;
    [SerializeField]
    private Transform camera_holder_transform;
    [SerializeField]
    private Transform camera_jt_all_bind_transform;
    [SerializeField]
    public Transform camera_jt_cam_bind_transform;
    [SerializeField]
    private Transform camera_jt_anim_bind_transform;
    [SerializeField]

    //Used for scrolling back and forth a scene
    private HPAnimation scene_cam_animation;

    AnimationClip anim_clip;

    private CameraState camera_state;
    private Model camera_model;
    private IEnumerator lerpCoroutine;

    private IEnumerator wait_camera_coroutine;
    private IEnumerator aov_player_coroutine;

    float pan_cam_on_track_pct = 0.0f;

    /*----------        Public        ----------*/
    public void initialise()
    {
        camera_model = new Model();
        camera_model.game_object = camera_holder_transform.gameObject;
        camera_model.jt_all_bind = camera_jt_all_bind_transform;
        setCameraState(CameraState.StateStatic);
        camera_transform.localPosition = Vector3.zero;
        camera_transform.localEulerAngles = Vector3.zero;
    }

    public bool getCameraControllable()
    {
        return camera_state == CameraState.StateStatic;
    }

    public void setMainLockedCamera(Vector3 position, Vector3 rotation)
    {
        setCameraState(CameraState.StateLocked);
        resetCamera();
        camera_transform.localPosition = position;
        camera_transform.localEulerAngles = rotation;
    }
    public void setMainLockedCamera(Vector3 position, Quaternion rotation)
    {
        setCameraState(CameraState.StateLocked);
        resetCamera();
        camera_transform.localPosition = position;
        camera_transform.localRotation = rotation;
    }

    public void setAOV(float aov)
    {
        camera_component.fieldOfView = aov;
    }

    public void setFOV(float fov)
    {
        camera_component.fieldOfView = fov;
    }

    public void cleanup()
    {
        camera_holder_animation_component.Stop();
    }

    public void resetCamera()
    {
        camera_holder_transform.transform.localScale = new Vector3(-1, 1, 1);
        camera_jt_anim_bind_transform.transform.localPosition = Vector3.zero;
        camera_jt_anim_bind_transform.transform.localEulerAngles = Vector3.zero;
        camera_jt_cam_bind_transform.transform.localPosition = Vector3.zero;
        camera_jt_cam_bind_transform.transform.localEulerAngles = Vector3.zero;
        camera_transform.localEulerAngles = new Vector3(0, 180, 0);
        camera_transform.localPosition = Vector3.zero;
    }

    public ConfigScene._Scene.Camera focusCam(ref string[] action_params)
    {
        if (camera_state == CameraState.StateLerp)
        {
            camera_transform.localPosition = Vector3.zero;
            camera_transform.localEulerAngles = new Vector3(0, 180, 0);
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
            scene_cam_animation = null;
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
        if (aov_player_coroutine != null) StopCoroutine(aov_player_coroutine);

        setCameraState(CameraState.StateAnimation);

        Dictionary<string, AnimationManager.BoneMod> bone_mods = new Dictionary<string, AnimationManager.BoneMod>();

        if (disable_jt_cam_bind)
        {
            bone_mods["jt_cam_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, Vector3.one);
            bone_mods["jt_cam_bind"].CameraHack = true;
        }

        var hpanim = AnimationManager.loadAnimationClip(animation, camera_model, null, null, bone_mods: bone_mods, is_camera: true);
        anim_clip = hpanim.anim_clip;

        camera_holder_animation_component.AddClip(anim_clip, "default");
        camera_holder_animation_component.Play("default");

        if (hpanim.verticalAOVs != null)
        {
            aov_player_coroutine = CameraAOVPlayer(hpanim.verticalAOVs, anim_clip);
            StartCoroutine(aov_player_coroutine);
        }

        wait_camera_coroutine = waitCameraAnimation(anim_clip, animation);
        StartCoroutine(wait_camera_coroutine);
    }

    public void stopCameraAnimation(string animation)
    {
        if (animation == null || anim_clip.name == animation)
            setCameraState(CameraState.StateStatic);
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
    public void panCamOnTrack(float target_pos, float duration)
    {
        setCameraState(CameraState.StatePanCamOnTrack);
        StartCoroutine(panCamCoroutine(target_pos, duration));
    }

    public void moveCamOnTrack(string time)
    {
        if (scene_cam_animation == null) return;
        if (!float.TryParse(time, out float float_time)) return;
        scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, float_time);
    }

    public void moveCamOnTrackCharacter(string character_id)
    {
        if (scene_cam_animation == null) return;
        var character = Actor.getActor(character_id);
        if (character == null) return;
        float best_time = 0f;
        float best_distance = 9999f;
        for (float time = 0f; time < scene_cam_animation.anim_clip.length; time += 0.01f)
        {
            scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, time);
            float distance = Vector3.Distance(character.gameObject.transform.position, camera_jt_cam_bind_transform.position);
            if (distance < best_distance)
            {
                best_time = time;
                best_distance = distance;
            }
        }
        scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, best_time);
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
        camera_holder_animation_component.Stop();
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
        camera_jt_cam_bind_transform.transform.localPosition = camera_transform.position;
        camera_jt_cam_bind_transform.transform.localRotation = camera_transform.rotation;

        //Set the sampled animation
        if (camera.animation == null)
        {
            scene_cam_animation = null;
        }
        else
        {
            scene_cam_animation = AnimationManager.loadAnimationClip(
                camera.animation, camera_model, null, null, null, is_camera: true);
            if (scene_cam_animation != null)
                scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, 0.0f);
        }

        //Game defined Field of View is often pretty bad. Usually looks better if its constant.
        if (camera.verticalAOV != 0.0f)
        {
            setAOV(camera.verticalAOV);
        }

        return camera;
    }

    //Lerps the camera between two scene cameras over a time in seconds
    //Player cannot be moved by the player
    private ConfigScene._Scene.Camera lerpFocusCam(string camera_name, float time)
    {
        setCameraState(CameraState.StateLerp);

        //Get Camera
        if (!Scene.current.camera_dict.ContainsKey(camera_name))
            return null;
        ConfigScene._Scene.Camera camera = Scene.current.camera_dict[camera_name];

        CameraTransform camera_transform = getCameraTransform(camera);
        Vector3 end_position = camera_transform.position;
        Quaternion end_rotation = camera_transform.rotation;

        playLerpCamera(camera_jt_cam_bind_transform.transform.localPosition, camera_jt_cam_bind_transform.transform.localRotation, end_position, end_rotation, time);
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

            camera_jt_cam_bind_transform.transform.localPosition = Vector3.Lerp(start_position, end_position, progress);
            camera_jt_cam_bind_transform.transform.localRotation = Quaternion.Lerp(start_rotation, end_rotation, progress);
            yield return null;
        }
        camera_jt_cam_bind_transform.transform.localPosition = end_position;
        camera_jt_cam_bind_transform.transform.localRotation = end_rotation;
    }

    private IEnumerator waitCameraAnimation(AnimationClip animation_clip, string animation)
    {
        GameStart.event_manager.setLastCamAnim(""); //Not entirely sure this is necessary
        do
        {
            yield return new WaitForSeconds(animation_clip.length);
            if (animation_clip.wrapMode != WrapMode.Loop)
                setCameraState(CameraState.StateStatic);

            GameStart.event_manager.notifyCamAnimFinished(animation);
        }
        while (animation_clip.wrapMode == WrapMode.Loop);
    }

    IEnumerator CameraAOVPlayer(List<VerticalAOV> vertical_aovs, AnimationClip anim_clip)
    {
        float start_time = Time.realtimeSinceStartup;
        float delta_time = 0.0f;

        while (delta_time < anim_clip.length)
        {
            delta_time = (Time.realtimeSinceStartup - start_time);

            foreach (var aov in vertical_aovs)
            {

                float divider = delta_time / anim_clip.length;

                if (divider < aov.start || divider > aov.end)
                    continue;

                divider += aov.start;
                divider += 1.0f - aov.end;
                divider = Mathf.Min(1.0f, divider);
                float val = Mathf.Lerp(aov.startVal, aov.endVal, divider);

                if (val != 0.0f)
                    setAOV(val);
            }


            yield return null;
        }
    }

    private IEnumerator panCamCoroutine(float target_pct, float length)
    {
        float start_time = Time.realtimeSinceStartup;

        float start_pct = pan_cam_on_track_pct;

        while (Time.realtimeSinceStartup < length + start_time)
        {
            if (camera_state != CameraState.StatePanCamOnTrack)
                yield break;
            float elapsed_time = Time.realtimeSinceStartup - start_time;
            float progress = elapsed_time / length;

            pan_cam_on_track_pct = Mathf.Lerp(start_pct, target_pct, progress);

            scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, scene_cam_animation.anim_clip.length * pan_cam_on_track_pct);
            yield return null;
        }
        pan_cam_on_track_pct = target_pct;
        if (scene_cam_animation != null)
            scene_cam_animation.anim_clip.SampleAnimation(camera_holder_transform.gameObject, scene_cam_animation.anim_clip.length * pan_cam_on_track_pct);
        setCameraState(CameraState.StateStatic);
    }
}
