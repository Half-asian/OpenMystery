using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;


public class c3bLoader : MonoBehaviour
{
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
	public static int c3bGetMeshAttribute(ref byte[] file, ref Attribute new_attribute, ref int pointer)
	{
		new_attribute.size = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		int num_type_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_attribute.type = Encoding.UTF8.GetString(file, pointer, num_type_letters);
		pointer += num_type_letters;
		int num_attribute_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_attribute.attribute = Encoding.UTF8.GetString(file, pointer, num_attribute_letters);
		pointer += num_attribute_letters;
		return pointer;
	}

	public static int c3bGetMeshVertices(ref byte[] file, ref Mesh new_mesh, ref int pointer, ref Attribute[] attributes, int stride)
	{
		int num_floats = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		int num_vertices = (int)(num_floats / stride);

		Vector3[] VERTEX_ATTRIB_POSITION = new Vector3[num_vertices];
		Vector2[] VERTEX_ATTRIB_TEX_COORD = new Vector2[num_vertices];
		Vector2[] VERTEX_ATTRIB_TEX_COORD1 = new Vector2[num_vertices];
		Vector2[] VERTEX_ATTRIB_TEX_COORD2 = new Vector2[num_vertices];
		Color[] VERTEX_ATTRIB_COLOR = new Color[num_vertices];
		BoneWeight[] weights = new BoneWeight[num_vertices];


		for (int i = 0; i < num_vertices; i++)
        {
			foreach (Attribute attribute in attributes)
			{
                switch (attribute.attribute)
                {
					case "VERTEX_ATTRIB_POSITION":
						VERTEX_ATTRIB_POSITION[i] = new Vector3(System.BitConverter.ToSingle(file, pointer), System.BitConverter.ToSingle(file, pointer + 4), System.BitConverter.ToSingle(file, pointer + 8));
						break;
					case "VERTEX_ATTRIB_TEX_COORD":
						VERTEX_ATTRIB_TEX_COORD[i] = new Vector2(System.BitConverter.ToSingle(file, pointer), System.BitConverter.ToSingle(file, pointer + 4));
						break;
					case "VERTEX_ATTRIB_TEX_COORD1":
						VERTEX_ATTRIB_TEX_COORD1[i] = new Vector2(System.BitConverter.ToSingle(file, pointer), System.BitConverter.ToSingle(file, pointer + 4));
						break;
					case "VERTEX_ATTRIB_TEX_COORD2":
						VERTEX_ATTRIB_TEX_COORD2[i] = new Vector2(System.BitConverter.ToSingle(file, pointer), System.BitConverter.ToSingle(file, pointer + 4));
						break;
					case "VERTEX_ATTRIB_COLOR":
						VERTEX_ATTRIB_COLOR[i] = new Color(System.BitConverter.ToSingle(file, pointer), System.BitConverter.ToSingle(file, pointer + 4), System.BitConverter.ToSingle(file, pointer + 8), System.BitConverter.ToSingle(file, pointer + 12));
						break;
					case "VERTEX_ATTRIB_BLEND_WEIGHT":
						weights[i].weight0 = System.BitConverter.ToSingle(file, pointer);
						weights[i].weight1 = System.BitConverter.ToSingle(file, pointer + 4);
						weights[i].weight2 = System.BitConverter.ToSingle(file, pointer + 8);
						weights[i].weight3 = System.BitConverter.ToSingle(file, pointer + 12);
						break;
					case "VERTEX_ATTRIB_BLEND_INDEX":
						weights[i].boneIndex0 = (int)System.BitConverter.ToSingle(file, pointer);
						weights[i].boneIndex1 = (int)System.BitConverter.ToSingle(file, pointer + 4);
						weights[i].boneIndex2 = (int)System.BitConverter.ToSingle(file, pointer + 8);
						weights[i].boneIndex3 = (int)System.BitConverter.ToSingle(file, pointer + 12);
						break;

					case "VERTEX_ATTRIB_NORMAL":
					case "VERTEX_ATTRIB_SOFT_NORMAL":
						break;
					default:
						Debug.Log("Unknown Vertex attribute " + attribute.attribute);
						break;	
				}
				pointer += attribute.size * 4;
			}
		}

		new_mesh.vertices = VERTEX_ATTRIB_POSITION;
		//new_mesh.uv = VERTEX_ATTRIB_TEX_COORD;
		//new_mesh.uv2 = VERTEX_ATTRIB_TEX_COORD1;
		//new_mesh.uv3 = VERTEX_ATTRIB_TEX_COORD2;
		//new_mesh.colors = VERTEX_ATTRIB_COLOR;
		//new_mesh.boneWeights = weights;
		return pointer;
	}

	
	public static int c3bGetMeshPart(ref byte[] file, ref Mesh new_mesh, int part, ref int pointer)
	//public static int c3bGetMeshPart(ref byte[] file, ref Mesh new_mesh, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_mesh.name = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		int num_part_indices = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		int[] triangles = new int[num_part_indices];




		for (int part_index = 0; part_index < num_part_indices; part_index++)
		{
			//Debug.Log(System.BitConverter.ToUInt16(file, pointer + part_index * 2));
			triangles[part_index] = System.BitConverter.ToUInt16(file, pointer + part_index * 2);
			//Debug.Log(new_mesh.triangles[part_index]);
		}



		pointer += num_part_indices * 2;
		//new_mesh.triangles = triangles;
		new_mesh.SetTriangles(triangles, part);
		
		//new_part.aabb = new float[6];
		//for (int i = 0; i < 6; i++)
		//{
			//new_part.aabb[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		//}
		return pointer + 24;
	}
	/*
	public static int c3bGetMaterial(ref byte[] file, ref c3t.Material new_material, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_material.id = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		new_material.ambient = new float[3];
		for (int a = 0; a < 3; a++)
		{
			new_material.ambient[a] = System.BitConverter.ToSingle(file, pointer + a * 4);
		}
		pointer += 12;
		new_material.diffuse = new float[3];
		for (int d = 0; d < 3; d++)
		{
			new_material.diffuse[d] = System.BitConverter.ToSingle(file, pointer + d * 4);
		}
		pointer += 12;
		new_material.emissive = new float[3];
		for (int e = 0; e < 3; e++)
		{
			new_material.emissive[e] = System.BitConverter.ToSingle(file, pointer + e * 4);
		}
		pointer += 12;
		new_material.opacity = System.BitConverter.ToSingle(file, pointer);
		pointer += 4;
		return pointer + 20;
	}*/

	public static int c3bGetNodePartBone(ref byte[] file, ref int pointer, ref Dictionary<string, Matrix4x4> pose_bones)
	{
		int num_node_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		string node = Encoding.UTF8.GetString(file, pointer, num_node_letters);
		pointer += num_node_letters;

		float[] transform = new float[16];
		for (int i = 0; i < 16; i++)
		{
			transform[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}
		if (!pose_bones.ContainsKey(node))
        {
			pose_bones[node] = new Matrix4x4(new Vector4(transform[0], transform[1], transform[2], transform[3]), new Vector4(transform[4], transform[5], transform[6], transform[7]), new Vector4(transform[8], transform[9], transform[10], transform[11]), new Vector4(transform[12], transform[13], transform[14], transform[15]));
		}
		pointer += 64;
		return pointer;
	}

	public static int c3bGetNodePart(ref byte[] file, ref int pointer, ref SkinnedMeshRenderer smr, ref Dictionary<string, Mesh> meshes, int part, ref Dictionary<string, Matrix4x4> pose_bones)
	{
		int num_meshpartid_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		//new_part.meshpartid = Encoding.UTF8.GetString(file, pointer, num_meshpartid_letters);
		string meshpartid = Encoding.UTF8.GetString(file, pointer, num_meshpartid_letters);
		if (meshpartid[meshpartid.Length - 1] == '1')
        {
			smr.sharedMesh = meshes[Encoding.UTF8.GetString(file, pointer, num_meshpartid_letters)];
        }
		pointer += num_meshpartid_letters;


		int num_materialid_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		//new_part.materialid = Encoding.UTF8.GetString(file, pointer, num_materialid_letters);
		pointer += num_materialid_letters;


		int num_bones = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		if (num_bones != 0)
		{
			//new_part.bones = new Dictionary<string, Node.Part.Bone>();
			for (int bone = 0; bone < num_bones; bone++)
			{
				//Node.Part.Bone new_bone = new Node.Part.Bone();
				c3bGetNodePartBone(ref file, ref pointer, ref pose_bones);
				//new_part.bones[new_bone.node] = new_bone;
			}
		}

		int num_uvmapping = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		pointer += num_uvmapping * 4; //NOTHING USES THIS?

		return pointer;
	}

	public static int processPoseSkeletonChild(ref byte[] file, ref int pointer, ref GameObject new_child, ref List<Transform> bones, ref List<Matrix4x4> bindPoses)
	{

		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_child.name = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		//bones.Add(new GameObject(Encoding.UTF8.GetString(file, pointer, num_name_letters)).transform);
		bones.Add(new_child.transform);
		pointer += num_name_letters;
		bool is_skeleton = System.BitConverter.ToBoolean(file, pointer);


		pointer += 1;

		float[] transform = new float[16];
		for (int i = 0; i < 16; i++)
		{
			transform[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}


		int bone_index = bones.Count - 1;

		Matrix4x4 child_transform_matrix = new Matrix4x4(new Vector4(transform[0], transform[1], transform[2], transform[3]), new Vector4(transform[4], transform[5], transform[6], transform[7]), new Vector4(transform[8], transform[9], transform[10], transform[11]), new Vector4(transform[12], transform[13], transform[14], transform[15]));

		bones[bone_index].localRotation = ExtractRotationFromMatrix(ref child_transform_matrix);
		bones[bone_index].localPosition = ExtractTranslationFromMatrix(ref child_transform_matrix);
		//bones[bone_index].localScale = ExtractScaleFromMatrix(ref child_transform_matrix); //Breaks shit
		pointer += 64;


		bones[bone_index].localPosition = new Vector3(bones[bone_index].localPosition.x * -1, bones[bone_index].localPosition.y, bones[bone_index].localPosition.z);
		bones[bone_index].localRotation = Quaternion.Euler(new Vector3(bones[bone_index].localRotation.eulerAngles.x, bones[bone_index].localRotation.eulerAngles.y * -1, bones[bone_index].localRotation.eulerAngles.z * -1));

		bindPoses.Add(bones[bones.Count - 1].worldToLocalMatrix);// * transform.localToWorldMatrix);

		int num_parts = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		int num_children = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		if (num_children != 0)
		{
			for (int part = 0; part < num_children; part++)
			{
				GameObject child = new GameObject();
				processPoseSkeletonChild(ref file, ref pointer, ref child, ref bones, ref bindPoses);
				child.transform.parent = new_child.transform;
			}
		}
		return pointer;
	}

	public static int c3bGetNode(ref byte[] file, ref int pointer, ref GameObject game_object, ref Dictionary<string, Mesh> meshes, ref Dictionary<string, Matrix4x4> pose_bones, ref List<Transform> bones, ref List<Matrix4x4> bindPoses)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		game_object.name = Encoding.UTF8.GetString(file, pointer, num_name_letters);

		pointer += num_name_letters;
		bool is_skeleton = System.BitConverter.ToBoolean(file, pointer);


		pointer += 1;

		float[] transform = new float[16];
		for (int i = 0; i < 16; i++)
		{
			transform[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}
		Matrix4x4 transform_matrix = new Matrix4x4(new Vector4(transform[0], transform[1], transform[2], transform[3]), new Vector4(transform[4], transform[5], transform[6], transform[7]), new Vector4(transform[8], transform[9], transform[10], transform[11]), new Vector4(transform[12], transform[13], transform[14], transform[15]));
		game_object.transform.localPosition = ExtractTranslationFromMatrix(ref transform_matrix);
		game_object.transform.localRotation = ExtractRotationFromMatrix(ref transform_matrix);
		game_object.transform.localScale = ExtractScaleFromMatrix(ref transform_matrix);

		pointer += 64;

		int num_parts = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		if (num_parts != 0)
		{
			SkinnedMeshRenderer smr = game_object.AddComponent<SkinnedMeshRenderer>();
			smr.materials = new Material[num_parts];
			for (int part = 0; part < num_parts; part++)
			{
				pointer = c3bGetNodePart(ref file, ref pointer, ref smr, ref meshes, part, ref pose_bones);
			}
		}

		int num_children = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		if (is_skeleton == true)
		{
			if (num_children != 0)
			{
				for (int part = 0; part < num_children; part++)
				{

					GameObject new_child = new GameObject();
					pointer = processPoseSkeletonChild(ref file, ref pointer, ref new_child, ref bones, ref bindPoses);
					new_child.transform.parent = game_object.transform;
				}
			}
		}

		return pointer;
	}
	/*
	public static int c3bGetAnimationBoneKeyframe(ref byte[] file, ref c3t.Animation.Bone.Keyframe new_keyframe, ref int pointer)
	{
		new_keyframe.keytime = System.BitConverter.ToSingle(file, pointer);
		pointer += 4;

		int mystery_byte = file[pointer];

		bool rotate = false;
		bool translate = false;
		bool scale = false;



		switch (mystery_byte)
		{
			case 1:
				rotate = true;
				break;
			case 2:
				scale = true;
				break;
			case 3:
				rotate = true;
				scale = true;
				break;
			case 4:
				translate = true;
				break;
			case 5:
				rotate = true;
				translate = true;
				break;
			case 6:
				scale = true;
				translate = true;
				break;
			case 7:
				rotate = true;
				scale = true;
				translate = true;
				break;
		}
		pointer += 1;

		if (rotate == true)
		{
			new_keyframe.rotation = new float[4];
			for (int r = 0; r < 4; r++)
			{
				new_keyframe.rotation[r] = System.BitConverter.ToSingle(file, pointer + r * 4);
			}
			pointer += 16;
		}
		if (scale == true)
		{
			new_keyframe.scale = new float[3];
			for (int s = 0; s < 3; s++)
			{
				new_keyframe.scale[s] = System.BitConverter.ToSingle(file, pointer + s * 4);
			}
			pointer += 12;
		}
		if (translate == true)
		{
			new_keyframe.translation = new float[3];
			for (int t = 0; t < 3; t++)
			{
				new_keyframe.translation[t] = System.BitConverter.ToSingle(file, pointer + t * 4);
			}
			pointer += 12;
		}
		return pointer;
	}


	public static int c3bGetAnimationBone(ref byte[] file, ref c3t.Animation.Bone new_bone, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_bone.boneId = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		int num_keyframes = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_bone.keyframes = new c3t.Animation.Bone.Keyframe[num_keyframes];
		for (int keyframe = 0; keyframe < num_keyframes; keyframe++)
		{
			c3t.Animation.Bone.Keyframe new_keyframe = new c3t.Animation.Bone.Keyframe();
			pointer = c3bGetAnimationBoneKeyframe(ref file, ref new_keyframe, ref pointer);
			new_bone.keyframes[keyframe] = new_keyframe;
		}
		return pointer;
	}


	public static int c3bGetAnimation(ref byte[] file, ref c3t.Animation new_animation, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_animation.id = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		new_animation.length = System.BitConverter.ToSingle(file, pointer);
		pointer += 4;
		int num_bones = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_animation.bones = new c3t.Animation.Bone[num_bones];
		for (int bone = 0; bone < num_bones; bone++)
		{
			c3t.Animation.Bone new_bone = new c3t.Animation.Bone();
			pointer = c3bGetAnimationBone(ref file, ref new_bone, ref pointer);
			new_animation.bones[bone] = new_bone;
		}
		return pointer;
	}*/

	public class Attribute
    {
		public int size;
		public string type;
		public string attribute;
	}

	//public Dictionary<string, Matrix4x4> pose_bones;

	/*public class Node
    {
		public class Part
        {
			public string meshpartid;
			public string materialid;
			public class Bone
			{
				public Vector3 translation;
				public Quaternion rotation;
				public Vector3 scale;
				public string node;
			}
			public Dictionary<string, Bone> bones;
		}

		public Part[] parts;
		public string id;
		public bool skeleton;
		public Vector3 translation;
		public Quaternion rotation;
		public Vector3 scale;
		public Node[] children;
	}*/

	public static GameObject loadc3bModel(string file_name)
	{
		byte[] file = File.ReadAllBytes(file_name);
		GameObject model_parent = new GameObject();
		int pointer = 6;
		int num_unknown = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		for (int offset = 0; offset < num_unknown; offset++)
		{

			pointer += System.BitConverter.ToInt32(file, pointer);
			pointer += 12;
		}
		int num_meshes = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		//Mesh[] meshes = new Mesh[num_meshes];
		//List<Mesh> meshes = new List<Mesh>();
		Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();

		for (int mesh = 0; mesh < num_meshes; mesh++)
		{
			Mesh new_mesh = new Mesh();
			int num_mesh_attributes = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			Attribute[] attributes = new Attribute[num_mesh_attributes];
			int stride = 0;
			for (int mesh_attribute = 0; mesh_attribute < num_mesh_attributes; mesh_attribute++)
			{
				Attribute new_attribute = new Attribute();
				pointer = c3bGetMeshAttribute(ref file, ref new_attribute, ref pointer);
				attributes[mesh_attribute] = new_attribute;
				stride += new_attribute.size;
			}

			pointer = c3bGetMeshVertices(ref file, ref new_mesh, ref pointer, ref attributes, stride);

			int num_mesh_parts = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			//new_mesh.parts = new c3t.Mesh.Part[num_mesh_parts];
			new_mesh.subMeshCount = num_mesh_parts;

			for (int part = 0; part < num_mesh_parts; part++)
			{
				/*if (part != 0)
                {
					new_mesh = Instantiate<Mesh>(new_mesh);
                }*/
				pointer = c3bGetMeshPart(ref file, ref new_mesh, part, ref pointer);
				//pointer = c3bGetMeshPart(ref file, ref new_mesh, ref pointer);
			}
			meshes[new_mesh.name] = new_mesh;
		}


		int num_materials = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		for (int material = 0; material < num_materials; material++)
		{
			int num_name_letters = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += num_name_letters;
			pointer += 60;
			//c3t.Material new_material = new c3t.Material();
			//pointer = c3bGetMaterial(ref file, ref new_material, ref pointer);
			//model_c3t.materials[material] = new_material;
		}

		int num_nodes = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;


		GameObject[] nodes = new GameObject[num_nodes];
		Dictionary<string, Matrix4x4> pose_bones = new Dictionary<string, Matrix4x4>();
		List<Transform> bones = new List<Transform>();
		List<Matrix4x4> bindPoses = new List<Matrix4x4>();


		for (int node = 0; node < num_nodes; node++)
		{
			GameObject new_node = new GameObject();
			pointer = c3bGetNode(ref file, ref pointer, ref new_node, ref meshes, ref pose_bones, ref bones, ref bindPoses);
			new_node.transform.parent = model_parent.transform;
			nodes[node] = new_node;
		}

		Debug.Log(bones.Count);

		foreach(GameObject node in nodes)
        {
			SkinnedMeshRenderer c_go_smr = node.GetComponent<SkinnedMeshRenderer>();
			if (c_go_smr != null)
			{
				Debug.Log(c_go_smr);
				//c_go_smr.bones = bones.ToArray();
				//c_go_smr.sharedMesh.bindposes = bindPoses.ToArray();
				//c_go_smr.rootBone = bones[0];
			}
			/*for (int c = 0; c < node.transform.childCount; c++)
            {
				GameObject c_go = node.transform.GetChild(c).gameObject;
				SkinnedMeshRenderer c_go_smr = c_go.GetComponent<SkinnedMeshRenderer>();
				if (c_go_smr != null)
                {
					Debug.Log(c_go_smr.gameObject);
					c_go_smr.bones = bones.ToArray();
					c_go_smr.sharedMesh.bindposes = bindPoses.ToArray();
					c_go_smr.rootBone = bones[0];
				}
            }*/
		}

		/*
		List<c3t.Animation> animations = new List<c3t.Animation>();

		while (pointer < file.Length)
		{
			c3t.Animation new_animation = new c3t.Animation();
			animations.Add(new_animation);
			pointer = c3bGetAnimation(ref file, ref new_animation, ref pointer);
		}

		if (animations.Count > 0)
		{
			model_c3t.animations = animations.ToArray();
		}*/

		/*foreach (Mesh m in meshes.Values)
		{
			//m.Optimize();
			m.RecalculateNormals();
			m.RecalculateBounds();
			GameObject g = new GameObject();
			g.transform.parent = model_parent.transform;
			MeshFilter s = g.AddComponent<MeshFilter>();
			s.sharedMesh = m;
			MeshRenderer mr = g.AddComponent<MeshRenderer>();
			mr.materials = new Material[m.subMeshCount];
		}*/

		return model_parent;
	}
}
