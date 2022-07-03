using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

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
        _camera_preview.SetActive(false);
        _camera_preview_depth.SetActive(false);

        customize_avatar_preview_images = GetComponent<CustomizeAvatarPreviewImages>();

        createCustomizableAvatar();

        createCustomizablePreviewAvatar();
    }

    void createCustomizableAvatar()
    {
        avatar_components = new AvatarComponents(GlobalEngineVariables.player_folder + "\\avatar.json");

        //_camera.transform.position = new Vector3(0, 0.773000002f, 0.338999987f);
        //_camera.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));

        customizable_avatar = CustomizableAvatarSpawner.spawnCustomizableAvatar(avatar_components.actor_id, "CustomizableAvatar");
        ConfigHPActorInfo._HPActorInfo reference_character = Configs.config_hp_actor_info.HPActorInfo[PlayerManager.current.character_id]; //We use this as a base to apply transforms to

        customizable_avatar.actor_animation.replaceCharacterIdle("c_Stu_DialogueIdle01");

        reference_model = ModelManager.loadModel(reference_character.modelId);

        reference_model.game_object.transform.position = new Vector3(-0.24f, 0.55f, 1.32f);
        reference_model.game_object.transform.eulerAngles = new Vector3(0, 160f, 0);
        customizable_avatar_modified_bones = new Dictionary<string, Transform>();

        avatar_components.setCharacterManager(customizable_avatar);

        avatar_components.base_model = customizable_avatar.model;

        avatar_components.spawnComponents();


        foreach (string bonemod in BonemodMap.bonemod_map.Keys)
        {
            customizable_avatar_modified_bones[bonemod] = avatar_components.base_model.pose_bones[bonemod];
        }
    }

    void createCustomizablePreviewAvatar()
    {
        avatar_components_preview = new AvatarComponents(GlobalEngineVariables.player_folder + "\\avatar.json");

        _camera_preview.transform.position = new Vector3(0, 0.773000002f - 10f, 0.338999987f);
        _camera_preview.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        _camera_preview_depth.transform.position = new Vector3(0, 0.773000002f - 10f, 0.338999987f);
        _camera_preview_depth.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        customizable_avatar_preview = CustomizableAvatarSpawner.spawnCustomizableAvatar(avatar_components_preview.actor_id, "CustomizableAvatar");
        ConfigHPActorInfo._HPActorInfo reference_character = Configs.config_hp_actor_info.HPActorInfo[PlayerManager.current.character_id]; //We use this as a base to apply transforms to
        customizable_avatar_preview.actor_animation.replaceCharacterIdle("c_Stu_DialogueIdle01");

        reference_model_preview = ModelManager.loadModel(reference_character.modelId);
        customizable_avatar_preview.animation_component["loop"].time = 0.01f;
        customizable_avatar_preview.animation_component["loop"].enabled = true;
        customizable_avatar_preview.animation_component["loop"].weight = 1;
        customizable_avatar_preview.animation_component.Sample();
        customizable_avatar_preview.animation_component["loop"].enabled = false;

        reference_model_preview.game_object.transform.position = new Vector3(0, -10, 0);
        reference_model_preview.game_object.transform.rotation = Quaternion.identity;
        customizable_avatar_modified_bones_preview = new Dictionary<string, Transform>();

        avatar_components_preview.setCharacterManager(customizable_avatar_preview);

        avatar_components_preview.base_model = customizable_avatar_preview.model;

        avatar_components_preview.spawnComponents();

        foreach (string bonemod in BonemodMap.bonemod_map.Keys)
        {
            customizable_avatar_modified_bones_preview[bonemod] = avatar_components_preview.base_model.pose_bones[bonemod];
        }
    }

    private void Update()
    {
        //Resets bones to reference.
        foreach (string bone in reference_model.pose_bones.Keys)
        {
            avatar_components.base_model.pose_bones[bone].transform.position = reference_model.pose_bones[bone].position;
            avatar_components.base_model.pose_bones[bone].transform.rotation = reference_model.pose_bones[bone].rotation;
            avatar_components.base_model.pose_bones[bone].transform.localScale = reference_model.pose_bones[bone].localScale;
        }

        if (avatar_components != null)
        {
            foreach (string bone_mod in avatar_components.bonemods.Keys)
            {
                customizable_avatar_modified_bones[bone_mod].transform.position = reference_model.pose_bones[bone_mod].position + avatar_components.bonemods[bone_mod].translation;
                customizable_avatar_modified_bones[bone_mod].transform.rotation = reference_model.pose_bones[bone_mod].rotation * avatar_components.bonemods[bone_mod].rotation;
                Vector3 new_scale = reference_model.pose_bones[bone_mod].localScale;
                new_scale.x *= avatar_components.bonemods[bone_mod].scale.x;
                new_scale.y *= avatar_components.bonemods[bone_mod].scale.y;
                new_scale.z *= avatar_components.bonemods[bone_mod].scale.z;
                customizable_avatar_modified_bones[bone_mod].transform.localScale = new_scale;
            }
        }




        foreach (string bone in reference_model_preview.pose_bones.Keys)
        {
            avatar_components_preview.base_model.pose_bones[bone].transform.position = reference_model_preview.pose_bones[bone].position;
            avatar_components_preview.base_model.pose_bones[bone].transform.rotation = reference_model_preview.pose_bones[bone].rotation;
            avatar_components_preview.base_model.pose_bones[bone].transform.localScale = reference_model_preview.pose_bones[bone].localScale;
        }

        if (avatar_components_preview != null)
        {
            foreach (string bone_mod in avatar_components_preview.bonemods.Keys)
            {
                customizable_avatar_modified_bones_preview[bone_mod].transform.position = reference_model_preview.pose_bones[bone_mod].position + avatar_components_preview.bonemods[bone_mod].translation;
                customizable_avatar_modified_bones_preview[bone_mod].transform.rotation = reference_model_preview.pose_bones[bone_mod].rotation * avatar_components_preview.bonemods[bone_mod].rotation;
                Vector3 new_scale = reference_model_preview.pose_bones[bone_mod].localScale;
                new_scale.x *= avatar_components_preview.bonemods[bone_mod].scale.x;
                new_scale.y *= avatar_components_preview.bonemods[bone_mod].scale.y;
                new_scale.z *= avatar_components_preview.bonemods[bone_mod].scale.z;
                customizable_avatar_modified_bones_preview[bone_mod].transform.localScale = new_scale;
            }
        }

        customizable_avatar_preview.animation_component["loop"].time = 0.01f;
        customizable_avatar_preview.animation_component["loop"].enabled = true;
        customizable_avatar_preview.animation_component["loop"].weight = 1;
        customizable_avatar_preview.animation_component.Sample();
        customizable_avatar_preview.animation_component["loop"].enabled = false;
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
