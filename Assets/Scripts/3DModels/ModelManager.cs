
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class ModelManager
{
	//public static Dictionary<string, ModelManager.C3T> loaded_models = new Dictionary<string, ModelManager.C3T>();

	static string[] known_shaders_array = { "ubershader", "ocean_vfx", "skinshader", "neweyeshader", "hairshader", "houserobeshader", "houseclothshader", "clothshader", "SimpleColor", "simpleColor", "glow_vfx", "skyceilingshader_vfx", "fire02_vfx", "panningfalloff", "eyeballshader", "SimpleTexture", "lightrays_vfx", "shadowplane_vfx", "vertecolor_vfx", "avatarfaceshader", "avatarskinshader", "avatarhairshader", "warpfloor_vfx", "ghost_vfx", "ghostfade_vfx", "outfitshader", "watershader", "panningb_vfx", "eyeballshader", "quidditchshader", "AnimateUV", "eyeshader", "dustmotes_vfx", "houseubershader", "FalloffAnimated", "patronusoutfit_vfx", "crowd_vfx", "transition_vfx", "panningbfresnel_vfx", "void_vfx", "dualpan", "opal_vfx"};

	static List<string> known_shaders = new List<string>(known_shaders_array);

	static string[] real_shaders = { "ubershader", "ubershader_transparent", "ocean_vfx", "skinshader", "neweyeshader", "hairshader", "houserobeshader", "houseclothshader", "clothshader", "SimpleColor", "glow_vfx", "skyceilingshader_vfx", "fire02_vfx", "eyeshader", "lightrays_vfx", "SimpleTexture", "panningfalloff", "shadowplane_vfx", "vertecolor_vfx", "avatarfaceshader", "avatarskinshader", "avatarhairshader", "warpfloor_vfx", "ghost_vfx", "ghostfade_vfx", "outfitshader", "watershader", "panningb_vfx", "eyeballshader", "quidditchshader", "AnimateUV", "dustmotes_vfx", "FalloffAnimated", "patronusoutfit_vfx", "crowd_vfx", "transition_vfx", "panningbfresnel_vfx", "void_vfx", "dualpan", "opal_vfx"};

	static string[] transparent_shaders_array = { "ocean_vfx", "lightrays_vfx", "shadowplane_vfx", "vertecolor_vfx", "panningb_vfx", "panningfalloff", "fire02_vfx", "ubershader_transparent", "AnimateUV", "dustmotes_vfx", "FalloffAnimated", "SimpleColor", "panningbfresnel_vfx", "ghost_vfx", "ghostfade_vfx" };
	static List<string> transparent_shaders = new List<string>(transparent_shaders_array);

	static string[] transparent_no_depth_write_shaders_array = { "glow_vfx", "ghost_vfx" };
	static List<string> transparent_no_depth_write_shaders = new List<string>(transparent_no_depth_write_shaders_array);

	static Material transparent_material;
	static Material transparent_no_depth_write_material;
	static Material opaque_material;
	static Dictionary<string, Shader> shader_dict;// = new Dictionary<string, Shader>();

	static string patch_text;

	public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 translate;
		translate.x = matrix.m03;
		translate.y = matrix.m13;
		translate.z = matrix.m23;
		return translate;
	}
	public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 forward;
		forward.x = matrix.m02;
		forward.y = matrix.m12;
		forward.z = matrix.m22;

		Vector3 upwards;
		upwards.x = matrix.m01;
		upwards.y = matrix.m11;
		upwards.z = matrix.m21;

		return Quaternion.LookRotation(forward, upwards);
	}
	public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
	{
		Vector3 scale;
		scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
		scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
		scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
		return scale;
	}   


	public static void processPoseSkeletonChild(CocosModel.Node node, Transform parent, ref List<Transform> bones, ref List<Matrix4x4> bindPoses)
    {

		bones.Add(new GameObject(node.id).transform);
		int bone_index = bones.Count - 1;

		Matrix4x4 child_transform_matrix = new Matrix4x4(new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]), new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]), new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]), new Vector4(node.transform[12], node.transform[13], node.transform[14], node.transform[15]));

		bones[bone_index].parent = parent;
		bones[bone_index].localRotation = ExtractRotationFromMatrix(ref child_transform_matrix);
		bones[bone_index].localPosition = ExtractTranslationFromMatrix(ref child_transform_matrix);
		bones[bone_index].localScale = ExtractScaleFromMatrix(ref child_transform_matrix);


		bones[bone_index].localPosition = new Vector3(bones[bone_index].localPosition.x * -1, bones[bone_index].localPosition.y, bones[bone_index].localPosition.z);
		bones[bone_index].localRotation = Quaternion.Euler(new Vector3(bones[bone_index].localRotation.eulerAngles.x, bones[bone_index].localRotation.eulerAngles.y * -1, bones[bone_index].localRotation.eulerAngles.z * -1));

		bindPoses.Add(bones[bones.Count - 1].worldToLocalMatrix);


		if (node.children != null)
		{
			foreach (CocosModel.Node child in node.children)
			{
				processPoseSkeletonChild(child, bones[bone_index], ref bones, ref bindPoses);
			}
		}
		return;
    }


	public static string getPatchedName(string name){

		if (name == "o_Female_ForestFormal_FULL1")
		{
			name = "o_Female_ForestFormal_FULL";

		}
		if (patch_text.Contains("patch:" + name))
		{

			int patch_index = patch_text.IndexOf("patch:" + name) + ("patch:" + name).Length + 1;
			Debug.Log("patch_index " + patch_index);

			Debug.Log("semicolon_index " + patch_text.IndexOf(';', patch_index));
			name = patch_text.Substring(patch_index, patch_text.IndexOf(';', patch_index) - patch_index);
			Debug.LogWarning("patched " + name);
		}
		return name;
	}

	public static void applyBoneTransforms(Transform base_transform, Dictionary<string, BoneTransform> bone_transform_modifiers = null)
    {
		Debug.Log("applyBoneTransforms");
		SkinnedMeshRenderer[] smrs = base_transform.GetComponentsInChildren<SkinnedMeshRenderer>();

		foreach (SkinnedMeshRenderer smr in smrs)
		{
			Matrix4x4[] matrices = smr.sharedMesh.bindposes;
			for (int i = 0; i < smr.bones.Length; i++)
            {
				if (bone_transform_modifiers.ContainsKey(smr.bones[i].name))
				{
					Debug.Log("Applying");
					bone_transform_modifiers[smr.bones[i].name].apply(ref smr.bones[i]);
				}
			}
			smr.sharedMesh.bindposes = matrices;
		}
	}

	public static Model loadModel(string name, Dictionary<string, Transform> parent_bones = null) {

		GameObject go = new GameObject();
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;


		if (string.IsNullOrEmpty(name))
        {
			throw new System.Exception("Null or empty model load");
        }

		Model return_model = new Model();
		Config3DModel c3m = Configs.config_3dmodel;

		name = getPatchedName(name);


		if (name == "")
		{
			Debug.LogError("c3b no name");
			return null;
		}
		if (name == "o_Female_ForestFormal_FULL1_skin")
		{
			name = "o_Female_ForestFormal_FULL_skin";
		}
		if (!c3m.ModelConfig.ContainsKey(name))
		{
			Debug.LogError("Not c3b config entry for " + name);
			return null;
		}


		CocosModel model = C3B.loadC3B(c3m.ModelConfig[name].jsonData[0].mesh, GlobalEngineVariables.models_folder);

		if (model == null)
		{
			Debug.LogError("Model loaded null " + name);
			return null;
		}

		#region mesh data

		go.name = name;

		foreach (CocosModel.Mesh C3T_mesh in model.meshes)
		{

			int stride = 0;
			foreach (CocosModel.Mesh.Attribute attrib in C3T_mesh.attributes)
			{
				switch (attrib.attribute)
				{
					case "VERTEX_ATTRIB_POSITION":
						C3T_mesh.VERTEX_ATTRIB_POSITION = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_NORMAL":
						C3T_mesh.VERTEX_ATTRIB_NORMAL = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_SOFT_NORMAL":
						C3T_mesh.VERTEX_ATTRIB_SOFT_NORMAL = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD":
						C3T_mesh.VERTEX_ATTRIB_TEX_COORD = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD1":
						C3T_mesh.VERTEX_ATTRIB_TEX_COORD1 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD2":
						C3T_mesh.VERTEX_ATTRIB_TEX_COORD2 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD3":
						C3T_mesh.VERTEX_ATTRIB_TEX_COORD3 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_BLEND_WEIGHT":
						C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT = new List<Vector4>();
						break;
					case "VERTEX_ATTRIB_BLEND_INDEX":
						C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX = new List<Vector4>();
						break;
					case "VERTEX_ATTRIB_COLOR":
						C3T_mesh.VERTEX_ATTRIB_COLOR = new List<Color>();
						break;
					default:
						//Debug.Log("ERROR, UNKNOWN VERTEX ATTRIB " + attrib.attribute);
						break;
				}
				stride += attrib.size;
			}

			for (int v = 0; v < C3T_mesh.vertices.Length; v += stride)
			{
				int offset = 0;
				foreach (CocosModel.Mesh.Attribute attrib in C3T_mesh.attributes)
				{
					switch (attrib.attribute)
					{
						case "VERTEX_ATTRIB_POSITION":
							C3T_mesh.VERTEX_ATTRIB_POSITION.Add(new Vector3(C3T_mesh.vertices[v + offset] * -1, C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_NORMAL":
							C3T_mesh.VERTEX_ATTRIB_NORMAL.Add(new Vector3(-C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_SOFT_NORMAL":
							C3T_mesh.VERTEX_ATTRIB_SOFT_NORMAL.Add(new Vector3(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD":
							C3T_mesh.VERTEX_ATTRIB_TEX_COORD.Add(new Vector2(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD1":
							C3T_mesh.VERTEX_ATTRIB_TEX_COORD1.Add(new Vector2(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD2":
							C3T_mesh.VERTEX_ATTRIB_TEX_COORD2.Add(new Vector2(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD3":
							C3T_mesh.VERTEX_ATTRIB_TEX_COORD3.Add(new Vector2(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_BLEND_WEIGHT":
							C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT.Add(new Vector4(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2], C3T_mesh.vertices[v + offset + 3]));
							break;
						case "VERTEX_ATTRIB_BLEND_INDEX":
							C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX.Add(new Vector4(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2], C3T_mesh.vertices[v + offset + 3]));
							break;
						case "VERTEX_ATTRIB_COLOR":
							C3T_mesh.VERTEX_ATTRIB_COLOR.Add(new Color(C3T_mesh.vertices[v + offset], C3T_mesh.vertices[v + offset + 1], C3T_mesh.vertices[v + offset + 2], C3T_mesh.vertices[v + offset + 3]));
							break;
						default:
							//Debug.Log("ERROR, UNKNOWN VERTEX ATTRIB " + attrib.attribute);
							break;
					}
					offset += attrib.size;
				}
			}
		}
        #endregion
        #region skeleton nodes

		List<Transform> pose_bones = new List<Transform>();
		Dictionary<string, Transform> pose_bone_dict = new Dictionary<string, Transform>();
		List<Matrix4x4> bindPoses = new List<Matrix4x4>();
		Dictionary<string, Matrix4x4> bindposes_dict = new Dictionary<string, Matrix4x4>();


		foreach (CocosModel.Node node in model.nodes)
        {
			if (node.skeleton == true)
			{
				GameObject amt = new GameObject("Armature");
				amt.transform.parent = go.transform;

				//Matrix4x4 jt_all_bind_matrix = new Matrix4x4(new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]), new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]), new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]), new Vector4(node.transform[12], node.transform[13], node.transform[14], node.transform[15]));
				//jt_all_bind is actually useless! Avoid!
				//Preferably, we would apply jt_all_bind after loading model and in gameworld

				Matrix4x4 transform_matrix = Matrix4x4.identity;


				pose_bones.Add(new GameObject(node.id).transform);
				pose_bones[0].parent = amt.transform;
				pose_bones[0].localRotation = ExtractRotationFromMatrix(ref transform_matrix);
				pose_bones[0].localPosition = ExtractTranslationFromMatrix(ref transform_matrix);
				pose_bones[0].localScale = ExtractScaleFromMatrix(ref transform_matrix);



				bindPoses.Add(pose_bones[0].worldToLocalMatrix);
				foreach (CocosModel.Node child in node.children)
				{
					processPoseSkeletonChild(child, pose_bones[0].transform, ref pose_bones, ref bindPoses);
				}
			}
		}

		for(int t = 0; t < pose_bones.Count; t++)
        {
			pose_bone_dict[pose_bones[t].gameObject.name] = pose_bones[t];
			bindposes_dict[pose_bones[t].gameObject.name] = bindPoses[t];
        }

		#endregion

		Dictionary<string, Texture2D> all_textures = new Dictionary<string, Texture2D>();

		foreach (string tex in c3m.ModelConfig[name].jsonData[0].neededTextureKeys)
		{
			if (TextureManager.loaded_textures.ContainsKey(tex))
			{
				all_textures[tex] = TextureManager.loaded_textures[tex];
			}
			else
			{
				all_textures[tex] = TextureManager.loadTextureDDS(tex);
				TextureManager.loaded_textures[tex] = all_textures[tex];
			}
		}

		#region mesh nodes

		foreach (CocosModel.Node node in model.nodes)
		{
			if (node.skeleton == false)
			{
				Matrix4x4 node_transform_matrix = new Matrix4x4(new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]), new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]), new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]), new Vector4(-node.transform[12], node.transform[13], node.transform[14], node.transform[15]));


				if (node.parts != null)
                {
					foreach (CocosModel.Node.Part node_part in node.parts)
					{
						GameObject node_go = new GameObject();
						node_go.transform.parent = go.transform;
						node_go.transform.position = ExtractTranslationFromMatrix(ref node_transform_matrix);

						Quaternion extracted_rotation = ExtractRotationFromMatrix(ref node_transform_matrix);
						node_go.transform.rotation = Quaternion.Euler(new Vector3(extracted_rotation.eulerAngles.x, -extracted_rotation.eulerAngles.y, -extracted_rotation.eulerAngles.z));
						node_go.transform.localScale = ExtractScaleFromMatrix(ref node_transform_matrix);
						node_go.name = node.id;


						CocosModel.Mesh C3T_mesh = null;
						CocosModel.Mesh.Part C3T_part = null;


						foreach (CocosModel.Mesh _mesh in model.meshes)
						{
							foreach (CocosModel.Mesh.Part C3T_mesh_part in _mesh.parts)
							{
								if (node_part.meshpartid == C3T_mesh_part.id)
								{
									C3T_mesh = _mesh;
									C3T_part = C3T_mesh_part;
								}
							}
						}

						if (C3T_part == null || C3T_mesh == null)
						{
							Debug.Log("Failed to find mesh part: " + node_part.meshpartid);
						}
						else
						{
							Mesh mesh = new Mesh();

							if (C3T_mesh.VERTEX_ATTRIB_POSITION != null)
								mesh.vertices = C3T_mesh.VERTEX_ATTRIB_POSITION.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_TEX_COORD != null)
								mesh.uv = C3T_mesh.VERTEX_ATTRIB_TEX_COORD.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_TEX_COORD1 != null)
								mesh.uv2 = C3T_mesh.VERTEX_ATTRIB_TEX_COORD1.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_TEX_COORD2 != null)
								mesh.uv3 = C3T_mesh.VERTEX_ATTRIB_TEX_COORD2.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_TEX_COORD3 != null)
								mesh.uv4 = C3T_mesh.VERTEX_ATTRIB_TEX_COORD3.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_COLOR != null)
								mesh.colors = C3T_mesh.VERTEX_ATTRIB_COLOR.ToArray();
							if (C3T_mesh.VERTEX_ATTRIB_NORMAL != null)
								mesh.normals = C3T_mesh.VERTEX_ATTRIB_NORMAL.ToArray();

							bool overflowing_indices = false;

							for (int i = 0; i < C3T_part.indices.Length; i++)
                            {
								if (C3T_part.indices[i] < 0)
                                {
									overflowing_indices = true;
									Debug.LogError(C3T_part.indices[i]);
									C3T_part.indices[i] = 0;// C3T_mesh.vertices.Length - 1;
                                }
                            }
							if (overflowing_indices == true)
								Debug.LogError("Overflowing indices on model: " + name + " part: " + C3T_part.id);

							if (C3T_part.indices != null)
							{
								mesh.triangles = C3T_part.indices;
							}

							BoneWeight[] weights = new BoneWeight[mesh.vertexCount];

							if (C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX != null)
							{
								for (int i = 0; i < mesh.vertexCount; i++)
								{
									weights[i].boneIndex0 = (int)C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX[i][0];
									weights[i].boneIndex1 = (int)C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX[i][1];
									weights[i].boneIndex2 = (int)C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX[i][2];
									weights[i].boneIndex3 = (int)C3T_mesh.VERTEX_ATTRIB_BLEND_INDEX[i][3];
									weights[i].weight0 = C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT[i][0];
									weights[i].weight1 = C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT[i][1];
									weights[i].weight2 = C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT[i][2];
									weights[i].weight3 = C3T_mesh.VERTEX_ATTRIB_BLEND_WEIGHT[i][3];
								}
								mesh.boneWeights = weights;
							}

							Material mat = new Material(shader_dict["ubershader"]);

							if (node_go.GetComponent<MeshRenderer>() == null && name[0] == 'b')
							{
								node_go.AddComponent<MeshRenderer>();
								node_go.AddComponent<MeshFilter>();
								node_go.GetComponent<MeshFilter>().mesh = mesh;
								if ((!node_go.name.ToLower().Contains("sky") && !node_go.name.ToLower().Contains("dome")) || node_go.name.ToLower().Contains("skye"))
									node_go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
								node_go.GetComponent<MeshRenderer>().material = mat;
							}

							else if (node_go.GetComponent<SkinnedMeshRenderer>() == null)
							{
								node_go.AddComponent<SkinnedMeshRenderer>();
								node_go.GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
								node_go.GetComponent<SkinnedMeshRenderer>().material = mat;
								if ((!node_go.name.ToLower().Contains("sky") && !node_go.name.ToLower().Contains("dome")) || node_go.name.ToLower().Contains("skye"))
									node_go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
								node_go.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
							}


							List<Transform> bones = new List<Transform>();// = new Transform[]
							List<Matrix4x4> localBindPosesList = new List<Matrix4x4>();

							if (node_part.bones != null)
							{

								foreach (CocosModel.Node.Part.Bone b in node_part.bones)
								{
									if (parent_bones != null)
									{
										if (parent_bones.ContainsKey(b.node))
										{
											bones.Add(parent_bones[b.node]);
										}
									}
									else
										bones.Add(pose_bone_dict[b.node]);



									Matrix4x4 new_bindpose = new Matrix4x4(new Vector4(b.transform[0], -b.transform[1], -b.transform[2], b.transform[3]), new Vector4(-b.transform[4], b.transform[5], b.transform[6], b.transform[7]), new Vector4(-b.transform[8], b.transform[9], b.transform[10], b.transform[11]), new Vector4(-b.transform[12], b.transform[13], b.transform[14], b.transform[15]));

									localBindPosesList.Add(new_bindpose);
								}

								node_go.GetComponent<SkinnedMeshRenderer>().bones = bones.ToArray();

								node_go.GetComponent<SkinnedMeshRenderer>().sharedMesh.bindposes = localBindPosesList.ToArray();
							}

							mesh.triangles = mesh.triangles.Reverse().ToArray();
							mesh.Optimize();
							mesh.RecalculateBounds();
							#endregion
							#region set materials



							if (c3m.ModelConfig[name].jsonData[0].materials.Length == 0)
							{
								Debug.Log("No material defined for " + node_go.name);
							}

							if (!c3m.ModelConfig[name].jsonData[0].material_dict.ContainsKey(node.id))
							{
								Debug.Log("No material defined for node id " + node.id);
								//node_go.gameObject.SetActive(false);
								break;
							}


							Config3DModel._Config3DModel.JsonData.Material material = c3m.ModelConfig[name].jsonData[0].material_dict[node.id];
							mat.name = material.nodeName;

							/*if (material.shaderName == "glow_vfx") //Ugly ass shader
                            {
								node_go.gameObject.SetActive(false);
							}*/


							if (!known_shaders.Contains(material.shaderName))
							{
								Debug.LogWarning("No shader " + material.shaderName + " for model " + name);
							}
							else
							{
								if (transparent_shaders.Contains(material.shaderName)){
									mat.CopyPropertiesFromMaterial(transparent_material);
								}

								else if (transparent_no_depth_write_shaders.Contains(material.shaderName))
                                {
									mat.CopyPropertiesFromMaterial(transparent_no_depth_write_material);
                                }


								if (material.shaderName == "ubershader")
								{

									bool cutout = false;
									if (material.intSettingIds != null)
									{
										for (int i = 0; i < material.intSettingIds.Length; i++)
										{
											if (material.intSettingIds[i] == "UseAsCutout_SWITCH" && material.intSettingValues[i] == 1)
											{
												cutout = true;
											}
										}
									}

									if (material.transparent == 1 && cutout == false)
									{
										mat.CopyPropertiesFromMaterial(transparent_material);
										mat.shader = shader_dict["ubershader_transparent"];
									}
									else
									{
										mat.CopyPropertiesFromMaterial(opaque_material);

										mat.shader = shader_dict["ubershader"];

									}

									mat.SetFloat("u_opacityAmount", 1.0f);
									mat.SetTexture("u_diffuseMap", (Texture)Resources.Load("Shaders/black"));
									mat.SetTexture("u_specularMap", (Texture)Resources.Load("default_dirtmap"));

									mat.SetTexture("u_lightmapMap", (Texture)Resources.Load("default_lightmap"));

									//mat.SetTexture("u_dirtMap", (Texture)Resources.Load("default_dirtmap"));

									mat.SetFloat("_Surface", 1.0f);

									
								}

								else if (material.shaderName == "houseubershader")
								{
									mat.shader = shader_dict["quidditchshader"];

								}


								else if (material.shaderName == "simpleColor")
								{
									mat.shader = shader_dict["SimpleColor"];
								}

								else if (material.shaderName == "SimpleTexture")
                                {
									mat.CopyPropertiesFromMaterial(opaque_material);
									mat.shader = shader_dict["SimpleTexture"];
								}
								else if (material.shaderName == "crowd_vfx")
                                {
									mat.CopyPropertiesFromMaterial(opaque_material);
									mat.shader = shader_dict[material.shaderName];
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

								if (material.shaderName == "avatarfaceshader")
								{
									mat.SetTexture("u_facePaintTexture", (Texture)Resources.Load("Shaders/transparent"));
								}

								if (material.stringValueKeys != null)
								{
									for (int i = 0; i < material.stringValueKeys.Length; i++)
									{
										if (all_textures.ContainsKey(material.stringValueKeys[i]))
										{
											mat.SetTexture(material.stringIds[i], all_textures[material.stringValueKeys[i]]);
										}
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
									switch (DialogueManager.local_avatar_house)
                                    {
										case "ravenclaw":
											mat.SetInt("is_ravenclaw", 1);
											break;
										case "gryffindor":
											mat.SetInt("is_gryffindor", 1);
											break;
										case "slytherin":
											mat.SetInt("is_slytherin", 1);
											break;
										case "hufflepuff":
											mat.SetInt("is_hufflepuff", 1);
											break;
									}
                                }

								if (material.shaderName == "avatarfaceshader")
                                {
									switch (DialogueManager.local_avatar_house)
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

								if (material.CastShadow == 0)
                                {

									if (name[0] != 'b')
										node_go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
									else
										node_go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
								}
								if (material.shaderName == "glow_vfx")
								{

									if (name[0] != 'b')
									{
										node_go.GetComponent<SkinnedMeshRenderer>().receiveShadows = false;
										node_go.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
									}
									else
									{
										node_go.GetComponent<MeshRenderer>().receiveShadows = false;
										node_go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
									}
								}
									#endregion
							}
						}
					}
				}
			}
		}
		go.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

		if (parent_bones != null)
			GameObject.Destroy(go.transform.Find("Armature").gameObject);

		return_model.game_object = go;
		return_model.pose_bones = pose_bone_dict;
		if (pose_bone_dict != null && pose_bone_dict.Count != 0)
			return_model.jt_all_bind = pose_bones[0];
		else
			return_model.jt_all_bind = return_model.game_object.transform;
		return return_model;
	}

    public static void Initialize()
    {
		patch_text = File.ReadAllText("patches\\patch.txt");
		transparent_material = (Material)Resources.Load("transparent_base", typeof(Material));
		opaque_material = (Material)Resources.Load("opaque_base", typeof(Material));
		transparent_no_depth_write_material = (Material)Resources.Load("transparent_no_depth_base", typeof(Material));

		shader_dict = new Dictionary<string, Shader>();

		foreach(string shader in real_shaders)
        {
			shader_dict[shader] = Shader.Find("Shader Graphs/" +  shader);
			if (shader_dict[shader] == null)
            {
				GameObject crash = GameObject.Find("Canvas").transform.Find("Crash").gameObject;
				crash.SetActive(true);
				crash.transform.Find("Error").gameObject.GetComponent<Text>().text = "couldn't find " + shader;
            }
        }

	}

	public static void loadModelsTextures(string model_name)
	{
		foreach (string texture in Configs.config_3dmodel.ModelConfig[model_name].jsonData[0].neededTextureKeys)
		{
			if (!TextureManager.loaded_textures.ContainsKey(texture))
			{
				TextureManager.loaded_textures[texture] = TextureManager.loadTextureDDS(texture);
			}
		}
	}
}