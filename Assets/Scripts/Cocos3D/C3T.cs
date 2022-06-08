using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C3T
{
	public static CocosModel loadC3T(string file_name, string root_folder)
	{
		file_name = file_name.Substring(0, file_name.Length - 4) + ".c3t";
		if (System.IO.File.Exists(root_folder + file_name))
		{
			return CocosModel.CreateFromJSON(root_folder + file_name);
		}
		Debug.LogError("Couldn't find " + root_folder + file_name);
		return null;
	}
}
