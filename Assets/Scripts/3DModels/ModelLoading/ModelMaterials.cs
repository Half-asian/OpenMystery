using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;
using System;
namespace ModelLoading
{
    public class ModelMaterials
    {
		public static string lighting_phase = "CHARACTER";

		static string[] known_shaders_array = { "ubershader", "ocean_vfx", "skinshader", "neweyeshader", "hairshader", "houserobeshader", "houseclothshader", "clothshader", "SimpleColor", "simpleColor", "glow_vfx", "skyceilingshader_vfx", "fire02_vfx", "panningfalloff", "eyeballshader", "SimpleTexture", "lightrays_vfx", "shadowplane_vfx", "vertecolor_vfx", "avatarfaceshader", "avatarskinshader", "avatarhairshader", "warpfloor_vfx", "ghost_vfx", "ghostfade_vfx", "outfitshader", "watershader", "panningb_vfx", "eyeballshader", "quidditchshader", "AnimateUV", "eyeshader", "dustmotes_vfx", "houseubershader", "FalloffAnimated", "patronusoutfit_vfx", "crowd_vfx", "transition_vfx", "panningbfresnel_vfx", "void_vfx", "dualpan", "opal_vfx", "warp2_vfx", "scaleuv_vfx", "foammiddle_vfx", "foamedge_vfx" };

		public static List<string> known_shaders = new List<string>(known_shaders_array);

		static string[] real_shaders = { "ubershader", "ocean_vfx", "skinshader", "neweyeshader", "hairshader", "houserobeshader", "houseclothshader", "clothshader", "SimpleColor", "glow_vfx", "skyceilingshader_vfx", "fire02_vfx", "eyeshader", "lightrays_vfx", "SimpleTexture", "panningfalloff", "shadowplane_vfx", "vertecolor_vfx", "avatarfaceshader", "avatarskinshader", "avatarhairshader", "warpfloor_vfx", "ghost_vfx", "ghostfade_vfx", "outfitshader", "watershader", "panningb_vfx", "eyeballshader", "quidditchshader", "AnimateUV", "dustmotes_vfx", "FalloffAnimated", "patronusoutfit_vfx", "crowd_vfx", "transition_vfx", "panningbfresnel_vfx", "void_vfx", "dualpan", "opal_vfx", "warp2_vfx", "scaleuv_vfx", "foammiddle_vfx", "foamedge_vfx" };

		static string[] transparent_shaders_array = {"SimpleColor", "simpleColor", "ocean_vfx", "shadowplane_vfx", "vertecolor_vfx", "panningb_vfx", "fire02_vfx", "AnimateUV", "dustmotes_vfx", "FalloffAnimated", "SimpleColor", "panningbfresnel_vfx", "ghost_vfx", "ghostfade_vfx", "warp2_vfx", "scaleuv_vfx", "SimpleTexture", "foammiddle_vfx", "foamedge_vfx" };
		static List<string> transparent_shaders = new List<string>(transparent_shaders_array);

		static string[] transparent_no_depth_write_shaders_array = { "glow_vfx", "lightrays_vfx", "panningfalloff" };
		static List<string> transparent_no_depth_write_shaders = new List<string>(transparent_no_depth_write_shaders_array);

		static Material transparent_material;
		static Material transparent_no_depth_write_material;
		static Material opaque_material;
		static Material opaque_skin_material;
		public static Dictionary<string, Shader> shader_dict;// = new Dictionary<string, Shader>();
		//private static Dictionary<string, Texture2D> all_textures;

		private static void setLightData(Material mat)
        {
			if (Scene.current == null || Scene.current.Lighting == null || Scene.current.Lighting.layers == null)
			{
				return;
			}

			ConfigScene._Scene._Lighting.Layer lighting_layer = null;


			if (lighting_phase == "CHARACTER")
			{

				if (!Scene.current.Lighting.layers.ContainsKey(lighting_phase))
				{
					Debug.LogWarning("No light layer for current lighting phase " + lighting_phase);
					return;
				}
				lighting_layer = Scene.current.Lighting.layers["CHARACTER"];
			}
			else
			{
				foreach (var layer in Scene.current.Lighting.layers.Values)
				{
					if (layer.name != "CHARACTER")
						lighting_layer = layer;
				}
				if (lighting_layer == null)
				{
					Debug.LogWarning("Failed to find the env lighting layer ");
					return;
				}
			}

			int dirlight_count = 0;
			foreach (string lightname in lighting_layer.lights)
            {
				//if (!Scene.scene_lights.ContainsKey(lightname))
				//	continue;
				SceneLight sceneLight = Scene.scene_lights[lightname];
				if (sceneLight is AmbLight)
                {
					mat.SetColor("u_AmbientLightSourceColor", sceneLight.color);
				}
				else if (sceneLight is DirLight)
                {
					DirLight dirLight = (DirLight)sceneLight;
					if (dirlight_count == 0)
					{
						mat.SetColor("u_DirLightSourceColor1", dirLight.color);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection1", direction);
					}
					else if (dirlight_count == 1)
					{
						mat.SetColor("u_DirLightSourceColor2", dirLight.color);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection2", direction);
					}
					else if (dirlight_count == 2)
					{
						mat.SetColor("u_DirLightSourceColor3", dirLight.color);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection3", direction);
					}
					else if (dirlight_count == 3)
					{
						mat.SetColor("u_DirLightSourceColor4", dirLight.color);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection4", direction);
					}
					dirlight_count++;
				}
			}
		}

		public static void setTexSwitches(Material mat, string tex_name)
        {
			switch (tex_name)
            {
				case "u_diffuseMap":
					mat.SetFloat("USE_DIFFUSE_COLOR", 0);
					mat.SetFloat("HAS_DIFFUSE_TEXTURE", 1);
					break;
				case "u_lightmapMap":
					mat.SetFloat("USE_LIGHTMAP_COLOR", 0);
					mat.SetFloat("HAS_LIGHTMAP_TEXTURE", 1);
					break;
				case "u_secondDiffuseMap":
					mat.SetFloat("HAS_DIFFUSE2_TEXTURE", 1);
					break;
				case "u_thirdDiffuseMap":
					mat.SetFloat("HAS_DIFFUSE3_TEXTURE", 1);
					break;
				case "u_emblemTexture":
					mat.SetFloat("USE_HOUSE_EMBLEM", 1);
					break;
			}
		}
		public static void applyModelMaterial(Material mat, Config3DModel._Config3DModel.JsonData.Material material, bool force_transparent)
        {
			if (!known_shaders.Contains(material.shaderName))
			{
				Debug.LogWarning("No shader " + material.shaderName);
				mat.SetVector("u_diffuseColor", new Vector3(255 / 255.0f, 20 / 255.0f, 147 / 255.0f));
				return;
			}

			if (transparent_shaders.Contains(material.shaderName))
			{
				mat.CopyPropertiesFromMaterial(transparent_material);
			}

			else if (transparent_no_depth_write_shaders.Contains(material.shaderName))
			{
				mat.CopyPropertiesFromMaterial(transparent_no_depth_write_material);
			}

			if (material.shaderName == "simpleColor")
				material.shaderName = "SimpleColor";

			Material default_material;
			if ((force_transparent || material.transparent == 1) && material.shaderName == "ubershader")
			{
				default_material = Resources.Load<Material>("ShaderDefaults/" + material.shaderName + "_transparent");
			}
			else
			{
				default_material = Resources.Load<Material>("ShaderDefaults/" + material.shaderName);
			}

			if (default_material != null)
			{
				mat.shader = shader_dict[material.shaderName];
				mat.CopyPropertiesFromMaterial(default_material);
				setLightData(mat);
			}
			else
			{
				if (material.shaderName == "ubershader")
				{

					//bool cutout = false;
					/*if (material.intSettingIds != null)
					{
						for (int i = 0; i < material.intSettingIds.Length; i++)
						{
							if (material.intSettingIds[i] == "UseAsCutout_SWITCH" && material.intSettingValues[i] == 1)
							{
								cutout = true;
							}
						}
					}*/

					mat.SetFloat("u_opacityAmount", 1.0f);
					mat.SetFloat("u_flatness", 1.0f);
					mat.SetFloat("_Surface", 1.0f);
				}

				else if (material.shaderName == "simpleColor")
				{
					mat.shader = shader_dict["SimpleColor"];
					mat.SetFloat("alpha", 1.0f);
				}

				else if (material.shaderName == "crowd_vfx")
				{
					mat.CopyPropertiesFromMaterial(opaque_material);
					mat.shader = shader_dict[material.shaderName];
				}
				else if (material.shaderName == "skinshader" || material.shaderName == "clothshader" || material.shaderName == "neweyeshader")// || material.shaderName == "houserobeshader" || material.shaderName == "houseclothshader")
				{
					mat.CopyPropertiesFromMaterial(opaque_skin_material);
					mat.shader = shader_dict[material.shaderName];

					setLightData(mat);

				}
				else
				{
					if (shader_dict.ContainsKey(material.shaderName))
					{
						mat.shader = shader_dict[material.shaderName];
					}
					else
					{
						mat.shader = shader_dict["ubershader"];
					}
				}

			}
			if (material.shaderName == "avatarfaceshader")
			{
				mat.SetTexture("u_facePaintTexture", (Texture)Resources.Load("Shaders/transparent"));
				mat.SetTexture("u_mask", (Texture)Resources.Load("Shaders/transparent"));

			}

			if (material.stringValueKeys != null)
			{
				for (int i = 0; i < material.stringValueKeys.Length; i++)
				{
					if (material.stringValueKeys[i] != "room1")
						mat.SetTexture(material.stringIds[i], TextureManager.loadTextureDDS(material.stringValueKeys[i]));
					setTexSwitches(mat, material.stringIds[i]);
				}
			}
			if (material.floatIds != null)
			{
				for (int i = 0; i < material.floatIds.Length; i++)
				{
					mat.SetFloat(material.floatIds[i], material.floatValues[i]);
				}
			}
			if (material.vec3Ids != null)
			{
				for (int i = 0; i < material.vec3Ids.Length; i++)
				{
					mat.SetVector(material.vec3Ids[i], new Vector3(material.vec3Values[i][0], material.vec3Values[i][1], material.vec3Values[i][2]));
				}
			}
			if (material.vec4Ids != null)
			{

				for (int i = 0; i < material.vec4Ids.Length; i++)
				{
                    mat.SetVector(material.vec4Ids[i], new Vector4(material.vec4Values[i][0], material.vec4Values[i][1], material.vec4Values[i][2], material.vec4Values[i][3]));
                    if (material.vec4Ids[i] == "color")
					{
                        mat.SetVector("_color", new Vector4(material.vec4Values[i][0], material.vec4Values[i][1], material.vec4Values[i][2], material.vec4Values[i][3]));
                    }
				}
			}
			if (material.intSettingIds != null)
			{
				for (int i = 0; i < material.intSettingIds.Length; i++)
				{
					mat.SetFloat(material.intSettingIds[i], material.intSettingValues[i]);

				}
			}
			if (material.shaderName == "houserobeshader" || material.shaderName == "houseclothshader" || material.shaderName == "quidditchshader" || material.shaderName == "houseubershader")
			{
                string house = "";

                if (material.intSettingIds == null || !material.intSettingIds.Contains("TeamId"))
                    house = Player.local_avatar_house;
                else
                {
                    int index = Array.IndexOf(material.intSettingIds, "TeamId");
                    int value = material.intSettingValues[index];

                    switch (value)
                    {
                        case 0:
                            house = "gryffindor";
                            break;
                        case 1:
                            house = "hufflepuff";
                            break;
                        case 2:
                            house = "ravenclaw";
                            break;
                        case 3:
                            house = "slytherin";
                            break;
                    }
                }

                setHouseUniforms(mat, house);
                int skin_color_id = PlayerManager.current.customization_categories["faces"].int_parameters["skinColor"];
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f);
                mat.SetColor("u_skinColor", c);
                int brow_color_id = PlayerManager.current.customization_categories["brows"].int_parameters["browColor"];
                int[] brow_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["browColor"].colorConfigs[brow_color_id].codes;
                Color b = new Color(brow_color_codes[0] / 255.0f, brow_color_codes[1] / 255.0f, brow_color_codes[2] / 255.0f, 1.0f);
                mat.SetColor("u_browColor", b);
            }
		}

		public static void setHouseUniforms(Material mat, string house)
		{
            switch (house)
            {
                case "ravenclaw":
                    mat.SetInt("is_ravenclaw", 1);
                    mat.SetColor("u_robeColor", new Color(0.161f, 0.329f, 0.588f));
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.0f, 0.5f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.14f, 0.308f, 0.656f));
                    mat.SetColor("u_secondaryColor", new Color(0.656f, 0.656f, 0.656f));
                    break;
                case "gryffindor":
                    mat.SetInt("is_gryffindor", 1);
                    mat.SetColor("u_robeColor", new Color(0.761f, 0.549f, 0.102f));
                    mat.SetVector("u_emblemTexOffset", new Vector2(0f, 0f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.706f, 0.15f, 0.15f));
                    mat.SetColor("u_secondaryColor", new Color(0.722f, 0.635f, 0.166f));
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.0f, 0.0f));
                    break;
                case "slytherin":
                    mat.SetInt("is_slytherin", 1);
                    mat.SetColor("u_robeColor", new Color(0.056f, 0.433f, 0.191f));
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.5f, 0.0f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.056f, 0.433f, 0.191f));
                    mat.SetColor("u_secondaryColor", new Color(0.822f, 0.822f, 0.822f));
                    break;
                case "hufflepuff":
                    mat.SetInt("is_hufflepuff", 1);
                    mat.SetColor("u_robeColor", new Color(0.761f, 0.549f, 0.102f));
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.5f, 0.5f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.833f, 0.72f, 0.239f));
                    mat.SetColor("u_secondaryColor", new Color(0.101f, 0.096f, 0.075f));
                    break;
                default:
                    throw new Exception("Team chooser wasn't set.");
            }

			if (mat.shader.name == "avatarfaceshader")
			{
				switch (Player.local_avatar_house)
				{
					case "ravenclaw":
						mat.SetVector("u_housePrimary", new Vector3(0.14f, 0.308f, 0.656f));
						mat.SetVector("u_houseSecondary", new Vector3(0.656f, 0.656f, 0.656f));
						break;
					case "gryffindor":
						mat.SetVector("u_housePrimary", new Vector3(0.706f, 0.15f, 0.15f));
						mat.SetVector("u_houseSecondary", new Vector3(0.722f, 0.635f, 0.166f));
						break;
					case "slytherin":
						mat.SetVector("u_housePrimary", new Vector3(0.056f, 0.308f, 0.191f));
						mat.SetVector("u_houseSecondary", new Vector3(0.822f, 0.822f, 0.822f));
						break;
					case "hufflepuff":
						mat.SetVector("u_housePrimary", new Vector3(0.833f, 0.72f, 0.239f));
						mat.SetVector("u_houseSecondary", new Vector3(0.101f, 0.096f, 0.075f));
						break;
				}
			}
        }

		public static void Initialize()
        {
			transparent_material = (Material)Resources.Load("transparent_base", typeof(Material));
			opaque_material = (Material)Resources.Load("opaque_base", typeof(Material));
			opaque_skin_material = (Material)Resources.Load("opaque_skin_base", typeof(Material));
			transparent_no_depth_write_material = (Material)Resources.Load("transparent_no_depth_base", typeof(Material));

			shader_dict = new Dictionary<string, Shader>();

			foreach (string shader in real_shaders)
			{
				shader_dict[shader] = Shader.Find("Shader Graphs/" + shader);
				if (shader_dict[shader] == null)
				{
					GameObject crash = GameObject.Find("Canvas").transform.Find("Crash").gameObject;
					crash.SetActive(true);
					crash.transform.Find("Error").gameObject.GetComponent<Text>().text = "couldn't find " + shader;
				}
			}
		}


    }
}
