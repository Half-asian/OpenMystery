using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using ModelLoading;
public class CustomizeAvatar : MonoBehaviour
{
    public static CustomizeAvatar current;

    private ActorController customizable_avatar;
    private ActorController customizable_avatar_preview;

    private Model reference_model;
    private Dictionary<string, Transform> customizable_avatar_modified_bones;
    private Model reference_model_preview;
    private Dictionary<string, Transform> customizable_avatar_modified_bones_preview;
    [SerializeField]
    GameObject _camera;
    [SerializeField]
    public GameObject _camera_preview;
    [SerializeField]
    public GameObject _camera_preview_depth;
    AvatarComponents avatar_components;
    public AvatarComponents avatar_components_preview;
    CustomizeAvatarPreviewImages customize_avatar_preview_images;

    [SerializeField]
    Image render_texture_test_image;
    [SerializeField]
    Image render_texture_test_image2;

    void Awake()
    {
        current = this;

        Scene.current = new ConfigScene._Scene();
        Scene.current.Lighting = new ConfigScene._Scene._Lighting();
        Scene.current.Lighting.layers = new Dictionary<string, ConfigScene._Scene._Lighting.Layer>();
        Scene.current.Lighting.lights = new Dictionary<string, ConfigScene._Scene._Lighting.Light>();

        ConfigScene._Scene._Lighting.Layer env_layer = new ConfigScene._Scene._Lighting.Layer();
        env_layer.name = "CHARACTER";
        env_layer.lights = new string[] { "amb", "dir" };
        Scene.current.Lighting.layers["CHARACTER"] = env_layer;

        ConfigScene._Scene._Lighting.Light amb_light = new ConfigScene._Scene._Lighting.Light();
        amb_light.color = new string[] { "255.0", "255.0", "255.0" };
        amb_light.intensity = 1.0f;
        amb_light.name = "amb";
        amb_light.type = "ambientLight";

        ConfigScene._Scene._Lighting.Light dir_light = new ConfigScene._Scene._Lighting.Light();
        dir_light.color = new string[] { "187.0", "207.0", "255.0" };
        dir_light.rotation = new string[] { "-130.331181", "20.980526", "-121.478907" };
        dir_light.intensity = 1.258741f;
        dir_light.name = "dir";
        dir_light.type = "directionalLight";

        ConfigScene._Scene._Lighting.Light dir_light2 = new ConfigScene._Scene._Lighting.Light();
        dir_light2.color = new string[] { "214.0", "241.0", "255.0" };
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

        Scene.current.Lighting.lights["amb"] = amb_light;
        Scene.current.Lighting.lights["dir"] = dir_light;
        Scene.current.Lighting.lights["dir2"] = dir_light2;
        Scene.current.Lighting.lights["dir3"] = dir_light3;

        Scene.spawnLights();

        ModelMaterials.lighting_layers = new List<string>() { "CHARACTER" };

        customize_avatar_preview_images = GetComponent<CustomizeAvatarPreviewImages>();

        createCustomizableAvatar();

        _camera_preview.transform.position = new Vector3(0, 0.773000002f - 10f, 0.338999987f);
        _camera_preview.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        _camera_preview_depth.transform.position = new Vector3(0, 0.773000002f - 10f, 0.338999987f);
        _camera_preview_depth.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        createCustomizablePreviewAvatar();
    }

    public static void setCameraBody()
    {
        current._camera.transform.position = new Vector3(0, 1, 0);
    }

    public static void setCameraHead()
    {
        current._camera.transform.position = new Vector3(-0.144f, 1.312f, 0.992f);
    }

    void createCustomizableAvatar()
    {
        avatar_components = new AvatarComponents(Path.Combine(GlobalEngineVariables.player_folder, "Avatar.json"));

        customizable_avatar = CustomizableAvatarSpawner.spawnCustomizableAvatar(avatar_components.actor_id, "CustomizableAvatar");
        ConfigHPActorInfo._HPActorInfo reference_character = Configs.config_hp_actor_info.HPActorInfo[PlayerManager.current.character_id]; //We use this as a base to apply transforms to

        customizable_avatar.replaceCharacterIdle("c_Stu_DialogueIdle01");

        reference_model = ModelManager.loadModel(reference_character.modelId);

        
        reference_model.game_object.transform.eulerAngles = new Vector3(0, 160f, 0);
        reference_model.game_object.AddComponent<Animation>();
        reference_model.game_object.GetComponent<Animation>().AddClip(AnimationManager.loadAnimationClip("c_Stu_DialogueIdle01", reference_model, reference_character).anim_clip, "default");
        reference_model.game_object.GetComponent<Animation>().Play("default");
        reference_model.game_object.name = "Reference Model";


        customizable_avatar_modified_bones = new Dictionary<string, Transform>();
        avatar_components.setCharacterManager(customizable_avatar);
        avatar_components.base_model = customizable_avatar.model;
        avatar_components.spawnComponents();

        foreach (string bonemod in BonemodMap.bonemod_map.Keys)
        {
            customizable_avatar_modified_bones[bonemod] = avatar_components.base_model.pose_bones[bonemod];
        }

        customizable_avatar.transform.position = new Vector3(-0.24f, 0.55f, 1.32f);
        customizable_avatar.transform.eulerAngles = new Vector3(0, 160, 0);
    }

    void createCustomizablePreviewAvatar()
    {
        avatar_components_preview = new AvatarComponents(Path.Combine(GlobalEngineVariables.player_folder, "Avatar.json"));

        customizable_avatar_preview = CustomizableAvatarSpawner.spawnCustomizableAvatar(avatar_components_preview.actor_id, "CustomizableAvatarPreview");
        ConfigHPActorInfo._HPActorInfo reference_character = Configs.config_hp_actor_info.HPActorInfo[PlayerManager.current.character_id]; //We use this as a base to apply transforms to
        customizable_avatar_preview.replaceCharacterIdle("c_Stu_DialogueIdle01");

        reference_model_preview = ModelManager.loadModel(reference_character.modelId);
        /*customizable_avatar_preview.animation_component["default"].time = 0.01f;
        customizable_avatar_preview.animation_component["default"].enabled = true;
        customizable_avatar_preview.animation_component["default"].weight = 1;
        customizable_avatar_preview.animation_component.Sample();
        customizable_avatar_preview.animation_component["default"].enabled = false;*/

        reference_model_preview.game_object.name = "reference model preview";

        customizable_avatar_modified_bones_preview = new Dictionary<string, Transform>();
        avatar_components_preview.setCharacterManager(customizable_avatar_preview);
        avatar_components_preview.base_model = customizable_avatar_preview.model;
        avatar_components_preview.spawnComponents();

        foreach (string bonemod in BonemodMap.bonemod_map.Keys)
        {
            customizable_avatar_modified_bones_preview[bonemod] = avatar_components_preview.base_model.pose_bones[bonemod];
        }


        customizable_avatar_preview.transform.position = new Vector3(0, -10, 0);
        customizable_avatar_preview.transform.rotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        //Resets bones to reference.
        foreach (string bone in reference_model.pose_bones.Keys)
        {
            avatar_components.base_model.pose_bones[bone].transform.localPosition = reference_model.pose_bones[bone].localPosition;
            avatar_components.base_model.pose_bones[bone].transform.localRotation = reference_model.pose_bones[bone].localRotation;
            avatar_components.base_model.pose_bones[bone].transform.localScale = reference_model.pose_bones[bone].localScale;
        }

        if (avatar_components != null)
        {
            foreach (string bone_mod in avatar_components.bonemods.Keys)
            {


                customizable_avatar_modified_bones[bone_mod].transform.localPosition = reference_model.pose_bones[bone_mod].localPosition + avatar_components.bonemods[bone_mod].translation;
                customizable_avatar_modified_bones[bone_mod].transform.localRotation = avatar_components.bonemods[bone_mod].rotation * reference_model.pose_bones[bone_mod].localRotation;

                Vector3 new_scale = reference_model.pose_bones[bone_mod].localScale;
                new_scale.x *= avatar_components.bonemods[bone_mod].scale.x;
                new_scale.y *= avatar_components.bonemods[bone_mod].scale.y;
                new_scale.z *= avatar_components.bonemods[bone_mod].scale.z;
                customizable_avatar_modified_bones[bone_mod].transform.localScale = new_scale;
            }
        }




        foreach (string bone in reference_model_preview.pose_bones.Keys)
        {
            avatar_components_preview.base_model.pose_bones[bone].transform.localPosition = reference_model_preview.pose_bones[bone].localPosition;
            avatar_components_preview.base_model.pose_bones[bone].transform.localRotation = reference_model_preview.pose_bones[bone].localRotation;
            avatar_components_preview.base_model.pose_bones[bone].transform.localScale = reference_model_preview.pose_bones[bone].localScale;
        }

        if (avatar_components_preview != null)
        {
            foreach (string bone_mod in avatar_components_preview.bonemods.Keys)
            {
                customizable_avatar_modified_bones_preview[bone_mod].transform.localPosition = reference_model_preview.pose_bones[bone_mod].localPosition + avatar_components_preview.bonemods[bone_mod].translation;
                customizable_avatar_modified_bones_preview[bone_mod].transform.localRotation = avatar_components_preview.bonemods[bone_mod].rotation * reference_model_preview.pose_bones[bone_mod].localRotation;
                Vector3 new_scale = reference_model_preview.pose_bones[bone_mod].localScale;
                new_scale.x *= avatar_components_preview.bonemods[bone_mod].scale.x;
                new_scale.y *= avatar_components_preview.bonemods[bone_mod].scale.y;
                new_scale.z *= avatar_components_preview.bonemods[bone_mod].scale.z;
                customizable_avatar_modified_bones_preview[bone_mod].transform.localScale = new_scale;
            }
        }

        customizable_avatar_preview.animation_component["default"].time = 0.01f;
        customizable_avatar_preview.animation_component["default"].enabled = true;
        customizable_avatar_preview.animation_component["default"].weight = 1;
        customizable_avatar_preview.animation_component.Sample();
        customizable_avatar_preview.animation_component["default"].enabled = false;
    }

    public void saveAvatarButtonPressed()
    {
        avatar_components.commitChanges();
        PlayerFile.save();
    }

    public void resetChangesButtonPressed()
    {
        avatar_components.undoChanges();
    }


    public void renderTest()
    {
        GetComponent<CustomizeAvatarGroup>().renderTest();



    }

    public void changeAvatarEyeColor(int colorId) { avatar_components.changeAvatarEyeColor(colorId); }
    public void changeAvatarSkinColor(int colorId) { avatar_components.changeAvatarSkinColor(colorId); }
    public void changeAvatarLipsColor(int colorId) { avatar_components.changeAvatarLipsColor(colorId); }
    public void changeAvatarBrowColor(int colorId) { avatar_components.changeAvatarBrowColor(colorId); }
    public void changeAvatarHairColor(int colorId) { avatar_components.changeAvatarHairColor(colorId); }
    public void changeAvatarBrowThickness(float browThickness) { avatar_components.changeAvatarBrowThickness(browThickness); avatar_components_preview.changeAvatarBrowThickness(browThickness); }
    public void changeAvatarEyeCloseness(float eyeCloseness) { avatar_components.changeAvatarEyeCloseness(eyeCloseness); avatar_components_preview.changeAvatarEyeCloseness(eyeCloseness); }
    public void changeAvatarEyeSize(float eyeSize) { avatar_components.changeAvatarEyeSize(eyeSize); avatar_components_preview.changeAvatarEyeSize(eyeSize); }
    public void changeAvatarEyeY(float eyeY) { avatar_components.changeAvatarEyeY(eyeY); avatar_components_preview.changeAvatarEyeY(eyeY); }
    public void changeAvatarChinSize(float chinSize) { avatar_components.changeAvatarChinSize(chinSize); avatar_components_preview.changeAvatarChinSize(chinSize); }
    public void changeAvatarJawSize(float jawSize) { avatar_components.changeAvatarJawSize(jawSize); avatar_components_preview.changeAvatarJawSize(jawSize); }
    public void changeAvatarNoseBridgeHeight(float noseBridgeHeight) { avatar_components.changeAvatarNoseBridgeHeight(noseBridgeHeight); avatar_components_preview.changeAvatarNoseBridgeHeight(noseBridgeHeight); }
    public void changeAvatarNoseBridgeLength(float noseBridgeLength) { avatar_components.changeAvatarNoseBridgeLength(noseBridgeLength); avatar_components_preview.changeAvatarNoseBridgeLength(noseBridgeLength); }
    public void changeAvatarNoseBridgeWidth(float noseBridgeWidth) { avatar_components.changeAvatarNoseBridgeWidth(noseBridgeWidth); avatar_components_preview.changeAvatarNoseBridgeWidth(noseBridgeWidth); }
    public void changeAvatarNoseFatness(float noseFatness) { avatar_components.changeAvatarNoseFatness(noseFatness); avatar_components_preview.changeAvatarNoseFatness(noseFatness); }
    public void changeAvatarNoseWidth(float noseWidth) { avatar_components.changeAvatarNoseWidth(noseWidth); avatar_components_preview.changeAvatarNoseWidth(noseWidth); }
    public void changeAvatarNoseLength(float noseLength) { avatar_components.changeAvatarNoseLength(noseLength); avatar_components_preview.changeAvatarNoseLength(noseLength); }
    public void changeAvatarNoseHeight(float noseHeight) { avatar_components.changeAvatarNoseHeight(noseHeight); avatar_components_preview.changeAvatarNoseHeight(noseHeight); }
    public void changeAvatarNoseTwist(float noseTwist) { avatar_components.changeAvatarNoseTwist(noseTwist); avatar_components_preview.changeAvatarNoseTwist(noseTwist); }
    public void changeAvatarComponent(string component) { avatar_components.equipAvatarComponent(component); }

}
