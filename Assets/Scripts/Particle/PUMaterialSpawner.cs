using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using static CocosPU.PUCommon;
using static CocosPU.PUMaterial.Material.Technique.Pass;
using UnityEngine.Assertions;
using System.Globalization;

namespace CocosPU {
    public partial class PUMaterial
    {
        public static UnityEngine.Material loadMaterial(string file)
        {
            Debug.Log("Loading Material " + file);
            string path = Path.Combine(GlobalEngineVariables.assets_folder, "particles", file);
            if (!File.Exists(path))
            {
                Debug.LogError("Particle material does not exist: " + path);
                return null;
            }
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();

            var split = PUCommon.splitFile(content);

            int line_index = 0;

            Material pu_material = parseMaterial(ref line_index, split);

            UnityEngine.Material new_material;
            switch (pu_material.technique.pass.scene_blend[0])
            {
                case "add":
                    new_material = new UnityEngine.Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                    break;
                case "alpha_blend":
                case "src_colour":
                    new_material = new UnityEngine.Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
                    break;
                default:
                    throw new System.Exception("Unknown PUMaterial scene_blend type " + pu_material.technique.pass.scene_blend[0]);
            }


             
            new_material.mainTexture = TextureManager.loadTextureDDS(pu_material.technique.pass.texture_unit.texture[0]);

            return new_material;
        }
    }
}