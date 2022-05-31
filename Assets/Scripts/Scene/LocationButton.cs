using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationButton : MonoBehaviour
{
    bool mouse_hover;
    Transform camera_transform;
    bool is_active = true;
    public string location_id;

    void OnMouseEnter()
    {
        mouse_hover = true;
    }

    private void OnMouseExit()
    {
        mouse_hover = false;
    }

    void FaceButtonToCamera(Transform textLookTargetTransform)
    {
        Vector3 targetDirection = textLookTargetTransform.position - transform.position;
        transform.rotation = Quaternion.LookRotation((targetDirection).normalized);
        transform.rotation = Quaternion.Euler(new Vector3(90.0f, transform.rotation.eulerAngles.y, 0.0f));
    }

    void Start()
    {
        camera_transform = GameObject.Find("Main Camera").GetComponent<Transform>();
        mouse_hover = false;
    }

    private void Update()
    {
        if (is_active)
        {
            FaceButtonToCamera(camera_transform);
            if ((Input.GetMouseButtonDown(0) && mouse_hover))
            {
                LocationScenarioMenu.showMenu(location_id);
            }
        }
    }
}
