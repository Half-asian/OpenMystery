using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using static CocosPU.PUCommon;

namespace CocosPU
{
    public partial class PUParticleSpawner : MonoBehaviour
    {
        private static Dictionary<string, PUParticle.PUSystem.Technique.Emitter> named_emitters;
        private static List<ParticleSystem> particle_systems;
        private static List<ParticleSystem.MainModule> particle_mains;
        private static List<ParticleSystemRenderer> particle_system_renderers;
        private static Vector3 scale;
        public static GameObject spawnParticle(string file, Vector3 _scale)
        {
            Debug.Log("spawnParticle " + file);
            GameObject particle_go = new GameObject(file.Substring(0, file.Length - 3));
            var pu_system = PUParticle.loadParticle(file);

            if (pu_system == null)
            {
                Debug.LogError("Failed to load particle system " + file);
                return null;
            }

            scale = _scale;

            foreach (var pu_technique in pu_system.techniques.Values)
            {
                GameObject technique_go = new GameObject(pu_technique.name);
                technique_go.transform.SetParent(particle_go.transform);

                Material mat = PUMaterial.loadMaterial(pu_technique.material + ".material");
                if (mat == null)
                {
                    continue;
                }

                particle_systems = new List<ParticleSystem>();
                particle_mains = new List<ParticleSystem.MainModule>();
                particle_system_renderers = new List<ParticleSystemRenderer>();

                named_emitters = new Dictionary<string, PUParticle.PUSystem.Technique.Emitter>();

                List<string> child_emitters = new List<string>();
                foreach (var pu_emitter in pu_technique.emitters)
                {
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
                    emitter_go.transform.localScale = scale;
                    emitter_go.transform.SetParent(technique_go.transform);
                    var particle_system = emitter_go.AddComponent<ParticleSystem>();
                    var particle_main = particle_system.main;
                    var particle_system_renderer = emitter_go.GetComponent<ParticleSystemRenderer>();
                    particle_main.maxParticles = (int)pu_technique.visual_particle_quota.min;

                    applyEmitter(emitter_go, particle_system, particle_main, pu_emitter);

                    switch (pu_technique.renderer.type)
                    {
                        case "RibbonTrail":
                            RendererRibbonTrail(particle_system, particle_main, pu_technique.renderer);
                            break;
                        case "Billboard":
                            RendererBillBoard(particle_system, particle_main, pu_technique.renderer);
                            break;
                    }

                    particle_systems.Add(particle_system);
                    particle_mains.Add(particle_main);
                    particle_system_renderers.Add(particle_system_renderer);

                }



                foreach (var particle_system_renderer in particle_system_renderers)
                {
                    particle_system_renderer.sharedMaterial = mat;
                    particle_system_renderer.trailMaterial = Resources.Load("PUMaterials/RibbonMat") as Material;
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
                            AffectorScale(particle_systems, pu_affector);
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

        private static void RendererCommon(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.PUSystem.Technique.Renderer renderer)
        {
            if (renderer.texture_coords_rows != null)
            {
                var texture_sheet_animation = particle_system.textureSheetAnimation;
                texture_sheet_animation.enabled = true;
                texture_sheet_animation.numTilesX = (int)renderer.texture_coords_rows.min;
            }
            if (renderer.texture_coords_columns != null)
            {
                var texture_sheet_animation = particle_system.textureSheetAnimation;
                texture_sheet_animation.enabled = true;
                texture_sheet_animation.numTilesY = (int)renderer.texture_coords_columns.min;
            }
        }
        private static void RendererBillBoard(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.PUSystem.Technique.Renderer renderer)
        {
            RendererCommon(particle_system, particle_main, renderer);
        }
        private static void RendererRibbonTrail(ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.PUSystem.Technique.Renderer renderer)
        {
            RendererCommon(particle_system, particle_main, renderer);
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

        private static void AffectorColour(List<ParticleSystem> particle_systems, PUParticle.PUSystem.Technique.Affector affector)
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
            try
            {
                gradient.SetKeys(color_keys.ToArray(), alpha_keys.ToArray());
            }
            catch
            {
                Debug.LogError("Couldn't set color keys");
            }
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

        private static void AffectorScale(List<ParticleSystem> particle_systems, PUParticle.PUSystem.Technique.Affector affector)
        {
            //This is severely nerfed. Might be possible to improve somewhat
            foreach (var particle_system in particle_systems)
            {
                if (affector.exclude_emitter == particle_system.name)
                    return;

                var size_over_lifetime = particle_system.sizeOverLifetime; //This only goes from 0.0 to 1.0 :(
                size_over_lifetime.enabled = true;

                if (affector.xyz_scale != null)
                {
                    float final_scale_min = Mathf.Max(affector.xyz_scale.min, 0.0f);
                    float final_scale_max = Mathf.Max(affector.xyz_scale.max, 0.0f);

                    if (affector.xyz_scale.value_type == ValueSingle.type.Random)
                    {
                        if (final_scale_min > particle_system.main.startSize.Evaluate(0.0f)) //Grow over life
                        {
                            AnimationCurve anim_curve = new AnimationCurve();
                            anim_curve.AddKey(0.0f, particle_system.main.startSize.Evaluate(0.0f) / final_scale_min);
                            anim_curve.AddKey(1.0f, 1.0f);
                            size_over_lifetime.size = new ParticleSystem.MinMaxCurve(1.0f, anim_curve);
                        }
                        else //Shrink over life
                        {
                            AnimationCurve anim_curve = new AnimationCurve();
                            anim_curve.AddKey(0.0f, 1.0f);
                            anim_curve.AddKey(1.0f, final_scale_min / particle_system.main.startSize.Evaluate(0.0f));
                            size_over_lifetime.size = new ParticleSystem.MinMaxCurve(1.0f, anim_curve);
                        }
                    }
                }
                /*else
                {
                    throw new System.Exception("unimplimented");
                }*/
            }
        }

        private static void AffectorLinearForce(List<ParticleSystem> particle_systems, PUParticle.PUSystem.Technique.Affector affector)
        {
            foreach (var particle_system in particle_systems)
            {
                if (affector.exclude_emitter == particle_system.name)
                    continue;
                var force_over_lifetime = particle_system.forceOverLifetime;
                if (affector.force_vector != null)
                {
                    force_over_lifetime.x = affector.force_vector.x;
                    force_over_lifetime.y = affector.force_vector.y;
                    force_over_lifetime.z = affector.force_vector.z;
                }
                force_over_lifetime.enabled = true;
            }
        }

        private static void AffectorTextureAnimator(List<ParticleSystem> particle_systems, PUParticle.PUSystem.Technique.Affector affector)
        {
            foreach (var particle_system in particle_systems)
            {
                if (affector.exclude_emitter == particle_system.name)
                    continue;
                var texture_sheet_animation = particle_system.textureSheetAnimation;
                if (affector.end_texture_coords_range != null)
                {
                    texture_sheet_animation.numTilesX = (int)Mathf.Sqrt(affector.end_texture_coords_range.min + 1);
                    texture_sheet_animation.numTilesY = (int)Mathf.Sqrt(affector.end_texture_coords_range.min + 1);
                }
                texture_sheet_animation.enabled = true;
            }
        }

        private static void AffectorVortex(List<ParticleSystem> particle_systems, PUParticle.PUSystem.Technique.Affector affector)
        {
            foreach (var particle_system in particle_systems)
            {
                if (affector.exclude_emitter == particle_system.name)
                    continue;
                var velocity_over_lifetime = particle_system.velocityOverLifetime;
                velocity_over_lifetime.enabled = true;
                if (affector.rotation_axis != null)
                {
                    velocity_over_lifetime.orbitalX = affector.rotation_axis.z;
                    velocity_over_lifetime.orbitalY = affector.rotation_axis.y;
                    velocity_over_lifetime.orbitalZ = -affector.rotation_axis.x;
                }
                if (affector.rotation_speed != null)
                    velocity_over_lifetime.speedModifier = new ParticleSystem.MinMaxCurve(affector.rotation_speed.min);
            }
        }
    }
}