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

		static string[] real_shaders = { "ubershader", "ocean_vfx", "skinshader", "neweyeshader", "hairshader", "houserobeshader", "houseclothshader", "houseubershader", "clothshader", "simplecolor", "glow_vfx", "skyceilingshader_vfx", "fire02_vfx", "eyeshader", "lightrays_vfx", "simpletexture", "shadowplane_vfx", "vertecolor_vfx", "avatarfaceshader", "avatarskinshader", "avatarhairshader", "warpfloor_vfx", "ghost_vfx", "ghostfade_vfx", "outfitshader", "watershader", "panningb_vfx", "eyeballshader", "quidditchshader", "animateuv", "dustmotes_vfx", "falloffanimated", "patronusoutfit_vfx", "crowd_vfx", "transition_vfx", "panningbfresnel_vfx", "void_vfx", "dualpan", "opal_vfx", "warp2_vfx", "scaleuv_vfx", "foammiddle_vfx", "foamedge_vfx", "warp_vfx"};

		static Shader error_shader;
		public static Dictionary<string, Shader> shader_dict;// = new Dictionary<string, Shader>();
															 //private static Dictionary<string, Texture2D> all_textures;

		static Dictionary<string, Material> material_dict = new Dictionary<string, Material>();


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
					mat.SetColor("u_AmbientLightSourceColor", sceneLight.color.gamma);
				}
				else if (sceneLight is DirLight)
                {
					DirLight dirLight = (DirLight)sceneLight;
					if (dirlight_count == 0)
					{
						mat.SetColor("u_DirLightSourceColor1", dirLight.color.gamma);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection1", direction);
					}
					else if (dirlight_count == 1)
					{
						mat.SetColor("u_DirLightSourceColor2", dirLight.color.gamma);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection2", direction);
					}
					else if (dirlight_count == 2)
					{
						mat.SetColor("u_DirLightSourceColor3", dirLight.color.gamma);
						Vector4 direction = new Vector4(dirLight.direction.x, dirLight.direction.y, dirLight.direction.z, 0.0f);
						mat.SetVector("u_DirLightSourceDirection3", direction);
					}
					else if (dirlight_count == 3)
					{
						mat.SetColor("u_DirLightSourceColor4", dirLight.color.gamma);
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
		public static Material applyModelMaterial(Config3DModel._Config3DModel.JsonData.Material material, bool force_transparent)
        {
			Material mat;


			string shader_name = material.shaderName.ToLower();


			Material default_material = null;
			string material_name = shader_name;


			if (force_transparent || material.transparent == 1)
			{
				material_name += "_transparent";
            }
			if (material_dict.ContainsKey(material_name))
			{
				default_material = material_dict[material_name];
			}
			else if (!shader_dict.ContainsKey(material_name))
			{
				Debug.LogError("shader not in dict " + shader_name);
			}
			else
			{
				Debug.LogError("Failed to find material with mat name " + material_name);
			}

			if (default_material != null && shader_dict.ContainsKey(shader_name))
			{
				if (shader_dict[shader_name] == null)
				{
					throw new Exception("shader : " + shader_name + " was null");
				}


				mat = new Material(shader_dict[shader_name]);
                mat.CopyPropertiesFromMaterial(default_material);
				setLightData(mat);
			}
			else
			{
				Debug.LogError("ERROR SHADER");
                return new Material(error_shader);
				
            }



			if (shader_name == "avatarfaceshader")
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
					mat.SetColor(material.vec3Ids[i], new Color(material.vec3Values[i][0], material.vec3Values[i][1], material.vec3Values[i][2]).gamma);
				}
			}
			if (material.vec4Ids != null)
			{

				for (int i = 0; i < material.vec4Ids.Length; i++)
				{
                    mat.SetColor(material.vec4Ids[i], new Color(material.vec4Values[i][0], material.vec4Values[i][1], material.vec4Values[i][2], material.vec4Values[i][3]).gamma);
                    if (material.vec4Ids[i] == "color")
					{
                        mat.SetColor("_color", new Color(material.vec4Values[i][0], material.vec4Values[i][1], material.vec4Values[i][2], material.vec4Values[i][3]).gamma);
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
			if (shader_name == "houserobeshader" || shader_name == "houseclothshader" || shader_name == "quidditchshader" || shader_name == "houseubershader")
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
								house = Player.local_avatar_house;
                            break;
                        case 1:
							switch (Player.local_avatar_house)
							{
								case "gryffindor":
								case "hufflepuff":
                                case "slytherin":
                                    house = "ravenclaw";
									break;
								case "ravenclaw":
									house = "slytherin";
									break;
							}
                            break;
                        case 2:
                            switch (Player.local_avatar_house)
                            {
                                case "gryffindor":
                                    house = "hufflepuff";
                                    break;
                                case "hufflepuff":
                                case "ravenclaw":
                                case "slytherin":
                                    house = "gryffindor";
                                    break;
                            }
                            break;
                        case 3:
                            switch (Player.local_avatar_house)
                            {
                                case "gryffindor":
                                case "hufflepuff":
                                    house = "slytherin";
                                    break;
                                case "ravenclaw":
                                case "slytherin":
                                    house = "hufflepuff";
                                    break;
                            }
                            break;
                    }
                }
                setHouseUniforms(mat, house);
                int skin_color_id = PlayerManager.current.customization_categories["faces"].int_parameters["skinColor"];
                int[] skin_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["skinColor"].colorConfigs[skin_color_id].codes;
                Color c = new Color(skin_color_codes[0] / 255.0f, skin_color_codes[1] / 255.0f, skin_color_codes[2] / 255.0f, 1.0f).gamma;
                mat.SetColor("u_skinColor", c);
                int brow_color_id = PlayerManager.current.customization_categories["brows"].int_parameters["browColor"];
                int[] brow_color_codes = Configs.config_avatar_attribute_colors.AvatarAttributeColors["browColor"].colorConfigs[brow_color_id].codes;
                Color b = new Color(brow_color_codes[0] / 255.0f, brow_color_codes[1] / 255.0f, brow_color_codes[2] / 255.0f, 1.0f).gamma;
                mat.SetColor("u_browColor", b);
            }
			return mat;
		}

		public static void setHouseUniforms(Material mat, string house)
		{
            switch (house)
            {
                case "ravenclaw":
                    mat.SetInt("is_ravenclaw", 1);
                    mat.SetColor("u_robeColor", new Color(0.161f, 0.329f, 0.588f).gamma);
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.0f, 0.5f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.14f, 0.308f, 0.656f).gamma);
                    mat.SetColor("u_secondaryColor", new Color(0.656f, 0.656f, 0.656f).gamma);
                    break;
                case "gryffindor":
                    mat.SetInt("is_gryffindor", 1);
                    mat.SetColor("u_robeColor", new Color(0.49f, 0.129f, 0.114f).gamma);
                    mat.SetVector("u_emblemTexOffset", new Vector2(0f, 0f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.706f, 0.15f, 0.15f).gamma);
                    mat.SetColor("u_secondaryColor", new Color(0.722f, 0.635f, 0.166f).gamma);
                    break;
                case "slytherin":
                    mat.SetInt("is_slytherin", 1);
                    mat.SetColor("u_robeColor", new Color(0.056f, 0.433f, 0.191f).gamma);
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.5f, 0.5f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.056f, 0.433f, 0.191f).gamma);
                    mat.SetColor("u_secondaryColor", new Color(0.822f, 0.822f, 0.822f).gamma);
                    break;
                case "hufflepuff":
                    mat.SetInt("is_hufflepuff", 1);
                    mat.SetColor("u_robeColor", new Color(0.761f, 0.549f, 0.102f).gamma);
                    mat.SetVector("u_emblemTexOffset", new Vector2(0.5f, 0.0f));
                    mat.SetFloat("u_houseSet", 1.0f);
                    mat.SetColor("u_primaryColor", new Color(0.833f, 0.72f, 0.239f).gamma);
                    mat.SetColor("u_secondaryColor", new Color(0.101f, 0.096f, 0.075f).gamma);
                    break;
                default:
                    throw new Exception("Team chooser wasn't set.");
            }

			if (mat.shader.name == "avatarfaceshader")
			{
				switch (Player.local_avatar_house)
				{
					case "ravenclaw":
						mat.SetColor("u_housePrimary", new Color(0.14f, 0.308f, 0.656f).gamma);
						mat.SetColor("u_houseSecondary", new Color(0.656f, 0.656f, 0.656f).gamma);
						break;
					case "gryffindor":
						mat.SetColor("u_housePrimary", new Color(0.706f, 0.15f, 0.15f).gamma);
						mat.SetColor("u_houseSecondary", new Color(0.722f, 0.635f, 0.166f).gamma);
						break;
					case "slytherin":
						mat.SetColor("u_housePrimary", new Color(0.056f, 0.308f, 0.191f).gamma);
						mat.SetColor("u_houseSecondary", new Color(0.822f, 0.822f, 0.822f).gamma);
						break;
					case "hufflepuff":
						mat.SetColor("u_housePrimary", new Color(0.833f, 0.72f, 0.239f).gamma);
						mat.SetColor("u_houseSecondary", new Color(0.101f, 0.096f, 0.075f).gamma);
						break;
				}
			}
        }

		public static void Initialize()
        {
			error_shader = Shader.Find("Shader Graphs/error_shader");
			shader_dict = new Dictionary<string, Shader>();

			var materials = Resources.LoadAll("ShaderDefaults", typeof(Material));

			foreach(var mat in materials)
			{
				material_dict[mat.name] = mat as Material;	
			}


            foreach (string shader in real_shaders)
			{
				Shader s = Shader.Find("Shader Graphs/" + shader);
				if (s is not null)
				{
					shader_dict[shader] = s;
                }
				else
				{
					Debug.LogError("Failed to load real shader " + shader);
				}
			}

			shader_dict["panningfalloff"] = Shader.Find("Shader Graphs/animateuvfalloff");


        }


    }
}
