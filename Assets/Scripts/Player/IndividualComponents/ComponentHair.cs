using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentHair : AvatarComponentWithModel
    {

        public ComponentHair(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override void setInt(int i, string s)
        {
            avatar_components.customization_categories["hair"].int_parameters[s] = i;
            setModifiers();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["hair"];
            string model_id = Configs.config_avatar_components.AvatarComponents[category.component_id].patchModelId;
            if (string.IsNullOrEmpty(model_id))
                return null;
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
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["hair"];

            if (category.int_parameters.ContainsKey("hairColor"))
            {
                int hair_color_id = category.int_parameters["hairColor"];

                int[] hair_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["hairColor"].colorConfigs[hair_color_id].codes;

                Color c = new Color(hair_color_codes[0] / 255.0f, hair_color_codes[1] / 255.0f, hair_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_hairColor", c);
            }
        }
    }
}
