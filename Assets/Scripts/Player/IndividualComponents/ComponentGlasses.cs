using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ModelLoading;

namespace IndividualComponents
{
    class ComponentGlasses : AvatarComponentWithModel
    {
        public ComponentGlasses(AvatarComponents _avatar_components)
        {
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["glasses"];
            string outfit_id = Configs.config_avatar_components.AvatarComponents[category.component_id].outfitId;
            if (Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles != null)
            {
                foreach (var style in Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles)
                {
                    if (Predicate.parsePredicate(style.appropriatePredicate))
                        outfit_id = style.outfitId;
                }
            }
            string model_id = Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].modelid;
            if (component_model != null)
                removeComponent();
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            return component_model;
        }
    }
}
