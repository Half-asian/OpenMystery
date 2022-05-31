using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TextureManager
{
	public static Dictionary<string, Texture2D> loaded_textures = new Dictionary<string, Texture2D>();
	public static Texture2D loadTexturePng(string name, string folder)
	{
		if (!Configs.config_texture.TextureConfig.ContainsKey(name))
		{
			Debug.Log("Not tex config entry for " + name);
			return null;
		}
		string file_name = Configs.config_texture.TextureConfig[name].filename;

		if (System.IO.File.Exists("patches\\textures\\" + file_name.Substring(0, file_name.Length - 11) + "@4x.png"))
        {
			using (FileStream fs = File.Open("patches\\textures\\" + file_name.Substring(0, file_name.Length - 11) + "@4x.png", FileMode.Open))
			{
				byte[] data = new BinaryReader(fs).ReadBytes((int)fs.Length);
				Texture2D new_image = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				new_image.LoadImage(data);
				new_image.name = name;
				return new_image;
			}
		}

		if (System.IO.File.Exists(folder + "\\" + file_name.Substring(0, file_name.Length - 11) + "@4x.png"))
		{   //Try to find specific file version
			using (FileStream fs = File.Open(folder + "\\" + file_name.Substring(0, file_name.Length - 11) + "@4x.png", FileMode.Open))
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
		name = Configs.config_texture.TextureConfig[name].filename.Replace(".compressed", "@4x.dds");


		if (System.IO.File.Exists(GlobalEngineVariables.assets_folder + "\\textures\\" + name)){

			byte[] buffer;
			FileStream stream = new FileStream(GlobalEngineVariables.assets_folder + "\\textures\\" + name, FileMode.Open);

			var ddsBytes = new byte[stream.Length];
			stream.Read(ddsBytes, 0, ddsBytes.Length);

			byte ddsSizeCheck = ddsBytes[4];
			if (ddsSizeCheck != 124)
				throw new System.Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

			int height = ddsBytes[13] * 256 + ddsBytes[12];
			int width = ddsBytes[17] * 256 + ddsBytes[16];
			Texture2D tex = new Texture2D(width, height, TextureFormat.BC7, true);

			int DDS_HEADER_SIZE = 148;
			buffer = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
			System.Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, buffer, 0, ddsBytes.Length - DDS_HEADER_SIZE);
			stream.Close();

			tex.LoadRawTextureData(buffer);
			tex.Apply();
			return tex;
		}
        else
        {
			Debug.LogError("Couldn't find tex " + GlobalEngineVariables.assets_folder + "\\textures\\" + name);
			Log.write("Couldn't find tex" + GlobalEngineVariables.assets_folder + "\\textures\\" + name, "error");
			return null;
        }
	}

}
