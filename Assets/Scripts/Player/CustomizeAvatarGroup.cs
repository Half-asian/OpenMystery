using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeAvatarGroup : MonoBehaviour
{
    CustomizeAvatar customize_avatar;
    CustomizeAvatarPreviewImages customize_avatar_preview_images;

    List<GameObject> preview_panels;
    [SerializeField]
    GameObject canvas;

    public int current_group_index = 0;

    string[] avatar_component_keys;
    void Start()
    {
        customize_avatar = GetComponent<CustomizeAvatar>();
        customize_avatar_preview_images = GetComponent<CustomizeAvatarPreviewImages>();
        spawnPreviewGrid();
    }

    public void renderTest()
    {
        avatar_component_keys = new string[Configs.config_avatar_components.AvatarComponents.Keys.Count];
        int i = 0;
        foreach (string s in Configs.config_avatar_components.AvatarComponents.Keys) {
            avatar_component_keys[i] = s;
            i++;
        }

        i = current_group_index * 5 * 3;
        foreach (GameObject g in preview_panels)
        {
            if (i < AvatarComponents.avatar_components_hair.Count)
            {
                GameObject avatar_image_go = g.transform.Find("AvatarImage").gameObject;
                g.SetActive(true);
                customize_avatar_preview_images.addToRenderQueue(AvatarComponents.avatar_components_hair[i], avatar_image_go.GetComponent<Image>());
                i++;
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    List<string> components_list;


    public void renderComponents()
    {
        int i = current_group_index * 5 * 3;
        foreach (GameObject g in preview_panels)
        {
            if (i < AvatarComponents.avatar_components_hair.Count)
            {
                GameObject avatar_image_go = g.transform.Find("AvatarImage").gameObject;
                g.SetActive(true);
                g.GetComponent<ComponentButton>().component_name = components_list[i];
                customize_avatar_preview_images.addToRenderQueue(components_list[i], avatar_image_go.GetComponent<Image>());
                i++;
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    public void renderHairComponents()
    {
        customize_avatar._camera_preview_depth.transform.position = new Vector3(0, -9.193f, 0.17f);
        customize_avatar._camera_preview.transform.position = new Vector3(0, -9.193f, 0.17f);
        customize_avatar.avatar_components_preview.resetFromPlayerFile();
        components_list = AvatarComponents.avatar_components_hair;
        current_group_index = 0;
        renderComponents();
    }

    public void renderTopsComponents()
    {
        customize_avatar._camera_preview_depth.transform.position = new Vector3(0, -9.39999962f, 0.270999998f);
        customize_avatar._camera_preview.transform.position = new Vector3(0, -9.39999962f, 0.270999998f);
        customize_avatar.avatar_components_preview.resetFromPlayerFile();
        components_list = AvatarComponents.avatar_components_tops;
        current_group_index = 0;
        renderComponents();
    }

    public void renderBottomsComponents()
    {
        customize_avatar._camera_preview_depth.transform.position = new Vector3(0, -9.805f, 0.441f);
        customize_avatar._camera_preview.transform.position = new Vector3(0, -9.805f, 0.441f);
        customize_avatar.avatar_components_preview.resetFromPlayerFile();
        components_list = AvatarComponents.avatar_components_bottoms;
        current_group_index = 0;
        renderComponents();
    }

    public void renderOnePieceComponents()
    {
        customize_avatar.avatar_components_preview.resetFromPlayerFile();
        customize_avatar._camera_preview_depth.transform.position = new Vector3(0, -9.6619997f, 0.67900002f);
        customize_avatar._camera_preview.transform.position = new Vector3(0, -9.6619997f, 0.67900002f);
        components_list = AvatarComponents.avatar_components_one_piece;
        current_group_index = 0;
        renderComponents();
    }

    public void nextPanel()
    {
        current_group_index++;
        renderComponents();
    }

    public void previousPanel()
    {
        current_group_index--;
        renderComponents();
    }

    public void spawnPreviewGrid()
    {
        preview_panels = new List<GameObject>();
        int x_start = 355;
        int y_start = 178;
        int x_offset = -82;
        int y_offset = -82;
        int x = 5;
        int y = 3;

        int component_counter = 0;
        component_counter += current_group_index * x * y;

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                GameObject new_panel = Instantiate(Resources.Load<GameObject>("PreviewPanel"));
                new_panel.transform.SetParent(canvas.transform);
                new_panel.transform.localPosition = new Vector3(x_start + x_offset * i, y_start + y_offset * j, 0);
                preview_panels.Add(new_panel);
                new_panel.SetActive(false);
            }
        }


    }

}
