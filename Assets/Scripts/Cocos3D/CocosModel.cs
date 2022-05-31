using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
[System.Serializable]
public class CocosModel
{
	public string version;
	public string id;

	[System.Serializable]
	public class Mesh
	{
		[System.Serializable]
		public class Attribute
		{
			public int size;
			public string type;
			public string attribute;
		}
		public Attribute[] attributes;
		public float[] vertices;
		public List<Vector3> VERTEX_ATTRIB_POSITION;
		public List<Vector3> VERTEX_ATTRIB_NORMAL;
		public List<Vector3> VERTEX_ATTRIB_SOFT_NORMAL;
		public List<Vector2> VERTEX_ATTRIB_TEX_COORD;
		public List<Vector2> VERTEX_ATTRIB_TEX_COORD1;
		public List<Vector2> VERTEX_ATTRIB_TEX_COORD2;
		public List<Vector2> VERTEX_ATTRIB_TEX_COORD3;
		public List<Vector4> VERTEX_ATTRIB_BLEND_WEIGHT;
		public List<Vector4> VERTEX_ATTRIB_BLEND_INDEX;
		public List<Color> VERTEX_ATTRIB_COLOR;

		[System.Serializable]
		public class Part
		{
			public string id;
			public string type;
			public int[] indices;
			public float[] aabb;
		}
		public Part[] parts;
	}
	public Mesh[] meshes;
	[System.Serializable]
	public class Material
	{
		public string id;
		public float[] ambient;
		public float[] diffuse;
		public float[] emissive;
		public float opacity;
	}
	public Material[] materials;

	[System.Serializable]
	public class Node
	{
		public string id;
		public bool skeleton;
		public float[] transform;
		[System.Serializable]
		public class Part
		{
			public string meshpartid;
			public string materialid;
			[System.Serializable]
			public class Bone
			{
				public string node;
				public float[] transform;
			}
			public Bone[] bones;
		}
		public Part[] parts;
		[SerializeReference]
		public Node[] children;
	}
	public Node[] nodes;
	[System.Serializable]
	public class Animation
	{
		public string id;
		public float length;
		[System.Serializable]
		public class Bone
		{
			public string boneId;
			[System.Serializable]
			public class Keyframe
			{
				public float keytime;
				public float[] rotation;
				public float[] scale;
				public float[] translation;
			}
			public Keyframe[] keyframes;
		}
		public Bone[] bones;
		public Dictionary<string, Bone> bone_dict;
	}
	public Animation[] animations;

	public static CocosModel CreateFromJSON(string file)
	{
		StreamReader reader = new StreamReader(file);
		string content = reader.ReadToEnd();
		reader.Close();
		return JsonConvert.DeserializeObject<CocosModel>(content);
	}
}
