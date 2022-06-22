using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentTop : AvatarComponentWithModel
    {
        public ComponentTop(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            if (!avatar_components.customization_categories.ContainsKey("tops"))
            {
                avatar_components.customization_categories["tops"] = new PlayerFile.CustomizationCategory();
                if (Player.local_avatar_gender == "female")
                    avatar_components.customization_categories["tops"].component_id = "o_Female_PrepOutfit_TOP";
                else
                    avatar_components.customization_categories["tops"].component_id = "o_Male_PrepOutfit_TOP";
            }
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["tops"];
            string outfit_id = Configs.config_avatar_components.AvatarComponents[category.component_id].outfitId;
            string model_id = Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].modelid;
            if (component_model != null)
                removeComponent();
            component_model = ModelManager.loadModel(model_id, avatar_components.base_model.pose_bones);
            component_model.game_object.transform.parent = avatar_components.base_model.game_object.transform;

            if (Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].armpatch != null)
            {
                avatar_components.replaceAvatarArms(Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].armpatch);
            }
            if (avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>().enabled == false)
                hideComponent();
            return component_model;
        }
    }
}
