
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using static ModelLoading.MatrixOperations;
using static ModelLoading.ModelMaterials;

namespace ModelLoading {

	public class ModelManager
	{
		//public static Dictionary<string, ModelManager.C3T> loaded_models = new Dictionary<string, ModelManager.C3T>();


		private static GameObject model_game_object;
		private static Dictionary<string, Transform> parent_bones;
		private static List<Transform> pose_bones;
		private static List<Matrix4x4> bindPoses;
		private static Config3DModel._Config3DModel c3m;
		private static CocosModel model;
		private static Dictionary<string, Transform> pose_bone_dict;

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

		private static void processMesh(CocosModel.Mesh mesh)
        {
			int stride = 0;
			foreach (CocosModel.Mesh.Attribute attrib in mesh.attributes)
			{
				switch (attrib.attribute)
				{
					case "VERTEX_ATTRIB_POSITION":
						mesh.VERTEX_ATTRIB_POSITION = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_NORMAL":
						mesh.VERTEX_ATTRIB_NORMAL = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_SOFT_NORMAL":
						mesh.VERTEX_ATTRIB_SOFT_NORMAL = new List<Vector3>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD":
						mesh.VERTEX_ATTRIB_TEX_COORD = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD1":
						mesh.VERTEX_ATTRIB_TEX_COORD1 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD2":
						mesh.VERTEX_ATTRIB_TEX_COORD2 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_TEX_COORD3":
						mesh.VERTEX_ATTRIB_TEX_COORD3 = new List<Vector2>();
						break;
					case "VERTEX_ATTRIB_BLEND_WEIGHT":
						mesh.VERTEX_ATTRIB_BLEND_WEIGHT = new List<Vector4>();
						break;
					case "VERTEX_ATTRIB_BLEND_INDEX":
						mesh.VERTEX_ATTRIB_BLEND_INDEX = new List<Vector4>();
						break;
					case "VERTEX_ATTRIB_COLOR":
						mesh.VERTEX_ATTRIB_COLOR = new List<Color>();
						break;
					default:
						Debug.LogError("ERROR, UNKNOWN VERTEX ATTRIB " + attrib.attribute);
						break;
				}
				stride += attrib.size;
			}

			for (int v = 0; v < mesh.vertices.Length; v += stride)
			{
				int offset = 0;
				foreach (CocosModel.Mesh.Attribute attrib in mesh.attributes)
				{
					switch (attrib.attribute)
					{
						case "VERTEX_ATTRIB_POSITION":
							mesh.VERTEX_ATTRIB_POSITION.Add(new Vector3(mesh.vertices[v + offset] * -1, mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_NORMAL":
							mesh.VERTEX_ATTRIB_NORMAL.Add(new Vector3(-mesh.vertices[v + offset], mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_SOFT_NORMAL":
							mesh.VERTEX_ATTRIB_SOFT_NORMAL.Add(new Vector3(mesh.vertices[v + offset], mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD":
							mesh.VERTEX_ATTRIB_TEX_COORD.Add(new Vector2(mesh.vertices[v + offset], mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD1":
							mesh.VERTEX_ATTRIB_TEX_COORD1.Add(new Vector2(mesh.vertices[v + offset], mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD2":
							mesh.VERTEX_ATTRIB_TEX_COORD2.Add(new Vector2(mesh.vertices[v + offset], mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_TEX_COORD3":
							mesh.VERTEX_ATTRIB_TEX_COORD3.Add(new Vector2(mesh.vertices[v + offset], mesh.vertices[v + offset + 1]));
							break;
						case "VERTEX_ATTRIB_BLEND_WEIGHT":
							mesh.VERTEX_ATTRIB_BLEND_WEIGHT.Add(new Vector4(mesh.vertices[v + offset], mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2], mesh.vertices[v + offset + 3]));
							break;
						case "VERTEX_ATTRIB_BLEND_INDEX":
							mesh.VERTEX_ATTRIB_BLEND_INDEX.Add(new Vector4(mesh.vertices[v + offset], mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2], mesh.vertices[v + offset + 3]));
							break;
						case "VERTEX_ATTRIB_COLOR":
							mesh.VERTEX_ATTRIB_COLOR.Add(new Color(mesh.vertices[v + offset], mesh.vertices[v + offset + 1], mesh.vertices[v + offset + 2], mesh.vertices[v + offset + 3]));
							break;
						default:
							Debug.LogError("ERROR, UNKNOWN VERTEX ATTRIB " + attrib.attribute);
							break;
					}
					offset += attrib.size;
				}
			}
		}

		private static void processSkeleton(CocosModel.Node node)
        {
			GameObject armature = new GameObject("Armature");
			armature.transform.parent = model_game_object.transform;

			Matrix4x4 transform_matrix = new Matrix4x4(new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]), new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]), new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]), new Vector4(node.transform[12], node.transform[13], node.transform[14], node.transform[15]));
			//jt_all_bind is actually useless! Avoid!
			//Preferably, we would apply jt_all_bind after loading model and in gameworld

			//Idk how old that last comment is but we need at least the scale component.

			Matrix4x4 reference_matrix = Matrix4x4.identity;

			pose_bones.Add(new GameObject(node.id).transform);
			pose_bones[0].parent = armature.transform;
			pose_bones[0].localRotation = ExtractRotationFromMatrix(ref reference_matrix);
			pose_bones[0].localPosition = ExtractTranslationFromMatrix(ref reference_matrix);
			pose_bones[0].localScale = ExtractScaleFromMatrix(ref transform_matrix);


			bindPoses.Add(pose_bones[0].worldToLocalMatrix);
			foreach (CocosModel.Node child in node.children)
			{
				processPoseSkeletonChild(child, pose_bones[0].transform, ref pose_bones, ref bindPoses);
			}
		}

		private static void buildNodePart(CocosModel.Node node, CocosModel.Node.Part node_part, Matrix4x4 node_transform_matrix)
        {
			GameObject node_part_gameobject = new GameObject(node.id);
			Quaternion extracted_rotation = ExtractRotationFromMatrix(ref node_transform_matrix);
			node_part_gameobject.transform.parent = model_game_object.transform;
			node_part_gameobject.transform.position = ExtractTranslationFromMatrix(ref node_transform_matrix);
			node_part_gameobject.transform.rotation = Quaternion.Euler(new Vector3(extracted_rotation.eulerAngles.x, -extracted_rotation.eulerAngles.y, -extracted_rotation.eulerAngles.z));
			node_part_gameobject.transform.localScale = ExtractScaleFromMatrix(ref node_transform_matrix);

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
				Debug.LogError("Failed to find mesh part " + node_part.meshpartid + " in model: " + c3m.name);
				return;
			}

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
				Debug.LogError("Overflowing indices on model: " + c3m.name + " part: " + C3T_part.id);

			if (C3T_part.indices != null)
				mesh.triangles = C3T_part.indices;

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

			if (!c3m.jsonData[0].material_dict.ContainsKey(node.id))
			{
				Debug.LogError("No material defined for node id " + node.id);
				return;
			}

			Material mat = new Material(shader_dict["ubershader"]);

			Config3DModel._Config3DModel.JsonData.Material material = c3m.jsonData[0].material_dict[node.id];
			mat.name = material.nodeName;

			SkinnedMeshRenderer skinned_mesh_renderer = null;

			if (c3m.name[0] == 'b')
			{
				MeshRenderer mesh_renderer = node_part_gameobject.AddComponent<MeshRenderer>();
				MeshFilter mesh_filter = node_part_gameobject.AddComponent<MeshFilter>();
				mesh_filter.mesh = mesh;

				if (ShadowDecider.decideShadow(node_part_gameobject.name.ToLower(), material.shaderName, c3m.name))
					mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
				else
				{
					mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mesh_renderer.receiveShadows = false;
				}
				if (material.CastShadow == 0)
				{
					mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					mesh_renderer.receiveShadows = false;
				}

				mesh_renderer.material = mat;
			}

			else
			{
				skinned_mesh_renderer = node_part_gameobject.AddComponent<SkinnedMeshRenderer>();
				skinned_mesh_renderer.sharedMesh = mesh;
				skinned_mesh_renderer.material = mat;
				if (ShadowDecider.decideShadow(node_part_gameobject.name.ToLower(), material.shaderName, c3m.name))
					skinned_mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
				else
				{
					skinned_mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					skinned_mesh_renderer.receiveShadows = false;
				}
				if (material.CastShadow == 0)
				{
					skinned_mesh_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
					skinned_mesh_renderer.receiveShadows = false;
				}
				skinned_mesh_renderer.updateWhenOffscreen = true;
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
							bones.Add(parent_bones[b.node]);
					}
					else
						bones.Add(pose_bone_dict[b.node]);

					Matrix4x4 new_bindpose = new Matrix4x4(new Vector4(b.transform[0], -b.transform[1], -b.transform[2], b.transform[3]), new Vector4(-b.transform[4], b.transform[5], b.transform[6], b.transform[7]), new Vector4(-b.transform[8], b.transform[9], b.transform[10], b.transform[11]), new Vector4(-b.transform[12], b.transform[13], b.transform[14], b.transform[15]));

					localBindPosesList.Add(new_bindpose);
				}

				skinned_mesh_renderer.bones = bones.ToArray();

				skinned_mesh_renderer.sharedMesh.bindposes = localBindPosesList.ToArray();
			}

			mesh.triangles = mesh.triangles.Reverse().ToArray();
			mesh.Optimize();
			mesh.RecalculateBounds();

			applyModelMaterial(mat, material);
		}

		public static Model loadModel(string name, Dictionary<string, Transform> _parent_bones = null)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new System.Exception("Null or empty model load");
			}
			if (!Configs.config_3dmodel.ModelConfig.ContainsKey(name))
			{
				Debug.LogError("Not c3b config entry for " + name);
				return null;
			}

			c3m = Configs.config_3dmodel.ModelConfig[name];

			model = C3B.loadC3B(c3m.jsonData[0].mesh, GlobalEngineVariables.models_folder);
			if (model == null)
			{
				Debug.LogError("Model loaded null " + name);
				return null;
			}

			model_game_object = new GameObject(name);
			model_game_object.transform.position = Vector3.zero;
			model_game_object.transform.rotation = Quaternion.identity;
			parent_bones = _parent_bones;
			pose_bones = new List<Transform>();
			pose_bone_dict = new Dictionary<string, Transform>();
			bindPoses = new List<Matrix4x4>();


			foreach (var mesh in model.meshes)
			{
				processMesh(mesh);
			}
			foreach (CocosModel.Node node in model.nodes)
			{
				if (node.skeleton == true)
					processSkeleton(node);
			}

			for (int t = 0; t < pose_bones.Count; t++)
			{
				pose_bone_dict[pose_bones[t].gameObject.name] = pose_bones[t];
			}

			foreach (CocosModel.Node node in model.nodes)
			{
				if (node.skeleton)
					continue;

				if (node.parts is null)
					continue;

				Matrix4x4 node_transform_matrix = new Matrix4x4(
					new Vector4(node.transform[0], node.transform[1], node.transform[2], node.transform[3]),
					new Vector4(node.transform[4], node.transform[5], node.transform[6], node.transform[7]),
					new Vector4(node.transform[8], node.transform[9], node.transform[10], node.transform[11]),
					new Vector4(-node.transform[12], node.transform[13], node.transform[14], node.transform[15]));

				foreach (CocosModel.Node.Part node_part in node.parts)
				{
					buildNodePart(node, node_part, node_transform_matrix);
				}
			}

			model_game_object.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

			if (parent_bones != null)
				GameObject.Destroy(model_game_object.transform.Find("Armature").gameObject);

			Model return_model = new Model();
			return_model.game_object = model_game_object;
			return_model.pose_bones = pose_bone_dict;
			if (pose_bone_dict != null && pose_bone_dict.Count != 0)
				return_model.jt_all_bind = pose_bones[0];
			else
				return_model.jt_all_bind = return_model.game_object.transform;
			return return_model;
		}

		public static void Initialize()
		{
			ModelMaterials.Initialize();
		}

	}
}