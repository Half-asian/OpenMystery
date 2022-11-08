using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentHands : AvatarComponentWithModel
    {
        public ComponentHands(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            string model_id = Configs.config_avatar_components.AvatarComponents["hands"].patchModelId;
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
