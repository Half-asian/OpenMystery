using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentLips : AvatarComponentWithModel
    {

        public ComponentLips(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override void setFloat(float f, string s)
        {
            avatar_components.customization_categories["lips"].float_parameters[s] = f;
            setModifiers();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["lips"];

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
        public override void setModifiers()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["lips"];

            if (category.int_parameters.ContainsKey("naturalLips"))
            {
                int lips_id = category.int_parameters["naturalLips"];
                int[] lip_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["naturalLips"].colorConfigs[lips_id].codes;
                Color c = new Color(lip_color_codes[0] / 255.0f, lip_color_codes[1] / 255.0f, lip_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_lipColor", c);
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
