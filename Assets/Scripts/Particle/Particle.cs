using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public static GameObject AttachParticleSystem(string type, Transform parent)
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
}
