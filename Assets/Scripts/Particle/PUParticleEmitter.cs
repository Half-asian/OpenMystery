using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CocosPU.PUCommon;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

namespace CocosPU
{
    public partial class PUParticleSpawner
    {

        public static void applyEmitter(GameObject subsystem_go, ParticleSystem particle_system, ParticleSystem.MainModule particle_main, PUParticle.PUSystem.Technique.Emitter pu_emitter)
        {
            Emitter emitter;
            switch (pu_emitter.type)
            {
                case "Point":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                case "SphereSurface":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                case "Circle":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                case "Box":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                case "Slave":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                case "Line":
                    emitter = new EmitterPoint(pu_emitter);
                    break;
                default:
                    throw new System.Exception("Unknown emitter type " + pu_emitter.type);
            }
            emitter.apply(particle_system, particle_main);
            subsystem_go.transform.localScale = scale;

        }

        public abstract class Emitter
        {
            protected PUParticle.PUSystem.Technique.Emitter emitter;
            protected ParticleSystem particle_system;
            protected ParticleSystem.MainModule particle_main;

            protected ValueSingle _dynAngle;

            public Emitter(PUParticle.PUSystem.Technique.Emitter _emitter)
            {
                emitter = _emitter;
            }
            public virtual void apply(ParticleSystem _particle_system, ParticleSystem.MainModule _particle_main)
            {
                particle_system = _particle_system;
                particle_main = _particle_main;

                particle_main.startSpeed = new ParticleSystem.MinMaxCurve(0.0f); //Disable all initial particle movement. This is handled in lifetime.

                setEmitsType();
                setEmissionRate();
                setTimeToLive();
                setVelocity();
                setAllParticleDimensions();
                setColourRange();
                setColour();
                setDuration();
                setAngle();
                setDirection();
            }

            private void setEmitsType()
            {
                if (emitter.emits_type == "emitter_particle")
                {
                    Assert.IsNotNull(emitter.emits_id);
                    GameObject subsystem_go = new GameObject(emitter.emits_id);
                    subsystem_go.transform.SetParent(particle_system.transform);
                    ParticleSystem subsystem = subsystem_go.AddComponent<ParticleSystem>();
                    var subsystem_main = subsystem.main;
                    particle_systems.Add(subsystem);
                    particle_mains.Add(subsystem_main);
                    particle_system_renderers.Add(subsystem_go.GetComponent<ParticleSystemRenderer>());
                    applyEmitter(subsystem_go, subsystem, subsystem_main, named_emitters[emitter.emits_id]);
                    var sub_emitters = particle_system.subEmitters;
                    sub_emitters.enabled = true;
                    sub_emitters.AddSubEmitter(subsystem, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritNothing);
                }
            }
            private void setEmissionRate()
            {
                if (emitter.emission_rate != null)
                {
                    var emission = particle_system.emission;
                    emission.enabled = true;
                    if (emitter.emission_rate.value_type == ValueSingle.type.Fixed)
                    {
                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(emitter.emission_rate.min);
                    }
                    else if (emitter.emission_rate.value_type == ValueSingle.type.CurvedLinear)
                    {
                        AnimationCurve animation_curve = new AnimationCurve();
                        foreach (var control_point in emitter.emission_rate.controlPoints)
                        {
                            animation_curve.AddKey(control_point.time, control_point.value);
                        }

                        emission.rateOverTime = new ParticleSystem.MinMaxCurve(1.0f, animation_curve);
                    }
                }
            }
            private void setTimeToLive()
            {
                if (emitter.time_to_live != null)
                {
                    if (emitter.time_to_live.value_type == ValueSingle.type.Random)
                    {
                        ParticleSystem.MinMaxCurve start_life_time = new ParticleSystem.MinMaxCurve(emitter.time_to_live.min, emitter.time_to_live.max);
                        particle_main.startLifetime = start_life_time;
                    }
                    else
                    {
                        particle_main.startLifetime = new ParticleSystem.MinMaxCurve(emitter.time_to_live.min);
                    }
                }
            }
            private void setVelocity()
            {
                if (emitter.velocity != null)
                {
                    if (emitter.velocity.value_type == ValueSingle.type.Random)
                    {
                        var velocity_over_lifetime = particle_system.velocityOverLifetime;
                        velocity_over_lifetime.enabled = true;
                        velocity_over_lifetime.speedModifier = new ParticleSystem.MinMaxCurve(emitter.velocity.min, emitter.velocity.max);
                    }
                    else
                    {
                        var velocity_over_lifetime = particle_system.velocityOverLifetime;
                        velocity_over_lifetime.enabled = true;
                        velocity_over_lifetime.speedModifier = new ParticleSystem.MinMaxCurve(emitter.velocity.min);
                    }
                }
            }
            private void setAllParticleDimensions()
            {
                if (emitter.all_particle_dimensions != null)
                {
                    if (emitter.all_particle_dimensions.value_type == ValueSingle.type.Random)
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
            }
            private void setColourRange()
            {
                if (emitter.start_colour_range != null)
                {
                    GradientColorKey[] gradient_min_color_keys;
                    if (emitter.end_colour_range != null)
                    {

                        gradient_min_color_keys = new GradientColorKey[4]
                        {
                        new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.end_colour_range.y    , emitter.end_colour_range.z    ).gamma, 0      ),
                        new GradientColorKey(new Color(emitter.end_colour_range.x   , emitter.start_colour_range.y  , emitter.end_colour_range.z    ).gamma, 0.33f  ),
                        new GradientColorKey(new Color(emitter.end_colour_range.x   , emitter.end_colour_range.y    , emitter.start_colour_range.z  ).gamma, 0.66f  ),
                        new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.end_colour_range.y    , emitter.end_colour_range.z    ).gamma, 1      ),
                        };
                    }
                    else
                    {
                        gradient_min_color_keys = new GradientColorKey[4]
                        {
                        new GradientColorKey(new Color(emitter.start_colour_range.x , 1.0f    , 1.0f    ).gamma, 0      ),
                        new GradientColorKey(new Color(1.0f   , emitter.start_colour_range.y  , 1.0f    ).gamma, 0.33f  ),
                        new GradientColorKey(new Color(1.0f   , 1.0f    , emitter.start_colour_range.z  ).gamma, 0.66f  ),
                        new GradientColorKey(new Color(emitter.start_colour_range.x , 1.0f    , 1.0f    ).gamma, 1      ),
                        };
                    }
                    GradientColorKey[] gradient_max_color_keys = new GradientColorKey[2]
                    {
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.start_colour_range.y    , emitter.start_colour_range.z  ).gamma, 0    ),
                new GradientColorKey(new Color(emitter.start_colour_range.x , emitter.start_colour_range.y    , emitter.start_colour_range.z  ).gamma, 1    ),
                    };
                    GradientAlphaKey[] gradient_min_alpha_keys;
                    if (emitter.end_colour_range != null)
                    {
                        gradient_min_alpha_keys = new GradientAlphaKey[2]
                        {
                        new GradientAlphaKey(emitter.start_colour_range.w   , 0.0f ),
                        new GradientAlphaKey(emitter.end_colour_range.w     , 1.0f )
                        };
                    }
                    else
                    {
                        gradient_min_alpha_keys = new GradientAlphaKey[2]
                        {
                        new GradientAlphaKey(emitter.start_colour_range.w   , 0.0f ),
                        new GradientAlphaKey(1.0f     , 1.0f )
                        };
                    }

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
            }
            private void setColour()
            {
                if (emitter.colour != null)
                {
                    particle_main.startColor = new ParticleSystem.MinMaxGradient(new Color(emitter.colour.x, emitter.colour.y, emitter.colour.z, emitter.colour.w).gamma);
                }
            }
            private void setDuration()
            {
                particle_system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                particle_main.duration = 0.0f;
                particle_main.loop = false;

                if (emitter.duration != null)
                {
                    particle_main.duration = emitter.duration.min;
                }
                particle_system.Play(true);
            }
            private void setAngle()
            {

                if (emitter.angle != null)
                {
                    //Angle to min-max direction-vectors
                    //Min Vec3(0, 1, 0) Max Vec3(0, 1, 0) is the default direction vectors. It points upwards. Angle = 0
                    //Min Vec3(1, 1, 1) Max Vec3(-1, -1, -1) is the max set of direction vectors. It allows any particle to move in any direction. Angle = 360
                    //There is still the issue of a particle getting Vec3(0, 0, 0) as its final vector. This is seemingly unavoidable

                    Vector3 min = new Vector3(emitter.angle.min / 360.0f, 1.0f, emitter.angle.min / 360.0f);
                    Vector3 max = new Vector3(emitter.angle.min / -360.0f, 1.0f + 2 * emitter.angle.min / -360.0f, emitter.angle.min / -360.0f);

                    var velocity_over_lifetime = particle_system.velocityOverLifetime;
                    velocity_over_lifetime.enabled = true;
                    velocity_over_lifetime.x = new ParticleSystem.MinMaxCurve(max.x, min.x);
                    velocity_over_lifetime.y = new ParticleSystem.MinMaxCurve(max.y, min.y);
                    velocity_over_lifetime.z = new ParticleSystem.MinMaxCurve(max.z, min.y);
                }
            }

            private void setDirection()
            {
                if (emitter.direction != null)
                {
                    var velocity_over_lifetime = particle_system.velocityOverLifetime;
                    velocity_over_lifetime.enabled = true;
                    velocity_over_lifetime.x = new ParticleSystem.MinMaxCurve(emitter.direction.x);
                    velocity_over_lifetime.y = new ParticleSystem.MinMaxCurve(emitter.direction.y);
                    velocity_over_lifetime.z = new ParticleSystem.MinMaxCurve(emitter.direction.z);
                }
            }
        }

        private class EmitterPoint : Emitter
        {
            public EmitterPoint(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.scale = new Vector3(0.01f, 0.01f, 0.01f);
                shape.randomDirectionAmount = 1.0f; //Randomize the direction
            }
        }
        private class EmitterSphereSurface : Emitter
        {
            public EmitterSphereSurface(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.randomDirectionAmount = 1.0f; //Randomize the direction
            }
        }
        private class EmitterCircle : Emitter
        {
            private float radius = 100.0f;
            private float circleAngle = 0.0f;
            private float originalCircleAngle = 0.0f;
            private float step = 0.1f;
            private float x = 0.0f;
            private float z = 0.0f;
            private bool random = true;
            private Quaternion orientation = Quaternion.identity;
            private Vector3 normal = Vector3.zero;
            public EmitterCircle(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
                if (_emitter.radius != null)
                    radius = (float)_emitter.radius;
                if (_emitter.angle != null)
                {
                    circleAngle = _emitter.angle.min;
                    originalCircleAngle = circleAngle;
                }
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.Circle; //This is more accurate
                shape.rotation = new Vector3(-90.0f, 0, 0);
                shape.arcMode = ParticleSystemShapeMultiModeValue.Loop;
                shape.radiusThickness = 0.0f;
                shape.radius = radius;
            }
        }
        private class EmitterBox : Emitter
        {
            private float height = 100.0f;
            private float width = 100.0f;
            private float depth = 100.0f;
            private float _xRange = 50.0f; //unused
            private float _yRange = 50.0f; //unused
            private float _zRange = 50.0f; //unused
            public EmitterBox(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
                if (_emitter.box_width != null)
                {
                    width = (float)_emitter.box_width;
                    _xRange = 0.5f * width;
                }
                if (_emitter.box_height != null)
                {
                    height = (float)_emitter.box_height;
                    _yRange = 0.5f * height;
                }
                if (_emitter.box_depth != null)
                {
                    depth = (float)_emitter.box_depth;
                    _zRange = 0.5f * depth;
                }
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.boxThickness = new Vector3(width, height, depth);
            }
        }
        private class EmitterSlave : Emitter
        {
            public EmitterSlave(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.randomDirectionAmount = 1.0f; //Randomize the direction
            }
        }
        private class EmitterLine : Emitter
        {
            public EmitterLine(PUParticle.PUSystem.Technique.Emitter _emitter) : base(_emitter)
            {
            }
            public override void apply(ParticleSystem particle_system, ParticleSystem.MainModule particle_main)
            {
                base.apply(particle_system, particle_main);
                var shape = particle_system.shape;
                shape.shapeType = ParticleSystemShapeType.SingleSidedEdge;
                shape.randomDirectionAmount = 1.0f; //Randomize the direction
            }
        }
    }
}
