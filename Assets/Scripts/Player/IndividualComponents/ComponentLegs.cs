using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentLegs : AvatarComponentWithModel
    {
        string component_id;

        public ComponentLegs(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            component_id = avatar_components.customization_categories["legs"].component_id;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            component_id = avatar_components.customization_categories["legs"].component_id;

            if (component_id == "LongPants" || component_id == null)
            {
                if (component_model != null)
                    removeComponent();
                return null;
            }


            ConfigAvatarPatchConfig._AvatarPatchConfig patch = Configs.config_avatar_patch_config.AvatarPatchConfig[component_id];
            string model_id;
            if (avatar_components.gender == "male")
                 model_id = patch.maleModelId;
            else
                model_id = patch.femaleModelId;

            if (component_model != null)
                removeComponent();
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;
            setModifiers();
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            return component_model;
        }

        public override void setModifiers()
        {
            if (component_model is null)
                return;
            if (avatar_components.customization_categories["faces"].int_parameters.ContainsKey("skinColor"))
            {
                int skin_color_id = avatar_components.customization_categories["faces"].int_parameters["skinColor"];
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();

                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].sets[0];
                Color u_skinColor = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f).gamma;
                smr.material.SetColor("u_skinColor", u_skinColor);

                int[] shade_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].sets[1];
                Color u_shadeColor = new Color(shade_color_codes[0] / 255.0f, shade_color_codes[1] / 255.0f, shade_color_codes[2] / 255.0f, 1.0f).gamma;
                smr.material.SetColor("u_shadeColor", u_shadeColor);

                int[] spec_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].sets[2];
                Color u_specColor = new Color(spec_color_codes[0] / 255.0f, spec_color_codes[1] / 255.0f, spec_color_codes[2] / 255.0f, 1.0f).gamma;
                smr.material.SetColor("u_specColor", u_specColor);
            }
        }
    }
}
