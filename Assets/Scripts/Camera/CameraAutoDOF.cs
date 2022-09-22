using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
public class CameraAutoDOF : MonoBehaviour
{
    public HDAdditionalCameraData cameradata;

    /*void aStart()
    {
        RTHandle m_TmpRT;       // The RTHandle used to render the AOV
        Texture2D m_ReadBackTexture = new Texture2D(camera.pixelWidth, camera.pixelHeight);

        var hdAdditionalCameraData = gameObject.GetComponent<HDAdditionalCameraData>();
        if (hdAdditionalCameraData != null)
        {
            // initialize a new AOV request
            var aovRequest = AOVRequest.NewDefault();

            AOVBuffers[] aovBuffers = null;

            // Request an AOV with the surface albedo
            aovRequest.SetFullscreenOutput(DebugFullScreen.Depth);
            aovBuffers = new[] { AOVBuffers.DepthStencil };

            // Allocate the RTHandle that will store the intermediate results
            m_TmpRT = RTHandles.Alloc(camera.pixelWidth, camera.pixelHeight);

            // Add the request to a new AOVRequestBuilder
            var aovRequestBuilder = new AOVRequestBuilder();
            aovRequestBuilder.Add(aovRequest,
                bufferId => m_TmpRT,
                null,
                aovBuffers,
                null,
                bufferId => m_TmpRT,
                (cmd, textures, customPassTextures, properties) => { });

            //});

            // Now build the AOV request
            var aovRequestDataCollection = aovRequestBuilder.Build();

            // And finally set the request to the camera
            hdAdditionalCameraData.SetAOVRequests(aovRequestDataCollection);
        }
    }

    private void Update()
    {
        //GameStart.post_process_manager.changeDOFDistance(new_depth);
        /*Ray raycast = new Ray(transform.position, transform.forward * 100);
        bool isHit = false;
        RaycastHit hit;
        if (Physics.Raycast(raycast, out hit, 100f))
        {
            float hitDistance = Vector3.Distance(transform.position, hit.point);
            Debug.Log("REALDEPTH: " + hitDistance);
        }*/
        //GameStart.post_process_manager.changeDOFDistance(Random.Range(0.0f, 10.0f));


    //}
}



//{
// callback to read back the AOV data and write them to disk
//if (textures.Count > 0)
//{
//new_depth = 1.0f;

/*
 * 
 *     public float divider;
public float offset;

float new_depth;
 * 
 * m_ReadBackTexture = m_ReadBackTexture ?? new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGBAFloat, false);
RenderTexture.active = textures[0].rt;
m_ReadBackTexture.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0, false);
m_ReadBackTexture.Apply();
RenderTexture.active = null;

Color middlepixel = m_ReadBackTexture.GetPixel(camera.pixelWidth / 2, camera.pixelHeight / 2);

float depthSample = middlepixel.r;
float zNear = camera.farClipPlane;
float zFar = camera.nearClipPlane;
float eyeDepth = zFar * zNear / ((zNear - zFar) * depthSample + zFar);


float realDepth = (eyeDepth / divider) * 10 - offset;
GameStart.post_process_manager.changeDOFDistance(realDepth);*/
//}

