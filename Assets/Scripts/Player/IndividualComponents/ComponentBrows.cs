using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentBrows : AvatarComponent
    {
        static private readonly Vector2 browThickness_scale_y_range = new Vector2(0.3f, 1.725f);
        static private readonly Vector2 browThickness_translate_y_range = new Vector2(-0.242f * 0.01f, 0f );
        public ComponentBrows(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            setModifiers();
        }

        public override void setFloat(float f, string s)
        {
            avatar_components.customization_categories["brows"].float_parameters[s] = f;
            setModifiers();
        }

        public override void setModifiers()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["brows"];
            avatar_components.bonemods["leftInBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["leftMidBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["leftOutBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["rightInBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["rightMidBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));
            avatar_components.bonemods["rightOutBrow1_Joint_bind"] = new AnimationManager.BoneMod(Vector3.zero, Quaternion.identity, new Vector3(1, 1, 1));

            if (category.float_parameters.ContainsKey("browThickness"))
            {
                //Debug.Log("Set browThickness to " + category.float_parameters["browThickness"]);
                Vector3 translation = new Vector3(0, Mathf.Lerp(browThickness_translate_y_range.x, browThickness_translate_y_range.y, category.float_parameters["browThickness"]), 0);
                Vector3 scale = new Vector3(1, Mathf.Lerp(browThickness_scale_y_range.x, browThickness_scale_y_range.y, category.float_parameters["browThickness"]), 1);
                avatar_components.bonemods["leftInBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["leftInBrow1_Joint_bind"].scale = scale;
                avatar_components.bonemods["leftMidBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["leftMidBrow1_Joint_bind"].scale = scale;
                avatar_components.bonemods["leftOutBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["leftOutBrow1_Joint_bind"].scale = scale;
                avatar_components.bonemods["rightInBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["rightInBrow1_Joint_bind"].scale = scale;
                avatar_components.bonemods["rightMidBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["rightMidBrow1_Joint_bind"].scale = scale;
                avatar_components.bonemods["rightOutBrow1_Joint_bind"].translation = translation;
                avatar_components.bonemods["rightOutBrow1_Joint_bind"].scale = scale;
            }

            if (category.int_parameters.ContainsKey("browColor"))
            {
                int brow_color_id = category.int_parameters["browColor"];

                int[] brow_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["browColor"].colorConfigs[brow_color_id].codes;

                Color c = new Color(brow_color_codes[0] / 255.0f, brow_color_codes[1] / 255.0f, brow_color_codes[2] / 255.0f, 1.0f);
                SkinnedMeshRenderer smr = avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();
                smr.material.SetColor("u_browColor", c);
            }
        }
    }
}
