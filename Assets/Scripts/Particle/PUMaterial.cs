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
        public class Material
        {
            public string name;

            public class Technique
            {
                public class Pass
                {
                    public class TextureUnit
                    {
                        public string name;
                        public string texture_alias;
                        public string[] texture;
                        public float max_anisotropy;
                        public string[] filtering;
                        public string tex_address_mode;
                    }
                    public TextureUnit texture_unit;
                    public string lighting;
                    public string[] scene_blend;
                    public string depth_write;
                    public string depth_check;
                    public string[] fog_override;
                }
                public Pass pass;

            }
            public Technique technique;
        }

        public static Material parseMaterial(ref int pointer, string[][] split)
        {
            Material material = new Material();

            material.name = split[pointer][1];
            pointer += 2; //{

            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "technique":
                        pointer++;
                        material.technique = parseTechnique(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown Material variable " + split[pointer][0]);
                }
                pointer++;
            }
            return material;
        }

        private static Material.Technique parseTechnique(ref int pointer, string[][] split)
        {
            Material.Technique technique = new Material.Technique();
            pointer++;//{

            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "pass":
                        technique.pass = parsePass(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown Material.Technique variable " + split[pointer][0]);
                }
                pointer++;
            }
            return technique;
        }

        private static Material.Technique.Pass parsePass(ref int pointer, string[][] split)
        {
            Material.Technique.Pass pass = new Material.Technique.Pass();
            pointer+=2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "lighting":
                        Assert.IsTrue(split[pointer].Length == 2);
                        pass.lighting = split[pointer][1];
                        break;
                    case "scene_blend":
                        Assert.IsTrue(split[pointer].Length <= 3);
                        if (split[pointer].Length < 3)
                            pass.scene_blend = new string[] { split[pointer][1] };
                        else
                            pass.scene_blend = new string[] { split[pointer][1], split[pointer][2] };
                        break;
                    case "depth_write":
                        Assert.IsTrue(split[pointer].Length == 2);
                        if (split[pointer].Length > 2) throw new System.Exception();
                        pass.depth_write = split[pointer][1];
                        break;
                    case "depth_check":
                        Assert.IsTrue(split[pointer].Length == 2);
                        if (split[pointer].Length > 2) throw new System.Exception();
                        pass.depth_check = split[pointer][1];
                        break;
                    case "texture_unit":
                        pass.texture_unit = parseTextureUnit(ref pointer, split);
                        break;
                    case "fog_override":
                        Assert.IsTrue(split[pointer].Length == 3);
                        pass.fog_override = new string[2] {  split[pointer][1], split[pointer][2] };
                        break;

                    default:
                        throw new System.Exception("Unknown Material.Technique.Pass variable " + split[pointer][0]);
                }
                pointer++;
            }
            return pass;
        }
        private static Material.Technique.Pass.TextureUnit parseTextureUnit(ref int pointer, string[][] split)
        {
            Material.Technique.Pass.TextureUnit texture_unit = new Material.Technique.Pass.TextureUnit();
            if (split[pointer].Length > 1)
                texture_unit.name = split[pointer][1];
            pointer+=2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "texture_alias":
                        Assert.IsTrue(split[pointer].Length == 2);
                        texture_unit.texture_alias = split[pointer][1];
                        break;
                    case "texture":
                        Assert.IsTrue(split[pointer].Length <= 3);
                        if (split[pointer].Length < 3)
                            texture_unit.texture = new string[] { split[pointer][1] };
                        else
                            texture_unit.texture = new string[] { split[pointer][1], split[pointer][2] };
                        break;
                    case "max_anisotropy":
                        texture_unit.max_anisotropy = float.Parse(split[pointer][1], CultureInfo.InvariantCulture);
                        break;
                    case "tex_address_mode":
                        Assert.IsTrue(split[pointer].Length == 2);
                        texture_unit.tex_address_mode = split[pointer][1];
                        break;
                    case "filtering":
                        Assert.IsTrue(split[pointer].Length == 4);
                        texture_unit.filtering = new string[3] { split[pointer][1], split[pointer][2], split[pointer][3] };
                        break;
                    default:
                        throw new System.Exception("Unknown Material.Technique.Pass.TextureUnit variable " + split[pointer][0]);
                }
                pointer++;
            }
            return texture_unit;
        }

    }
}