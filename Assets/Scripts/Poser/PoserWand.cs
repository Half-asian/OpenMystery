using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoserWand : MonoBehaviour
{
    MeshRenderer mesh_renderer;
    Vector3 particle_position = new Vector3(-0.05f, -0.0017f, -0.0008f);
    Vector3 particle_rotation = new Vector3(-7.573f, -76.699f, 171.077f);
    void Start()
    {
        mesh_renderer = GetComponent<MeshRenderer>();
        Poser.on_poser_enabled += toggleModel;
        Poser.on_tool_used += toolUsed;
    }

    void toggleModel(bool enabled)
    {
        mesh_renderer.enabled = enabled;
    }

    void toolUsed()
    {
        var particle = Instantiate(Resources.Load("ElderWandParticle") as GameObject);
        particle.transform.parent = transform;
        particle.transform.localPosition = particle_position;
        particle.transform.localEulerAngles = particle_rotation;
    }
}
