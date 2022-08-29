using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
public class C3B
{
	public static int c3bGetMeshAttribute(ref byte[] file, ref CocosModel.Mesh.Attribute new_attribute, ref int pointer)
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

	public static int c3bGetMeshVertices(ref byte[] file, ref CocosModel.Mesh new_mesh, ref int pointer)
	{
		int num_vertices = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_mesh.vertices = new float[num_vertices];
		for (int vertex = 0; vertex < num_vertices; vertex++)
		{
			new_mesh.vertices[vertex] = System.BitConverter.ToSingle(file, pointer + vertex * 4);
		}
		pointer += num_vertices * 4;
		return pointer;
	}
	public static int c3bGetMeshPart(ref byte[] file, ref CocosModel.Mesh.Part new_part, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_part.id = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		new_part.type = "TRIANGLES";
		int num_part_indices = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_part.indices = new int[num_part_indices];
		for (int part_index = 0; part_index < num_part_indices; part_index++)
		{
			new_part.indices[part_index] = (int)System.BitConverter.ToUInt16(file, pointer + part_index * 2);
		}
		pointer += num_part_indices * 2;
		new_part.aabb = new float[6];
		for (int i = 0; i < 6; i++)
		{
			new_part.aabb[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}
		return pointer + 24;
	}

	public static int c3bGetMaterial(ref byte[] file, ref CocosModel.Material new_material, ref int pointer)
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
		pointer += 16;

		int num_textures = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		for (int i = 0; i < num_textures; i++)
		{
			int id_letters = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += id_letters;
			int filename_letters = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += filename_letters;
			pointer += 16;
			int type_letters = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += type_letters;
			int wrapModeU = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += wrapModeU;
			int wrapModeV = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			pointer += wrapModeV;
		}



		return pointer;
	}

	public static int c3bGetNodePartBone(ref byte[] file, ref CocosModel.Node.Part.Bone new_bone, ref int pointer)
	{
		int num_node_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_bone.node = Encoding.UTF8.GetString(file, pointer, num_node_letters);
		pointer += num_node_letters;

		new_bone.transform = new float[16];
		for (int i = 0; i < 16; i++)
		{
			new_bone.transform[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}
		pointer += 64;
		return pointer;
	}
	public static int c3bGetNodeUvMapping(ref byte[] file, ref int pointer)
	{
		int num_uv = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		pointer += 4 * num_uv;
		return pointer;
	}

	public static int c3bGetNodePart(ref byte[] file, ref CocosModel.Node.Part new_part, ref int pointer)
	{
		int num_meshpartid_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_part.meshpartid = Encoding.UTF8.GetString(file, pointer, num_meshpartid_letters);
		pointer += num_meshpartid_letters;

		int num_materialid_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_part.materialid = Encoding.UTF8.GetString(file, pointer, num_materialid_letters);
		pointer += num_materialid_letters;


		int num_bones = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;

		if (num_bones != 0)
		{
			new_part.bones = new CocosModel.Node.Part.Bone[num_bones];
			for (int bone = 0; bone < num_bones; bone++)
			{
				CocosModel.Node.Part.Bone new_bone = new CocosModel.Node.Part.Bone();
				c3bGetNodePartBone(ref file, ref new_bone, ref pointer);
				new_part.bones[bone] = new_bone;
			}
		}

		int num_uvmapping = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		for (int i = 0; i < num_uvmapping; i++)
		{
			pointer = c3bGetNodeUvMapping(ref file, ref pointer);
		}

		return pointer;
	}

	public static int c3bGetNode(ref byte[] file, ref CocosModel.Node new_node, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_node.id = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		new_node.skeleton = System.BitConverter.ToBoolean(file, pointer);
		pointer += 1;

		new_node.transform = new float[16];
		for (int i = 0; i < 16; i++)
		{
			new_node.transform[i] = System.BitConverter.ToSingle(file, pointer + i * 4);
		}
		pointer += 64;

		int num_parts = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		if (num_parts != 0)
		{
			new_node.parts = new CocosModel.Node.Part[num_parts];
			for (int part = 0; part < num_parts; part++)
			{
				CocosModel.Node.Part new_part = new CocosModel.Node.Part();
				pointer = c3bGetNodePart(ref file, ref new_part, ref pointer);
				new_node.parts[part] = new_part;
			}
		}

		int num_children = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		if (num_children != 0)
		{
			new_node.children = new CocosModel.Node[num_children];
			for (int part = 0; part < num_children; part++)
			{
				CocosModel.Node new_child = new CocosModel.Node();
				pointer = c3bGetNode(ref file, ref new_child, ref pointer);
				new_node.children[part] = new_child;
			}
		}

		return pointer;
	}

	public static int c3bGetAnimationBoneKeyframe(ref byte[] file, ref CocosModel.Animation.Bone.Keyframe new_keyframe, ref int pointer)
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


	public static int c3bGetAnimationBone(ref byte[] file, ref CocosModel.Animation.Bone new_bone, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_bone.boneId = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		int num_keyframes = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_bone.keyframes = new CocosModel.Animation.Bone.Keyframe[num_keyframes];
		for (int keyframe = 0; keyframe < num_keyframes; keyframe++)
		{
			CocosModel.Animation.Bone.Keyframe new_keyframe = new CocosModel.Animation.Bone.Keyframe();
			pointer = c3bGetAnimationBoneKeyframe(ref file, ref new_keyframe, ref pointer);
			new_bone.keyframes[keyframe] = new_keyframe;
		}
		return pointer;
	}


	public static int c3bGetAnimation(ref byte[] file, ref CocosModel.Animation new_animation, ref int pointer)
	{
		int num_name_letters = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_animation.id = Encoding.UTF8.GetString(file, pointer, num_name_letters);
		pointer += num_name_letters;
		new_animation.length = System.BitConverter.ToSingle(file, pointer);
		pointer += 4;
		int num_bones = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		new_animation.bones = new CocosModel.Animation.Bone[num_bones];
		for (int bone = 0; bone < num_bones; bone++)
		{
			CocosModel.Animation.Bone new_bone = new CocosModel.Animation.Bone();
			pointer = c3bGetAnimationBone(ref file, ref new_bone, ref pointer);
			new_animation.bones[bone] = new_bone;
		}
		return pointer;
	}


	public static CocosModel readC3B(string file_name)
	{
		byte[] file = File.ReadAllBytes(file_name);
		CocosModel model_C3B = new CocosModel();
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
		model_C3B.meshes = new CocosModel.Mesh[num_meshes];
		for (int mesh = 0; mesh < num_meshes; mesh++)
		{
			CocosModel.Mesh new_mesh = new CocosModel.Mesh();
			int num_mesh_attributes = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			new_mesh.attributes = new CocosModel.Mesh.Attribute[num_mesh_attributes];
			for (int mesh_attribute = 0; mesh_attribute < num_mesh_attributes; mesh_attribute++)
			{
				CocosModel.Mesh.Attribute new_attribute = new CocosModel.Mesh.Attribute();
				pointer = c3bGetMeshAttribute(ref file, ref new_attribute, ref pointer);
				new_mesh.attributes[mesh_attribute] = new_attribute;
			}

			pointer = c3bGetMeshVertices(ref file, ref new_mesh, ref pointer);

			int num_mesh_parts = System.BitConverter.ToInt32(file, pointer);
			pointer += 4;
			new_mesh.parts = new CocosModel.Mesh.Part[num_mesh_parts];
			for (int part = 0; part < num_mesh_parts; part++)
			{
				CocosModel.Mesh.Part new_part = new CocosModel.Mesh.Part();
				pointer = c3bGetMeshPart(ref file, ref new_part, ref pointer);
				new_mesh.parts[part] = new_part;
			}
			model_C3B.meshes[mesh] = new_mesh;
		}

		int num_materials = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		model_C3B.materials = new CocosModel.Material[num_materials];
		for (int material = 0; material < num_materials; material++)
		{
			CocosModel.Material new_material = new CocosModel.Material();
			pointer = c3bGetMaterial(ref file, ref new_material, ref pointer);
			model_C3B.materials[material] = new_material;
		}

		int num_nodes = System.BitConverter.ToInt32(file, pointer);
		pointer += 4;
		model_C3B.nodes = new CocosModel.Node[num_nodes];
		for (int node = 0; node < num_nodes; node++)
		{
			CocosModel.Node new_node = new CocosModel.Node();
			pointer = c3bGetNode(ref file, ref new_node, ref pointer);
			model_C3B.nodes[node] = new_node;
		}

		List<CocosModel.Animation> animations = new List<CocosModel.Animation>();

		while (pointer < file.Length)
		{
			CocosModel.Animation new_animation = new CocosModel.Animation();
			animations.Add(new_animation);
			pointer = c3bGetAnimation(ref file, ref new_animation, ref pointer);
		}

		if (animations.Count > 0)
		{
			model_C3B.animations = animations.ToArray();
		}

		return model_C3B;
	}

	public static CocosModel loadC3B(string file_name, string root_folder)
	{
		if (file_name.StartsWith("mods/") || file_name.StartsWith("mods\\")){
			root_folder = GlobalEngineVariables.mods_folder;
			file_name = file_name.Substring(5);
        }

		string model_path = Path.Combine(root_folder, file_name);

		if (File.Exists(model_path))
		{
			return readC3B(model_path);
		}
		Debug.LogError("Couldn't find " + model_path);
		return null;
	}
}
