using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentEyes : AvatarComponentWithModel
    {
        public ComponentEyes(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override void setFloat(float f, string s) {
            avatar_components.customization_categories["eyes"].float_parameters[s] = f;
            setModifiers();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["eyes"];
            string model_id = Configs.config_avatar_components.AvatarComponents[category.component_id].patchModelId;
            if (component_model != null)
                GameObject.Destroy(component_model.game_object);
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            setModifiers();
            return component_model;
        }

        static private readonly Vector2 eyeCloseness_translate_z_range = new Vector2(-0.361f * 0.01f, 0.168f * 0.01f);
        static private readonly Vector2 eyeY_translate_y_range = new Vector2(-0.309f * 0.01f, 0.203f * 0.01f);
        static private readonly Vector2 eyeSize_scale_range = new Vector2(0.824f, 1.173f);

        public override void setModifiers()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["eyes"];
            avatar_components.bonemods["jt_L_eye_MOD_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["jt_R_eye_MOD_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));


            if (category.float_parameters.ContainsKey("eyeCloseness"))
            {
                Vector3 translationL = new Vector3(Mathf.Lerp(eyeCloseness_translate_z_range.x, eyeCloseness_translate_z_range.y, category.float_parameters["eyeCloseness"]), 0, 0);
                Vector3 translationR = new Vector3(Mathf.Lerp(eyeCloseness_translate_z_range.x, eyeCloseness_translate_z_range.y, 1 - category.float_parameters["eyeCloseness"]), 0, 0);
                avatar_components.bonemods["jt_L_eye_MOD_bind"].translation += translationL;
                avatar_components.bonemods["jt_R_eye_MOD_bind"].translation += translationR;
            }
            if (category.float_parameters.ContainsKey("eyeSize"))
            {
                float scale_float = Mathf.Lerp(eyeSize_scale_range.x, eyeSize_scale_range.y, category.float_parameters["eyeSize"]);
                Vector3 scale = new Vector3(scale_float, scale_float, scale_float);
                avatar_components.bonemods["jt_L_eye_MOD_bind"].scale = scale;
                avatar_components.bonemods["jt_R_eye_MOD_bind"].scale = scale;
            }

            if (category.float_parameters.ContainsKey("eyeY"))
            {
                Vector3 translationL = new Vector3(0, Mathf.Lerp(eyeY_translate_y_range.x, eyeY_translate_y_range.y, category.float_parameters["eyeY"]), 0);
                Vector3 translationR = new Vector3(0, Mathf.Lerp(eyeY_translate_y_range.x, eyeY_translate_y_range.y, category.float_parameters["eyeY"]), 0);
                avatar_components.bonemods["jt_L_eye_MOD_bind"].translation += translationL;
                avatar_components.bonemods["jt_R_eye_MOD_bind"].translation += translationR;
            }

            if (avatar_components.customization_categories["faces"].int_parameters.ContainsKey("skinColor"))
            {
                int skin_color_id = avatar_components.customization_categories["faces"].int_parameters["skinColor"];
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_skinColor", c);
            }

            if (category.int_parameters.ContainsKey("eyeColor"))
            {
                int eye_color_id = category.int_parameters["eyeColor"];

                int[] eye_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["eyeColor"].colorConfigs[eye_color_id].codes;

                Color c = new Color(eye_color_codes[0] / 255.0f, eye_color_codes[1] / 255.0f, eye_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = avatar_components.base_model.game_object.transform.Find("c_eyes_mesh").GetComponent<SkinnedMeshRenderer>();

                if (smr.material.HasColor("u_diffuseColor"))
                {
                    smr.material.SetColor("u_diffuseColor", c);
                }

            }
        }
    }
}
