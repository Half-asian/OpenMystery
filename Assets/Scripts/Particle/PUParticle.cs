using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using static PUParticle.System.Technique;

public class PUParticle
{
    public class ValueSingle
    {
        public enum type
        {
            Fixed,
            Random,
            CurvedLinear
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

    public class System
    {
        public System()
        {
            techniques = new Dictionary<string, Technique>();
        }

        public string name;
        public float iteration_interval;
        public float[] fast_forward;

        public class Technique
        {
            public Technique()
            {
                emitters = new List<Emitter>();
                affectors = new List<Affector>();
            }

            public string name;
            public ValueSingle visual_particle_quota = new ValueSingle(1000);
            public ValueSingle emitted_emitter_quota; //unused
            public ValueSingle default_particle_width = new ValueSingle(1); //Tested - no effect
            public ValueSingle default_particle_height = new ValueSingle(1); //Tested - no effect
            public ValueSingle default_particle_depth = new ValueSingle(1); //Tested - no effect
            public ValueSingle spatial_hashtable_size; //unused
            public ValueSingle max_velocity;
            public string material;

            public class Renderer
            {
                public string type;
                public ValueSingle max_elements;
                /*Ribbontrail*/
                public ValueSingle ribbontrail_length;
                public ValueSingle ribbontrail_width;
                public bool random_initial_colour;
                public ValueQuad initial_colour;
                public ValueQuad colour_change;

            }
            public Renderer renderer;
            public class Emitter
            {
                public string type;
                public string name;
                public string emits_type;
                public string emits_id;
                public ValueSingle emission_rate;
                public ValueSingle angle; //Can't figure out what this does
                public ValueSingle duration;
                public ValueSingle particle_width;
                public ValueSingle particle_height;
                public ValueSingle radius;
                //public Value direction;
                public ValueSingle time_to_live;
                public ValueSingle mass;
                public ValueSingle velocity;
                public ValueSingle all_particle_dimensions;
                public ValueSingle repeat_delay;
                public ValueQuad start_colour_range;
                public ValueQuad end_colour_range;
                public ValueQuad colour;
                public bool auto_direction;
            }
            public List<Emitter> emitters;
            public class Affector
            {
                public Affector()
                {
                    timeColours = new List<TimeColour>();
                }

                public string type;
                public string name;
                public ValueSingle rotation;
                public ValueTriple rotation_axis;
                public ValueSingle rotation_speed;
                public string exclude_emitter;
                /*LinearForce*/
                public ValueTriple force_vector;
                /*Color*/
                public class TimeColour
                {
                    public TimeColour(float _time, float _r, float _g, float _b, float _a)
                    {
                        time = _time;
                        r = _r;
                        g = _g;
                        b = _b;
                        a = _a;
                    }
                    public float time;
                    public float r;
                    public float g;
                    public float b;
                    public float a;
                }
                public List<TimeColour> timeColours;
                /*Scale*/
                public ValueSingle xyz_scale;
                /*TextureAnimator*/
                public ValueSingle end_texture_coords_range;
                public bool texture_start_random;
            }
            public List<Affector> affectors;
            public class Behaviour
            {
                public string type;
            }
            public Behaviour behaviour;
        }
        public Dictionary<string, Technique> techniques;
    }

    public static System loadParticle(string file)
    {
        StreamReader reader = new StreamReader(file);
        string content = reader.ReadToEnd();
        reader.Close();
        content = content.Replace('\x0d', ' ');
        content = content.Replace('\x0a', ' ');
        content = Regex.Replace(content, @"\s+", " ");

        var split = content.Split(' ');

        //First word is System
        int pointer = 1;

        return parseSystem(ref pointer, split);
    }

    private static System parseSystem(ref int pointer, string[] split)
    {
        System system = new System();
        system.name = split[pointer];
        pointer += 2; //{
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "iteration_interval":
                    pointer++;
                    system.iteration_interval = float.Parse(split[pointer]);
                    break;
                case "fast_forward":
                    system.fast_forward = new float[2];
                    pointer++;
                    system.fast_forward[0] = float.Parse(split[pointer]);
                    pointer++;
                    system.fast_forward[1] = float.Parse(split[pointer]);
                    break;
                case "technique":
                    pointer++;
                    var technique = parseTechnique(ref pointer, split);
                    system.techniques.Add(technique.name, technique);
                    break;
            }
            pointer++;
        }
        return system;
    }
    private static System.Technique parseTechnique(ref int pointer, string[] split)
    {
        System.Technique technique = new System.Technique();
        if (split[pointer] != "{")
        {
            technique.name = split[pointer];
            pointer++;
        }
        else
        {
            technique.name = "unnamed technique";
        }
        pointer ++;//{
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "renderer":
                    pointer++;
                    technique.renderer = parseRenderer(ref pointer, split);
                    break;
                case "emitter":
                    pointer++;
                    var emitter = parseEmitter(ref pointer, split);
                    technique.emitters.Add(emitter);
                    break;
                case "affector":
                    pointer++;
                    var affector = parseAffector(ref pointer, split);
                    technique.affectors.Add(affector);
                    break;
                case "behaviour":
                    pointer++;
                    technique.behaviour = parseBehaviour(ref pointer, split);
                    break;
                case "visual_particle_quota":
                    pointer++;
                    technique.visual_particle_quota = parseValue(ref pointer, split);
                    break;
                case "emitted_emitter_quota":
                    pointer++;
                    technique.emitted_emitter_quota = parseValue(ref pointer, split);
                    break;
                case "default_particle_width":
                    pointer++;
                    technique.default_particle_width = parseValue(ref pointer, split);
                    break;
                case "default_particle_height":
                    pointer++;
                    technique.default_particle_height = parseValue(ref pointer, split);
                    break;
                case "default_particle_depth":
                    pointer++;
                    technique.default_particle_depth = parseValue(ref pointer, split);
                    break;
                case "spatial_hashtable_size":
                    pointer++;
                    technique.spatial_hashtable_size = parseValue(ref pointer, split);
                    break;
                case "max_velocity":
                    pointer++;
                    technique.max_velocity = parseValue(ref pointer, split);
                    break;
                case "material":
                    pointer++;
                    technique.material = split[pointer];
                    break;
            }
            pointer++;
        }
        return technique;
    }
    private static System.Technique.Renderer parseRenderer(ref int pointer, string[] split)
    {
        System.Technique.Renderer renderer = new System.Technique.Renderer();
        renderer.type = split[pointer];
        pointer++;
        pointer++; //{
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "max_elements":
                    pointer++;
                    renderer.max_elements = parseValue(ref pointer, split);
                    break;
                case "ribbontrail_length":
                    pointer++;
                    renderer.ribbontrail_length = parseValue(ref pointer, split);
                    break;
                case "ribbontrail_width":
                    pointer++;
                    renderer.ribbontrail_width = parseValue(ref pointer, split);
                    break;
                case "random_initial_colour":
                    pointer++;
                    renderer.random_initial_colour = bool.Parse(split[pointer]);
                    break;
                case "initial_colour":
                    pointer++;
                    renderer.initial_colour = parseValueQuad(ref pointer, split);
                    break;
                case "colour_change":
                    pointer++;
                    renderer.colour_change = parseValueQuad(ref pointer, split);
                    break;
            }
            pointer++;
        }
        return renderer;
    }
    private static System.Technique.Emitter parseEmitter(ref int pointer, string[] split)
    {
        System.Technique.Emitter emitter = new System.Technique.Emitter();
        emitter.type = split[pointer];
        pointer++;
        if (split[pointer] != "{")
        {
            emitter.name = split[pointer];
            pointer++;
        }
        else
        {
            emitter.name = "unnamed emitter";
        }
        pointer++; //{

        Debug.Log("Loading emitter " + emitter.name);


        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "time_to_live":
                    pointer++;
                    emitter.time_to_live = parseValue(ref pointer, split);
                    break;
                case "mass":
                    pointer++;
                    emitter.mass = parseValue(ref pointer, split);
                    break;
                case "velocity":
                    pointer++;
                    emitter.velocity = parseValue(ref pointer, split);
                    break;
                case "all_particle_dimensions":
                    pointer++;
                    emitter.all_particle_dimensions = parseValue(ref pointer, split);
                    break;
                case "emission_rate":
                    pointer++;
                    emitter.emission_rate = parseValue(ref pointer, split);
                    break;
                case "angle":
                    pointer++;
                    emitter.angle = parseValue(ref pointer, split);
                    break;
                case "duration":
                    pointer++;
                    emitter.duration = parseValue(ref pointer, split);
                    break;
                case "particle_width":
                    pointer++;
                    emitter.particle_width = parseValue(ref pointer, split);
                    break;
                case "particle_height":
                    pointer++;
                    emitter.particle_height = parseValue(ref pointer, split);
                    break;
                case "radius":
                    pointer++;
                    emitter.radius = parseValue(ref pointer, split);
                    break;
                case "start_colour_range":
                    pointer++;
                    emitter.start_colour_range = parseValueQuad(ref pointer, split);
                    break;
                case "end_colour_range":
                    pointer++;
                    emitter.end_colour_range = parseValueQuad(ref pointer, split);
                    break;
                case "colour":
                    pointer++;
                    emitter.colour = parseValueQuad(ref pointer, split);
                    break;
                case "auto_direction":
                    pointer++;
                    emitter.auto_direction = bool.Parse(split[pointer]);
                    break;
                case "repeat_delay":
                    pointer++;
                    emitter.repeat_delay = parseValue(ref pointer, split);
                    break;
                case "emits":
                    Debug.Log("EMITS");
                    pointer++;
                    emitter.emits_type = split[pointer];
                    pointer++;
                    emitter.emits_id = split[pointer];
                    break;
            }
            pointer++;
        }
        return emitter;
    }
    private static System.Technique.Affector parseAffector(ref int pointer, string[] split)
    {
        System.Technique.Affector affector = new System.Technique.Affector();
        affector.type = split[pointer];
        pointer++;
        if (split[pointer] != "{")
        {
            affector.name = split[pointer];
            pointer++;
        }
        else
        {
            affector.name = "unnamed affector";
        }
        pointer ++; //{
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "exclude_emitter":
                    pointer++;
                    affector.exclude_emitter = split[pointer];
                    break;
                case "rotation":
                    pointer++;
                    affector.rotation = parseValue(ref pointer, split);
                    break;
                case "rotation_axis":
                    pointer++;
                    affector.rotation_axis = parseValueTriple(ref pointer, split);
                    break;
                case "rotation_speed":
                    pointer++;
                    affector.rotation_speed = parseValue(ref pointer, split);
                    break;
                case "xyz_scale":
                    pointer++;
                    affector.xyz_scale = parseValue(ref pointer, split);
                    break;
                case "force_vector":
                    pointer++;
                    affector.force_vector = parseValueTriple(ref pointer, split);
                    break;
                case "time_colour":
                    pointer++;
                    affector.timeColours.Add(parseTimeColour(ref pointer, split));
                    break;
                case "end_texture_coords_range":
                    pointer++;
                    affector.end_texture_coords_range = parseValue(ref pointer, split);
                    break;
                case "texture_start_random":
                    pointer++;
                    affector.texture_start_random = bool.Parse(split[pointer]);
                    break;
            }
            pointer++;
        }
        return affector;
    }
    private static System.Technique.Behaviour parseBehaviour(ref int pointer, string[] split)
    {
        System.Technique.Behaviour behaviour = new System.Technique.Behaviour();
        behaviour.type = split[pointer];
        pointer += 2; //{
        while (split[pointer] != "}")
        {
            pointer++;
        }

        return behaviour;
    }
    private static ValueSingle parseValue(ref int pointer, string[] split)
    {
        ValueSingle value;
        if (split[pointer] == "dyn_random")
        {
            pointer++;
            value = parseDynRandom(ref pointer, split);
        }
        else if (split[pointer] == "dyn_curved_linear")
        {
            pointer++;
            value = parseDynCurvedLinear(ref pointer, split);
        }
        else
        {
            value = new ValueSingle(float.Parse(split[pointer]));
        }
        return value;
    }
    private static ValueSingle parseDynRandom(ref int pointer, string[] split)
    {
        pointer += 1; //{
        float min = 0;
        float max = 0;
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "min":
                    pointer++;
                    min = float.Parse(split[pointer]);
                    break;
                case "max":
                    pointer++;
                    max = float.Parse(split[pointer]);
                    break;
            }
            pointer++;
        }
        return new ValueSingle(min, max);
    }
    private static ValueSingle parseDynCurvedLinear(ref int pointer, string[] split)
    {
        Debug.Log("parseDynCurvedLinear");
        var value_curved = new ValueSingle(0);
        value_curved.value_type = ValueSingle.type.CurvedLinear;
        pointer += 1; //{
        while (split[pointer] != "}")
        {
            switch (split[pointer])
            {
                case "control_point":
                    pointer++;
                    float time = float.Parse(split[pointer]);
                    pointer++;
                    float value = float.Parse(split[pointer]);
                    value_curved.controlPoints.Add(new ValueSingle.ControlPoint(time, value));
                    break;
            }
            pointer++;
        }
        return value_curved;
    }

    private static ValueTriple parseValueTriple(ref int pointer, string[] split)
    {
        float x = float.Parse(split[pointer]);
        pointer++;
        float y = float.Parse(split[pointer]);
        pointer++;
        float z = float.Parse(split[pointer]);
        return new ValueTriple(x, y, z);
    }

    private static ValueQuad parseValueQuad(ref int pointer, string[] split)
    {
        float x = float.Parse(split[pointer]);
        pointer++;
        float y = float.Parse(split[pointer]);
        pointer++;
        float z = float.Parse(split[pointer]);
        pointer++;
        float w = float.Parse(split[pointer]);
        return new ValueQuad(x, y, z, w);
    }

    private static System.Technique.Affector.TimeColour parseTimeColour(ref int pointer, string[] split)
    {
        float time = float.Parse(split[pointer]);
        pointer++;
        float r = float.Parse(split[pointer]);
        pointer++;
        float g = float.Parse(split[pointer]);
        pointer++;
        float b = float.Parse(split[pointer]);
        pointer++;
        float a = float.Parse(split[pointer]);
        return new System.Technique.Affector.TimeColour(time, r, g, b, a);
    }
}
