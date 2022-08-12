using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CustomizeAvatarPreviewImages : MonoBehaviour
{

    List<(string, Image)> render_queue = new List<(string, Image)>();
    CustomizeAvatar customize_avatar;

    Coroutine render_queue_coroutine = null;

    [SerializeField]
    Shader ui_remove_depth;

    public void Start()
    {
        customize_avatar = GetComponent<CustomizeAvatar>();
    }

    IEnumerator processRenderQueue()
    {
        while (render_queue.Count != 0)
        {
            customize_avatar.avatar_components_preview.equipAvatarComponent(render_queue[0].Item1);
            renderToImage(render_queue[0].Item2, customize_avatar._camera_preview.GetComponent<Camera>(), customize_avatar._camera_preview_depth.GetComponent<Camera>());
            render_queue.RemoveAt(0);
            yield return new WaitForEndOfFrame();

        }
        render_queue_coroutine = null;
    }


    public void renderToImage(Image image, Camera camera_colour, Camera camera_depth)
    {
        image.material = new Material(ui_remove_depth);


        RenderTexture rt_colour = new RenderTexture(new RenderTextureDescriptor(400, 400, RenderTextureFormat.Default));
        rt_colour.name = render_queue[0].Item1 + "colour";
        camera_colour.targetTexture = rt_colour;
        RenderTexture.active = camera_depth.targetTexture;
        camera_colour.Render();

        RenderTexture rt_depth = new RenderTexture(new RenderTextureDescriptor(400, 400, RenderTextureFormat.Depth, 24));
        rt_depth.name = render_queue[0].Item1 + "depth";
        camera_depth.targetTexture = rt_depth;
        RenderTexture.active = camera_depth.targetTexture;
        camera_depth.Render();

        image.material.SetTexture("_MainTex", rt_colour);
        image.material.SetTexture("_DepthTex", rt_depth);
        image.RecalculateMasking();
    }

    public void addToRenderQueue(string component, Image image)
    {
        render_queue.Add((component, image));

        if (render_queue_coroutine == null)
            render_queue_coroutine = StartCoroutine(processRenderQueue());
    }

}
