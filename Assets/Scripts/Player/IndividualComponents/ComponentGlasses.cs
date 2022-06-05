using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentGlasses : AvatarComponentWithModel
    {
        public ComponentGlasses(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["glasses"];
            string outfit_id = Configs.config_avatar_components.AvatarComponents[category.component_id].outfitId;
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
