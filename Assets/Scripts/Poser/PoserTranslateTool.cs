using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoserTranslateTool : MonoBehaviour
{
    [SerializeField] PoserCollider x_arrow;
    [SerializeField] PoserCollider y_arrow;
    [SerializeField] PoserCollider z_arrow;
    Camera _camera;
    enum axis { none, X, Y, Z };

    bool x_enter;
    bool y_enter;
    bool z_enter;

    private void Start()
    {
        x_arrow.OnMouseEnterEvent += onXEnter;
        x_arrow.OnMouseExitEvent += onXExit;
        y_arrow.OnMouseEnterEvent += onYEnter;
        y_arrow.OnMouseExitEvent += onYExit;
        z_arrow.OnMouseEnterEvent += onZEnter;
        z_arrow.OnMouseExitEvent += onZExit;
        mouse_previous_position = Input.mousePosition;
        _camera = CameraManager.current.camera_component;
    }

    private axis current_axis = axis.none;
    Vector3 mouse_previous_position;
    float speed = 0.05f;
    private void Update()
    {
        Quaternion q = _camera.transform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));

        float z_camera_mod = (Mathf.Abs(_camera.transform.rotation.y) - 0.5f) * -2.0f;
        float x_camera_mod = (Mathf.Abs(q.y) - 0.5f) * -2.0f;

        if (Input.GetMouseButtonDown(0) && current_axis == axis.none) {
            if (x_enter)
                current_axis = axis.X;
            if (y_enter)
                current_axis = axis.Y;
            if (z_enter)
                current_axis = axis.Z;
        }
        if (Input.GetMouseButtonUp(0))
            current_axis = axis.none;

        if (Input.GetMouseButton(0) && current_axis != axis.none)
        {
            float difference_x = Input.mousePosition.x - mouse_previous_position.x;
            float difference_y = Input.mousePosition.y - mouse_previous_position.y;
            switch (current_axis)
            {
                case axis.Z:
                    transform.position = new Vector3(transform.position.x + difference_x * z_camera_mod * speed * Time.deltaTime, transform.position.y, transform.position.z);
                    break;
                case axis.Y:
                    transform.position = new Vector3(transform.position.x, transform.position.y + difference_y * speed * Time.deltaTime, transform.position.z);
                    break;
                case axis.X:
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + difference_x * x_camera_mod * speed  * Time.deltaTime);
                    break;
            }
        }
        mouse_previous_position = Input.mousePosition;
    }

    void onXEnter()
    {
        x_enter = true;
    }

    void onXExit()
    {
        x_enter = false;
    }

    void onYEnter()
    {
        y_enter = true;
    }

    void onYExit()
    {
        y_enter = false;
    }

    void onZEnter()
    {
        z_enter = true;
    }

    void onZExit()
    {
        z_enter = false;
    }
}
