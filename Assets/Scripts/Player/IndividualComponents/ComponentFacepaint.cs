using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace IndividualComponents
{
    class ComponentFacepaint : AvatarComponent
    {

        public ComponentFacepaint(AvatarComponents _avatar_components)
        {
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            PlayerFile.CustomizationCategory category = avatar_components.customization_categories["facePaint"];
            string outfit_id = Configs.config_avatar_components.AvatarComponents[category.component_id].outfitId;
            if (Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles != null)
            {
                foreach (var style in Configs.config_avatar_components.AvatarComponents[category.component_id].componentStyles)
                {
                    if (Predicate.parsePredicate(style.appropriatePredicate))
                        outfit_id = style.outfitId;
                }
            }

            SkinnedMeshRenderer smr = avatar_components.base_model.game_object.GetComponentInChildren<SkinnedMeshRenderer>();


            if (outfit_id == null)
            {
                //smr.materials[0].SetVector("u_housePrimary", Vector3.zero);
                //smr.materials[0].SetVector("u_houseSecondary", Vector3.zero);
                smr.materials[0].SetTexture("u_facePaintTexture", Resources.Load("whiteNoAlpha") as Texture2D);
                return null;
            }
            
            
            ConfigAvatarOutfitData._AvatarOutfitData.Material mat = Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].patchMaterials;

            
            for(int i = 0; i < mat.stringIds.Length; i++)
            {
                Debug.Log("replaced texture " + mat.stringIds[i]);
                smr.materials[0].SetTexture(mat.stringIds[i], TextureManager.loadTextureDDS(mat.stringValueKeys[i]));
            }

            for (int i = 0; i < mat.vec3Ids.Length; i++)
            {
                if (mat.vec3Ids[i] != "u_housePrimary" && mat.vec3Ids[i] != "u_houseSecondary")
                    smr.materials[0].SetVector(mat.vec3Ids[i], new Vector3(mat.vec3Values[i][0], mat.vec3Values[i][1], mat.vec3Values[i][2]));
            }

            return null;
        }
    }
}
