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
            AvatarComponents.onReapplyModifiers += setModifiers;
            avatar_components = _avatar_components;
            replaceComponent();
        }

        public override Model replaceComponent()
        {
            if (!avatar_components.customization_categories.ContainsKey("facePaint"))
                return null;
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

            List<Model> facialComponents = new List<Model>();
            facialComponents.Add(avatar_components.base_model);
            facialComponents.Add(avatar_components.components["nose"].getModel());
            facialComponents.Add(avatar_components.components["eyes"].getModel());
            facialComponents.Add(avatar_components.components["lips"].getModel());




            foreach (var model in facialComponents)
            {
                if (model == null || model.game_object == null)
                    continue;
                var smr = model.game_object.transform.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr.material.shader.name != "Shader Graphs/skin")
                    continue;

                if (outfit_id == null)
                {
                    //smr.materials[0].SetVector("u_housePrimary", Vector3.zero);
                    //smr.materials[0].SetVector("u_houseSecondary", Vector3.zero);
                    smr.material.SetTexture("u_facePaintTexture", Resources.Load("whiteNoAlpha") as Texture2D);
                    continue;
                }

                ConfigAvatarOutfitData._AvatarOutfitData.Material patch = Configs.config_avatar_outfit_data.AvatarOutfitData[outfit_id].patchMaterials;


                for (int i = 0; i < patch.stringIds.Length; i++)
                {
                    smr.materials[0].SetTexture(patch.stringIds[i], TextureManager.loadTexture(patch.stringValueKeys[i]));
                }

                for (int i = 0; i < patch.vec3Ids.Length; i++)
                {
                    //if (mat.vec3Ids[i] != "u_housePrimary" && mat.vec3Ids[i] != "u_houseSecondary")
                    smr.materials[0].SetColor(patch.vec3Ids[i], new Color(patch.vec3Values[i][0], patch.vec3Values[i][1], patch.vec3Values[i][2]).gamma);
                }
            }





            return null;
        }
    }
}
