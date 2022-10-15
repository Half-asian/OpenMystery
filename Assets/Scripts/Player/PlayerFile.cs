using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class PlayerFile
{
    public string character_id;
    public string gender;

    public class CustomizationCategory
    {
        public string component_id;
        public Dictionary<string, float> float_parameters;
        public Dictionary<string, int> int_parameters;
    }
    public Dictionary<string, CustomizationCategory> customization_categories;

    public static PlayerFile createFromJson(string file)
    {

        byte[] byte_array = File.ReadAllBytes(file);

        if ((char)byte_array[0] != '{')
        {
            ConfigDecrypt.decrypt(byte_array, file);
        }

        string content = Encoding.UTF8.GetString(byte_array);

        PlayerFile playerfile = JsonConvert.DeserializeObject<PlayerFile>(content);
        
        return playerfile;
    }

    public static void save()
    {
        string json_file = JsonConvert.SerializeObject(PlayerManager.current);
        File.WriteAllText(Path.Combine(GlobalEngineVariables.player_folder, "Avatar.json"), json_file);
    }

}
