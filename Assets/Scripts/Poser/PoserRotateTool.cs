using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoserRotateTool : MonoBehaviour
{
    [SerializeField] PoserCollider x_ring;
    [SerializeField] PoserCollider y_ring;
    [SerializeField] PoserCollider z_ring;
    Camera _camera;
    enum axis { none, X, Y, Z };

    bool x_enter;
    bool y_enter;
    bool z_enter;

    private void Start()
    {
        x_ring.OnMouseEnterEvent += onXEnter;
        x_ring.OnMouseExitEvent += onXExit;
        y_ring.OnMouseEnterEvent += onYEnter;
        y_ring.OnMouseExitEvent += onYExit;
        z_ring.OnMouseEnterEvent += onZEnter;
        z_ring.OnMouseExitEvent += onZExit;
        mouse_previous_position = Input.mousePosition;
        _camera = CameraManager.current.camera_component;
    }

    private axis current_axis = axis.none;
    Vector3 mouse_previous_position;
    float speed = 2f;
    private void Update()
    {
        Quaternion q = _camera.transform.rotation * Quaternion.Euler(new Vector3(0, 90, 0));

        float z_camera_mod = (Mathf.Abs(_camera.transform.rotation.y) - 0.5f) * -2.0f;
        float x_camera_mod = (Mathf.Abs(q.y) - 0.5f) * -2.0f;
        Debug.Log(z_camera_mod);
        Debug.Log(x_camera_mod);


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
            difference_x += difference_y;
            switch (current_axis)
            {
                case axis.X:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + difference_x * z_camera_mod * speed * Time.deltaTime);
                    break;
                case axis.Y:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + difference_y * speed * Time.deltaTime, transform.eulerAngles.z);
                    break;
                case axis.Z:
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x + difference_x * z_camera_mod * speed * Time.deltaTime, transform.eulerAngles.y, transform.eulerAngles.z);
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
