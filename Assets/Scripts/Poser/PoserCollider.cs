using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoserCollider : MonoBehaviour
{
    public event Action OnMouseEnterEvent = delegate { };
    public event Action OnMouseExitEvent = delegate { };

    void OnMouseEnter()
    {
        OnMouseEnterEvent.Invoke();
    }

    void OnMouseExit()
    {
        OnMouseExitEvent.Invoke();
    }
}
