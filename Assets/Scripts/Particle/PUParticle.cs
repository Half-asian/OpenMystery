using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using static CocosPU.PUCommon;
using static CocosPU.PUParticle.PUSystem.Technique;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace CocosPU
{
    public partial class PUParticle
    {
        public partial class PUSystem
        {
            public PUSystem()
            {
                techniques = new Dictionary<string, Technique>();
            }

            public string name;
            public string category;
            public float iteration_interval;
            public float[] fast_forward;
            public ValueTriple scale;

            public class Technique
            {
                public Technique()
                {
                    emitters = new List<Emitter>();
                    affectors = new List<Affector>();
                    externs = new List<Extern>();
                }

                public string name;
                public ValueSingle visual_particle_quota = new ValueSingle(1000);
                public ValueSingle emitted_system_quota;
                public ValueSingle emitted_technique_quota;
                public ValueSingle emitted_emitter_quota;
                public ValueSingle emitted_affector_quota;
                public ValueSingle default_particle_width = new ValueSingle(1); //Tested - no effect
                public ValueSingle default_particle_height = new ValueSingle(1); //Tested - no effect
                public ValueSingle default_particle_depth = new ValueSingle(1); //Tested - no effect
                public ValueSingle spatial_hashtable_size; //unused
                public ValueSingle max_velocity;
                public ValueSingle lod_index;
                public string material;
                public ValueTriple position;
                public bool enabled;
                public bool keep_local;

                public class Renderer
                {
                    public string type;
                    public ValueSingle max_elements;
                    //Ribbontrail
                    public ValueSingle ribbontrail_length;
                    public ValueSingle ribbontrail_width;
                    public bool random_initial_colour;
                    public bool sorting;
                    public bool accurate_facing;
                    public bool use_vertex_colours;
                    public bool use_soft_particles;
                    public ValueQuad initial_colour;
                    public ValueQuad colour_change;
                    public ValueSingle texture_coords_rows;
                    public ValueSingle texture_coords_columns;
                    public ValueSingle render_queue_group;
                    public ValueSingle soft_particles_contrast_power;
                    public ValueSingle soft_particles_scale;
                    public ValueSingle soft_particles_delta;
                    public string billboard_rotation_type;
                    public string billboard_type;
                    public string billboard_origin;
                    public ValueTriple common_direction;

                }
                public Renderer renderer;
                public partial class Emitter
                {
                    public string type;
                    public string name;
                    public string emits_type;
                    public string emits_id;
                    public string master_technique_name; //Slave Emitter
                    public string master_emitter_name; //Slave Emitter
                    public string mesh_name; //?
                    public bool auto_direction; 
                    public bool emit_random;
                    public bool keep_local;
                    public float? box_width; //Box Emitter
                    public float? box_height; //Box Emitter
                    public float? box_depth; //Box Emitter
                    public float? radius; //Circle, Sphere Emitter
                    public ValueSingle angle; //Circle
                    public ValueSingle emission_rate;
                    public ValueSingle duration;
                    public ValueSingle particle_width;
                    public ValueSingle particle_height;
                    public ValueSingle time_to_live;
                    public ValueSingle mass;
                    public ValueSingle velocity;
                    public ValueSingle all_particle_dimensions;
                    public ValueSingle repeat_delay;
                    public ValueSingle start_texture_coords_range;
                    public ValueSingle end_texture_coords_range;
                    public ValueSingle step;
                    public ValueSingle max_deviation;
                    public ValueSingle min_increment;
                    public ValueSingle max_increment;
                    public ValueTriple direction;
                    public ValueTriple position;
                    public ValueTriple normal;
                    public ValueTriple end;
                    public ValueQuad start_colour_range;
                    public ValueQuad end_colour_range;
                    public ValueQuad colour;
                    public ValueQuad start_orientation_range;
                    public ValueQuad end_orientation_range;
                    public ValueQuad orientation;
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
                    //LinearForce
                    public ValueTriple force_vector;
                    public ValueTriple position;
                    //Color
                    //Colour
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
                    public string colour_operation;
                    //Scale
                    public ValueSingle xyz_scale;
                    public bool since_start_system;
                    public ValueSingle x_scale;
                    public ValueSingle y_scale;
                    //TextureAnimator
                    public ValueSingle end_texture_coords_range;
                    public bool texture_start_random;
                    public ValueSingle start_texture_coords_range;
                    public string texture_animation_type;
                    //Jet
                    public ValueSingle acceleration;
                    //ScaleVelocity
                    public ValueSingle velocity_scale;
                    public bool stop_at_flip;
                    public ValueSingle mass_affector;
                    //Randomiser
                    public ValueSingle max_deviation_x;
                    public ValueSingle max_deviation_y;
                    public ValueSingle max_deviation_z;
                    public ValueSingle time_step;
                    //TextureRotator
                    public bool use_own_rotation;
                    //ForceField
                    public ValueSingle delta;
                    public ValueSingle force;
                    public ValueSingle octaves;
                    public ValueSingle frequency;
                    public ValueSingle forcefield_size;
                    public ValueSingle movement_frequency;
                    public ValueSingle amplitude;
                    public ValueSingle persistence;
                    public bool ignore_negative_y;
                    public bool ignore_negative_z;
                    public ValueTriple movement;
                    public ValueTriple worldsize;
                    public string forcefield_type;
                    //Gravity
                    public ValueSingle gravity;
                    public bool enabled;
                    //SineForce
                    public ValueSingle min_frequency;
                    public ValueSingle max_frequency;
                    //VelocityMatching
                    public ValueSingle radius;
                    //BoxCollider
                    public ValueSingle box_width;
                    public ValueSingle box_height;
                    public ValueSingle box_depth;
                }
                public List<Affector> affectors;
                public class Behaviour
                {
                    public string type;
                }
                public Behaviour behaviour;

                public class Extern
                {
                    public string name;
                    public string type;
                    public ValueSingle box_width;
                    public string collision_intersection;
                }
                public List<Extern> externs;

            }
            public Dictionary<string, Technique> techniques;
        }

        public static PUSystem loadParticle(string file)
        {
            string path = Path.Combine(GlobalEngineVariables.assets_folder, "particles", file);
            if (!File.Exists(path))
            {
                Debug.LogError("Particle file does not exist: " + path);
                return null;
            }
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();


            var split = PUCommon.splitFile(content);

            int pointer = 0;
            var system = parseSystem(ref pointer, split);
            return system;
        }

        private static PUSystem parseSystem(ref int pointer, string[][] split)
        {
            PUSystem system = new PUSystem();
            system.name = split[pointer][1];
            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "iteration_interval":
                        system.iteration_interval = float.Parse(split[pointer][1]);
                        break;
                    case "fast_forward":
                        system.fast_forward = new float[2] { float.Parse(split[pointer][1]), float.Parse(split[pointer][2]) };
                        break;
                    case "technique":
                        var technique = parseTechnique(ref pointer, split);
                        if (technique.name != null)
                            system.techniques.Add(technique.name, technique);
                        break;
                    case "category":
                        system.category = split[pointer][1];
                        break;
                    case "scale":
                        system.scale = parseValueTriple(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown System parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return system;
        }
        
        private static PUSystem.Technique parseTechnique(ref int pointer, string[][] split)
        {
            PUSystem.Technique technique = new PUSystem.Technique();
            if (split[pointer].Length > 1)
                technique.name = split[pointer][1];
            pointer+=2;//{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "renderer":
                        technique.renderer = parseRenderer(ref pointer, split);
                        break;
                    case "emitter":
                        technique.emitters.Add(parseEmitter(ref pointer, split));
                        break;
                    case "affector":
                        technique.affectors.Add(parseAffector(ref pointer, split));
                        break;
                    case "behaviour":
                        technique.behaviour = parseBehaviour(ref pointer, split);
                        break;
                    case "extern":
                        technique.externs.Add(parseExtern(ref pointer, split));
                        break;
                    case "visual_particle_quota":
                        technique.visual_particle_quota = parseValue(ref pointer, split);
                        break;
                    case "emitted_system_quota":
                        technique.emitted_system_quota = parseValue(ref pointer, split);
                        break;
                    case "emitted_technique_quota":
                        technique.emitted_technique_quota = parseValue(ref pointer, split);
                        break;
                    case "emitted_emitter_quota":
                        technique.emitted_emitter_quota = parseValue(ref pointer, split);
                        break;
                    case "emitted_affector_quota":
                        technique.emitted_affector_quota = parseValue(ref pointer, split);
                        break;
                    case "default_particle_width":
                        technique.default_particle_width = parseValue(ref pointer, split);
                        break;
                    case "default_particle_height":
                        technique.default_particle_height = parseValue(ref pointer, split);
                        break;
                    case "default_particle_depth":
                        technique.default_particle_depth = parseValue(ref pointer, split);
                        break;
                    case "spatial_hashtable_size":
                        technique.spatial_hashtable_size = parseValue(ref pointer, split);
                        break;
                    case "max_velocity":
                        technique.max_velocity = parseValue(ref pointer, split);
                        break;
                    case "lod_index":
                        technique.lod_index = parseValue(ref pointer, split);
                        break;
                    case "material":
                        technique.material = split[pointer][1];
                        break;
                    case "position":
                        technique.position = parseValueTriple(ref pointer, split);
                        break;
                    case "enabled":
                        technique.enabled = bool.Parse(split[pointer][1]);
                        break;
                    case "keep_local":
                        technique.keep_local = bool.Parse(split[pointer][1]);
                        break;

                    default:
                        throw new System.Exception("Unknown Technique parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return technique;
        }

        private static PUSystem.Technique.Renderer parseRenderer(ref int pointer, string[][] split)
        {
            PUSystem.Technique.Renderer renderer = new PUSystem.Technique.Renderer();
            renderer.type = split[pointer][1];
            pointer+=2;//{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "max_elements":
                        renderer.max_elements = parseValue(ref pointer, split);
                        break;
                    case "ribbontrail_length":
                        renderer.ribbontrail_length = parseValue(ref pointer, split);
                        break;
                    case "ribbontrail_width":
                        renderer.ribbontrail_width = parseValue(ref pointer, split);
                        break;
                    case "random_initial_colour":
                        renderer.random_initial_colour = bool.Parse(split[pointer][1]);
                        break;
                    case "sorting":
                        renderer.sorting = bool.Parse(split[pointer][1]);
                        break;
                    case "accurate_facing":
                        renderer.accurate_facing = bool.Parse(split[pointer][1]);
                        break;
                    case "use_vertex_colours":
                        renderer.use_vertex_colours = bool.Parse(split[pointer][1]);
                        break;
                    case "use_soft_particles":
                        renderer.use_soft_particles = bool.Parse(split[pointer][1]);
                        break;
                    case "initial_colour":
                        renderer.initial_colour = parseValueQuad(ref pointer, split);
                        break;
                    case "colour_change":
                        renderer.colour_change = parseValueQuad(ref pointer, split);
                        break;
                    case "texture_coords_rows":
                        renderer.texture_coords_rows = parseValue(ref pointer, split);
                        break;
                    case "texture_coords_columns":
                        renderer.texture_coords_columns = parseValue(ref pointer, split);
                        break;
                    case "render_queue_group":
                        renderer.render_queue_group = parseValue(ref pointer, split);
                        break;
                    case "soft_particles_contrast_power":
                        renderer.soft_particles_contrast_power = parseValue(ref pointer, split);
                        break;
                    case "soft_particles_scale":
                        renderer.soft_particles_scale = parseValue(ref pointer, split);
                        break;
                    case "soft_particles_delta":
                        renderer.soft_particles_delta = parseValue(ref pointer, split);
                        break;
                    case "billboard_rotation_type":
                        renderer.billboard_rotation_type = split[pointer][1];
                        break;
                    case "billboard_type":
                        renderer.billboard_type = split[pointer][1];
                        break;
                    case "billboard_origin":
                        renderer.billboard_origin = split[pointer][1];
                        break;
                    case "common_direction":
                        renderer.common_direction = parseValueTriple(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown Renderer parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return renderer;
        }
        private static PUSystem.Technique.Emitter parseEmitter(ref int pointer, string[][] split)
        {
            PUSystem.Technique.Emitter emitter = new PUSystem.Technique.Emitter();
            emitter.type = split[pointer][1];
            if (split[pointer].Length > 2)
                emitter.name = split[pointer][2];
            pointer+=2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "time_to_live":
                        emitter.time_to_live = parseValue(ref pointer, split);
                        break;
                    case "mass":
                        emitter.mass = parseValue(ref pointer, split);
                        break;
                    case "velocity":
                        emitter.velocity = parseValue(ref pointer, split);
                        break;
                    case "all_particle_dimensions":
                        emitter.all_particle_dimensions = parseValue(ref pointer, split);
                        break;
                    case "emission_rate":
                        emitter.emission_rate = parseValue(ref pointer, split);
                        break;
                    case "angle":
                        emitter.angle = parseValue(ref pointer, split);
                        break;
                    case "duration":
                        emitter.duration = parseValue(ref pointer, split);
                        break;
                    case "particle_width":
                        emitter.particle_width = parseValue(ref pointer, split);
                        break;
                    case "particle_height":
                        emitter.particle_height = parseValue(ref pointer, split);
                        break;
                    case "radius":
                        emitter.radius = float.Parse(split[pointer][1], CultureInfo.InvariantCulture);
                        break;
                    case "start_colour_range":
                        emitter.start_colour_range = parseValueQuad(ref pointer, split);
                        break;
                    case "end_colour_range":
                        emitter.end_colour_range = parseValueQuad(ref pointer, split);
                        break;
                    case "colour":
                        emitter.colour = parseValueQuad(ref pointer, split);
                        break;
                    case "auto_direction":
                        emitter.auto_direction = bool.Parse(split[pointer][1]);
                        break;
                    case "emit_random":
                        emitter.emit_random = bool.Parse(split[pointer][1]);
                        break;
                    case "keep_local":
                        emitter.keep_local = bool.Parse(split[pointer][1]);
                        break;
                    case "repeat_delay":
                        emitter.repeat_delay = parseValue(ref pointer, split);
                        break;
                    case "emits":
                        emitter.emits_type = split[pointer][1];
                        emitter.emits_id = split[pointer][2];
                        break;
                    case "box_width":
                        emitter.box_width = float.Parse(split[pointer][1], CultureInfo.InvariantCulture);
                        break;
                    case "box_height":
                        emitter.box_height = float.Parse(split[pointer][1], CultureInfo.InvariantCulture);
                        break;
                    case "box_depth":
                        emitter.box_depth = float.Parse(split[pointer][1], CultureInfo.InvariantCulture);
                        break;
                    case "start_texture_coords_range":
                        emitter.end_texture_coords_range = parseValue(ref pointer, split);
                        break;
                    case "end_texture_coords_range":
                        emitter.end_texture_coords_range = parseValue(ref pointer, split);
                        break;
                    case "step":
                        emitter.step = parseValue(ref pointer, split);
                        break;
                    case "max_deviation":
                        emitter.max_deviation = parseValue(ref pointer, split);
                        break;
                    case "min_increment":
                        emitter.min_increment = parseValue(ref pointer, split);
                        break;
                    case "max_increment":
                        emitter.max_increment = parseValue(ref pointer, split);
                        break;
                    case "direction":
                        emitter.direction = parseValueTriple(ref pointer, split);
                        break;
                    case "position":
                        emitter.position = parseValueTriple(ref pointer, split);
                        break;
                    case "normal":
                        emitter.normal = parseValueTriple(ref pointer, split);
                        break;
                    case "end":
                        emitter.end = parseValueTriple(ref pointer, split);
                        break;
                    case "master_technique_name":
                        emitter.master_technique_name = split[pointer][1];
                        break;
                    case "master_emitter_name":
                        emitter.master_emitter_name = split[pointer][1];
                        break;
                    case "mesh_name":
                        emitter.mesh_name = split[pointer][1];
                        break;
                    case "start_orientation_range":
                        emitter.start_orientation_range = parseValueQuad(ref pointer, split);
                        break;
                    case "end_orientation_range":
                        emitter.end_orientation_range = parseValueQuad(ref pointer, split);
                        break;
                    case "orientation":
                        emitter.orientation = parseValueQuad(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown Emitter parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return emitter;
        }
        private static PUSystem.Technique.Affector parseAffector(ref int pointer, string[][] split)
        {
            PUSystem.Technique.Affector affector = new PUSystem.Technique.Affector();
            affector.type = split[pointer][1];
            if (split[pointer].Length > 2)
                affector.name = split[pointer][2];

            pointer+=2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "exclude_emitter":
                        affector.exclude_emitter = split[pointer][1];
                        break;
                    case "rotation":
                        affector.rotation = parseValue(ref pointer, split);
                        break;
                    case "rotation_axis":
                        affector.rotation_axis = parseValueTriple(ref pointer, split);
                        break;
                    case "rotation_speed":
                        affector.rotation_speed = parseValue(ref pointer, split);
                        break;
                    case "use_own_rotation":
                        affector.use_own_rotation = bool.Parse(split[pointer][1]);
                        break;
                    case "xyz_scale":
                        affector.xyz_scale = parseValue(ref pointer, split);
                        break;
                    case "x_scale":
                        affector.x_scale = parseValue(ref pointer, split);
                        break;
                    case "y_scale":
                        affector.y_scale = parseValue(ref pointer, split);
                        break;
                    case "since_start_system":
                        affector.since_start_system = bool.Parse(split[pointer][1]);
                        break;
                    case "force_vector":
                        affector.force_vector = parseValueTriple(ref pointer, split);
                        break;
                    case "position":
                        affector.position = parseValueTriple(ref pointer, split);
                        break;
                    case "time_colour":
                        affector.timeColours.Add(parseTimeColour(ref pointer, split));
                        break;
                    case "colour_operation":
                        affector.colour_operation = split[pointer][1];
                        break;
                    case "start_texture_coords_range":
                        affector.start_texture_coords_range = parseValue(ref pointer, split);
                        break;
                    case "end_texture_coords_range":
                        affector.end_texture_coords_range = parseValue(ref pointer, split);
                        break;
                    case "texture_start_random":
                        affector.texture_start_random = bool.Parse(split[pointer][1]);
                        break;
                    case "texture_animation_type":
                        affector.texture_animation_type = split[pointer][1];
                        break;
                    case "acceleration":
                        affector.acceleration = parseValue(ref pointer, split);
                        break;
                    case "velocity_scale":
                        affector.velocity_scale = parseValue(ref pointer, split);
                        break;
                    case "stop_at_flip":
                        affector.stop_at_flip = bool.Parse(split[pointer][1]);
                        break;
                    case "mass_affector":
                        affector.mass_affector = parseValue(ref pointer, split);
                        break;
                    case "max_deviation_x":
                        affector.max_deviation_x = parseValue(ref pointer, split);
                        break;
                    case "max_deviation_y":
                        affector.max_deviation_y = parseValue(ref pointer, split);
                        break;
                    case "max_deviation_z":
                        affector.max_deviation_z = parseValue(ref pointer, split);
                        break;
                    case "time_step":
                        affector.time_step = parseValue(ref pointer, split);
                        break;
                    case "delta":
                        affector.delta = parseValue(ref pointer, split);
                        break;
                    case "force":
                        affector.force = parseValue(ref pointer, split);
                        break;
                    case "octaves":
                        affector.octaves = parseValue(ref pointer, split);
                        break;
                    case "frequency":
                        affector.frequency = parseValue(ref pointer, split);
                        break;
                    case "amplitude":
                        affector.amplitude = parseValue(ref pointer, split);
                        break;
                    case "persistence":
                        affector.persistence = parseValue(ref pointer, split);
                        break;
                    case "forcefield_size":
                        affector.forcefield_size = parseValue(ref pointer, split);
                        break;
                    case "movement_frequency":
                        affector.movement_frequency = parseValue(ref pointer, split);
                        break;
                    case "ignore_negative_y":
                        affector.ignore_negative_y = bool.Parse(split[pointer][1]);
                        break;
                    case "ignore_negative_z":
                        affector.ignore_negative_z = bool.Parse(split[pointer][1]);
                        break;
                    case "movement":
                        affector.movement = parseValueTriple(ref pointer, split);
                        break;
                    case "worldsize":
                        affector.worldsize = parseValueTriple(ref pointer, split);
                        break;
                    case "forcefield_type":
                        affector.forcefield_type = split[pointer][1];
                        break;
                    case "gravity":
                        affector.gravity = parseValue(ref pointer, split);
                        break;
                    case "enabled":
                        affector.enabled = bool.Parse(split[pointer][1]);
                        break;
                    case "min_frequency":
                        affector.min_frequency = parseValue(ref pointer, split);
                        break;
                    case "max_frequency":
                        affector.max_frequency = parseValue(ref pointer, split);
                        break;
                    case "radius":
                        affector.radius = parseValue(ref pointer, split);
                        break;
                    case "box_height":
                        affector.box_height = parseValue(ref pointer, split);
                        break;
                    case "box_width":
                        affector.box_width = parseValue(ref pointer, split);
                        break;
                    case "box_depth":
                        affector.box_depth = parseValue(ref pointer, split);
                        break;
                    default:
                        throw new System.Exception("Unknown Affector parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return affector;
        }

        private static PUSystem.Technique.Extern parseExtern(ref int pointer, string[][] split)
        {
            PUSystem.Technique.Extern _extern = new PUSystem.Technique.Extern();
            _extern.type = split[pointer][1];
            if (split[pointer].Length > 2)
                _extern.name = split[pointer][2];

            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    case "box_width":
                        _extern.box_width = parseValue(ref pointer, split);
                        break;
                    case "collision_intersection":
                        _extern.collision_intersection = split[pointer][1];
                        break;
                    default:
                        throw new System.Exception("Unknown Extern parameter " + split[pointer][0]);
                }
                pointer++;
            }
            return _extern;
        }
        private static PUSystem.Technique.Behaviour parseBehaviour(ref int pointer, string[][] split)
        {
            PUSystem.Technique.Behaviour behaviour = new PUSystem.Technique.Behaviour();
            behaviour.type = split[pointer][1];
            pointer += 2; //{
            while (split[pointer][0] != "}")
            {
                switch (split[pointer][0])
                {
                    default:
                        throw new System.Exception("Unknown Behaviour parameter " + split[pointer][0]);
                }
                //pointer++;
            }

            return behaviour;
        }

        private static PUSystem.Technique.Affector.TimeColour parseTimeColour(ref int pointer, string[][] split)
        {
            float time = float.Parse(split[pointer][1]);
            float r = float.Parse(split[pointer][2]);
            float g = float.Parse(split[pointer][3]);
            float b = float.Parse(split[pointer][4]);
            float a = float.Parse(split[pointer][5]);
            return new PUSystem.Technique.Affector.TimeColour(time, r, g, b, a);
        }
    }
}