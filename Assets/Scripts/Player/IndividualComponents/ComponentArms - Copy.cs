using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentArms : AvatarComponentWithModel
    {
        string component_id;

        public ComponentArms(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            component_id = avatar_components.customization_categories["arms"].component_id;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            component_id = avatar_components.customization_categories["arms"].component_id;
            if (component_id == "LongSleeve")
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
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f).gamma;
                SkinnedMeshRenderer smr = component_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_skinColor", c);
            }
        }
    }
}
