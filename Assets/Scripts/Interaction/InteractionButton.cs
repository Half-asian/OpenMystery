using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionButton : MonoBehaviour
{
    bool mouse_hover;
    public Interaction interaction;
    Transform camera_transform;
    public bool is_active = false;
    MeshRenderer mesh_renderer;

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
        mesh_renderer = GetComponent<MeshRenderer>();
        camera_transform = GameObject.Find("Main Camera").GetComponent<Transform>();
        mouse_hover = false;
        StartCoroutine(waitForEventsStackClear());
    }

    IEnumerator waitForEventsStackClear()
    {
        while (GameStart.event_manager.areEventsActive())
            yield return null;

        is_active = true;
    }


    private void Update()
    {
        if (GameStart.dialogue_manager.dialogue_status != DialogueStatus.Finished)
        {
            if (mesh_renderer) mesh_renderer.enabled = false;
        }

        else if (is_active)
        {
            FaceButtonToCamera(camera_transform);

            if (mesh_renderer) mesh_renderer.enabled = true;

            if ((Input.GetMouseButtonDown(0) && mouse_hover)) //Clicked on
            {
                interaction.activate();
                if (!interaction.shouldShow)
                {
                    is_active = false;
                    if (this == null) return;
                    if (mesh_renderer) mesh_renderer.enabled = false;
                }
                else
                {
                    if (this == null) return;
                    if (mesh_renderer) mesh_renderer.enabled = true;
                }
            }

            if (Input.GetKeyDown("space") && interaction.config_interaction != null) //Project
            {
                if (interaction.config_interaction.type == "AutotuneGroup" || interaction.config_interaction.type == null)
                {
                    is_active = false;
                    interaction.interactionComplete();
                }
            }

            if (interaction != null && interaction.config_interaction != null) //Real interaction
            {
                if (interaction.config_interaction.AutoSelect == true) //Autoselect
                {
                    //interaction.activate();
                    is_active = false;
                    if (mesh_renderer) mesh_renderer.enabled = false;
                }
                else
                {

                    if (mesh_renderer) mesh_renderer.enabled = true;
                }
            }
            else
            {
                if (mesh_renderer) mesh_renderer.enabled = true;
            }
        }
        else
        {
            if (mesh_renderer) mesh_renderer.enabled = false;
        }

    }
}
