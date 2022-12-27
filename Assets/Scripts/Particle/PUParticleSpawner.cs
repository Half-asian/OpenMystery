using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PUParticleSpawner : MonoBehaviour
{
    public static Dictionary<string, PUParticle.System.Technique.Emitter> named_emitters;
    public static List<ParticleSystem> particle_systems;
    public static List<ParticleSystem.MainModule> particle_mains;
    public static List<ParticleSystemRenderer> particle_system_renderers;
    public static GameObject spawnParticle(string file)
    {
        GameObject particle_go = new GameObject(file.Substring(0, file.Length - 3));
        var pu_system = PUParticle.loadParticle(file);

        foreach (var pu_technique in pu_system.techniques.Values)
        {
            GameObject technique_go = new GameObject(pu_technique.name);
            technique_go.transform.SetParent(particle_go.transform);

            particle_systems = new List<ParticleSystem>();
            particle_mains = new List<ParticleSystem.MainModule>();
            particle_system_renderers = new List<ParticleSystemRenderer>();

            named_emitters = new Dictionary<string, PUParticle.System.Technique.Emitter>();

            List<string> child_emitters = new List<string>();
            foreach (var pu_emitter in pu_technique.emitters) {
                if (pu_emitter.name != null)
                {
                    named_emitters.Add(pu_emitter.name, pu_emitter);
                }

                if (pu_emitter.emits_type == "emitter_particle")
                {
                    child_emitters.Add(pu_emitter.emits_id);
                }
            }

            foreach (var pu_emitter in pu_technique.emitters)
            {
                if (pu_emitter.name != null && child_emitters.Contains(pu_emitter.name))
                    continue;

                GameObject emitter_go = new GameObject(pu_emitter.name);
                emitter_go.transform.SetParent(technique_go.transform);
                var particle_system = emitter_go.AddComponent<ParticleSystem>();
                var particle_main = particle_system.main;
                var particle_system_renderer = emitter_go.GetComponent<ParticleSystemRenderer>();
                particle_main.maxParticles = (int)pu_technique.visual_particle_quota.min;

                switch (pu_emitter.type)
                {
                    case "Point":
                        EmitterPoint(particle_system, particle_main, pu_emitter);
                        break;
                    case "SphereSurface":
                        EmitterSphereSurface(particle_system, particle_main, pu_emitter);
                        break;
                    case "Circle":
                        EmitterCircle(particle_system, particle_main, pu_emitter);
                        break;
                }

                switch (pu_technique.renderer.type)
                {
                    case "RibbonTrail":
                        RendererRibbonTrail(particle_system, particle_main, pu_technique.renderer);
                        break;
                }

                particle_systems.Add(particle_system);
                particle_mains.Add(particle_main);
                particle_system_renderers.Add(particle_system_renderer);

            }
            Material mat = null;

            switch (pu_technique.material)
            {
                case "flare01_v7":
                case "mat_flare04_v248":
                    mat = Resources.Load("pumpflare") as Material;
                    break;
                case "mat_mysmoke_v253":
                    mat = Resources.Load("smokePuffSheet_04_v24") as Material;
                    break;
                case "mywater_v22":
                    mat = Resources.Load("mywater_v22") as Material;
                    break;
                case "mat_flare06_v17":
                    mat = Resources.Load("pump_flare_06_v1") as Material;
                    break;
                case "mat_flare05_v25":
                    mat = Resources.Load("pump_flare_05_v1") as Material;
                    break;

            }
            foreach(var particle_system_renderer in particle_system_renderers)
            {
                particle_system_renderer.sharedMaterial = mat;
                particle_system_renderer.trailMaterial = Resources.Load("RibbonMat") as Material;
            }

            foreach (var pu_affector in pu_technique.affectors)
            {
                switch (pu_affector.type)
                {
                    case "LinearForce":
                        AffectorLinearForce(particle_systems, pu_affector);
                        break;
                    case "ScaleVelocity":
                        break;
                    case "Scale":
                        for (int i = 0; i < particle_mains.Count; i++)
                        {
                            AffectorScale(particle_mains[i], particle_systems[i].name, pu_affector);
                        }
                        break;
                    case "Colour":
                        AffectorColour(particle_systems, pu_affector);
                        break;
                    case "TextureRotator":
                        break;
                    case "SineForce":
                        break;
                    case "TextureAnimator":
                        AffectorTextureAnimator(particle_systems, pu_affector);
                        break;
                    case "Slave":
                        break;
                    case "ForceField":
                        break;
                    case "Vortex":
                        AffectorVortex(particle_systems, pu_affector);
                        break;
                }
            }

        }

        return particle_go;
    }

    private static void EmitterCommon(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.System.Technique.Emitter emitter)
    {
        if (emitter.emits_type == "emitter_particle")
        {
            GameObject subsystem_go = new GameObject(emitter.emits_id);
            subsystem_go.transform.SetParent(particle_system.transform);
            ParticleSystem subsystem = subsystem_go.AddComponent<ParticleSystem>();
            var subsystem_main = subsystem.main;
            particle_systems.Add(subsystem);
            particle_mains.Add(subsystem_main);
            particle_system_renderers.Add(subsystem_go.GetComponent<ParticleSystemRenderer>());
            EmitterCommon(subsystem, subsystem_main, named_emitters[emitter.emits_id]);
            var sub_emitters = particle_system.subEmitters;
            sub_emitters.enabled = true;
            sub_emitters.AddSubEmitter(subsystem, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritEverything);
        }

        if (emitter.emission_rate != null)
        {
            var emission = particle_system.emission;
            if (emitter.emission_rate.value_type == PUParticle.ValueSingle.type.Fixed)
            {
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(emitter.emission_rate.min);
            }
            else if (emitter.emission_rate.value_type == PUParticle.ValueSingle.type.CurvedLinear)
            {
                AnimationCurve animation_curve = new AnimationCurve();
                foreach(var control_point in emitter.emission_rate.controlPoints)
                {
                    animation_curve.AddKey(control_point.time, control_point.value);
                }

                emission.rateOverTime = new ParticleSystem.MinMaxCurve(1.0f, animation_curve);
            }
        }

        if (emitter.time_to_live != null)
        {
            if (emitter.time_to_live.value_type == PUParticle.ValueSingle.type.Random)
            {
                ParticleSystem.MinMaxCurve start_life_time = new ParticleSystem.MinMaxCurve(emitter.time_to_live.min, emitter.time_to_live.max);
                particle_main.startLifetime = start_life_time;
            }
            else
            {
                particle_main.startLifetime = new ParticleSystem.MinMaxCurve(emitter.time_to_live.min);
            }
        }

        if (emitter.velocity != null)
        {
            if (emitter.velocity.value_type == PUParticle.ValueSingle.type.Random)
            {
                Debug.Log("Setting random emitter velocity");
                var minMaxCurve = new ParticleSystem.MinMaxCurve(emitter.velocity.min, emitter.velocity.max);
                minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
                particle_main.startSpeed = minMaxCurve;
            }
            else
            {
                particle_main.startSpeed = new ParticleSystem.MinMaxCurve(emitter.velocity.min);
            }
        }

        if (emitter.all_particle_dimensions != null)
        {
            if (emitter.all_particle_dimensions.value_type == PUParticle.ValueSingle.type.Random)
            {
                var minMaxCurve = new ParticleSystem.MinMaxCurve(emitter.all_particle_dimensions.min, emitter.all_particle_dimensions.max);
                minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
                particle_main.startSize = minMaxCurve;
            }
            else
            {
                particle_main.startSize = new ParticleSystem.MinMaxCurve(emitter.all_particle_dimensions.min);
            }
        }

        if (emitter.start_colour_range != null)
        {

            //This works quite weirdly. 0001 to 1111 is not black to white.
            //4 samples
            //1st sample takes R max, B min, G min.
            //2nd sample takes R min, B max, G min.
            //3rd sample takes R min, B min, G max.
            //4th sample takes R max, B min, G min.

            GradientColorKey[] gradient_min_color_keys = new GradientColorKey[4]
            {
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.end_colour_range.y    , emitter.end_colour_range.z    ).gamma, 0      ),
                new GradientColorKey(new Color(emitter.end_colour_range.x   , emitter.start_colour_range.y  , emitter.end_colour_range.z    ).gamma, 0.33f  ),
                new GradientColorKey(new Color(emitter.end_colour_range.x   , emitter.end_colour_range.y    , emitter.start_colour_range.z  ).gamma, 0.66f  ),
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.end_colour_range.y    , emitter.end_colour_range.z    ).gamma, 1      ),

            };
            GradientColorKey[] gradient_max_color_keys = new GradientColorKey[2]
            {
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.start_colour_range.y    , emitter.start_colour_range.z  ).gamma, 0    ),
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.start_colour_range.y    , emitter.start_colour_range.z  ).gamma, 1    ),
            };
            GradientAlphaKey[] gradient_min_alpha_keys = new GradientAlphaKey[2]
            {
                new GradientAlphaKey(emitter.start_colour_range.w   , 0.0f ),
                new GradientAlphaKey(emitter.end_colour_range.w     , 1.0f )
            };

            Gradient gradient_min = new Gradient();
            gradient_min.SetKeys(gradient_min_color_keys, gradient_min_alpha_keys);

            Gradient gradient_max = new Gradient();
            gradient_max.SetKeys(gradient_max_color_keys, gradient_min_alpha_keys);

            var start_color = new ParticleSystem.MinMaxGradient(gradient_max, gradient_min);
            start_color.mode = ParticleSystemGradientMode.RandomColor; 
            //Problem with unity, there's no way to randomly select between two gradients. So no way to randomise saturation.
            //All colours are full saturation but random.
            //Alternative is random saturation but predicted color
            particle_main.startColor = start_color;
        }
        
        if (emitter.colour != null)
        {
            particle_main.startColor = new ParticleSystem.MinMaxGradient(new Color(emitter.colour.x, emitter.colour.y, emitter.colour.z, emitter.colour.w).gamma);
        }

        if (emitter.duration != null)
        {
            particle_system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle_main.duration = emitter.duration.min;
            //particle_main.loop = false;
            particle_system.Play(true);
        }
        var shape = particle_system.shape;

        if (emitter.angle != null)
            shape.angle = emitter.angle.min;
        if (emitter.radius != null)
            shape.radius = emitter.radius.min;

    }

    private static void EmitterPoint(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.System.Technique.Emitter emitter)
    {
        EmitterCommon(particle_system, particle_main, emitter);


        var shape = particle_system.shape;

        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.scale = new Vector3(0.01f, 0.01f, 0.01f);
        shape.randomDirectionAmount = 1.0f; //Randomize the direction
    }
    private static void EmitterCircle(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.System.Technique.Emitter emitter)
    {
        EmitterCommon(particle_system, particle_main, emitter);


        var shape = particle_system.shape;

        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.scale = new Vector3(0.01f, 0.01f, 0.01f);
        shape.randomDirectionAmount = 1.0f; //Randomize the direction
    }
    private static void EmitterSphereSurface(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.System.Technique.Emitter emitter)
    {
        EmitterCommon(particle_system, particle_main, emitter);

        var shape = particle_system.shape;

        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.randomDirectionAmount = 1.0f; //Randomize the direction
    }

    private static void RendererRibbonTrail(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.System.Technique.Renderer renderer)
    {

        var trails = particle_system.trails;
        trails.mode = ParticleSystemTrailMode.Ribbon;
        trails.enabled = true;
        trails.attachRibbonsToTransform = true;
        if (renderer.initial_colour != null)
        {
            Color ribbon_color = new Color(renderer.initial_colour.x, renderer.initial_colour.y, renderer.initial_colour.z, renderer.initial_colour.w).gamma;
            trails.colorOverLifetime = new ParticleSystem.MinMaxGradient(ribbon_color);
        }
        if (renderer.max_elements != null)
        {
            trails.ribbonCount = (int)renderer.max_elements.min;
        }
        if (renderer.ribbontrail_width != null)
        {
            trails.widthOverTrail = renderer.ribbontrail_width.min;
        }
    }

    private static void AffectorColour(List<ParticleSystem> particle_systems, PUParticle.System.Technique.Affector affector)
    {
        Gradient gradient = new Gradient();
        List<GradientColorKey> color_keys = new List<GradientColorKey>();
        List<GradientAlphaKey> alpha_keys = new List<GradientAlphaKey>();

        foreach (var time_colour in affector.timeColours)
        {
            GradientColorKey colour_key = new GradientColorKey();
            colour_key.time = time_colour.time;
            colour_key.color = new Color(time_colour.r, time_colour.g, time_colour.b).gamma;
            GradientAlphaKey alpha_key = new GradientAlphaKey();
            alpha_key.time = time_colour.time;
            alpha_key.alpha = time_colour.a;

            color_keys.Add(colour_key);
            alpha_keys.Add(alpha_key);
        }

        gradient.SetKeys(color_keys.ToArray(), alpha_keys.ToArray());
        ParticleSystem.MinMaxGradient minMaxGradient = new ParticleSystem.MinMaxGradient(gradient);

        foreach (var particle_system in particle_systems)
        {
            if (affector.exclude_emitter == particle_system.name)
                continue;
            var colorOverLifetime = particle_system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = minMaxGradient;
        }
    }

    private static void AffectorScale(ParticleSystem.MainModule main, string emitter_name, PUParticle.System.Technique.Affector affector)
    {
        if (affector.exclude_emitter == emitter_name)
            return;
        if (affector.xyz_scale.value_type == PUParticle.ValueSingle.type.Random == true)
        {
            main.startSize = new ParticleSystem.MinMaxCurve(Mathf.Abs(affector.xyz_scale.min), Mathf.Abs(affector.xyz_scale.max));
        }
        else
        {
            main.startSize = new ParticleSystem.MinMaxCurve(Mathf.Abs(affector.xyz_scale.min));
        }
    }

    private static void AffectorLinearForce(List<ParticleSystem> particle_systems, PUParticle.System.Technique.Affector affector)
    {
        foreach (var particle_system in particle_systems)
        {
            if (affector.exclude_emitter == particle_system.name)
                continue;
            var force_over_lifetime = particle_system.forceOverLifetime;
            force_over_lifetime.x = affector.force_vector.x;
            force_over_lifetime.y = affector.force_vector.y;
            force_over_lifetime.z = affector.force_vector.z;
            force_over_lifetime.enabled = true;
        }
    }

    private static void AffectorTextureAnimator(List<ParticleSystem> particle_systems, PUParticle.System.Technique.Affector affector)
    {
        foreach (var particle_system in particle_systems)
        {
            if (affector.exclude_emitter == particle_system.name)
                continue;
            var texture_sheet_animation = particle_system.textureSheetAnimation;
            texture_sheet_animation.numTilesX = (int)Mathf.Sqrt(affector.end_texture_coords_range.min + 1);
            texture_sheet_animation.numTilesY = (int)Mathf.Sqrt(affector.end_texture_coords_range.min + 1);
            texture_sheet_animation.enabled = true;
        }
    }

    private static void AffectorVortex(List<ParticleSystem> particle_systems, PUParticle.System.Technique.Affector affector)
    {
        foreach (var particle_system in particle_systems)
        {
            if (affector.exclude_emitter == particle_system.name)
                continue;
            var velocity_over_lifetime = particle_system.velocityOverLifetime;
            velocity_over_lifetime.enabled = true;
            velocity_over_lifetime.orbitalX = affector.rotation_axis.z;
            velocity_over_lifetime.orbitalY = affector.rotation_axis.y;
            velocity_over_lifetime.orbitalZ = -affector.rotation_axis.x;
            velocity_over_lifetime.speedModifier = new ParticleSystem.MinMaxCurve(affector.rotation_speed.min);
        }
    }
}
