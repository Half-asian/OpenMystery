using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentButton : MonoBehaviour
{
    public string component_name = null;

    public void setComponent()
    {
        GameStart.current.GetComponent<CustomizeAvatar>().changeAvatarComponent(component_name);
    }
}
