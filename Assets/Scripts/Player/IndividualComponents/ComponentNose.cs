using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentNose : AvatarComponentWithModel
    {
        public ComponentNose(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override void setFloat(float f, string s) {
            avatar_components.customization_categories["nose"].float_parameters[s] = f;
            setModifiers();
        }


        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["nose"];
            string model_id = Configs.config_avatar_components.AvatarComponents[category.component_id].patchModelId;
            if (component_model != null)
                removeComponent();
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            setModifiers();
            return component_model;
        }

        static private readonly Vector2 noseBridgeHeight_translate_y_range = new Vector2(-0.479f * 0.01f, 0.193f * 0.01f);
        static private readonly Vector2 noseBridgeLength_translate_z_range = new Vector2(-0.303f * 0.01f, 0.366f * 0.01f);
        static private readonly Vector2 noseBridgeWidth_scale_x_range = new Vector2(0.7f, 1.3f);
        static private readonly Vector2 noseFatness_scale_y_range = new Vector2(0.6f, 1.4f);
        static private readonly Vector2 noseWidth_scale_x_range = new Vector2(0.7f, 1.9f);
        static private readonly Vector2 noseLength_scale_z_range = new Vector2(0.6f, 2);
        static private readonly Vector2 noseLength_translate_z_range = new Vector2(-0.05f * 0.01f, 0.22f * 0.01f);
        static private readonly Vector2 noseHeight_translate_y_range = new Vector2(-0.3f * 0.01f, 0.3f * 0.01f);
        static private readonly Vector2 noseTwist_rotation_x_range = new Vector2(-0.610865f * 45, 0.610865f * 45);
        public override void setModifiers()
        {
            if (component_model is null)
                return;
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["nose"];
            avatar_components.bonemods["jt_noseBridge_MOD_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["jt_nose_MOD_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));


            if (category.float_parameters.ContainsKey("noseBridgeHeight"))
            {
                float translation_y = Mathf.Lerp(noseBridgeHeight_translate_y_range.x, noseBridgeHeight_translate_y_range.y, category.float_parameters["noseBridgeHeight"]);
                avatar_components.bonemods["jt_noseBridge_MOD_bind"].translation.x -= translation_y;    
            }
            if (category.float_parameters.ContainsKey("noseBridgeLength"))
            {
                float translation_z = Mathf.Lerp(noseBridgeLength_translate_z_range.x, noseBridgeLength_translate_z_range.y, category.float_parameters["noseBridgeLength"]);
                avatar_components.bonemods["jt_noseBridge_MOD_bind"].translation.z += translation_z;
            }
            if (category.float_parameters.ContainsKey("noseBridgeWidth"))
            {
                float scale_x = Mathf.Lerp(noseBridgeWidth_scale_x_range.x, noseBridgeWidth_scale_x_range.y, category.float_parameters["noseBridgeWidth"]);
                avatar_components.bonemods["jt_noseBridge_MOD_bind"].scale.x *= scale_x;
            }
            if (category.float_parameters.ContainsKey("noseFatness"))
            {
                float scale_y = Mathf.Lerp(noseFatness_scale_y_range.x, noseFatness_scale_y_range.y, category.float_parameters["noseFatness"]);
                avatar_components.bonemods["jt_nose_MOD_bind"].scale.y *= scale_y;
            }
            if (category.float_parameters.ContainsKey("noseWidth"))
            {
                float scale_x = Mathf.Lerp(noseWidth_scale_x_range.x, noseWidth_scale_x_range.y, category.float_parameters["noseWidth"]);
                avatar_components.bonemods["jt_nose_MOD_bind"].scale.x *= scale_x;
            }
            if (category.float_parameters.ContainsKey("noseLength"))
            {
                float scale_z = Mathf.Lerp(noseLength_scale_z_range.x, noseLength_scale_z_range.y, category.float_parameters["noseLength"]);
                float translate_z = Mathf.Lerp(noseLength_translate_z_range.x, noseLength_translate_z_range.y, category.float_parameters["noseLength"]);
                avatar_components.bonemods["jt_nose_MOD_bind"].scale.z *= scale_z;
                avatar_components.bonemods["jt_nose_MOD_bind"].translation.z += translate_z;
            }
            if (category.float_parameters.ContainsKey("noseHeight"))
            {
                float translate_y = Mathf.Lerp(noseHeight_translate_y_range.x, noseHeight_translate_y_range.y, category.float_parameters["noseHeight"]);
                avatar_components.bonemods["jt_nose_MOD_bind"].translation.x -= translate_y;
            }
            if (category.float_parameters.ContainsKey("noseTwist"))
            {
                float rotate_x = Mathf.Lerp(noseTwist_rotation_x_range.x, noseTwist_rotation_x_range.y, category.float_parameters["noseTwist"]);
                avatar_components.bonemods["jt_nose_MOD_bind"].rotation = Quaternion.Euler(new Vector3(0, rotate_x, 0));
            }

            if (avatar_components.customization_categories["faces"].int_parameters.ContainsKey("skinColor"))
            {
                int skin_color_id = avatar_components.customization_categories["faces"].int_parameters["skinColor"];
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_skinColor", c);
            }
        }
    }
}
