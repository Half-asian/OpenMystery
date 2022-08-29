using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TextureManager
{
	public static Dictionary<string, Texture2D> loaded_textures = new Dictionary<string, Texture2D>();
	public static Texture2D loadTexturePng(string name, string folder = "")
	{
		if (folder.Length < 1) {
			folder = Path.Combine(GlobalEngineVariables.assets_folder, "textures");
		}

		if (!Configs.config_texture.TextureConfig.ContainsKey(name))
		{
			Debug.Log("Not tex config entry for " + name);
			return null;
		}
		string file_name = Configs.config_texture.TextureConfig[name].filename;

		string texture_patch_path = Path.Combine("patches", "textures", file_name.Substring(0, file_name.Length - 11), "@4x.png");
		if (System.IO.File.Exists(texture_patch_path))
        {
			using (FileStream fs = File.Open(texture_patch_path, FileMode.Open))
			{
				byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
				Texture2D new_image = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				new_image.LoadImage(data);
				new_image.name = name;
				return new_image;
			}
		}

		string texture_normal_path = Path.Combine(folder, file_name.Substring(0, file_name.Length - 11), "@4x.png");
		if (System.IO.File.Exists(texture_normal_path))
		{   //Try to find specific file version
			using (FileStream fs = File.Open(texture_normal_path, FileMode.Open))
			{
				byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
				Texture2D new_image = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				new_image.LoadImage(data);
				new_image.name = name;
				return new_image;
			}
		}
		DirectoryInfo dir = new DirectoryInfo(folder);  //If not found, try find any version NOT VERY ACCURATE, if another file has the name in it, it will select wrong file
		//Debug.LogWarning("Couldn't find exact version of " + file_name);
		FileInfo[] info = dir.GetFiles(name + "_v" + "*");
		if (info.Length != 0)
		{
			using (FileStream fs = File.Open(info[0].ToString(), FileMode.Open))
			{
				byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
				Texture2D new_image = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				new_image.LoadImage(data);
				new_image.name = name;
				return new_image;
			}
		}
		Debug.Log("Failed to find texture " + name);

		return null;    //Give up :(
	}
	public static Texture2D loadTextureDDS(string name)
    {
		if (!Configs.config_texture.TextureConfig.ContainsKey(name))
		{
			Debug.Log("Not tex config entry for " + name);
			return null;
		}

		if (loaded_textures.ContainsKey(name))
			return loaded_textures[name];

		string full_path = "";
		string file_name = Configs.config_texture.TextureConfig[name].filename;
		if (file_name.StartsWith("mods/") || file_name.StartsWith("mods\\"))
		{
			string root_folder = GlobalEngineVariables.mods_folder;
			file_name = file_name.Substring(5);
			full_path = root_folder + file_name;
		}
        else
        {
			name = Configs.config_texture.TextureConfig[name].filename.Replace(".compressed", "@4x.dds");
			full_path = Path.Combine(GlobalEngineVariables.assets_folder, "textures", name);
		}

		if (!System.IO.File.Exists(full_path))
		{
			Debug.LogError("Couldn't find tex " + full_path);
			return null;
		}

		byte[] buffer;
		FileStream stream = new FileStream(full_path, FileMode.Open);

		var ddsBytes = new byte[stream.Length];
		stream.Read(ddsBytes, 0, ddsBytes.Length);

		byte ddsSizeCheck = ddsBytes[4];
		if (ddsSizeCheck != 124)
			throw new System.Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

		int height = ddsBytes[13] * 256 + ddsBytes[12];
		int width = ddsBytes[17] * 256 + ddsBytes[16];
		Texture2D tex = new Texture2D(width, height, TextureFormat.BC7, false);

		int DDS_HEADER_SIZE = 148;
		buffer = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
		System.Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, buffer, 0, ddsBytes.Length - DDS_HEADER_SIZE);
		stream.Close();

		tex.LoadRawTextureData(buffer);
		tex.Apply();

        loaded_textures[name] = tex;

		return tex;

	}

    public static void Initialize()
    {
		Scenario.onScenarioLoaded += () => loaded_textures.Clear();
    }

}
