using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentLegs : AvatarComponentWithModel
    {
        string component_id;

        public ComponentLegs(AvatarComponents _avatar_components)
        {
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
                    GameObject.Destroy(component_model.game_object);
                return null;
            }


            ConfigAvatarPatchConfig._AvatarPatchConfig patch = Configs.config_avatar_patch_config.AvatarPatchConfig[component_id];
            string model_id;
            if (avatar_components.gender == "male")
                 model_id = patch.maleModelId;
            else
                model_id = patch.femaleModelId;

            if (component_model != null)
                GameObject.Destroy(component_model.game_object); ;
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;
            setModifiers();
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            return component_model;
        }

        public override void setModifiers()
        {
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
