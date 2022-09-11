using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentBottom : AvatarComponentWithModel
    {
        public ComponentBottom(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;

            replaceComponent();
        }

        public override Model replaceComponent()
        {
            if (!avatar_components.customization_categories.ContainsKey("bottoms"))
            {
                avatar_components.customization_categories["bottoms"] = new PlayerFile.CustomizationCategory();
                if (Player.local_avatar_gender == "female")
                    avatar_components.customization_categories["bottoms"].component_id = "femaleBottom1";
                else
                    avatar_components.customization_categories["bottoms"].component_id = "maleBottom1";
            }

            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["bottoms"];
            if (!Configs.config_avatar_components.AvatarComponents.ContainsKey(category.component_id))
            {
                return component_model;
            }
            string outfit_id = Configs.config_avatar_components.AvatarComponents[category.component_id].outfitId;
            if (Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles != null)
            {
                foreach (var style in Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles)
                {
                    if (Predicate.parsePredicate(style.appropriatePredicate))
                        outfit_id = style.outfitId;
                }
            }

            if (!Configs.config_avatar_outfit_data.AvatarOutfitData.ContainsKey(outfit_id))
            {
                return component_model;
            }
            string model_id = Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].modelid;
            if (component_model != null)
                GameObject.Destroy(component_model.game_object);
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            if (component_model == null)
                return component_model;

            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;

            if (Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].legpatch != null)
            {
                avatar_components.replaceAvatarLegs(Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].legpatch);
            }
            else
            {
                avatar_components.replaceAvatarLegs(null);
            }

            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            return component_model;
        }
    }
}
