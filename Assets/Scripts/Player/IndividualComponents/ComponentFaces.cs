using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentFaces : AvatarComponent
    {
        public ComponentFaces(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            setModifiers();
        }

        public override void setFloat(float f, string s) {
            avatar_components.customization_categories["faces"].float_parameters[s] = f;
            setModifiers();
        }


        static private readonly Vector2 chinSize_translate_z_range = new Vector2(-0.41f * 0.01f, 0.794f * 0.01f);
        static private readonly Vector2 chinSize_scale_range = new Vector2(0.9f, 1.5f);
        static private readonly Vector2 jawSize_scale_x_range = new Vector2(0.9f, 1.2f);

        public override void setModifiers()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["faces"];
            avatar_components.bonemods["chin_MOD_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["jawCorners_MOD_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

            if (category.float_parameters.ContainsKey("chinSize"))
            {
                Vector3 translation = new Vector3(0, 0, Mathf.Lerp(chinSize_translate_z_range.x, chinSize_translate_z_range.y, category.float_parameters["chinSize"]));
                float scale_float = Mathf.Lerp(chinSize_scale_range.x, chinSize_scale_range.y, category.float_parameters["chinSize"]);
                Vector3 scale = new Vector3(scale_float, scale_float, scale_float);
                avatar_components.bonemods["chin_MOD_Joint_bind"].translation = translation;
                avatar_components.bonemods["chin_MOD_Joint_bind"].scale = scale;
            }

            if (category.float_parameters.ContainsKey("jawSize"))
            {
                Vector3 scale = new Vector3(Mathf.Lerp(jawSize_scale_x_range.x, jawSize_scale_x_range.y, category.float_parameters["jawSize"]), 1, 1);
                avatar_components.bonemods["jawCorners_MOD_Joint_bind"].scale = scale;
            }

            if (category.int_parameters.ContainsKey("skinColor"))
            {
                int skin_color_id = category.int_parameters["skinColor"];
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f).gamma;
                SkinnedMeshRenderer smr = avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_skinColor", c);
            }

        }
    }
}
