using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class Particle : MonoBehaviour
{
    public static GameObject AttachParticleSystemOld(string type, Transform parent)
    {
        GameObject particle = null;
        Debug.Log("AttachParticleSystem");
        if (parent.Find(type))
            return null;
        GameObject g = Resources.Load<GameObject>(type);
        if (g == null)
        {
            Debug.Log("Unknown particle type " + type);
            return g;     
        }
        particle = GameObject.Instantiate(g);
        particle.name = type;
        particle.transform.parent = parent;
        particle.transform.localPosition = Vector3.zero;
        //particle.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);                        
        switch (type)
        {
            case "fx_lumos":
                if (parent.Find("fx_lumos"))
                    return null;
                Debug.Log("FXLUMOS");
                particle = GameObject.Instantiate(Resources.Load<GameObject>("fx_lumos"));
                particle.name = "fx_lumos";
                particle.transform.parent = parent;
                particle.transform.localPosition = Vector3.zero;
                break;
            case "fx_wandSuccess":
                if (parent.Find("fx_wandSuccess"))
                    return null;
                Debug.Log("fx_wandSuccess");
                particle = GameObject.Instantiate(Resources.Load<GameObject>("fx_wandSuccess"));
                particle.name = "fx_wandSuccess";
                particle.transform.parent = parent;
                particle.transform.localPosition = Vector3.zero;
                break;
            default:
                Debug.Log("Unknown particle " + type);
                break;
        }
        return particle;
    }

    public static GameObject AttachParticleSystem(string particle_instance_id, Transform parent)
    {
        Debug.Log("AttachParticleSystem");
        if (parent.Find(particle_instance_id))
            return null;

        if (!Configs.config_particle_instance.ParticleInstance.ContainsKey(particle_instance_id))
        {
            Debug.LogError("Unknown particle instance type " + particle_instance_id);
            return null;
        }
        var particle_instance = Configs.config_particle_instance.ParticleInstance[particle_instance_id];

        if (!Configs.config_particle_config.ParticleConfig.ContainsKey(particle_instance.systemId))
        {
            Debug.LogError("Unknown particle system type " + particle_instance.systemId);
            return null;
        }

        var particle_system = Configs.config_particle_config.ParticleConfig[particle_instance.systemId];

        Vector3 scale_override = new Vector3(0.01f, 0.01f, 0.01f);
        if (particle_instance.scaleOverride != null)
        {
            scale_override = new Vector3(0.01f * particle_instance.scaleOverride[0], 0.01f * particle_instance.scaleOverride[1], 0.01f * particle_instance.scaleOverride[2]);
        }
        GameObject particle_go;
        if (particle_instance_id == "fx_lumos")
        {
            particle_go = GameObject.Instantiate(Resources.Load("fx_lumos") as GameObject);
        }
        else {
            particle_go = CocosPU.PUParticleSpawner.spawnParticle(particle_system.systemFilename, scale_override);
        }
        particle_go.transform.SetParent(parent);
        particle_go.transform.localPosition = new Vector3(0, 0, 0);
        return particle_go;
    }

}
