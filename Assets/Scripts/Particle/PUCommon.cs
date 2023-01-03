using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CocosPU
{
    public class PUCommon
    {

        public static string[][] splitFile(string content)
        {
            content = content.Replace("\x0d", "");
            content = content.Replace('\t', ' ');
            var line_split = content.Split('\x0a');
            List<string> line_split_cleaned = new List<string>();
            foreach (var line in line_split)
            {
                if (line.Length != 0)
                    line_split_cleaned.Add(line);
            }

            string[][] split = new string[line_split_cleaned.Count][];
            for (int i = 0; i < line_split_cleaned.Count; i++)
            {
                List<string> temp = new List<string>();

                var words = line_split_cleaned[i].Split(' ');
                foreach (var word in words)
                {
                    if (word != "")
                        temp.Add(word);
                }
                split[i] = temp.ToArray();
            }
            return split;
        }
        public class ValueSingle
        {
            public enum type
            {
                Fixed,
                Random,
                CurvedLinear,
                Oscillate
            }
            public ValueSingle(float value)
            {
                min = value;
            }
            public ValueSingle(float _min, float _max)
            {
                value_type = type.Random;
                min = _min;
                max = _max;
            }

            public type value_type = type.Fixed;
            public float min;
            public float max;
            public float oscillate_frequency;
            public float oscillate_phase;
            public float oscillate_base;
            public float oscillate_amplitude;
            public string oscillate_type;
            public class ControlPoint
            {
                public ControlPoint(float _time, float _value)
                {
                    time = _time;
                    value = _value;
                }

                public float time;
                public float value;
            }
            public List<ControlPoint> controlPoints = new List<ControlPoint>();
        }

        public class ValueTriple
        {
            public ValueTriple(float value1, float value2, float value3)
            {
                x = value1;
                y = value2;
                z = value3;
            }
            public float x;
            public float y;
            public float z;
        }

        public class ValueQuad
        {
            public ValueQuad(float value1, float value2, float value3, float value4)
            {
                x = value1;
                y = value2;
                z = value3;
                w = value4;
            }
            public float x;
            public float y;
            public float z;
            public float w;
        }
        public static ValueSingle parseValue(ref int pointer, string[][] split)
        {
            ValueSingle value;
            if (split[pointer][1] == "dyn_random")
            {
                value = parseDynRandom(ref pointer, split);
            }
            else if (split[pointer][1] == "dyn_curved_linear")
            {
                value = parseDynCurvedLinear(ref pointer, split);
            }
            else if (split[pointer][1] == "dyn_curved_spline")
            {
                value = parseDynCurvedSpline(ref pointer, split);
            }
            else if (split[pointer][1] == "dyn_oscillate")
            {
                value = parseDynOscillate(ref pointer, split);
            }
            else
            {
                value = new ValueSingle(float.Parse(split[pointer][1]));
            }
            return value;
        }
        public static ValueSingle parseDynRandom(ref int pointer, string[][] split)
        {
            pointer += 2; //{
            float min = 0;
            float max = 0;
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "min":
                        min = float.Parse(split[pointer][1]);
                        break;
                    case "max":
                        max = float.Parse(split[pointer][1]);
                        break;
                }
                pointer++;
            }
            return new ValueSingle(min, max);
        }
        public static ValueSingle parseDynCurvedLinear(ref int pointer, string[][] split)
        {
            var value_curved = new ValueSingle(0);
            value_curved.value_type = ValueSingle.type.CurvedLinear;
            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "control_point":
                        float time = float.Parse(split[pointer][1]);
                        float value = float.Parse(split[pointer][2]);
                        value_curved.controlPoints.Add(new ValueSingle.ControlPoint(time, value));
                        break;
                }
                pointer++;
            }
            return value_curved;
        }

        public static ValueSingle parseDynCurvedSpline(ref int pointer, string[][] split)
        {
            var value_curved = new ValueSingle(0);
            value_curved.value_type = ValueSingle.type.CurvedLinear;
            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "control_point":
                        float time = float.Parse(split[pointer][1]);
                        float value = float.Parse(split[pointer][2]);
                        value_curved.controlPoints.Add(new ValueSingle.ControlPoint(time, value));
                        break;
                }
                pointer++;
            }
            return value_curved;
        }

        public static ValueSingle parseDynOscillate(ref int pointer, string[][] split)
        {
            var value_oscillate = new ValueSingle(0);
            value_oscillate.value_type = ValueSingle.type.Oscillate;
            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "oscillate_frequency":
                        value_oscillate.oscillate_frequency = float.Parse(split[pointer][1]);
                        break;
                    case "oscillate_phase":
                        value_oscillate.oscillate_phase = float.Parse(split[pointer][1]);
                        break;
                    case "oscillate_base":
                        value_oscillate.oscillate_base = float.Parse(split[pointer][1]);
                        break;
                    case "oscillate_amplitude":
                        value_oscillate.oscillate_amplitude = float.Parse(split[pointer][1]);
                        break;
                    case "oscillate_type":
                        value_oscillate.oscillate_type = split[pointer][1];
                        break;
                }
                pointer++;
            }
            return value_oscillate;
        }

        public static ValueTriple parseValueTriple(ref int pointer, string[][] split)
        {
            float x = float.Parse(split[pointer][1]);
            float y = float.Parse(split[pointer][2]);
            float z = float.Parse(split[pointer][3]);
            return new ValueTriple(x, y, z);
        }

        public static ValueQuad parseValueQuad(ref int pointer, string[][] split)
        {
            float x = float.Parse(split[pointer][1]);
            float y = float.Parse(split[pointer][2]);
            float z = float.Parse(split[pointer][3]);
            float w = float.Parse(split[pointer][4]);
            return new ValueQuad(x, y, z, w);
        }



    }

    public class PUDynamicAttribute
    {

    }

    public class PUDynamicAttributeHelper
    {

    }

    public class Particle3D
    {

    }
    public class PUParticle3D
    {

    }

    public class ParticleSystem3D
    {

    }
}